#region License
// Copyright (c) 2017 Doppler Relay Team
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using MakingSense.DopplerFeatureToggle.Internal;
using MakingSense.DopplerFeatureToggle.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = MakingSense.DopplerFeatureToggle.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
namespace MakingSense.DopplerFeatureToggle.Tests
{
    [TestFixture]
    public class HttpFeatureToggleClientTests : TestFixtureBase
    {
        const string ValidJsonDocument =
        #region large JSON document
        @"
{
    ""features"": [
        {
            ""name"": ""Feature1"",
            ""treatments"": [
                { ""name"": ""Treatment1A"", ""includedDifferentiators"": [ ""A"", ""B"", ""C"" ] },
                { ""name"": ""Treatment1B"", ""includedDifferentiators"": [ ""D"", ""E"", ""F""] }
            ]
        },
        {
            ""name"": ""Feature2"",
            ""treatments"": [
                { ""name"": ""Treatment2A"", ""includedDifferentiators"": [ ""G"", ""H"", ""I""] },
                { ""name"": ""Treatment2B"", ""includedDifferentiators"": [ ] }
            ]
        },
        {
            ""name"": ""Feature3"",
            ""treatments"": [
                { ""name"": ""Enabled"", ""includedDifferentiators"": [ ] },
                { ""name"": ""Disabled"", ""includedDifferentiators"": [ ""J"", ""K"", ""L"", ""M"" ] }
            ]
        }
    ]
}
";
        #endregion
        const string AnotherValidJsonDocument =
        #region large JSON document
        @"
{
    ""features"": [
        {
            ""name"": ""Feature1"",
            ""treatments"": [
                { ""name"": ""Treatment1A"", ""includedDifferentiators"": [ ""A"", ""B"", ""C"" ] },
                { ""name"": ""Treatment1B"", ""includedDifferentiators"": [ ""D"", ""E"", ""F""] }
            ]
        },
        {
            ""name"": ""Feature2"",
            ""treatments"": [
                { ""name"": ""Treatment2A"", ""includedDifferentiators"": [ ""G"", ""H"", ""I""] },
                { ""name"": ""Treatment2B"", ""includedDifferentiators"": [ ""Z""] }
            ]
        },
        {
            ""name"": ""Feature3"",
            ""treatments"": [
                { ""name"": ""Enabled"", ""includedDifferentiators"": [ ] },
                { ""name"": ""Disabled"", ""includedDifferentiators"": [ ""J"", ""K"", ""L"", ""M"" ] }
            ]
        }
    ]
}
";
        #endregion
        const string EmptyValidJsonDocument = @"{ ""features"": [ ] }";

        const string OnlineExample1 = "https://raw.githubusercontent.com/MakingSense/doppler-feature-toggle/resources/example1.json";

        [Test]
        public void HttpFeatureToggleClient_should_update_rules_after_update()
        {
            // Arrange
            var httpClientDouble = new HttpClientDouble();
            httpClientDouble.Setup_GetString(ValidJsonDocument);

            var client = new HttpFeatureToggleClient(httpClientDouble, "url");

            // Assert
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature2", "H", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature2", "Z", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature3", "M", "default").Result);

            // Act
            client.UpdateAsync().Wait();

            // Assert
            Assert.AreEqual("Treatment2A", client.GetTreatmentAsync("Feature2", "H", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature2", "Z", "default").Result);
            Assert.AreEqual("Disabled", client.GetTreatmentAsync("Feature3", "M", "default").Result);

            // Arrange
            httpClientDouble.Setup_GetString(AnotherValidJsonDocument);

            // Act
            client.UpdateAsync().Wait();

            // Assert
            Assert.AreEqual("Treatment2B", client.GetTreatmentAsync("Feature2", "Z", "default").Result);

            // Arrange
            httpClientDouble.Setup_GetString(EmptyValidJsonDocument);

            // Act
            client.UpdateAsync().Wait();

            // Assert
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature2", "H", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature2", "Z", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature3", "M", "default").Result);
        }

        [Test]
        public void HttpFeatureToggleClient_should_ignore_invalid_responses()
        {
            // Arrange
            var httpClientDouble = new HttpClientDouble();
            httpClientDouble.Setup_GetString(ValidJsonDocument);
            var client = new HttpFeatureToggleClient(httpClientDouble, "url");
            client.UpdateAsync().Wait();

            Assert.AreEqual("Treatment2A", client.GetTreatmentAsync("Feature2", "H", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature2", "Z", "default").Result);
            Assert.AreEqual("Disabled", client.GetTreatmentAsync("Feature3", "M", "default").Result);

            httpClientDouble.Setup_GetString("INVALID CONTENT");

            // Act
            try
            {
                client.UpdateAsync().Wait();
            }
            catch
            {
                // Ignoring exception, UpdatePeriodically ignore them
            }

            // Assert
            Assert.AreEqual("Treatment2A", client.GetTreatmentAsync("Feature2", "H", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature2", "Z", "default").Result);
            Assert.AreEqual("Disabled", client.GetTreatmentAsync("Feature3", "M", "default").Result);
        }

        [Test]
        public void HttpFeatureToggleClient_should_continue_working_after_invalid_response()
        {
            // Arrange
            var httpClientDouble = new HttpClientDouble();
            httpClientDouble.Setup_GetString(ValidJsonDocument);
            var client = new HttpFeatureToggleClient(httpClientDouble, "url");
            client.UpdateAsync().Wait();

            Assert.AreEqual("Treatment2A", client.GetTreatmentAsync("Feature2", "H", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature2", "Z", "default").Result);
            Assert.AreEqual("Disabled", client.GetTreatmentAsync("Feature3", "M", "default").Result);

            httpClientDouble.Setup_GetString("INVALID CONTENT");
            try
            {
                client.UpdateAsync().Wait();
            }
            catch
            {
                // Ignoring exception, UpdatePeriodically ignore them
            }

            Assert.AreEqual("Treatment2A", client.GetTreatmentAsync("Feature2", "H", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature2", "Z", "default").Result);
            Assert.AreEqual("Disabled", client.GetTreatmentAsync("Feature3", "M", "default").Result);

            httpClientDouble.Setup_GetString(EmptyValidJsonDocument);

            // Act
            client.UpdateAsync().Wait();

            // Assert
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature2", "H", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature2", "Z", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Feature3", "M", "default").Result);
        }

        [Test]
        public void HttpFeatureToggleClient_update_rules_based_on_remote_resource()
        {
            // Arrange
            var client = new HttpFeatureToggleClient(OnlineExample1); // OnlineExample1 refers to a real online JSON file

            // Assert
            Assert.AreEqual("default", client.GetTreatmentAsync("UnexistentFeature", "AnyValue", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Example1Feature2", "Example1H", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Example1Feature2", "Example1Z", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Example1Feature3", "Example1M", "default").Result);

            // Act
            client.UpdateAsync().Wait();

            // Assert
            Assert.AreEqual("default", client.GetTreatmentAsync("UnexistentFeature", "AnyValue", "default").Result);
            Assert.AreEqual("Example1Treatment2A", client.GetTreatmentAsync("Example1Feature2", "Example1H", "default").Result);
            Assert.AreEqual("default", client.GetTreatmentAsync("Example1Feature2", "Example1Z", "default").Result);
            Assert.AreEqual("Disabled", client.GetTreatmentAsync("Example1Feature3", "Example1M", "default").Result);

        }
    }
}

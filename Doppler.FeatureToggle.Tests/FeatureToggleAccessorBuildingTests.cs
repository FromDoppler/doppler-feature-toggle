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

using Doppler.FeatureToggle.Internal;
using Doppler.FeatureToggle.Tests;
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
using Assert = Doppler.FeatureToggle.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
namespace Doppler.FeatureToggle.Tests
{
    [TestFixture]
    public class FeatureToggleAccessorBuildingTests : TestFixtureBase
    {
        const string ValidJsonDocument =
        #region large JSON document
        @"
{
    ""features"": [
        {
            ""name"": ""BooleanFeature"",
            ""treatments"": [
                { ""name"": ""Enabled"", ""includedDifferentiators"": [ ""N"", ""O"" ] },
                { ""name"": ""Disabled"", ""includedDifferentiators"": [ ""P"", ""Q"" ] }
            ]
        },
        {
            ""name"": ""DateBehavior"",
            ""treatments"": [
                { ""name"": ""ISO"", ""includedDifferentiators"": [ ""Andres"" ] },
                { ""name"": ""English"", ""includedDifferentiators"": [ ""Cristian"" ] },
                { ""name"": ""Spanish"", ""includedDifferentiators"": [ ""Mauro"" ] }
            ]
        }
    ]
}
";
        #endregion

        [Test]
        public void AccessorBuilding_general_test()
        {
            // Arrange
            var httpClientDouble = new HttpClientDouble();
            httpClientDouble.Setup_GetString(ValidJsonDocument);

            var client = new HttpFeatureToggleClient(httpClientDouble, "url");

            var booleanAccessor = client
                .CreateFeature<bool>("BooleanFeature")
                .AddBehavior("Disabled", false)
                .AddBehavior("Enabled", true)
                .SetDefaultTreatment("Disabled")
                .ForceTreatmentIfNotNull(null)
                .Build();

            var forcedBooleanAccessor = client
                .CreateFeature<bool>("BooleanFeature")
                .AddBehavior("Disabled", false)
                .AddBehavior("Enabled", true)
                .SetDefaultTreatment("Disabled")
                .ForceTreatmentIfNotNull("Enabled")
                .Build();

            var dateFormatAccessor = client
                .CreateFeature<DateTime, string>("DateBehavior")
                .AddBehavior("ISO", x => x.ToString("yyyy-MM-dd"))
                .AddBehavior("English", x => x.ToString("MM/dd/yyyy"))
                .AddBehavior("Spanish", x => x.ToString("dd/MM/yyyy"))
                .SetDefaultTreatment("ISO")
                .Build();

            // Act
            client.UpdateAsync().Wait();

            // Assert
            Assert.AreEqual(true, booleanAccessor.Get("N"));
            Assert.AreEqual(true, booleanAccessor.Get("O"));
            Assert.AreEqual(false, booleanAccessor.Get("P"));
            Assert.AreEqual(false, booleanAccessor.Get("Q"));
            Assert.AreEqual(false, booleanAccessor.Get("NotExistentDifferentiator"));

            Assert.AreEqual(true, forcedBooleanAccessor.Get("N"));
            Assert.AreEqual(true, forcedBooleanAccessor.Get("O"));
            Assert.AreEqual(true, forcedBooleanAccessor.Get("P"));
            Assert.AreEqual(true, forcedBooleanAccessor.Get("Q"));
            Assert.AreEqual(true, forcedBooleanAccessor.Get("NotExistentDifferentiator"));

            var date = new DateTime(2018, 12, 20);
            Assert.AreEqual("20/12/2018", dateFormatAccessor.Get("Mauro", date));
            Assert.AreEqual("12/20/2018", dateFormatAccessor.Get("Cristian", date));
            Assert.AreEqual("2018-12-20", dateFormatAccessor.Get("Andres", date));
            Assert.AreEqual("2018-12-20", dateFormatAccessor.Get("NotExistentDifferentiator", date));
        }

        [Test]
        public void AccessorBuilding_should_not_accept_unexistent_ForceTreatment()
        {
            // Arrange
            var httpClientDouble = new HttpClientDouble();
            var client = new HttpFeatureToggleClient(httpClientDouble, "url");

            try
            {
                var booleanAccessor = client
                    .CreateFeature<bool>("BooleanFeature")
                    .AddBehavior("Disabled", false)
                    .AddBehavior("Enabled", true)
                    .SetDefaultTreatment("Disabled")
                    .ForceTreatmentIfNotNull("ANOTHER")
                    .Build();
                Assert.Fail("Expected exception not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Forced treatment does not match a defined treatment.", e.Message);
            }
        }

        [Test]
        public void AccessorBuilding_should_not_accept_unexistent_DefaultTreatment()
        {
            // Arrange
            var httpClientDouble = new HttpClientDouble();
            var client = new HttpFeatureToggleClient(httpClientDouble, "url");

            try
            {
                var booleanAccessor = client
                    .CreateFeature<bool>("BooleanFeature")
                    .AddBehavior("Disabled", false)
                    .AddBehavior("Enabled", true)
                    .SetDefaultTreatment("ANOTHER")
                    .Build();
                Assert.Fail("Expected exception not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Default treatment does not match a defined treatment.", e.Message);
            }
        }

        [Test]
        public void AccessorBuilding_should_require_DefaultTreatment()
        {
            // Arrange
            var httpClientDouble = new HttpClientDouble();
            var client = new HttpFeatureToggleClient(httpClientDouble, "url");

            try
            {
                var booleanAccessor = client
                    .CreateFeature<bool>("BooleanFeature")
                    .AddBehavior("Disabled", false)
                    .AddBehavior("Enabled", true)
                    .Build();
                Assert.Fail("Expected exception not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Cannot build a feature without default treatment defined.", e.Message);
            }
        }
    }
}

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
    public class FeatureToggleAccessorTests : TestFixtureBase
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

        public class BooleanFeatureAccessorDemo : FeatureToggleAccessor<bool>
        {
            public BooleanFeatureAccessorDemo(IFeatureToggleClient featureToggleClient)
                : base(
                    featureToggleClient,
                    featureName: "BooleanFeature",
                    defaultTreatment: "Disabled",
                    forceTreatment: null,
                    treatments: new Dictionary<string, Func<bool>>()
                    {
                        ["Disabled"] = () => false,
                        ["Enabled"] = () => true
                    })
            {

            }
        }

        public class ForcedBooleanFeatureAccessorDemo : FeatureToggleAccessor<bool>
        {
            public ForcedBooleanFeatureAccessorDemo(IFeatureToggleClient featureToggleClient, string forceTreatment)
                : base(
                    featureToggleClient,
                    featureName: "BooleanFeature",
                    defaultTreatment: "Disabled",
                    forceTreatment: forceTreatment,
                    treatments: new Dictionary<string, Func<bool>>()
                    {
                        ["Disabled"] = () => false,
                        ["Enabled"] = () => true
                    })
            {

            }
        }

        public class DateBehaviorAccessorDemo : FeatureToggleAccessor<Func<DateTime, string>>
        {
            public DateBehaviorAccessorDemo(IFeatureToggleClient featureToggleClient)
                : base(
                    featureToggleClient,
                    featureName: "DateBehavior",
                    defaultTreatment: "ISO",
                    forceTreatment: null,
                    treatments: new Dictionary<string, Func<Func<DateTime, string>>>()
                    {
                        ["ISO"] = () => (date => date.ToString("yyyy-MM-dd")),
                        ["English"] = () => (date => date.ToString("MM/dd/yyyy")),
                        ["Spanish"] = () => (date => date.ToString("dd/MM/yyyy"))
                    })
            {

            }
        }

        [Test]
        public void Accessors_general_test()
        {
            // Arrange
            var httpClientDouble = new HttpClientDouble();
            httpClientDouble.Setup_GetString(ValidJsonDocument);

            var client = new HttpFeatureToggleClient(httpClientDouble, "url");
            var booleanAccessor = new BooleanFeatureAccessorDemo(client);
            var forcedBooleanAccessor = new ForcedBooleanFeatureAccessorDemo(client, "Enabled");
            var dateFormatAccessor = new DateBehaviorAccessorDemo(client);

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

            var mauroFormatter = dateFormatAccessor.Get("Mauro");
            var cristianFormatter = dateFormatAccessor.Get("Cristian");
            var andresFormatter = dateFormatAccessor.Get("Andres");
            var defaultFormatter = dateFormatAccessor.Get("NotExistentDifferentiator");

            var date = new DateTime(2018, 12, 20);
            Assert.AreEqual("20/12/2018", mauroFormatter(date));
            Assert.AreEqual("12/20/2018", cristianFormatter(date));
            Assert.AreEqual("2018-12-20", andresFormatter(date));
            Assert.AreEqual("2018-12-20", defaultFormatter(date));
        }
    }
}

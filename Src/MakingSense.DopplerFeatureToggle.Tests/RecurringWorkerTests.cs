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

namespace MakingSense.DopplerFeatureToggle
{
    [TestFixture]
    public class RecurringWorkerTests : TestFixtureBase
    {
        [Test]
        public void RecurringWorker_should_execute_tasks_periodically()
        {
            // Arrange
            var counter = 0;
            var delayBeforeStart = TimeSpan.FromMilliseconds(400);
            var delayAfterEnd = TimeSpan.FromMilliseconds(400);
            var due = TimeSpan.FromMilliseconds(1200);
            var period = TimeSpan.FromMilliseconds(300);
            var workingTime = TimeSpan.FromMilliseconds(3800);
            var expectedCount = 9;
            var tolerance = 3;

            Task testAction()
            {
                counter++;
                return TaskUtilities.CompletedTask;
            }

            using (var sut = new RecurringWorker(testAction, due, period))
            {
                sut.ExecutionOmitted += (o, e) => counter++;
                Delay(delayBeforeStart);
                sut.Start();
                Assert.AreEqual(0, counter);

                // Act
                Delay(workingTime);
            }
            Delay(delayAfterEnd);

            // Assert
#if (DNXCORE50)
            Assert.InRange(counter, expectedCount - tolerance, expectedCount + tolerance);
#else
            Assert.GreaterOrEqual(counter, expectedCount - tolerance);
            Assert.LessOrEqual(counter, expectedCount + tolerance);
#endif
        }
    }
}

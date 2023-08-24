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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Doppler.FeatureToggle.Internal;
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
    public class UpdaterExtensionsTests : TestFixtureBase
    {

        [Test]
        public void UpdatePeriodically_should_create_an_already_started_RecurringWorker_to_call_updater()
        {
            // Arrange
            var updater = new UpdaterDouble();

            var due = TimeSpan.FromMilliseconds(200);
            var period = TimeSpan.FromMilliseconds(1000);
            var testTime = TimeSpan.FromMilliseconds(400);

            // Act
            var worker = updater.UpdatePeriodically(due, period);

            // Assert
            Assert.IsNotNull(worker);
            Assert.AreEqual(period, worker.Period);
            Assert.AreEqual(due, worker.DueTime);
            Assert.IsTrue(worker.Running);
            Assert.IsFalse(worker.WorkInProgress);
            Assert.AreEqual(0, updater.UpdateCount);

            Delay(testTime);
            Assert.AreEqual(1, updater.UpdateCount);

            // Act
            worker.Dispose();

            // Assert
            Assert.IsNotNull(worker);
            Assert.AreEqual(period, worker.Period);
            Assert.AreEqual(due, worker.DueTime);
            Assert.IsFalse(worker.Running);
            Assert.IsFalse(worker.WorkInProgress);
        }
    }

    public class UpdaterDouble : IUpdater
    {
        public int UpdateCount { get; private set; }

        public Task UpdateAsync()
        {
            UpdateCount++;
            return TaskUtilities.CompletedTask;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakingSense.DopplerFeatureToggle
{
    /// <summary>
    /// Helper methods for IUpdater
    /// </summary>
    public static class UpdaterExtensions
    {
        /// <summary>
        /// Run update periodically
        /// </summary>
        public static RecurringWorker UpdatePeriodically(this IUpdater updater, TimeSpan dueTime, TimeSpan period)
        {
            var worker = new RecurringWorker(updater.UpdateAsync, dueTime, period);
            worker.Start();
            return worker;
        }
    }
}

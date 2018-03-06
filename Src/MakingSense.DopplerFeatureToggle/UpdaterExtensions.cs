using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakingSense.DopplerFeatureToggle
{
    public static class UpdaterExtensions
    {
        public static RecurringWorker UpdatePeriodically(this IUpdater updater, TimeSpan dueTime, TimeSpan period) =>
            new RecurringWorker(updater.UpdateAsync, dueTime, period);
    }
}

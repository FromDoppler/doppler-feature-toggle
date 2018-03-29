using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakingSense.DopplerFeatureToggle
{
    /// <summary>
    /// Feature-toggle client
    /// </summary>
    public interface IFeatureToggleClient : IUpdater
    {
        /// <summary>
        /// Allow to get the treatment as string based on feature name, differentiator and default treatment.
        /// </summary>
        Task<string> GetTreatmentAsync(string featureName, string differentiator, string defaultTreatment);
    }
}

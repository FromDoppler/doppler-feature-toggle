using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doppler.FeatureToggle
{
    /// <summary>
    /// Something that can update an object based on a resource
    /// </summary>
    public interface IUpdater
    {
        /// <summary>
        /// Execute the update.
        /// </summary>
        /// <returns></returns>
        Task UpdateAsync();
    }
}

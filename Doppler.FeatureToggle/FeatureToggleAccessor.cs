using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakingSense.DopplerFeatureToggle
{
    /// <summary>
    /// Accessor that allows to resolve a feature behavior
    /// </summary>
    public class FeatureToggleAccessor<T>
    {
        private readonly IFeatureToggleClient _client;
        private readonly string _featureName;
        private readonly string _defaultTreatment;
        private readonly string _forceTreatment;
        private readonly IDictionary<string, Func<T>> _treatments;

        /// <summary>
        /// Create a new accessor to resolve feature behavior based on specified rules.
        /// </summary>
        protected internal FeatureToggleAccessor(
           IFeatureToggleClient featureToggleClient,
           string featureName,
           string defaultTreatment,
           string forceTreatment,
           IDictionary<string, Func<T>> treatments)
        {
            if (defaultTreatment == null)
            {
                throw new InvalidOperationException("Cannot build a feature without default treatment defined.");
            }

            if (!treatments.ContainsKey(defaultTreatment))
            {
                throw new InvalidOperationException("Default treatment does not match a defined treatment.");
            }

            if (!string.IsNullOrEmpty(forceTreatment) && !treatments.ContainsKey(forceTreatment))
            {
                throw new InvalidOperationException("Forced treatment does not match a defined treatment.");
            }

            _client = featureToggleClient;
            _featureName = featureName;
            _defaultTreatment = defaultTreatment;
            _forceTreatment = forceTreatment;
            _treatments = treatments;
        }

        /// <summary>
        /// Feature value based on differentiator
        /// </summary>
        public T Get(string differentiator) => GetAsync(differentiator).Result;

        /// <summary>
        /// Feature value based on differentiator
        /// </summary>
        public async Task<T> GetAsync(string differentiator)
        {
            var treatment = !string.IsNullOrEmpty(_forceTreatment)
                ? _forceTreatment
                : await _client.GetTreatmentAsync(
                    _featureName,
                    differentiator,
                    _defaultTreatment);

            return _treatments.TryGetValue(treatment, out Func<T> behavior)
                ? behavior()
                : throw new NotImplementedException($"There is not register behavior for {treatment}. Differentiator: {differentiator}");
        }
    }
}

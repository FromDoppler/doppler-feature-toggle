using MakingSense.DopplerFeatureToggle.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakingSense.DopplerFeatureToggle
{
    /// <inheritdoc />
    public class HttpFeatureToggleClient : IFeatureToggleClient
    {
        // TODO: this class is breaking SRP, split in more classes

        private readonly IHttpClient _httpClient;
        private readonly string _url;
        private string _lastEtag = null;
#if NET45 || NETSTANDARD1_0 || NETSTANDARD1_3
        private IReadOnlyDictionary<string, Feature> _features = new ReadOnlyDictionary<string, Feature>(new Dictionary<string, Feature>());
#else
        private IDictionary<string, Feature> _features = new Dictionary<string, Feature>();
#endif

        /// <summary>
        /// Create an HttpFeatureToggleClient using specified HttpClient and URL
        /// </summary>
        public HttpFeatureToggleClient(IHttpClient httpClient, string url)
        {
            _httpClient = httpClient;
            _url = url;
        }

        /// <summary>
        /// Create an HttpFeatureToggleClient using specified URL and default HttpClient
        /// </summary>
        public HttpFeatureToggleClient(string url)
            : this(new HttpClient(), url)
        {
        }

        /// <inheritdoc />
        public Task<string> GetTreatmentAsync(string featureName, string differentiator, string defaultTreatment)
        {
            // In general
            // * INDETERMINATE_TREATMENT => return defaultTreatment
            // * Should not be errors here, if there were an error updating rules or segments, it is transparent here.
            var treatment = _features.TryGetValue(featureName, out Feature feature)
                ? feature.Treatments
                    .Where(x => x.Value.IncludedDifferentiators.Contains(differentiator))
                    .Select(x => x.Key)
                    .FirstOrDefault()
                : null;
            return TaskUtilities.FromResult(treatment ?? defaultTreatment);
        }

        /// <summary>
        /// Update feature-toggle rules based on remote storage
        /// </summary>

        public async Task UpdateAsync()
        {
            var response = await _httpClient.GetStringAsync(_url, _lastEtag);
            if (response.NotModified)
            {
                return;
            }

            _lastEtag = response.Etag;
            var json = JObject.Parse(response.Body);
            var parsed = ParseFeatures(json);
#if NET45 || NETSTANDARD1_0 || NETSTANDARD1_3
            _features = new ReadOnlyDictionary<string, Feature>(parsed);
#else
            _features = parsed;
#endif
        }

        private class Feature
        {
            public string Name { get; }
#if NET45 || NETSTANDARD1_0 || NETSTANDARD1_3
            public IReadOnlyDictionary<string, Treatment> Treatments { get; }
#else
            public Dictionary<string, Treatment> Treatments { get; }
#endif

            public Feature(string name, IDictionary<string, Treatment> treatments)
            {
                Name = name;
#if NET45 || NETSTANDARD1_0 || NETSTANDARD1_3
                Treatments = new ReadOnlyDictionary<string, Treatment>(treatments);
#else
                Treatments = treatments.ToDictionary(x => x.Key, x => x.Value);
#endif
            }
        }

        private class Treatment
        {
            public string Name { get; }
#if NET35
            public HashSet<string> IncludedDifferentiators { get; }
#else
            public ISet<string> IncludedDifferentiators { get; }
#endif

            public Treatment(string name, IEnumerable<string> includedDifferentiators)
            {
                Name = name;
                IncludedDifferentiators = new HashSet<string>(includedDifferentiators, StringComparer.OrdinalIgnoreCase);
            }
        }

        private IDictionary<string, Feature> ParseFeatures(JObject json) =>
                json["features"]
                    .Select(ParseFeature)
                    .ToDictionary(x => x.Name);

        private Feature ParseFeature(JToken json) =>
            new Feature(
                json.Value<string>("name"),
                json["treatments"]
                    .Select(ParseTreatment)
                    .ToDictionary(x => x.Name));

        private Treatment ParseTreatment(JToken json) =>
            new Treatment(
                json.Value<string>("name"),
                json["includedDifferentiators"].Values<string>());
    }
}

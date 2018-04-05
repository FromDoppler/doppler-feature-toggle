using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakingSense.DopplerFeatureToggle
{
    /// <summary>
    /// Some extensions to easily create accessors
    /// </summary>
    public static class FeatureToggleClientExtensions
    {
        public static FeatureToggleAccessorBuilder<T> CreateFeature<T>(
            this IFeatureToggleClient splitClient,
            string featureName)
            => new FeatureToggleAccessorBuilder<T>(splitClient, featureName);
        public static FeatureToggleAccessorBuilder<TArg, TResult> CreateFeature<TArg, TResult>(
            this IFeatureToggleClient splitClient,
            string featureName)
            => new FeatureToggleAccessorBuilder<TArg, TResult>(splitClient, featureName);
        public static FeatureToggleAccessorBuilder<TArg1, TArg2, TResult> CreateFeature<TArg1, TArg2, TResult>(
            this IFeatureToggleClient splitClient,
            string featureName)
            => new FeatureToggleAccessorBuilder<TArg1, TArg2, TResult>(splitClient, featureName);
        public static FeatureToggleAccessorBuilder<TArg1, TArg2, TArg3, TResult> CreateFeature<TArg1, TArg2, TArg3, TResult>(
            this IFeatureToggleClient splitClient,
            string featureName)
            => new FeatureToggleAccessorBuilder<TArg1, TArg2, TArg3, TResult>(splitClient, featureName);
    }

    /// <summary>
    /// Some syntactic sugar for accessors
    /// </summary>
    public static class FeatureToggleAccessorExtensions
    {
        public static TResult Get<TArg, TResult>(
            this FeatureToggleAccessor<Func<TArg, TResult>> featureToggleAccessor,
            string differentiator,
            TArg arg)
            => featureToggleAccessor.Get(differentiator)(arg);
        public static TResult Get<TArg1, TArg2, TResult>(
            this FeatureToggleAccessor<Func<TArg1, TArg2, TResult>> featureToggleAccessor,
            string differentiator,
            TArg1 arg1,
            TArg2 arg2)
            => featureToggleAccessor.Get(differentiator)(arg1, arg2);
        public static TResult Get<TArg1, TArg2, TArg3, TResult>(
            this FeatureToggleAccessor<Func<TArg1, TArg2, TArg3, TResult>> featureToggleAccessor,
            string differentiator,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3)
            => featureToggleAccessor.Get(differentiator)(arg1, arg2, arg3);
    }

    /// <summary>
    /// Helper class to define accessors
    /// </summary>
    public class FeatureToggleAccessorBuilder<T>
    {
        private readonly Dictionary<string, Func<T>> _treatments = new Dictionary<string, Func<T>>(StringComparer.OrdinalIgnoreCase);
        private readonly IFeatureToggleClient _splitClient;
        private readonly string _featureName;
        string _defaultTreatment;
        string _forceTreatment;

        public FeatureToggleAccessorBuilder(IFeatureToggleClient splitClient, string featureName)
        {
            _splitClient = splitClient;
            _featureName = featureName ?? throw new ArgumentNullException(nameof(featureName));
        }

        public FeatureToggleAccessor<T> Build()
            => new FeatureToggleAccessor<T>(
                _splitClient,
                _featureName,
                _defaultTreatment,
                _forceTreatment,
                _treatments);

        public FeatureToggleAccessorBuilder<T> SetDefaultTreatment(string treatment)
        {
            _defaultTreatment = treatment ?? throw new ArgumentNullException(nameof(treatment));
            return this;
        }

        public FeatureToggleAccessorBuilder<T> AddBehavior(string treatment, Func<T> behavior)
        {
            _treatments[treatment] = behavior;
            return this;
        }

        /// <summary>
        /// If treatmentName is null, it will not be forced
        /// </summary>
        public FeatureToggleAccessorBuilder<T> ForceTreatmentIfNotNull(string behaviorName)
        {
            _forceTreatment = string.IsNullOrEmpty(behaviorName)
                ? null
                : behaviorName;
            return this;
        }
    }

    /// <summary>
    /// Helper class to define accessors
    /// </summary>
    public class FeatureToggleAccessorBuilder<TArg, TResult> : FeatureToggleAccessorBuilder<Func<TArg, TResult>>
    {
        public FeatureToggleAccessorBuilder(IFeatureToggleClient splitClient, string featureName)
            : base(splitClient, featureName) { }
        public FeatureToggleAccessorBuilder<TArg, TResult> AddBehavior(
            string treatment,
            Func<TArg, TResult> behavior)
            => (FeatureToggleAccessorBuilder<TArg, TResult>)AddBehavior(treatment, () => behavior);
    }

    /// <summary>
    /// Helper class to define accessors
    /// </summary>
    public class FeatureToggleAccessorBuilder<TArg1, TArg2, TResult> : FeatureToggleAccessorBuilder<Func<TArg1, TArg2, TResult>>
    {
        public FeatureToggleAccessorBuilder(IFeatureToggleClient splitClient, string featureName)
            : base(splitClient, featureName) { }
        public FeatureToggleAccessorBuilder<TArg1, TArg2, TResult> AddBehavior(
            string treatment,
            Func<TArg1, TArg2, TResult> behavior)
            => (FeatureToggleAccessorBuilder<TArg1, TArg2, TResult>)AddBehavior(treatment, () => behavior);
    }

    /// <summary>
    /// Helper class to define accessors
    /// </summary>
    public class FeatureToggleAccessorBuilder<TArg1, TArg2, TArg3, TResult> : FeatureToggleAccessorBuilder<Func<TArg1, TArg2, TArg3, TResult>>
    {
        public FeatureToggleAccessorBuilder(IFeatureToggleClient splitClient, string featureName)
            : base(splitClient, featureName) { }
        public FeatureToggleAccessorBuilder<TArg1, TArg2, TArg3, TResult> AddBehavior(
            string treatment,
            Func<TArg1, TArg2, TArg3, TResult> behavior)
            => (FeatureToggleAccessorBuilder<TArg1, TArg2, TArg3, TResult>)AddBehavior(treatment, () => behavior);
    }
}

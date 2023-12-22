# doppler-feature-toggle

[![Conventional Commits](https://img.shields.io/badge/Conventional%20Commits-1.0.0-yellow.svg)](https://conventionalcommits.org)
[![Renovate enabled](https://img.shields.io/badge/renovate-enabled-brightgreen.svg)](https://renovatebot.com/)
[![FromDoppler](https://github.com/FromDoppler/doppler-feature-toggle/actions/workflows/fromdoppler.yml/badge.svg)](https://github.com/FromDoppler/doppler-feature-toggle/actions/workflows/fromdoppler.yml)
[![codecov](https://codecov.io/gh/FromDoppler/doppler-feature-toggle/graph/badge.svg?token=ZpCHpYSkgq)](https://codecov.io/gh/FromDoppler/doppler-feature-toggle)

Simple Feature Toggle client for .NET.

## NuGet feed

[`NuGet.config` file example:](https://github.com/FromDoppler/.github/blob/main/dotnet-examples/nuget.config)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!--To inherit the global NuGet package sources remove the <clear/> line below -->
    <clear />
    <add key="nuget" value="https://api.nuget.org/v3/index.json" />
    <add key="fromdoppler-github-packages" value="https://nuget.pkg.github.com/FromDoppler/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <fromdoppler-github-packages>
      <add key="Username" value="readonly" />
      <add key="ClearTextPassword" value="%FROMDOPPLER_GITHUB_PACKAGES_TOKEN%" />
    </github-fromdoppler>
  </fromdoppler-github-packages>
</configuration>
```

## Usage

First, it is necessary to setup the _FeatureToggleClient_ and configure it to be updated periodically for example:

```csharp
var client = new HttpFeatureToggleClient(
    "https://raw.githubusercontent.com/MakingSense/doppler-feature-toggle/resources/example1.json");

client.UpdatePeriodically(
    dueTime: TimeSpan.FromSeconds(10),
    period: TimeSpan.FromMinutes(1));
```

Then, we need to configure how to access to the features using the accessors. There are two ways to do it:

Using classes:

```csharp
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

// In composition root:

var booleanAccessor = new BooleanFeatureAccessorDemo(client);
var forcedBooleanAccessor = new ForcedBooleanFeatureAccessorDemo(client, "Enabled");
var dateFormatAccessor = new DateBehaviorAccessorDemo(client);

DependencyInjectionContainer.RegisterSingleton(booleanAccessor);
DependencyInjectionContainer.RegisterSingleton(dateFormatAccessor);
```

Using builders:

```csharp
// In composition root:

var booleanAccessor = client
    .CreateFeature<bool>("BooleanFeature")
    .AddBehavior("Disabled", false)
    .AddBehavior("Enabled", true)
    .SetDefaultTreatment("Disabled")
    .ForceTreatmentIfNotNull(null)
    .Build();

var dateFormatAccessor = client
    .CreateFeature<DateTime, string>("DateBehavior")
    .AddBehavior("ISO", x => x.ToString("yyyy-MM-dd"))
    .AddBehavior("English", x => x.ToString("MM/dd/yyyy"))
    .AddBehavior("Spanish", x => x.ToString("dd/MM/yyyy"))
    .SetDefaultTreatment("ISO")
    .Build();

DependencyInjectionContainer.RegisterSingleton(booleanAccessor);
DependencyInjectionContainer.RegisterSingleton(dateFormatAccessor);
```

Take into account that these examples are only to simplify, in general is not a good idea to resolve booleans.

Finally, we could use the accessors to have different behavior based on the _differentiator_:

```csharp
class AnyService
{
    private readonly _dateFormatAccessor;

    public AnyService(FeatureToggleAccessor<Func<DateTime, String>> dateFormatAccessor)
    {
        _dateFormatAccessor = dateFormatAccessor;
    }

    public string GetNameWithDate(User user)
    {
        var differentiator = user.PreferredDateFormat;
        var date = DateTime.Now;
        var formattedDate = dateFormatAccessor.Get(differentiator, date));
        return $"{user.Name} {formattedDate}";
    }
}
```

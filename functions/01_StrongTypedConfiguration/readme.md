# Strongly Typed Configuration in Azure Functions

When you´re dealing with configurations in Azure Functions, you normally use the AppSettings. In a local development environment this is better known as local.settings.json which looks like the following as an example:

```JavaScript
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "ConfigSetting1": "Config1Value",
        "ConfigSetting2": "Config2Value",
        "ConfigSetting3": "Config3Value"
    }
}
```

You can use the configuration settings inside of your code like the following:

```CSharp
    [FunctionName("SampleHttpTrigger")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {

        string configValue1 = Environment.GetEnvironmentVariable("ConfigSetting1");
        string configValue2 = Environment.GetEnvironmentVariable("ConfigSetting2");    
        
        return new OkObjectResult(responseMessage);
    }
```

All your AppSettings are available as an Environment Variable. It is absolutely fine to do it like this way, but everytime you need a setting from configuration you have to repeat yourself by writing

```CSharp
    Environment.GetEnvironmentVariable("{YourConfigSetting}")
```
While business is changing and underlying software too, it can become neccessary to rename your ConfigSettings. In that particular case you have to change all your functions to use the new naming. To avoid this, you can write your own wrapper for your AppSettings but in Azure Functions this is not neccessary because you can bind your AppSettings to your own Configuration Type. Let´s have a look how to do this.

## Bind AppSettings to your own Configuration Type

First we will modify our AppSettings and add a prefix to our ConfigSettings section as shown below:

```JavaScript
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "MyConfigSectionPrefix:ConfigSetting1": "Config1Value",
        "MyConfigSectionPrefix:ConfigSetting2": "Config2Value",
        "MyConfigSectionPrefix:ConfigSetting3": "Config3Value"
    }
}
```

Next we will create our configuration type by adding a new class (_MyConfigurationType.cs_) to our functions project:

```CSharp
namespace Sample.Models;

public class MyConfigurationType
{
    public string ConfigSetting1 { get; set; }
    public string ConfigSetting2 { get; set; }
    public string ConfigSetting3 { get; set; }
}

```

Add each ConfigSetting from AppSettings as a Property to your configuration type. Name must be identically to name in AppSettings. Do not use the prefix here.

Now let´s add a Startup Class (_Startup.cs_) to our project which will do the binding and inject the Configuration Type into our functions.

---
**IMPORTANT**

Please add the following packages to your project to keep compiler errors away from you ;-)

- Microsoft.Azure.Functions.Extensions
- Microsoft.Extensions.DependencyInjection

---

```CSharp
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Models;

[assembly: FunctionsStartup(typeof(Sample.Startup))]
namespace Sample;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddOptions<MyConfigurationType>()
            .Configure<IConfiguration>((settings, configuration) => 
            {
                configuration.GetSection("MyConfigSectionPrefix").Bind(settings);
            });
    }
}
```

In _Startup.cs_ we are changing the Startup of our functions. We override the Configure-Method and bind our Configuration Type to our prefixed config section inside of our AppSettings.  Now it can be injected to our functions.

For this we need to update our functions to the following:

```CSharp
    public class SampleHttpTrigger
    {
        public MyConfigurationType _typedConfigSettings;

        public SampleHttpTrigger(IOptions<MyConfigurationType> configSettings)
        {
            _typedConfigSettings = configSettings.Value;
        }

        [FunctionName("SampleHttpTrigger")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            string configValue1 = _typedConfigSettings.ConfigSetting1;
            string configValue2 = _typedConfigSettings.ConfigSetting2;    
            string configValue3 = _typedConfigSettings.ConfigSetting3;  

            return new OkObjectResult($"ConfigValue1 = {configValue1}, ConfigValue2 = {configValue2}, ConfigValue3 = {configValue3}");
        }
    }
```

Now you have a strongly typed configuration approach, which can be easily reused in all your functions by just injecting it.


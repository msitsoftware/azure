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
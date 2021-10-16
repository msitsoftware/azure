using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Sample.Models;

namespace Sample
{
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
}

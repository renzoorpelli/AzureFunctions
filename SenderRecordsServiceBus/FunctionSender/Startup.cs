using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


[assembly: FunctionsStartup(typeof(FunctionSender.Startup))]

namespace FunctionSender
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
           

            builder.Services.AddScoped<ILogicBus, LogicBus>();

            //builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
        }
    }

}

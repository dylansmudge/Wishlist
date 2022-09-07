
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Azure;
using Azure.Data.Tables;

[assembly: FunctionsStartup(typeof(TableStorage.Startup))]

namespace TableStorage 
{
    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder) 
        {
            // Database ids
            builder.Services.AddOptions<WishListOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("WishListOptions").Bind(settings);
                }); 

        }

    }

}
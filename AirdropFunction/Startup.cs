using Algorand.V2.Algod;
using Algorand.V2.Indexer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading;
using Utils.Algod;
using Utils.Cosmos;
using Utils.Indexer;
using Utils.KeyManagers;

[assembly: FunctionsStartup(typeof(AirdropFunction.Startup))]

namespace AirdropFunction
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: false, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false);

            var settings = builder.ConfigurationBuilder.Build();

            string azureKeyVaultEndpoint = settings.GetValue<string>("Endpoints:AzureKeyVault");
            builder.ConfigurationBuilder
                .AddAzureKeyVault(azureKeyVaultEndpoint);
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            IServiceCollection services = builder.Services;

            services.AddHttpClient();

            services.AddHttpClient<IDefaultApi, DefaultApi>(client =>
            {
                client.BaseAddress = new Uri(context.Configuration["Endpoints:Algod"]);
                client.DefaultRequestHeaders.Add("X-Algo-API-Token", context.Configuration["AlgodToken"]);
                client.Timeout = Timeout.InfiniteTimeSpan;
            });

            services.AddHttpClient<ILookupApi, LookupApi>(client =>
            {
                client.BaseAddress = new Uri(context.Configuration["Endpoints:Indexer"]);
                client.DefaultRequestHeaders.Add("X-Algo-API-Token", context.Configuration["IndexerToken"]);
                client.Timeout = Timeout.InfiniteTimeSpan;
            });

            services.AddHttpClient<ISearchApi, SearchApi>(client =>
            {
                client.BaseAddress = new Uri(context.Configuration["Endpoints:Indexer"]);
                client.DefaultRequestHeaders.Add("X-Algo-API-Token", context.Configuration["IndexerToken"]);
                client.Timeout = Timeout.InfiniteTimeSpan;
            });

            services.AddTransient<IAlgodUtils, AlgodUtils>();
            services.AddTransient<IIndexerUtils, IndexerUtils>();
            services.AddTransient<ICosmos, Cosmos>();
            services.AddTransient<IKeyManager, KeyManager>();
        }
    }
}

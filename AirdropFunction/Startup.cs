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
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false);

            var settings = builder.ConfigurationBuilder.Build();

            string azureKeyVaultEndpoint = settings.GetValue<string>("endpoints:azureKeyVault");

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
                client.BaseAddress = new Uri(context.Configuration["endpoints:algod"]);
                client.DefaultRequestHeaders.Add("X-Algo-API-Token", context.Configuration["node2ApiToken"]);
                client.Timeout = Timeout.InfiniteTimeSpan;
            });

            services.AddHttpClient<ILookupApi, LookupApi>(client =>
            {
                client.BaseAddress = new Uri(context.Configuration["endpoints:indexer"]);
                client.DefaultRequestHeaders.Add("X-Algo-API-Token", context.Configuration["indexerToken"]);
                client.Timeout = Timeout.InfiniteTimeSpan;
            });

            services.AddHttpClient<ISearchApi, SearchApi>(client =>
            {
                client.BaseAddress = new Uri(context.Configuration["endpoints:indexer"]);
                client.DefaultRequestHeaders.Add("X-Algo-API-Token", context.Configuration["indexerToken"]);
                client.Timeout = Timeout.InfiniteTimeSpan;
            });

            services.AddTransient<IAlgodUtils, AlgodUtils>();
            services.AddTransient<IIndexerUtils, IndexerUtils>();
            services.AddTransient<ICosmos, Cosmos>();
            services.AddTransient<IKeyManager, KeyManager>();
        }
    }
}

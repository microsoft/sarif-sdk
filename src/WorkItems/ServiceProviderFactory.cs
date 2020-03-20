using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WorkItems.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.WorkItems
{
    public static class ServiceProviderFactory
    {
        internal const string AppSettingsEnvironmentVariableName = "SarifWorkItemAppSettingsFile";
        private const string LoggingSection = "Sarif-WorkItems:Logging";
        private const string LoggingApplicationInsightsSection = "Sarif-WorkItems:Logging:ApplicationInsights";
        private const string LoggingApplicationInsightsInstrumentationKey = "Sarif-WorkItems:Logging:ApplicationInsights:InstrumentationKey";
        private const string LoggingConsoleSection = "Sarif-WorkItems:Logging:Console";

        public static IServiceProvider ServiceProvider { get; }

        static ServiceProviderFactory()
        {
            IServiceCollection services = new ServiceCollection();

            IConfiguration config = GetConfiguration();
            services.AddSingleton(typeof(IConfiguration), config);

            ITelemetryChannel channel = new InMemoryChannel();
            services.AddSingleton(typeof(ITelemetryChannel), channel);

            services.AddLogging(builder =>
            {
                if (config.GetSection(LoggingConsoleSection).Exists())
                {
                    builder.AddConsole();
                }

                if (config.GetSection(LoggingApplicationInsightsSection).Exists())
                {
                    if (!string.IsNullOrEmpty(config[LoggingApplicationInsightsInstrumentationKey]))
                    {
                        builder.AddApplicationInsights(config[LoggingApplicationInsightsInstrumentationKey], options => options.FlushOnDispose = true);
                    }
                }

                builder.AddConfiguration(config.GetSection(LoggingSection));
            });

            services.Configure<TelemetryConfiguration>((o) =>
            {
                o.TelemetryChannel = channel;
                o.TelemetryInitializers.Add(new ApplicationInsightsTelemetryInitializer());
                o.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            });

            ServiceProvider = services.BuildServiceProvider();
        }

        internal static string GetAppSettingsFilePath()
        {
            string appSettingsFile = Environment.GetEnvironmentVariable(AppSettingsEnvironmentVariableName);

            if (!string.IsNullOrEmpty(appSettingsFile))
            {
                return appSettingsFile;
            }

            return "appSettings.json";
        }

        internal static IConfiguration GetConfiguration()
        {
            string appSettingsFile = GetAppSettingsFilePath();
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(appSettingsFile, optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
                .Build();

            return config;
        }
    }
}

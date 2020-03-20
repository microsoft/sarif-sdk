using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.CodeAnalysis.WorkItems.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.CodeAnalysis.WorkItems
{
    public static class ServiceProviderFactory
    {
        private const string AppSettingsEnvironmentVariableName = "SarifWorkItemAppSettingsFile";
        private const string LoggingSection = "Sarif-WorkItems:Logging";
        private const string LoggingApplicationInsightsSection = "Sarif-WorkItems:Logging:ApplicationInsights";
        private const string LoggingApplicationInsightsInstrumentationKey = "Sarif-WorkItems:Logging:ApplicationInsights:InstrumentationKey";
        private const string LoggingConsoleSection = "Sarif-WorkItems:Logging:Console";

        public static IServiceProvider ServiceProvider { get; }
        private static ITelemetryChannel Channel { get; }

        static ServiceProviderFactory()
        {
            IConfiguration config = GetConfiguration();

            IServiceCollection services = new ServiceCollection();

            Channel = new InMemoryChannel();

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
                o.TelemetryChannel = Channel;
                o.TelemetryInitializers.Add(new ApplicationInsightsTelemetryInitializer());
                o.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            });

            ServiceProvider = services.BuildServiceProvider();
        }

        private static string GetAppSettingsFilePath()
        {
            string appSettingsFile = Environment.GetEnvironmentVariable(AppSettingsEnvironmentVariableName);

            if (!string.IsNullOrEmpty(appSettingsFile))
            {
                return appSettingsFile;
            }

            return "appSettings.json";
        }

        private static IConfiguration GetConfiguration()
        {
            string appSettingsFile = GetAppSettingsFilePath();
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(appSettingsFile, optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
                .Build();

            return config;
        }

        public static void Flush()
        {
            if (Channel != null)
            {
                Channel.Flush();
            }
        }
    }
}

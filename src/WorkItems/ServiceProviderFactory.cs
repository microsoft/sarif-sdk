using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
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
        private const string LoggingSection = "Sarif-SDK:Logging";
        private const string LoggingApplicationInsightsSection = "Sarif-SDK:Logging:ApplicationInsights";
        private const string LoggingApplicationInsightsInstrumentationKey = "Sarif-SDK:Logging:ApplicationInsights:InstrumentationKey";
        private const string LoggingConsoleSection = "Sarif-SDK:Logging:Console";

        public static IServiceProvider ServiceProvider { get; }
        private static ITelemetryChannel Channel { get; }

        static ServiceProviderFactory()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

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

        public static void Flush()
        {
            if (Channel != null)
            {
                Channel.Flush();
            }
        }
    }
}

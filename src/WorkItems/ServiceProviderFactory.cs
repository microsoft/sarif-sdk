// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.WorkItems.Logging;

namespace Microsoft.WorkItems
{
    public static class ServiceProviderFactory
    {
        internal const string AppSettingsEnvironmentVariableName = "SarifWorkItemAppSettingsFile";
        private const string LoggingSection = "Sarif-WorkItems:Logging";
        private const string LoggingApplicationInsightsSection = "Sarif-WorkItems:Logging:ApplicationInsights";
        private const string LoggingApplicationInsightsInstrumentationKey = "Sarif-WorkItems:Logging:ApplicationInsights:InstrumentationKey";
        private const string LoggingConsoleSection = "Sarif-WorkItems:Logging:Console";

        public static IServiceProvider ServiceProvider { get; private set; }

        static ServiceProviderFactory()
        {
            Initialize(customLogger: null);
        }

        public static void Initialize(ILogger customLogger)
        {
            IServiceCollection services = new ServiceCollection();

            IConfiguration config = GetConfiguration();
            services.AddSingleton(typeof(IConfiguration), config);

            ITelemetryChannel channel = new InMemoryChannel();
            services.AddSingleton(typeof(ITelemetryChannel), channel);

            if (customLogger == null)
            {
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
            }
            else
            {
                services.AddSingleton(typeof(ILogger), customLogger);
            }

            services.Configure<TelemetryConfiguration>((o) =>
            {
                o.TelemetryChannel = channel;
                o.TelemetryInitializers.Add(new ApplicationInsightsTelemetryInitializer());
                o.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            });

            ServiceProvider = services.BuildServiceProvider();

            // The logging in the application uses an ILogger, not a ILogger<T>. So if the ILogger is not
            // present, re-register the service.
            ILogger baseLogger = ServiceProviderFactory.ServiceProvider.GetService<ILogger>();
            if (baseLogger == null)
            {
                ILogger categoryLogger = ServiceProviderFactory.ServiceProvider.GetService<ILogger<ILogger>>();
                services.AddSingleton(typeof(ILogger), categoryLogger);
                ServiceProvider = services.BuildServiceProvider();
            }
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
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
                .Build();

            return config;
        }
    }
}

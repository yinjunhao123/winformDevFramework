using Autofac;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;

namespace WinformDevFramework.Core.Logging
{
    public static class LoggingConfig
    {
        public static ILoggerFactory CreateLoggerFactory()
        {
            var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(
                    path: Path.Combine(logDirectory, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10485760, // 10MB
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                );

            Log.Logger = loggerConfig.CreateLogger();

            return LoggerFactory.Create(builder =>
            {
                builder.AddSerilog();
                builder.AddDebug();
            });
        }

        public static void RegisterLogger(ContainerBuilder builder)
        {
            var loggerFactory = CreateLoggerFactory();
            builder.RegisterInstance(loggerFactory).As<ILoggerFactory>().SingleInstance();

            builder.RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>))
                .InstancePerDependency();
        }
    }
}
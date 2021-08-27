namespace EventDrive.Utils.Helpers
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IO;

    public static class ConfigurationHelper
    {
        public static IConfigurationRoot BuildConfigurationRoot(string[] args)
        {
            var envArg = Array.IndexOf(args, "--environment");
            var envFromArgs = envArg >= 0 ? args[envArg + 1] : null;

            var environment = envFromArgs ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .Build();
        }
    }
}
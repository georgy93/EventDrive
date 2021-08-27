namespace EventDrive.Worker.Host
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Utils.Helpers;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            var configuration = ConfigurationHelper.BuildConfigurationRoot(args);

            InitLogger(configuration);
            Log.Information("Starting up");

            try
            {
                var host = CreateHost(args);

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application crashed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHost CreateHost(string[] args) => Host
            .CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel();
                //webBuilder.UseUrls("http://*:80", "https://*:443");
                // webBuilder.UseIISIntegration();
                webBuilder.UseStartup(Assembly.GetEntryAssembly().FullName);
            })
            .Build();

        private static void InitLogger(IConfiguration configuration) => Log.Logger = new LoggerConfiguration()
           .Enrich.FromLogContext()
           .Enrich.WithMachineName()
           .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
           .WriteTo.Console()
           .ReadFrom.Configuration(configuration)
           .CreateLogger();

        private static readonly EventHandler<UnobservedTaskExceptionEventArgs> OnUnobservedTaskException = (obj, eventArgs) =>
        {
            Log.Error(eventArgs.Exception, $"UnobservedTaskException caught");

            foreach (var innerException in eventArgs.Exception.InnerExceptions)
            {
                Log.Error(innerException, "UnobservedTaskException inner exception");
            }
        };
    }
}
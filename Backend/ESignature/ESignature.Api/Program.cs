using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace ESignature.Api
{
    public class Program
    {
        public static readonly string Namespace = typeof(Program).Namespace;
        public static readonly string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);

        public static void Main(string[] args)
        {
            try
            {
                var configuration = GetConfiguration();
                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

                var host = CreateHostBuilder(args);
                Log.Information("Starting Web ({ApplicationContext})...", AppName);
                host.Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            SetAsposeLicense();
            var configuration = GetConfiguration();
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                //webBuilder.UseKestrel(opt => opt.AddServerHeader = false);
                webBuilder.UseIISIntegration();
                webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                webBuilder.UseStartup<Startup>();
                webBuilder.UseConfiguration(configuration);
                webBuilder.UseSerilog();
            });
        }

        private static IConfiguration GetConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            return configuration;
        }

        private static void SetAsposeLicense()
        {
            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
            using var reader1 = embeddedProvider.GetFileInfo("License.txt").CreateReadStream();
            {
                Aspose.Cells.License cellsLicense = new Aspose.Cells.License();
                cellsLicense.SetLicense(reader1);
            }
            using var reader2 = embeddedProvider.GetFileInfo("License.txt").CreateReadStream();
            {
                Aspose.Words.License docLicense = new Aspose.Words.License();
                docLicense.SetLicense(reader2);
            }
            using var reader3 = embeddedProvider.GetFileInfo("License.txt").CreateReadStream();
            {
                Aspose.Slides.License slidesLicense = new Aspose.Slides.License();
                slidesLicense.SetLicense(reader3);
            }
        }
    }
}
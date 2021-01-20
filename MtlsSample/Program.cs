using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;

namespace MtlsSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder
                    .ConfigureKestrel(cfg =>
                    {
                        cfg.ConfigureHttpsDefaults(https =>
                        {
                            https.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                            https.AllowAnyClientCertificate();
                        });
                    })
                    .UseStartup<Startup>(); })
                ;
    }
}
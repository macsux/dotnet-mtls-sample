using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MtlsSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // tell asp.net pipeline that SSL cert is available as part of HTTP header
            services.AddCertificateForwarding(opt => opt.CertificateHeader = "X-Forwarded-Client-Cert");
            // ensure the app properly recognizes that it was called with HTTPS even though termination happened at load balancer
            services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
            // set certificate as authentication provider
            services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(opt => opt.AllowedCertificateTypes = CertificateTypes.All); // you probably don't wanna trust self signed certs when going to prod, so change this
            // create an authorization policy called "secure"
            services.AddAuthorization(cfg =>
                cfg.AddPolicy("secure", policy => 
                    policy // the rules for authorization - this is what we're asserting on
                        .AddAuthenticationSchemes(CertificateAuthenticationDefaults.AuthenticationScheme) // must be logged in via certificate auth scheme
                        .RequireAuthenticatedUser()  // not anonymous
                        .RequireClaim(ClaimTypes.X500DistinguishedName, "CN=trustedsource.internal") // assert on claim in Principal.Claims collection
                    ));
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseForwardedHeaders();
            app.UseCertificateForwarding();
            
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>  endpoints
                .MapControllers()
                .RequireAuthorization("secure")); // force every controller to be secured by "secure" authz policy
        }
    }
}
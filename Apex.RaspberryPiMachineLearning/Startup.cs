using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace Apex.RaspberryPiMachineLearning
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public static IConfiguration ConfigurationRoot { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RaspberryPiSettings>(Configuration.GetSection("RaspberryPiSettings"));
            services.Configure<CognitiveServicesSettings>(Configuration.GetSection("CognitiveServicesSettings"));

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseFileServer();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

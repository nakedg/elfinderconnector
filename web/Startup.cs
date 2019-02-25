using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElFinder.Connector.Volume;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var rootPathElfinder = System.IO.Path.Combine(Environment.WebRootPath, "files");
            var tmbPath = System.IO.Path.Combine(Environment.WebRootPath, "tmb");

            services
                .AddElFinderDefaultFactory(options => {
                    options.Volumes = new Volume[] {
                         new Volume(new FsVolumeDriver(rootPathElfinder))
                         {
                             ThumbnailPath = tmbPath
                         }
                    };

                    options.TmbPath = tmbPath;
                })
                .AddElFinderConnector(options => {
                    options.ConnectorUrl = "/connector";
                });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(p =>
                {
                    p.WithOrigins("http://localhost:8080").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    //p.AllowAnyHeader().AllowAnyMethod().WithOrigins("localhost:8080");
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseCors();

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseElFinderConnector();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

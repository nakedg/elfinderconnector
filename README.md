# elfinderconnector
elfinder connector for .Net core

Implement connetor for elfinder 2.1.

Use example
  In configure services:
        
        public void ConfigureServices(IServiceCollection services)
        {
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
                })
                .AddElFinderConnector(options => {
                    options.ConnectorUrl = "/connector";
                });
           
        }
  
  In Configure
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // standart code

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
 
	
Partially implement driver for local file system
	
It is planned to implement a driver for the yandex disk

For image resize used library https://github.com/SixLabors/ImageSharp
 
 Реализован еще не весь api.
 Реализованные команды:
 1. open
 2. mkdir
 3. file
 4. ls
 5. parents
 6. tree
 7. upload
 8. rename
 9. rm
 10. size
 11. tmb

# elfinderconnector
elfinder connector for .Net core

Implement connetor for elfinder 2.1.

Use example
Config file or user secrets example

	{
	  "yaDiskConfig": {
	    "url": "https://webdav.yandex.ru",
	    "login": "login",
	    "password": "password",
	    "rootPath":  "/yandexFiles"
	  }
	}


  In configure services:
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			
	    var yaDiskUrl = Configuration.GetValue<string>("yaDiskConfig:url");
            var yaDiskLogin = Configuration.GetValue<string>("yaDiskConfig:login");
            var yaDiskPassword = Configuration.GetValue<string>("yaDiskConfig:password");
            var yaDiskRootPath = Configuration.GetValue<string>("yaDiskConfig:rootPath");
			
	    var rootPathElfinder = System.IO.Path.Combine(Environment.WebRootPath, "files");
	    var tmbPath = System.IO.Path.Combine(Environment.WebRootPath, "tmb");
			
            services
                .AddElFinderDefaultFactory(options => {
                    options.Volumes = new Volume[] {
                         new Volume(new FsVolumeDriver(rootPathElfinder))
                         {
                             ThumbnailPath = tmbPath
                         },
                         new Volume(new YaDiskVolumeDriver(yaDiskUrl, yaDiskLogin, yaDiskPassword, yaDiskRootPath)) {
                             ThumbnailPath = tmbPath,
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
 
	
Partially implement driver for local file system and yandex disk driver
	

For image resize used library https://github.com/SixLabors/ImageSharp <br />
For webdav access used library https://github.com/skazantsev/WebDavClient
 
 Implemented commands:
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
 
 For using example project, need retore clients libraries with libman (https://devblogs.microsoft.com/aspnet/library-manager-client-side-content-manager-for-web-apps/)

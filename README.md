# elfinderconnector
elfinder connector for .Net core
Implement connetor for elfinder 2.1.
Use example
  In configure services:
  services.AddElFinderConnector(options => {
    options.ConnectorUrl = "/connector";
  });
  
  In Configure
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ...

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
        
 Реализован еще не весь api.
 Реализованные команды:
 1. open
 2. mkdir
 3. file
 4. ls
 5. parents
 6. tree
 7. upload
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_Playground.Authentication.Services;
using WebApplication_Playground.DepedencyInjection;
using WebApplication_Playground.Models.Configuration;

namespace WebApplication_Playground
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
            services.AddControllersWithViews();
            //services.AddMvc();
            services.AddControllers();

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // convert runtime properties to constants
            Console.WriteLine($"Startup.cs --> is null (should be false): {this.Configuration.GetSection("CustomConfig")["appName"] == null}");

            /*
            CustomConfiguration customCofig = new CustomConfiguration();
            this.Configuration.GetSection("CustomConfig").Bind(customCofig);
            */

            CustomConfiguration customCofig = this.Configuration.GetSection("CustomConfig").Get<CustomConfiguration>();

            Console.WriteLine($"CustomConfig: appName-{customCofig.appName} | locale- {customCofig.locale}");
            services.Configure<CustomConfiguration>(this.Configuration.GetSection("CustomConfig"));

            // dependency injections

            /*
               (default)Singleton: IoC container will create and share a single instance of a service throughout the application's lifetime.
               Transient: The IoC container will create a new instance of the specified service type every time you ask for it.
               Scoped: IoC container will create an instance of the specified service type once per request and will be shared in a single request.
             */

            services.Add(new ServiceDescriptor(typeof(CustomInjectionInterface), new CustomInjection())); // singleton
            // services.Add(new ServiceDescriptor(typeof(CustomInjectionInterface), typeof(CustomInjection), ServiceLifetime.Singleton));

            // service that is nested w/ another
            services.Add(new ServiceDescriptor(typeof(IWrappedCustomInjection), typeof(WrappedCustomInjection), ServiceLifetime.Singleton));
            Console.WriteLine("dependencies injected");

            //services.AddAuthentication("BasicAuthentication").AddScheme

            // authentication & authorization
            // configure basic authentication 
            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            // configure DI for application services
            services.AddScoped<IUserService, UserService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /*
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            */

            //app.UseMvc();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // app.UseAuthorization();

            // authentication
            // note: must be placed between "useRouting" & "useEndpoints"
            // global cors policy
            app.UseCors( (CorsPolicyBuilder x) =>
                x.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                /*
                endpoints.MapControllerRoute(
                        name: "Value",
                        pattern: "api/Values"
                    );

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");*/

                endpoints.MapControllers();

            });
        }
    }
}

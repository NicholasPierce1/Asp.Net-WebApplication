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
using WebApplication_Playground.Repository.Adapter;
using WebApplication_Playground.Repository.Entities;
using WebApplication_Playground.Repository.Repos;
using WebApplication_Playground.Repository.Shared;

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
            services.Configure<CustomConfiguration>(
                this.Configuration.GetSection("CustomConfig")
                );

            // optional: register action to mutate config object
            services.Configure<CustomConfiguration>(
                // optional action to mutate instance
                (CustomConfiguration customConfiguration) => {
                    customConfiguration.appName = customConfiguration.appName.ToUpper();
                    }
                );

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


            // DATABASE
            Dictionary<string, string> connectionBindings =
                this.Configuration.GetSection("ConnectionStringMappings").Get<Dictionary<string, string>>();

            foreach (string keyValue in connectionBindings.Keys.Select<string, string>(key => $"{key}-{connectionBindings[key]}"))
                Console.WriteLine(
                    $"{nameof(Startup)} --> key-value: ({keyValue}) " +
                    $"with its connection string ({this.Configuration.GetConnectionString(keyValue.Split("-")[1])})"
                );

            // add root service for acquiring connection strings for various connection types (ms sql, oracle, mongo)
            services.Add(
                new ServiceDescriptor(
                    typeof(ConnectionHelper),
                    new ConnectionHelper(
                        connectionBindings,
                        this.Configuration.GetConnectionString)
                    )
                );

            //services.Add(new ServiceDescriptor(typeof(SqlServerConnection), typeof(SqlServerConnection)));
            services.Add(
                new ServiceDescriptor(
                    typeof(SqlServerConnection),
                    (IServiceProvider serviceProvider) => new SqlServerConnection(serviceProvider.GetRequiredService<ConnectionHelper>()),
                    ServiceLifetime.Singleton
                    )
                );

            services.Add(new ServiceDescriptor(typeof(SqlEntityMapper), new SqlEntityMapper()));

            services.Add(
                new ServiceDescriptor(
                    typeof(StudentRepository),
                    (IServiceProvider serviceProvider) =>
                        new StudentRepository(
                            serviceProvider.GetRequiredService<SqlServerConnection>(),
                            serviceProvider.GetRequiredService<SqlEntityMapper>()
                        ),
                    ServiceLifetime.Singleton
                    )
                );

            services.Add(
                new ServiceDescriptor(
                    typeof(Adapter),
                    (IServiceProvider serviceProvider) => 
                        new Adapter(
                            serviceProvider.GetRequiredService<StudentRepository>()
                        ),
                    ServiceLifetime.Singleton
                    )
                );
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

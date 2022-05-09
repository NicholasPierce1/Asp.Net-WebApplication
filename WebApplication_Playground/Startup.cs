using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebApplication_Playground.Authentication.Services;
using WebApplication_Playground.DepedencyInjection;
using WebApplication_Playground.Filters;
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

            services.AddSingleton<IConfiguration>(this.Configuration);

            services.Add(new ServiceDescriptor(typeof(CustomInjectionInterface), new CustomInjection())); // singleton
            // services.Add(new ServiceDescriptor(typeof(CustomInjectionInterface), typeof(CustomInjection), ServiceLifetime.Singleton));

            // service that is nested w/ another
            services.Add(new ServiceDescriptor(typeof(IWrappedCustomInjection), typeof(WrappedCustomInjection), ServiceLifetime.Singleton));
            Console.WriteLine("dependencies injected");

            //services.AddAuthentication("BasicAuthentication").AddScheme

            // authentication & authorization
            // configure basic authentication 
            // default scheme
            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            
            // Bearer scheme (not-default)
            services.AddAuthentication()
                .AddJwtBearer(
                    options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = Configuration["Jwt:Issuer"],
                            ValidAudience = Configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Secret"]))
                        };
                    }
            );

            // policy that can be used on either BasicAuthentication or Bearer (all policies can since the token produces the same user/ClaimPrincipal)
            /*
             * CAN: have multiple service registrations of same type 
             *  - https://stackoverflow.com/questions/39174989/how-to-register-multiple-implementations-of-the-same-interface-in-asp-net-core
             *  - https://stackoverflow.com/questions/39174989/how-to-register-multiple-implementations-of-the-same-interface-in-asp-net-core
             *  
             *  .Net is smart enough to filter this registration by the appropriate IRequirement
             *  
             *  for generic/custom you receive the DI as IEnumerable<IAuthorizationHandler>, or whatever the type is.
             *  filter the one you need (tightly-coupled, but unfortunate limitation)
             *  Spring-Bean names are also tightly-coupled
             */

            /*
             * what if you want multiple policies?
             * You have to make a policy that calls the other policies.
             * remember, the handler gets an AuthorizationContext AND you can use DI to inject other sub-policy handlers.
             * just call those sub-handler, passing the context & requirement.
             * 
             * Get sub-policies' requirements: use the 'options' to 'getPolicy' to get the policies and then pass them into your requirement constructor
             *  easy :)
             */
            services.AddSingleton<IAuthorizationHandler,RolePolicyAuthorizationHandler>();
            services.AddAuthorization(
                    options =>
                    {
                        // Hacky: services.AddSingleton<AuthorizationOptions>(options);
                        
                        options.AddPolicy(
                            "EmployeeRoleRequirementPolicy",
                            policy => policy.Requirements.Add(new RoleRequirementPolicy("Employee"))
                        );
                    }
                );

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
                        typeof(DogRepository),
                        (serviceProvider) =>
                            new DogRepository(
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

            // ServiceFilter - IoC
            services.AddScoped<MyServiceFilter>();

            //Mediatr & CQRS
            // links to explanation and examples:
            //  - https://medium.com/dotnet-hub/use-mediatr-in-asp-net-or-asp-net-core-cqrs-and-mediator-in-dotnet-how-to-use-mediatr-cqrs-aspnetcore-5076e2f2880c
            //  - https://code-maze.com/cqrs-mediatr-in-aspnet-core/
            // No need to create IoC for handlers and requests : )
            // Mediatr's dependency injection extension handles that for us
            services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ADD: ILoggerFactory, DI will sastify the parameter
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            /*
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            */

            //app.UseMvc();

            // only need serilog.extensions.loggging.file (all others were accident)
            // path: C:\Test\Logs
            // Date (yyyyMMdd)
            // will create if not exist and add to if it does exist by default
            loggerFactory.AddFile("C:\\Test\\Logs\\webApplicationPlayground-{Date}.txt");

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

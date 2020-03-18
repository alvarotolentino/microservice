using System;
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microservice.Api.Health;
using HealthChecks.UI.Client;
using Api.Infraestructure.Formatter;
using Utf8Json.Resolvers;
using Api.Infraestructure.Filters;
using Microservice.Application;

namespace Microservice.Api
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
            services
            .AddCustomMVC(Configuration)
            .AddCustomOptions(Configuration)
            .AddCustomVersioning(Configuration)
            .AddSwagger(Configuration)
            .AddCustomHelathCheck(Configuration)
            .AddApplication();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // static files
            app.UseStaticFiles();

            // swagger
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "Microservice API v1");
                o.InjectStylesheet("/swagger-ui/custom.css");
            });

            app.UseCors("CorsPolicy");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });

        }

    }

    public static class CustomExtensionMethods
    {
        public static IServiceCollection AddCustomMVC(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
            })
            //Utf8JSON serializer
            .AddMvcOptions(options =>
            {
                options.OutputFormatters.Clear();
                options.OutputFormatters.Add(new Utf8JsonOutputFormatter(StandardResolver.Default));
                options.InputFormatters.Clear();
                options.InputFormatters.Add(new Utf8JsonInputFormatter());
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                builder => builder.SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            });
            return services;
        }

        public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiBehaviorOptions>(options =>
                    {
                        options.InvalidModelStateResponseFactory = context =>
                        {
                            var problemDetails = new ValidationProblemDetails(context.ModelState)
                            {
                                Instance = context.HttpContext.Request.Path,
                                Status = StatusCodes.Status400BadRequest,
                                Detail = "Please refer to the errors property for additional details."
                            };

                            return new BadRequestObjectResult(problemDetails)
                            {
                                ContentTypes = { "application/problem+json", "application/problem+xml" }
                            };
                        };
                    });
            return services;
        }
        
        public static IServiceCollection AddCustomVersioning(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApiVersioning(o =>
                        {
                            o.ReportApiVersions = true;
                            o.AssumeDefaultVersionWhenUnspecified = true;
                            o.DefaultApiVersion = new ApiVersion(1, 0);
                        });
            return services;
        }
        
        public static IServiceCollection AddCustomHelathCheck(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddCheck<SystemMemoryHealthCheck>("Memory");

            return services;
        }

        public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(o =>
                       {
                           o.SwaggerDoc("v1", new OpenApiInfo
                           {
                               Version = "v1",
                               Title = "Microservice API",
                               Description = "Clean Architecture for microservice"
                           });
                           var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                           var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                           o.IncludeXmlComments(xmlPath);

                       });
            return services;
        }
    }
}

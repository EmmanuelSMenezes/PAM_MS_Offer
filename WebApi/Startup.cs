using Application.Service;
using Domain.Model;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using NSwag;
using NSwag.Generation.Processors.Security;
using Serilog;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace MS_Offer
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

            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddControllers().AddFluentValidation();
            services.AddControllers();
            services.AddCors();
            services.AddLogging();

            var key = Encoding.ASCII.GetBytes(Configuration.GetSection("MSOfferSettings").GetSection("PrivateSecretKey").Value);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Add framework services.

            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "V1";
                    document.Info.Title = "PAM - Microservice Offer";
                    document.Info.Description = "API's Documentation of Microservice Offer of PAM Plataform";
                };
                config.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                });

                config.OperationProcessors.Add(
                    new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });

            services.AddControllers().AddJsonOptions(x =>
            {
                // serialize enums as strings in api responses (e.g. Role)
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                // ignore omitted parameters on models to enable optional params (e.g. User update)
                x.JsonSerializerOptions.IgnoreNullValues = true;
            });

            string logFilePath = Configuration.GetSection("LogSettings").GetSection("LogFilePath").Value;
            string logFileName = Configuration.GetSection("LogSettings").GetSection("LogFileName").Value;

            string connectionString = Configuration.GetSection("MSOfferSettings").GetSection("ConnectionString").Value;
            string privateSecretKey = Configuration.GetSection("MSOfferSettings").GetSection("PrivateSecretKey").Value;
            string tokenValidationMinutes = Configuration.GetSection("MSOfferSettings").GetSection("TokenValidationMinutes").Value;

            EmailSettings emailSettings = new EmailSettings()
            {
                PrimaryDomain = Configuration.GetSection("EmailSettings:PrimaryDomain").Value,
                PrimaryPort = Configuration.GetSection("EmailSettings:PrimaryPort").Value,
                UsernameEmail = Configuration.GetSection("EmailSettings:UsernameEmail").Value,
                UsernamePassword = Configuration.GetSection("EmailSettings:UsernamePassword").Value,
                FromEmail = Configuration.GetSection("EmailSettings:FromEmail").Value,
                ToEmail = Configuration.GetSection("EmailSettings:ToEmail").Value,
                CcEmail = Configuration.GetSection("EmailSettings:CcEmail").Value,
                EnableSsl = Configuration.GetSection("EmailSettings:EnableSsl").Value,
                UseDefaultCredentials = Configuration.GetSection("EmailSettings:UseDefaultCredentials").Value
            };

            services.AddSingleton((ILogger)new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.File(Path.Combine(logFilePath, logFileName), rollingInterval: RollingInterval.Day)
              .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
              .CreateLogger());

            services.AddScoped<IOfferRepository, OfferRepository>(
                provider => new OfferRepository(connectionString, provider.GetService<ILogger>()));

            services.AddScoped<IOfferService, OfferService>(
                provider => new OfferService(provider.GetService<IOfferRepository>(),
                provider.GetService<ILogger>(), privateSecretKey, tokenValidationMinutes));

            services.AddTransient<IValidator<Coordinates>, CoordinatesRequestValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOpenApi();
            // add the Swagger generator and the Swagger UI middlewares   
            app.UseSwaggerUi3();

            app.UseReDoc(options =>
            {
                options.RoutePrefix = "docs";
                options.DocumentTitle = "Microservice Offer - PAM";
            });

            app.UseCors(builder =>
                builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseMvc();


        }
    }
}
using ESignature.Api.BackgroundServices;
using ESignature.Api.Messages;
using ESignature.Core.BaseDtos;
using ESignature.Core.Helpers;
using ESignature.Core.Infrastructure;
using ESignature.Core.RestClient;
using ESignature.DAL;
using ESignature.DAL.Models;
using ESignature.ServiceLayer.Authentications;
using ESignature.ServiceLayer.ESignCloud;
using ESignature.ServiceLayer.Services.Commands;
using ESignature.ServiceLayer.Services.OnStartup;
using ESignature.ServiceLayer.Settings;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ESignature.Api
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
            services.AddMvc()
                    .ConfigureApiBehaviorOptions(options =>
                    {
                        options.InvalidModelStateResponseFactory = c =>
                        {
                            var errors = string.Join('\n', c.ModelState.Values.Where(v => v.Errors.Count > 0)
                              .SelectMany(v => v.Errors)
                              .Select(v => v.ErrorMessage));

                            var res = new ResponseDto<bool>();
                            res.Errors.Add(new ErrorDto
                            {
                                Code = 400,
                                Message = errors
                            });
                            res.Result = false;
                            return new BadRequestObjectResult(res);
                        };
                    })
                    .AddFluentValidation(s =>
                    {
                        s.RegisterValidatorsFromAssembly(AppDomain.CurrentDomain.Load("ESignature.ServiceLayer"));
                        s.DisableDataAnnotationsValidation = true;
                    });

            services.AddSwaggerGen(config =>
            {
                config.OperationFilter<IgnorePropertyHelper>();
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ESignature APIs",
                    Version = "v1",
                    Description = "ESignature"
                });
                config.AddSecurityDefinition("ES-Token", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "input your token: ES-Token {your token key}",
                    Name = "ES-Token",
                    Type = SecuritySchemeType.ApiKey
                });
                config.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                     new OpenApiSecurityScheme
                     {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ES-Token"
                        }
                     },
                     Array.Empty<string>()
                    }
                });
            });

            services.AddAuthentication(AuthenticationSchemaConstants.ValidateTokenSchema)
                    .AddScheme<ValidateTokenSchemaOptions, ValidateTokenSchemaOptionsHandler>(AuthenticationSchemaConstants.ValidateTokenSchema, option => { });

            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.User.RequireUniqueEmail = true;
            }).AddDefaultTokenProviders().AddEntityFrameworkStores<ESignatureContext>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            services.Configure<FormOptions>(o =>
            {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });

            services.AddHealthChecks();
            ConfigureApplicationServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHealthChecks("/api/healthcheck");

            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "ESignature APIs");
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            var localizationOptions = new RequestLocalizationOptions
            {
                SupportedCultures = new List<CultureInfo> { new CultureInfo("en-US") },
                SupportedUICultures = new List<CultureInfo> { new CultureInfo("en-US") },
                DefaultRequestCulture = new RequestCulture("en-US")
            };
            app.UseRequestLocalization(localizationOptions);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            await this.InitDatabase(app);
        }

        private async Task InitDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                await scope.ServiceProvider.GetService<ESignatureContext>().Database.MigrateAsync();
                await scope.ServiceProvider.GetService<IMediator>().Send(new SeedDataCommand());
            }
        }

        private void ConfigureApplicationServices(IServiceCollection services)
        {
            services.Configure<RsspCloudSetting>(Configuration.GetSection("RsspCloud"));
            services.Configure<ESignatureSetting>(Configuration.GetSection("ESignature"));

            var serviceAssembly = AppDomain.CurrentDomain.Load("ESignature.ServiceLayer");
            services.AddMediatR(serviceAssembly);
            services.AddAutoMapper(serviceAssembly);

            services.AddDbContext<ESignatureContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Database"), opt =>
                {
                    opt.MigrationsAssembly("ESignature.DAL");
                })
            );
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ApiSourceData>();
            services.AddSingleton<ServiceData>();
            services.AddSingleton<IMessagePublisher, MessagePublisher>();
            
            services.AddUnitOfWork<ESignatureContext>();
            services.AddTransient<IRestClient, RestClient>();
            services.AddTransient<ESignCloudFunction>();


            // add background jobs
            //services.AddHostedService<RabbitMQConsumerPendingService>();

            services.AddHostedService<PendingJob>();
            services.AddHostedService<InProgressJob>();
            services.AddHostedService<CallBackJob>();
            services.AddHostedService<HistoryJob>();
        }
    }
}
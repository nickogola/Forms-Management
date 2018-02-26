using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Forms.Infrastructure.Interfaces;
using Forms.Infrastructure;
using Forms.Filters;
using FluentValidation.AspNetCore;
using BLG.AspNetCore;

namespace Forms
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
            services.AddOptions();
            services.Configure<P8ServicesV1Options>(Configuration.GetSection("P8ServicesV1"));
            services.AddAuthentication(IISDefaults.AuthenticationScheme);
            services.AddMediatR(typeof(Startup));
            services.AddBLGExceptionHandling(Configuration.GetSection("BLGException"));
            services.AddP8Services(Configuration.GetSection("P8ServicesV1"));
            //services.AddResponseCaching();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.DefaultSecurityPolicy, policy => policy.Requirements.Add(new PermissionRequirement(Constants.DefaultSecurityPermission)));
            });

            //Singletons
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //Transients
            services.AddTransient<IUserInfo, UserInfo>();
            services.AddTransient<Forms.Features.Home.ISelectListItemBuilder, Forms.Features.Home.SelectListItemBuilder>();

            //Scoped
            services.AddScoped<IDbContext, DbContext>(x => new DbContext(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();

            services
                .AddMvc(opt =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

                    opt.Filters.Add(new AuthorizeFilter(policy));
                    opt.Filters.Add(typeof(ValidateModelActionFilter));
                })
                .AddFeatureFolders()
                .AddFluentValidation(cfg => { cfg.RegisterValidatorsFromAssemblyContaining<Startup>(); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();
            }
            else
            {
                loggerFactory.AddFile(Configuration.GetSection("Logging"));
            }

            app.UseAuthentication();
            app.UseBLGExceptionHandling();
            //app.UseMiddleware<ExceptionHandlerMiddleware>();
            //app.UseStatusCodePagesWithReExecute("/Error/Index/{0}");
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute("GetPDFRoute", "{controller=Home}/{action=GetPDF}/{id}/{fileName}");
                routes.MapRoute("Default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

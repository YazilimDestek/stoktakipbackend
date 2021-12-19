using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.IO;
using CinigazStokEntity;
using System;
using CinigazStokService.Security.Helpers;
using CinigazStokService.Handler;
using CinigazStokService.Helper;
using System.Security.Permissions;
using System.Security;
using CinigazStokService.services;
using Microsoft.OpenApi.Models;
using Swashbuckle.Swagger;
using Swashbuckle.AspNetCore.Swagger;
using CinigazStokEntity.Persistence.CinigazStokEntity.Entity.Persistence;

namespace CinigazStokService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();


            var UploadedImagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Files\\images");
            FileIOPermission f2 = new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, UploadedImagesPath);

            try
            {
                f2.Demand();
            }
            catch (SecurityException s)
            {
                Console.WriteLine(s.Message);
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(x => x.FullName);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api Docs", Version = "v1", Description = "Stok takip api" });
            });
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddControllers();
            services.AddDbContext<StokDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("Default")));
            // ----- AUTOMAPPER
            services.AddAutoMapper(new Type[] { typeof(Mapping.MappingProfile) });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddTransient<IImageHandler, ImageHandler>();
            services.AddTransient<ICatagoryValidation, CatagoryValidation>();
            services.AddTransient<IImageWriter, ImageWriter>();
            services.AddTransient<IpaginationService, paginationService>();


            // ----- CORS HEADERS, THOSE SETTINGS CANNOT SET AZURE SETTINGS
            services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("LocalHost",
                builder => builder.WithOrigins("http://localhost:4200"));
            });

            services.AddCors(options =>
            {
                options.AddPolicy("Domain",
                builder => builder.WithOrigins("http://192.168.2.70:5000"));
            });

            services.AddCors(options =>
            {
                options.AddPolicy("DomainWithoutWWW",
                builder => builder.WithOrigins("http://192.168.2.70"));
            });

            // ----- AUTHENTICATION
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = "entray.com",
                            ValidAudience = "entray.com",
                            IssuerSigningKey = JwtSecurityKey.Create("CINIGAZ2019-92223K-324957-K3596U")
                        };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, StokDbContext context)
        {
            StokDbInitializer.Initialize(context);

            app.UseCors(
                builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("swagger/v1/swagger.json", "API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "Files")),
                RequestPath = "/files"
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "Files/images")),
                RequestPath = "/files/images"
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}

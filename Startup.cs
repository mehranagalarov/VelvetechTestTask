using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TodoApp.Data;
using AutoMapper;
using Serilog;
using TodoApp.DAL.Configuration;
using Microsoft.OpenApi.Models;
using TodoApp.Mappings;
using TodoApp.Common.Middleware;
using TodoApp.Common.Logging;
using TodoApp.Services;
using TodoApp.Data.Todo.Repositories;

namespace TodoApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            appVersion = GetType().Assembly.GetName().Version.ToString();
        }

        public IConfiguration Configuration { get; }
        private readonly string appVersion;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TodoContext>(opt =>
               opt.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"], m => m.MigrationsAssembly("TodoApp")));
            services.AddControllers();
            services.Configure<AppSettings>(Configuration);
            services.AddAutoMapper(Assembly.GetAssembly(typeof(TodoProfile)));
            services.AddLogging(logging =>
            {
                var logger = new LoggerConfiguration()
                                  .ReadFrom.Configuration(Configuration)
                                  .Enrich.FromLogContext()
                                  .CreateLogger();

                logging.ClearProviders();
                logging.AddSerilog(logger);
            }
            );
            services.AddSwaggerGen(s=> s.SwaggerDoc("TodoAppSwagger", new OpenApiInfo { Title = "TodoApp", Version =$"v{appVersion}"}));

            services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
            services.AddScoped<ITodoService, TodoService>();
            services.AddScoped<ITodoRepository, TodoRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(s => { s.SwaggerEndpoint($"/swagger/{appVersion}/swagger.json", $"v{appVersion}"); });
            }

            app.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}

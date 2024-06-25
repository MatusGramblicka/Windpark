using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Net.Http;
using WindParkAPIAggregation.Contracts;
using WindParkAPIAggregation.Core;
using WindParkAPIAggregation.Extensions;
using WindParkAPIAggregation.HostedServices;
using WindParkAPIAggregation.Interface;
using WindParkAPIAggregation.Repository;

namespace WindParkAPIAggregation;

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
        //services.AddScoped<IWindParkApiAggregator, WindParkApiAggregator>();
        services.AddSingleton<IMessageProducer, RabbitMqProducer>();
        services.AddScoped<IDatabaseOperation, DatabaseOperation>();

        services.AddLogging();

        services.AddHostedService<WindParkDataGetterHost>();
        services.AddHostedService<WindParkDataAggregator>();

        services.AddHttpClient<IWindParkClient, WindParkClient>("WindParkAPI",
            (s, c) => { c.BaseAddress = new Uri(Configuration.GetValue<string>("WindParkApi:BaseAddress")); });

        services.AddSingleton(
            sp => sp.GetService<IHttpClientFactory>().CreateClient("WindParkAPI"));

        services.Configure<WindParkIntervalConfiguration>(Configuration.GetSection("WindParkInterval"));
        services.Configure<RabbitMqConfiguration>(Configuration.GetSection("RabbitMqSection"));

        services.AddControllers();
        services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "WindParkDto", Version = "v1"}); });

        services.AddScoped<IRepositoryManager, RepositoryManager>();

        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(Configuration.GetConnectionString("sqlConnection"),
                b => b.MigrationsAssembly("WindParkAPIAggregation")));

        //var windParkIntervalConfig = Configuration.GetSection("WindParkInterval");
        //services.ConfigureQuartz(Convert.ToInt32(windParkIntervalConfig["WindParkAggregationFrequencyMinutes"]));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WindParkDto v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Net.Http;
using WindparkAPIAggregation.Contracts;
using WindparkAPIAggregation.Core;
using WindparkAPIAggregation.Extensions;
using WindparkAPIAggregation.HostedServices;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation;

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
        services.AddSingleton<IWindparkApiAggregator, WindparkApiAggregator>();
        services.AddSingleton<IMessageProducer, RabbitMQProducer>();
        services.AddSingleton<WindParkAggregationPersistor>();

        services.AddLogging();

        services.AddHostedService<RunScheduler>();

        services.AddHttpClient<IWindparkClient, WindparkClient>("WindParkAPI", (s, c) =>
        {
            c.BaseAddress = new Uri(Configuration.GetValue<string>("WindparkApi:BaseAddress"));
        });

        services.AddSingleton(
            sp => sp.GetService<IHttpClientFactory>().CreateClient("WindParkAPI"));

        services.Configure<WindparkIntervalConfiguration>(Configuration.GetSection("WindparkInterval"));
        services.Configure<RabbitMqConfiguration>(Configuration.GetSection("RabbitMqSection"));

        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "WindPark", Version = "v1" });
        });

        var windparkIntervalConfig = Configuration.GetSection("WindparkInterval");
        services.ConfigureQuartz(Convert.ToInt32(windparkIntervalConfig["WindparkAggregationFrequencyMinutes"]));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WindPark v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
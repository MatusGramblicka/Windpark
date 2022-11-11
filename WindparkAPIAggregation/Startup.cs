using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using Quartz;
using WindparkAPIAggregation.Core;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation
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
            services.AddSingleton<IWindparkApiAggregator, WindparkApiAggregator>();
            services.AddSingleton<IMessageProducer, RabbitMQProducer>();

            services.AddHttpClient<IWindparkClient, WindparkClient>((s, c) =>
            {
                c.BaseAddress = new Uri(Configuration.GetValue<string>("WindparkApi:BaseAddress"));
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WindPark", Version = "v1" });
            });

            ConfigureQuartz(services);
        }

        private static void ConfigureQuartz(IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UseInMemoryStore();

                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 5;
                });

                

                var sendAggregatedDataToRabbitMqJobKey = new JobKey(nameof(WindparkApiAggregator));
                q.AddJob<WindparkApiAggregator>(opts => opts
                    .WithIdentity(sendAggregatedDataToRabbitMqJobKey)
                );
                q.AddTrigger(opts => opts
                    .ForJob(sendAggregatedDataToRabbitMqJobKey)
                    .WithIdentity($"{sendAggregatedDataToRabbitMqJobKey.Name}Trigger")
                    .StartNow()
                    .WithSimpleSchedule(a =>
                        a.WithIntervalInMinutes(5).RepeatForever()
                    )
                );

                var windparkClientGetDataJobKey = new JobKey(nameof(WindparkClient));
                q.AddJob<WindparkClient>(opts => opts
                    .WithIdentity(windparkClientGetDataJobKey)
                );
                q.AddTrigger(opts => opts
                    .ForJob(windparkClientGetDataJobKey)
                    .WithIdentity($"{windparkClientGetDataJobKey.Name}Trigger")
                    .StartNow()
                    .WithSimpleSchedule(a =>
                        a.WithIntervalInSeconds(10 + 2).RepeatForever()
                    )
                );
            });

            // background service that handles scheduler lifecycle
            services.AddQuartzHostedService(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });
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
}

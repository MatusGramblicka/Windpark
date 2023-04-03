using Microsoft.Extensions.DependencyInjection;
using Quartz;
using WindParkAPIAggregation.Core;

namespace WindParkAPIAggregation.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureQuartz(this IServiceCollection services, int windParkAggregationFrequencyMinutes)
    {
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
            q.UseInMemoryStore();

            q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 5; });

            var sendAggregatedDataToRabbitMqJobKey = new JobKey(nameof(WindParkApiAggregator));
            q.AddJob<WindParkApiAggregator>(opts => opts
                .WithIdentity(sendAggregatedDataToRabbitMqJobKey)
            );
            q.AddTrigger(opts => opts
                .ForJob(sendAggregatedDataToRabbitMqJobKey)
                .WithIdentity($"{sendAggregatedDataToRabbitMqJobKey.Name}Trigger")
                .StartNow()
                .WithSimpleSchedule(a =>
                    a.WithIntervalInMinutes(windParkAggregationFrequencyMinutes).RepeatForever()
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
}
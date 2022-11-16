using Microsoft.Extensions.DependencyInjection;
using Quartz;
using WindparkAPIAggregation.Core;

namespace WindparkAPIAggregation.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureQuartz(this IServiceCollection services, int windparkAggregationFrequencyMinutes)
    {
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
            q.UseInMemoryStore();

            q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 5; });

            var sendAggregatedDataToRabbitMqJobKey = new JobKey(nameof(WindparkApiAggregator));
            q.AddJob<WindparkApiAggregator>(opts => opts
                .WithIdentity(sendAggregatedDataToRabbitMqJobKey)
            );
            q.AddTrigger(opts => opts
                .ForJob(sendAggregatedDataToRabbitMqJobKey)
                .WithIdentity($"{sendAggregatedDataToRabbitMqJobKey.Name}Trigger")
                .StartNow()
                .WithSimpleSchedule(a =>
                    a.WithIntervalInMinutes(windparkAggregationFrequencyMinutes).RepeatForever()
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
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WindParkAPIAggregation.Core;

public static class ExecutionDecorator
{
    public static async Task ExecuteAction<T>(T message, Func<T, Task> handler, ILogger logger,
        SemaphoreSlim semaphore)
    {
        logger.LogInformation($"Handling {typeof(T).FullName}");

        try
        {
            await semaphore.WaitAsync();
            await handler(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "request handling error");
        }
        finally
        {
            semaphore.Release();
        }
    }
}
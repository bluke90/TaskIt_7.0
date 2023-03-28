using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskIt.Mechanics
{
    /// <summary>
    /// Service to perform any background task needed
    /// </summary>
    public class PeriodicUpdateService : BackgroundService
    {
        private readonly TimeSpan _period = TimeSpan.FromSeconds(15);    // Period between each update
        private readonly ILogger<PeriodicUpdateService> _logger;
        private readonly IServiceScopeFactory _factory;
        private int _executionCount = 0;    // Amount of times loop was executed
        public bool IsEnabled { get; set; }

        
        public PeriodicUpdateService(
            ILogger<PeriodicUpdateService> logger,
            IServiceScopeFactory factory)
        {
            _logger = logger;
            _factory = factory;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            Console.WriteLine("[PeriodicUpdateService] Executing...");
            using PeriodicTimer timer = new PeriodicTimer(_period);
           
            // Update Loop
            while (!stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(stoppingToken)) { 
            
                // Loop Content
                try {
                
                    if (IsEnabled) { // check if enabled
                        await using AsyncServiceScope asyncScope = _factory.CreateAsyncScope();
                        TaskUpdateService updateService = asyncScope.ServiceProvider.GetRequiredService<TaskUpdateService>();
                    
                        // *** Start Update Actions here ***
                        // 1. Update Next Occurances for Recurring Task
                        await updateService.UpdateNextOccurances();
                        // 2. Reschedule Recurring updates
                        await updateService.UpdateNotifications();

                        // *** End Update Actions ***
                        _executionCount++;
                        _logger.LogInformation($"Executed Periodic Updates - Count => {_executionCount}");
                    } else {
                        _logger.LogInformation("Skipped Periodic Updates");
                    }

                } catch (Exception ex) {
                    _logger.LogInformation($"Failed to execute PeriodicUpdateService with exception message {ex.Message}... ");
                    _logger.LogInformation($"Execution Count => {_executionCount}");
                }
            }
        }
    }

    record PeriodicUpdateServiceState(bool IsEnabled);
}

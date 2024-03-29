﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskIt.Mechanics
{
    // *!* Depreciated - Remove *!* 
    internal sealed class HostedServiceExecutor
    {
        private readonly IEnumerable<IHostedService> _services;
        private readonly ILogger<HostedServiceExecutor> _logger;

        public HostedServiceExecutor(ILogger<HostedServiceExecutor> logger, IEnumerable<IHostedService> services) {
            _logger = logger;
            _services = services;
        }

        public Task StartAsync(CancellationToken token) {
            return ExecuteAsync(service => service.StartAsync(token));
        }

        public Task StopAsync(CancellationToken token) {
            return ExecuteAsync(service => service.StopAsync(token), throwOnFirstFailure: false);
        }

        private async Task ExecuteAsync(Func<IHostedService, Task> callback, bool throwOnFirstFailure = true) {
            List<Exception>? exceptions = null;

            foreach (var service in _services) {
                try {
                    await callback(service);
                } catch (Exception ex) {
                    if (throwOnFirstFailure) {
                        throw;
                    }

                    if (exceptions == null) {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            // Throw an aggregate exception if there were any exceptions
            if (exceptions != null) {
                throw new AggregateException(exceptions);
            }
        }
    }
}

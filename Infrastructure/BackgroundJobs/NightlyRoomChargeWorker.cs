using ESMART.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Infrastructure.BackgroundJobs
{
    public class NightlyRoomChargeWorker(IServiceProvider services, ILogger<NightlyRoomChargeWorker> logger) : BackgroundService
    {
        private readonly IServiceProvider _services = services;
        private readonly ILogger<NightlyRoomChargeWorker> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = DateTime.Today.AddDays(1);
                var delay = nextRun - now;

                _logger.LogInformation($"Nightly charge worker sleeping for {delay.TotalMinutes} minutes until {nextRun}...");

                await Task.Delay(delay, stoppingToken);

                try
                {
                    using var scope = _services.CreateScope();
                    var nightlyService = scope.ServiceProvider.GetRequiredService<NightlyRoomChargeService>();
                    await nightlyService.PostNightlyRoomChargesAsync();

                    _logger.LogInformation("Nightly room charges posted.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to post nightly room charges.");
                }
            }
        }
    }

}

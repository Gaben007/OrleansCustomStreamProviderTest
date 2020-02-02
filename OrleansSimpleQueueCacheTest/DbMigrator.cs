using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansSimpleQueueCacheTest
{
    internal class DbMigrator : ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly TestDbContext _dbContext;
        private readonly ILogger<DbMigrator> _logger;

        public DbMigrator(ILogger<DbMigrator> logger, TestDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(
                nameof(DbMigrator),
                ServiceLifecycleStage.First,
                StartAsync);
        }

        private async Task StartAsync(CancellationToken arg)
        {
            _logger.LogDebug("DB migration started.");
            await _dbContext.Database.MigrateAsync();
            _logger.LogDebug("DB migration finished.");
        }
    }
}

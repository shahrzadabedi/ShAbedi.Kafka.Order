using Hangfire;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderJobs.Application;
using OrderJobs.Application.DTOs;
using ShAbedi.OrderJobs.Persistence;
using System.Data;
using System.Text.Json;

namespace OrderJobs.Jobs
{
    [DisableConcurrentExecution(3600)]
    public class OutboxProcessorJob
    {
        private AppDbContext dbContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private IUnitOfWork _unitOfWork;
        private ITopicProducer<string, OrderCreated> _topicProducer;

        public OutboxProcessorJob(AppDbContext db,
            IServiceScopeFactory scopeFactory,
            ITopicProducer<string, OrderCreated> topicProducer)
        {
            this.dbContext = db;
            this._scopeFactory = scopeFactory;
            this._topicProducer = topicProducer;
        }

        public async Task ProcessOrderCreatedOutbox(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            _topicProducer = scope.ServiceProvider.GetRequiredService<ITopicProducer<string, OrderCreated>>();

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            int batchSize = 20;

            var messages = dbContext.Outbox
                .FromSql($@"SELECT TOP ({batchSize}) Id, Type, Payload,IsProcessed, ProcessedOn, OccurredOn, RetryCount 
                    FROM Outbox WITH (ROWLOCK, UPDLOCK, READPAST)
                    WHERE IsProcessed = 0 
                    ORDER BY OccurredOn")
                            .ToList();

            foreach (var message in messages)
            {
                var eventDto = JsonSerializer.Deserialize<OrderCreated>(message.Payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                message.MarkAsProcessed();

                await _topicProducer.Produce(eventDto.OrderId.ToString()
                    ,eventDto
                    ,cancellationToken);
            }

            if (messages.Count > 0)
            {
                await _unitOfWork.CommitAsync(cancellationToken);
            }
        }
    }
}

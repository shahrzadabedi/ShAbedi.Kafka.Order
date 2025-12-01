using Hangfire;
using Hangfire.Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderJobs.Application;
using OrderJobs.Application.DTOs;
using OrderJobs.Jobs;
using ProfileJobs.Infrastructure.Persistence;
using Serilog;
using ShAbedi.OrderJobs.Persistence;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();
Serilog.Debugging.SelfLog.Enable(Console.Out);

builder.Services.AddControllers();

builder.Services.AddMassTransit(x =>
{
    const string kafkaBrokerServers = "localhost:19092,localhost:19093,localhost:19094";
        //"localhost:19092";
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    x.AddRider(rider =>
    {
        rider.AddProducer<string,OrderCreated>("order-created-topic");

        rider.UsingKafka((context, k) => { k.Host(kafkaBrokerServers); });

        //x.UsingRabbitMq((ctx, cfg) =>
        //{
        //    cfg.Message<OrderCreated>(x => x.SetEntityName("order-created-exchange"));

        //    cfg.Publish<OrderCreated>(x =>
        //    {
        //        x.ExchangeType = "topic";
        //        x.AutoDelete = false;
        //        x.Durable = true;
        //    });

        //    cfg.Host("localhost", "/", h =>
        //    {
        //        h.Username("guest");
        //        h.Password("guest");
        //    });
        //});
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options => { options.UseSqlServer(cs); });

var hangfireConnection = builder.Configuration.GetConnectionString("Hangfire");
builder.Services.AddHangfire(x => x.UseSqlServerStorage(hangfireConnection));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

var recurring = app.Services.GetRequiredService<IRecurringJobManager>();

recurring.AddOrUpdate(
    recurringJobId: "OutboxProcessorJob",
    job: Job.FromExpression<OutboxProcessorJob>(job =>
        job.ProcessOrderCreatedOutbox(CancellationToken.None)),
    cronExpression: "*/5 * * * *",
    options: new RecurringJobOptions());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

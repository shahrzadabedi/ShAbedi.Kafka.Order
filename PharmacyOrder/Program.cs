using MassTransit;
using OrderJobs.Application.DTOs;
using PharmacyOrder.Application.Consumers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();
Serilog.Debugging.SelfLog.Enable(Console.Out);
// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
    
    x.AddRider(rider =>
    {
        var kafkaBrokerServers = builder.Configuration["KafkaConfig:KafkaBrokerServers"];

        rider.AddConsumer<OrderCreatedConsumer>();
        
        rider.UsingKafka((context, k) =>
        {
            k.Host(kafkaBrokerServers);
            
            k.TopicEndpoint<string, OrderCreated>(
                "order-created-topic",
                  "order-pharmacy-group",
                e =>
                {
                    e.ConfigureConsumer<OrderCreatedConsumer>(context);
                });
        });
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

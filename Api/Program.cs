using Api.Database;
using Api.Database.Repositories;
using Api.Dtos;
using Api.Entities;
using Api.Integrations.MessageBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextPool<DatabaseContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
    opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});
builder.Services.AddScoped<SchedulingRepository>();
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddSingleton(_ => new ServiceBus(builder.Configuration.GetConnectionString("ServiceBus")));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

app.MapPost("/schedules", async ([FromBody] AddSchedulingRequest body, 
    [FromServices] ServiceBus messageBus, 
    [FromServices] SchedulingRepository repository, 
    [FromServices] UnitOfWork unitOfWork,
    CancellationToken cancellationToken) =>
{
    var response = await messageBus.ScheduleMessageAsync(new MessageBusScheduleRequest<string>
    {
        Payload = body.Description,
        QueueName = "schedule-destination",
        ScheduleTo = body.ScheduleTo,
    }, cancellationToken);

    var entity = new Scheduling
    {
        CreatedAt = DateTime.UtcNow,
        Description = body.Description,
        MessageNumber = response.MessageNumber,
    };

    await repository.AddAsync(entity, cancellationToken);
    await unitOfWork.CommitAsync(cancellationToken);

    return Results.StatusCode(201);
});

app.MapGet("/schedules", ([FromServices] SchedulingRepository repository) =>
{
    var schedulings = repository.GetAllAsync();
    return Results.Ok(schedulings);
});

app.MapPost("/schedules:cancel", async ([FromBody] CancelSchedulingRequest body, 
    [FromServices] ServiceBus messageBus,
    CancellationToken cancellationToken) =>
{
    await messageBus.CancelScheduleMessageAsync(new MessageBusCancelScheduledRequest
    {
        MessageNumber = body.MessageNumber,
        QueueName = "schedule-destination",
    }, cancellationToken);

    return Results.Ok();
});

await app.Services.GetService<ServiceBus>().SubscribeAsync(new MessageBusSubscribeRequest<string>
{
    QueueName = "schedule-destination",
    Handler = (string payload) =>
    {
        Console.WriteLine($"{DateTime.Now} - {payload}");
        return Task.FromResult(true);
    }
});

app.Run();

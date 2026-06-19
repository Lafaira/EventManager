using EventManager.Middleware;
using EventManager.Services;
using EventManager.Services.Interfaces;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EventManager API",
        Version = "v1",
        Description = "API для управления событиями"
    });
});

builder.Services.AddSingleton<IEventService, EventService>();
builder.Services.AddSingleton<IBookingService, BookingService>();
builder.Services.AddSingleton<IBookingQueue, BookingQueue>();
builder.Services.AddHostedService<BookingBackgroundService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventManager API v1");
        c.RoutePrefix = "swagger"; 
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

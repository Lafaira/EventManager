using EventManager.DataAccess;
using EventManager.Middleware;
using EventManager.Repositories;
using EventManager.Repositories.Interfaces;
using EventManager.Services;
using EventManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddSingleton<IBookingQueue, BookingQueue>();
builder.Services.AddHostedService<BookingBackgroundService>();
builder.Services.AddScoped<IBookingRepositories, BookingRepositories>();
builder.Services.AddScoped<IEventRepositorie, EventRepositorie>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
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

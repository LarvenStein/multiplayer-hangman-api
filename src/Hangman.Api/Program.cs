using Hangman.Application;
using Hangman.Application.Database;
using Hangman.Api.Endpoints;
using Hangman.Api.Mapping;
using Hangman.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Configuration.AddJsonFile("databasesettings.json");

var config = builder.Configuration;

builder.Services.AddCors();

// Add services to the container.

//builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOutputCache();

builder.Services.AddApplication();

var getEnv = (string key) => Environment.GetEnvironmentVariable(key);

builder.Services.AddDatabase($"server={getEnv("Server") ?? config["ConnectionSettings:Server"]!};" +
    $"uid={getEnv("User") ?? config["ConnectionSettings:User"]!};" +
    $"pwd={getEnv("Password") ?? config["ConnectionSettings:Password"]!};" +
    $"database={getEnv("Database") ?? config["ConnectionSettings:Database"]!}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials());

//app.UseAuthorization();
app.MapApiEndpoints();

app.UseOutputCache();

app.UseMiddleware<ValidationMappingMiddleware>();
//app.MapControllers();
app.MapHub<GameHub>("/api/hub");


var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();

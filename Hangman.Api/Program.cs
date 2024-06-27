using Hangman.Application;
using Hangman.Application.Database;
using Hangman.Api.Endpoints;
using Hangman.Api.Mapping;

var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddDatabase($"server={config["ConnectionSettings:Server"]!};" +
    $"uid={config["ConnectionSettings:User"]!};" +
    $"pwd={config["ConnectionSettings:Password"]!};" +
    $"database={config["ConnectionSettings:Database"]!}");

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

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();

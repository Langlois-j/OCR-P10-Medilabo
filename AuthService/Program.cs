using AuthService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IUserService, InMemoryUserService>();

var app = builder.Build();

app.MapControllers();

app.Run();

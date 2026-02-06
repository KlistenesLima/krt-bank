var builder = WebApplication.CreateBuilder(args);

// Adiciona o YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Adiciona CORS para permitir o Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        builder => builder.WithOrigins("http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

var app = builder.Build();

app.UseCors("AllowAngular");
app.MapReverseProxy();

app.Run();

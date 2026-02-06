var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", b =>
        b.WithOrigins("http://localhost:4200")
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials());
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors("AllowAngular");
app.MapReverseProxy();
app.MapHealthChecks("/health");

app.Run();

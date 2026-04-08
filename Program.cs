var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:4000");

var startup = new WebApi.Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);

app.Run();

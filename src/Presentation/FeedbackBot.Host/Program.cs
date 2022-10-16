using FeedbackBot.Host.Services;
using FeedbackBot.Persistence;
using FeedbackBot.Application.Extensions;
using FeedbackBot.Persistence.Extensions;
using Serilog;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.AppSettings()
    .WriteTo.Console()
    .WriteTo.File("logs/session.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddHostedService<WebhookConfigurator>();

var token = builder.Configuration["Telegram:BotApiToken"];
builder.Services.AddHttpClient("tgwebhook")
    .AddTypedClient<ITelegramBotClient>(client => new TelegramBotClient(token, client));

builder.Services
    .AddApplication()
    .AddPersistence(builder.Configuration)
    .AddControllers()
    .AddNewtonsoftJson();

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DbInitializer.Initialize(context);
}

app.MapControllers();
app.UseRouting();
app.UseCors();

app.Run();
using System.Formats.Asn1;
using FeedbackBot.Host.Services;
using FeedbackBot.Persistence;
using FeedbackBot.Application.Extensions;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Services;
using FeedbackBot.Host;
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
    .AddApplication(builder.Configuration)
    .AddPersistence(builder.Configuration)
    .AddControllers()
    .AddNewtonsoftJson();
builder.Services.AddHostedService<EmailReceiverService>();

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DbInitializer.Initialize(context);
    // var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
    // _emailService.StartReceivingEmailsAsync();
}

app.MapControllers();
app.UseRouting();
app.UseCors();

app.Run();
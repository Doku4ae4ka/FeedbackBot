//using FeedbackBot.Application.Extensions;
//using FeedbackBot.Persistence.Extensions;

//namespace FeedbackBot.Host;

//public class Startup
//{
//    public IConfiguration Configuration { get; }

//    public Startup(IConfiguration configuration) => Configuration = configuration;

//    public void ConfigureServices(IServiceCollection services)
//    {
//        services.AddApplication();
//        services.AddPersistence(Configuration);
//        services.AddControllers();

//        services.AddCors(options =>
//        {
//            options.AddPolicy("AllowAll", policy =>
//            {
//                policy.AllowAnyHeader();
//                policy.AllowAnyMethod();
//                policy.AllowAnyOrigin();
//            });
//        });
//    }
//}

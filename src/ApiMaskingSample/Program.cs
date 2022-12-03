using ApiMaskingSample.Swagger;
using Microsoft.AspNetCore.HttpLogging;
using NLog;
using NLog.Web;
using Slin.Masking;
using Slin.Masking.NLog;

var logger = LogManager
    .Setup(setupBuilder: (setupBuilder) => setupBuilder.UseMasking("masking.json"))//Masking: here is the magic, which inject the masking for NLog
    .GetCurrentClassLogger();

logger.Debug($"init main. Slin.Masking version: {typeof(Masker).Assembly.GetName().Version}, Slin.Masking.NLog version: {typeof(NLogExtensions).Assembly.GetName().Version}");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options=>options.OperationFilter<RequestIdOperationFilter>());

    //traffic logging: injection
    builder.Services.AddHttpLogging(logging =>
    {
        logging.LoggingFields = HttpLoggingFields.RequestBody |
         HttpLoggingFields.RequestPath | HttpLoggingFields.RequestMethod |
         HttpLoggingFields.ResponseBody;
        logging.RequestHeaders.Add("X-Response-Id");
        logging.ResponseHeaders.Add("X-Response-Id");
        logging.MediaTypeOptions.AddText("application/json");
        logging.MediaTypeOptions.AddText("text/plain");
        logging.RequestBodyLogLimit = 40960;
        logging.ResponseBodyLogLimit = 40960;
    });

    //Masking: enable or disable masking is supported by "disabled" property in layout render "event-properties-masker"
    builder.Host.UseNLog(); 

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    //just to ensure that X-Request-Id id set in this demo project. You can remove this from your project.
    app.Use(async (context, next) =>
    {
        string key = "X-Request-ID";
        string value = "";
        if (context.Request.Headers.ContainsKey(key))
        {
            value = context.Request.Headers[key].ToString().Trim();

            if (string.IsNullOrEmpty(value))
            {
                value = Guid.NewGuid().ToString();
            }
        }
        else
        {
            context.Request.Headers.Add(key, value);
        };

        //MappedDiagnosticsLogicalContext.Set("correlcationId", value);
        await next(context);
    });

    app.UseHttpLogging();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}


using Microsoft.AspNetCore.HttpLogging;
using NLog;
using NLog.Web;
using System.Text;
using System.Text.Json.Nodes;
using WebApi6;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Slin.Masking;
using Slin.Masking.NLog;

var logger = LogManager.Setup(setupBuilder: (setupBuilder) =>
{
	var configBuilder = new ConfigurationBuilder();
	configBuilder.AddJsonFile("masking.json")
	.AddJsonFile("masking.user.json", true);
	var cfg = configBuilder.Build();

	var profile = cfg.GetSection("masking").Get<MaskingProfile>();
	profile.Normalize();
	var logMaskOptions = cfg.GetSection("masking:LogMaskingOptions").Get<LogMaskingOptions>();

	var masker = new Masker(profile);

	setupBuilder.SetupSerialization(serialization =>
	{
		serialization.RegisterJsonConverter(new JsonConverter2());
	});
	setupBuilder.SetupExtensions(s =>
	   //s.RegisterLayoutRenderer("trace_id", (logevent) => CorrelationIdentifier.TraceId.ToString())
	   s.RegisterSingletonService<IMasker>(masker)
	   .RegisterSingletonService<IObjectMasker>(new ObjectMasker(masker, logMaskOptions))	   
	   .RegisterLayoutRenderer<EventPropertiesMaskLayoutRenderer>("event-properties-masker")
	);
}).GetCurrentClassLogger();
NLog.LogManager.ConfigurationChanged += (object? sender, NLog.Config.LoggingConfigurationChangedEventArgs e) =>
{
	NLog.LogManager.Configuration.Reload();
	NLog.LogManager.ReconfigExistingLoggers();
};

logger.Debug("init main");

try
{
	var builder = WebApplication.CreateBuilder(args);

	// Add services to the container.
	builder.Configuration.AddJsonFile("masking.json")
	.AddJsonFile("masking.user.json", true);
	
	builder.Services.AddControllers();
	// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen();
	//builder.Services.AddSingleton<IJsonMasker>(new LogMasker());

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
	builder.Services.AddSingleton<MaskingProfile>(sp =>
	{
		var cfg = sp.GetRequiredService<IConfiguration>();
		var profile = cfg.GetSection("masking").Get<MaskingProfile>();
		return profile;
	});
	builder.Services.AddSingleton<LogMaskingOptions>(sp =>
	{
		var cfg = sp.GetRequiredService<IConfiguration>();

		var logMaskOptions = cfg.GetSection("masking:LogMaskingOptions").Get<LogMaskingOptions>();
		return logMaskOptions;
	});
	builder.Services.AddSingleton<IMasker, Masker>();
	builder.Services.AddSingleton<IObjectMasker, ObjectMasker>();

	builder.Host
	//.UseMaskableNLog(null);
	.UseNLog();

	var app = builder.Build();

	// Configure the HTTP request pipeline.
	if (app.Environment.IsDevelopment())
	{
		app.UseSwagger();
		app.UseSwaggerUI();
	}


	app.UseHttpsRedirection();

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
		};

		MappedDiagnosticsLogicalContext.Set("correlcationId", value);
		MappedDiagnosticsLogicalContext.Set("correlcationId2", DateTime.Now.Ticks.ToString());
		await next(context);
	});


	app.UseHttpLogging();

	app.UseAuthorization();

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

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Slin.Masking.NLog;

namespace WebApi6.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private static readonly string[] Summaries = new[]
		{
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	};

		private readonly ILogger<WeatherForecastController> _logger;

		List<KeyValuePair<string, object>> CreateLogEntry()
		{
			var data = new List<KeyValuePair<string, object>>();

			data.Add(new KeyValuePair<string, object>("user", new
			{
				UserName = "userslin",
				FirstName = "Shawn",
				LastName = "Shawn",
				Password = "123456",
				DOB = new DateTime(1988, 1, 1),
				SSN = "123456789",
				Pan = "6225000099991234",
				Amount = 9.9m
			}));
			data.Add(new KeyValuePair<string, object>("data", new { SSN="123456789",PAN="1234567890123456" }));
			data.Add(new KeyValuePair<string, object>("ts", "5.99ms"));
			data.Add(new KeyValuePair<string, object>("school", "shixun"));

			return data;
		}

		public WeatherForecastController(ILogger<WeatherForecastController> logger)
		{
			_logger = logger;
		}

		[HttpGet(Name = "GetWeatherForecast")]
		public IEnumerable<WeatherForecast> Get([FromServices] IObjectMasker engine)
		{
			var entry = CreateLogEntry();
			entry.Add(new KeyValuePair<string, object>("action", nameof(Get)));

			//_logger.LogInformation(entry);


			//var obj = JsonSerializer.SerializeToNode(entry);

			//engine.MaskObjectString(obj);

			return Enumerable.Range(1, 2).Select(index => new WeatherForecast
			{
				Date = DateTime.Now.AddDays(index),
				TemperatureC = Random.Shared.Next(-20, 55),
				DOB = new DateTime(1988,8,8),
				SSN = "123456789",
				PAN = $"622500009999{DateTime.Now.Second:D4}",
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		[HttpPost(Name = "CreateWeatherForecast")]
		public WeatherForecast Post(WeatherForecast request)
		{
			var entry = CreateLogEntry();
			entry.Add(new KeyValuePair<string, object>("action", nameof(Post)));

			_logger.LogInformation(entry);
			return request;
		}
	}
}
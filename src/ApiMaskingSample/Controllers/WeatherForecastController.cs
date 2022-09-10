using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Slin.Masking;
using Slin.Masking.Tests;

namespace ApiMaskingSample.Controllers
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


		public WeatherForecastController(ILogger<WeatherForecastController> logger)
		{
			_logger = logger;
		}

		[HttpGet(Name = "GetWeatherForecast")]
		public IEnumerable<WeatherForecast> Get([FromServices] IObjectMasker engine)
		{
			var entry = DummyData.CreateLogEntry();
			entry.Add(new KeyValuePair<string, object>("eventName", nameof(Get)));

			_logger.LogInformation(entry);

			return Enumerable.Range(1, 2).Select(index => new WeatherForecast
			{
				Date = DateTime.Now.AddDays(index),
				TemperatureC = Random.Shared.Next(-20, 55),
				DOB = new DateTime(1988, 8, 8),
				SSN = "123456789",
				PAN = $"622500009999{DateTime.Now.Second:D4}",
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		[HttpPost(Name = "CreateWeatherForecast")]
		public WeatherForecast Post(WeatherForecast request)
		{
			var entry = DummyData.CreateLogEntry();
			entry.Add(new KeyValuePair<string, object>("action", nameof(Post)));

			_logger.LogInformation(entry);
			return request;
		}
	}


}
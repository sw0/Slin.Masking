namespace WebApi6
{
	public class WeatherForecast
	{
		public DateTime Date { get; set; }

		public int TemperatureC { get; set; }

		public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

		public string SSN { get; set; }

		public DateTime DOB { get; set; }

		public string PAN { get; set; }

		public string? Summary { get; set; }
	}
}
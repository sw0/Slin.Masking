using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Slin.Masking;
using Slin.Masking.Tests;
using ApiMaskingSample.Models;

namespace ApiMaskingSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestLoggerController : ControllerBase
    {
        private static readonly string[] ProductNames = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<TestLoggerController> _logger;


        public TestLoggerController(ILogger<TestLoggerController> logger)
        {
            _logger = logger;
        }

        [HttpGet("masking", Name = "TestMasking")]
        public IActionResult MaskComplexDataEntries()
        {
            var entry = DummyData.CreateLogEntry();
            entry.Add(new KeyValuePair<string, object>("eventName", nameof(MaskComplexDataEntries)));

            //application logging. example
            _logger.LogInformation(entry);

            return Ok();
        }

        [HttpGet("", Name = "GetOrders")]
        public IEnumerable<CustomerOrder> Get()
        {
            var entry = DummyData.CreateLogEntry().Picks(DummyData.Keys.data, DummyData.Keys.dataInBytes, DummyData.Keys.formdata);
            entry.Add(new KeyValuePair<string, object>("eventName", nameof(Get)));

            //application logging. example
            _logger.LogInformation(entry);

            //traffic logging: request/response will be logged
            return Enumerable.Range(1, 2).Select(index => new CustomerOrder
            {
                CreateDate = DateTime.Now.AddDays(index),
                Quantity = Random.Shared.Next(1, 10),
                Product = ProductNames[Random.Shared.Next(ProductNames.Length)],
                Balance = Random.Shared.Next(100, 6999) / 100.00m,
                Customer = new Customer
                {
                    FirstName = $"Steve{index:D2}",
                    LastName = $"Jobs{index:D2}",
                    DOB = new DateTime(1988, 8, 8),
                    SSN = "123456789",
                    PAN = $"45380000{DateTime.Now.Ticks.ToString().Substring(10, 8)}",
                }
            })
            .ToArray();
        }

        [HttpPost(Name = "CreateOrder")]
        public CustomerOrder Post(CustomerOrder request)
        {
            var entry = DummyData.CreateLogEntry().Picks(DummyData.Keys.arrayOfKvpCls, DummyData.Keys.requestUrl);
            entry.Add(new KeyValuePair<string, object>("eventName", nameof(Post)));

            //application logging. example
            _logger.LogInformation(entry);

            request.Id = Guid.NewGuid().ToString();

            //traffic logging: request/response will be logged
            return request;
        }
    }
}
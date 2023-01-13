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
            var exception = default(Exception);
            var entry = LogEntry.New()
                .AddKvp("eventName", nameof(MaskComplexDataEntries));
            try
            {
                entry.AddKvpIfNotEmpty("fieldx", "field value")
                    .AddKvpIfNotEmpty("ssn", "101123456")
                    .SetMessage("test log");

                //do something
                entry.AddComment("step 1 note");
                //do something
                entry.AddComment("step 2 note");
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                //application logging. example
                if (exception != null) _logger.LogError(exception, entry);
                else _logger.LogInformation(entry);
            }

            return Ok(new { message = "this is just basic log sample" });
        }

        [HttpGet("", Name = "GetOrders")]
        public IEnumerable<CustomerOrder> Get()
        {
            var entry = LogEntry.New().AddKvp("eventName", nameof(Get));

            entry.AddKvp(DummyData.Keys.data, DummyData.DataInBytes)
                .AddKvp(DummyData.Keys.formdata, DummyData.Keys.formdata);

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
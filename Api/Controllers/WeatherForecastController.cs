using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microservice.Api.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Microservice.Api.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [ApiConventionType(typeof(CustomApiConventions))]
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


        /// <summary>
        /// Get a weather's forecast
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /WeatherForecast
        ///     {
        ///       "date": "2020-02-04T10:44:30.9442078-05:00",
        ///       "temperatureC": 52,
        ///       "temperatureF": 125,
        ///       "summary": "Freezing"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Returns all records</response>
        /// <response code="404">If the resource was not found or is null</response>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var rng = new Random();

            return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWeatherForecast(int id)
        {
            var rng = new Random();

            return Ok(new WeatherForecast
            {
                Date = DateTime.UtcNow,
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });

        }

        [HttpPost]
        public async Task<IActionResult> CreateWeatherForecast([FromBody]WeatherForecast weatherForecast)
        {
            if (weatherForecast == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest();

            var rng = new Random();
            return CreatedAtRoute(
              nameof(GetWeatherForecast),
              routeValues: new { id = rng.Next(100) },
              value: weatherForecast);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWeatherForecast(int id, [FromBody]WeatherForecast weatherForecast)
        {
            if (weatherForecast == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest();

            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWeatherForecast(int id)
        {
            return new NoContentResult();
        }


    }
}

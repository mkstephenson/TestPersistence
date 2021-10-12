using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Common;
using DataPersistor.Data;

namespace DataPersistor.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class WeatherForecastsPersistorController : ControllerBase
  {
    private readonly DataPersistorContext _context;

    public WeatherForecastsPersistorController(DataPersistorContext context)
    {
      _context = context;
    }

    // GET: api/WeatherForecastsPersistor
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetWeatherForecast()
    {
      return await _context.WeatherForecast.ToListAsync();
    }

    // GET: api/WeatherForecastsPersistor/5
    [HttpGet("{id}")]
    public async Task<ActionResult<WeatherForecast>> GetWeatherForecast(int id)
    {
      var weatherForecast = await _context.WeatherForecast.FindAsync(id);

      if (weatherForecast == null)
      {
        return NotFound();
      }

      return weatherForecast;
    }

    [HttpGet("lastforlocation/{location}")]
    public async Task<ActionResult<WeatherForecast>> GetLastForLocation(string location)
    {
      var forecast = await _context.WeatherForecast
        .Where(f => f.Location == location)
        .OrderByDescending(f => f.Date)
        .FirstAsync();

      if (forecast == null)
      {
        return NotFound();
      }

      return forecast;
    }

    // POST: api/WeatherForecastsPersistor
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<WeatherForecast>> PostWeatherForecast(WeatherForecast weatherForecast)
    {
      _context.WeatherForecast.Add(weatherForecast);
      await _context.SaveChangesAsync();

      return CreatedAtAction("GetWeatherForecast", new { id = weatherForecast.Id }, weatherForecast);
    }

    private bool WeatherForecastExists(int id)
    {
      return _context.WeatherForecast.Any(e => e.Id == id);
    }
  }
}

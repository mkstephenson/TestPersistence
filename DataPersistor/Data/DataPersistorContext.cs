using Microsoft.EntityFrameworkCore;
using Common;

namespace DataPersistor.Data
{
  public class DataPersistorContext : DbContext
  {
    public DataPersistorContext(DbContextOptions<DataPersistorContext> options)
        : base(options)
    {
    }

    public DbSet<WeatherForecast> WeatherForecast { get; set; }
  }
}

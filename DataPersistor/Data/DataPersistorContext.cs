using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Common;

namespace DataPersistor.Data
{
    public class DataPersistorContext : DbContext
    {
        public DataPersistorContext (DbContextOptions<DataPersistorContext> options)
            : base(options)
        {
        }

        public DbSet<Common.WeatherForecast> WeatherForecast { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common
{
  public class WeatherForecast
  {
    [JsonIgnore]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    [MaxLength(50)]
    public string Summary { get; set; }

    [MaxLength(50)]
    public string Location { get; set; }
  }
}

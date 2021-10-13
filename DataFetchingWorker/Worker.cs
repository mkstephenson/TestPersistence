using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DataFetchingWorker
{
  public class Worker : BackgroundService
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<Worker> _logger;
    private readonly Uri _dataPersistorUrl;
    private readonly Uri _dataProviderUrl;
    private readonly DateTime _startingTimestamp = DateTime.MinValue;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public Worker(HttpClient httpClient, IConfiguration configuration, ILogger<Worker> logger)
    {
      _httpClient = httpClient;
      _logger = logger;

      _dataPersistorUrl = new UriBuilder
      {
        Host = Environment.GetEnvironmentVariable("DATAPERSISTOR_SERVICE_SERVICE_HOST") ?? configuration["DataPersistor:Host"],
        Port = int.Parse(Environment.GetEnvironmentVariable("DATAPERSISTOR_SERVICE_SERVICE_PORT") ?? configuration["DataPersistor:Port"]),
        Scheme = "http"
      }.Uri;

      _dataProviderUrl = new UriBuilder
      {
        Host = Environment.GetEnvironmentVariable("DATAPROVIDER_SERVICE_SERVICE_HOST") ?? configuration["DataProvider:Host"],
        Port = int.Parse(Environment.GetEnvironmentVariable("DATAPROVIDER_SERVICE_SERVICE_PORT") ?? configuration["DataProvider:Port"]),
        Scheme = "http"
      }.Uri;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var tasks = new List<Task>();
      Parallel.For(1, 10, i => tasks.Add(Task.Run(() => ExecuteFetch($"{Environment.MachineName}-{i}", stoppingToken), stoppingToken)));
      Task.WaitAll(tasks.ToArray());
      return Task.CompletedTask;
    }

    private async Task ExecuteFetch(string location, CancellationToken stoppingToken)
    {
      DateTime currentTime = _startingTimestamp;
      var lastItem = await _httpClient.GetAsync(new Uri(_dataPersistorUrl, $"WeatherForecastsPersistor/lastforlocation/{location}"));
      if (lastItem.IsSuccessStatusCode)
      {
        var lastItemTimestamp = JsonSerializer.Deserialize<WeatherForecast>(await lastItem.Content.ReadAsStringAsync(), _jsonSerializerOptions)?.Date;
        if (lastItemTimestamp.HasValue)
        {
          currentTime = lastItemTimestamp.Value.AddDays(1);
        }
      }

      var forecastUrl = new UriBuilder(_dataProviderUrl)
      {
        Path = "WeatherForecast"
      };

      var queryStringDictionary = new Dictionary<string, string>
      {
        { "numberDays", "7" },
        { "location", location }
      };

      while (!stoppingToken.IsCancellationRequested)
      {
        queryStringDictionary["start"] = currentTime.ToString("O");
        forecastUrl.Query = string.Join('&', queryStringDictionary.Select(i => $"{i.Key}={i.Value}"));
        var nextForecast = await _httpClient.GetAsync(forecastUrl.Uri, stoppingToken);
        if (nextForecast.IsSuccessStatusCode)
        {
          var resultContent = await nextForecast.Content.ReadAsStringAsync(stoppingToken);
          var forecasts = JsonSerializer.Deserialize<WeatherForecast[]>(resultContent, _jsonSerializerOptions);
          foreach (var f in forecasts)
          {
            var isSuccessful = false;
            do
            {
              var result = await _httpClient.PostAsync(new Uri(_dataPersistorUrl, "WeatherForecastsPersistor"), new StringContent(JsonSerializer.Serialize(f, _jsonSerializerOptions), Encoding.UTF8, "application/json"), stoppingToken);
              isSuccessful = result.IsSuccessStatusCode;
            } while (!isSuccessful);
          }
          currentTime += TimeSpan.FromDays(7);
        }
        else
        {
          await Task.Delay(5000, stoppingToken);
        }
      }
    }
  }
}

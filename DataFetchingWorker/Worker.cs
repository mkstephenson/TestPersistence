using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DataFetchingWorker
{
  public class Worker : BackgroundService
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<Worker> _logger;
    private readonly Uri _dataProviderUrl;

    public Worker(HttpClient httpClient, ILogger<Worker> logger)
    {
      _httpClient = httpClient;
      _logger = logger;

      _dataProviderUrl = new UriBuilder
      {
        Host = Environment.GetEnvironmentVariable("DATAPERSISTOR_SERVICE_SERVICE_HOST"),
        Port = int.Parse(Environment.GetEnvironmentVariable("DATAPERSISTOR_SERVICE_SERVICE_PORT")),
        Scheme = "http"
      }.Uri;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        var result = await _httpClient.GetAsync(new Uri(_dataProviderUrl, "/WeatherForecast?numberDays=5&start=2020-01-01&location=London"));
      }
    }
  }
}

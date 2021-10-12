using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using DataPersistor.Data;
using System.Threading.Tasks;
using System.Linq;

namespace DataPersistor
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();
      services.AddHealthChecks();
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "DataPersistor", Version = "v1" });
      });

      services.AddDbContext<DataPersistorContext>(options =>
              options.UseSqlServer(Configuration.GetConnectionString("DataPersistorContext")));
      services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseDeveloperExceptionPage();
      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.RoutePrefix = "";
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DataPersistor v1");
      });

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/health");
      });

      using var scope = app.ApplicationServices.CreateScope();
      var ctx = scope.ServiceProvider.GetRequiredService<DataPersistorContext>();
      if (ctx.Database.GetPendingMigrations().Any())
      {
        ctx.Database.Migrate();
      }
    }
  }
}

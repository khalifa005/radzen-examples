using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SpaServices.AngularCli;

using ExportWithFilter.Data;

namespace ExportWithFilter
{
  public partial class Startup
  {
    public Startup(IConfiguration configuration, IHostingEnvironment env)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    partial void OnConfigureServices(IServiceCollection services);

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddOptions();
      services.AddCors();

      services.AddMvc(options =>
      {
          options.FormatterMappings.SetMediaTypeMappingForFormat("csv", "text/csv");
          options.OutputFormatters.Add(new ExportWithFilter.Data.CsvDataContractSerializerOutputFormatter());
          options.OutputFormatters.Add(new ExportWithFilter.Data.XlsDataContractSerializerOutputFormatter());
      });

      services.AddAuthorization();
      services.AddOData();
      services.AddODataQueryFilter();


      services.AddDbContext<ExportWithFilter.Data.SampleContext>(options =>
      {
        options.UseSqlServer(Configuration.GetConnectionString("SampleConnection"));
      });

      OnConfigureServices(services);
    }

    partial void OnConfigure(IApplicationBuilder app);
    partial void OnConfigureOData(ODataConventionModelBuilder builder);
   
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();

      IServiceProvider provider = app.ApplicationServices.GetRequiredService<IServiceProvider>();

      app.UseCors(builder =>
        builder.WithOrigins("*")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials()
               .AllowAnyOrigin()
      );

      app.Use(async (context, next) => {
          if (context.Request.Path.Value == "/__ssrsreport" || context.Request.Path.Value == "/ssrsproxy") {
            await next();
            return;
          }
          try
          {
              await next();
          }
#pragma warning disable 0168
          catch (Microsoft.AspNetCore.Mvc.Internal.AmbiguousActionException ex) {
#pragma warning restore 0168
              if (!Path.HasExtension(context.Request.Path.Value)) {
                  context.Request.Path = "/index.html";
                  await next();
              }
          }

          if ((context.Response.StatusCode == 404 || context.Response.StatusCode == 401) && !Path.HasExtension(context.Request.Path.Value)) {
              context.Request.Path = "/index.html";
              await next();
          }
      });

      app.UseDefaultFiles();
      app.UseStaticFiles();

      app.UseDeveloperExceptionPage();

      app.UseMvc(builder =>
      {
          builder.Count().Filter().OrderBy().Expand().Select().MaxTop(null).SetTimeZoneInfo(TimeZoneInfo.Utc);

          if (env.EnvironmentName == "Development")
          {
              builder.MapRoute(name: "default",
                template: "{controller}/{action}/{id?}",
                defaults: new { controller = "Home", action = "Index" }
              );
          }

          var oDataBuilder = new ODataConventionModelBuilder(provider);

          oDataBuilder.EntitySet<ExportWithFilter.Models.Sample.Order>("Orders");
          oDataBuilder.EntitySet<ExportWithFilter.Models.Sample.OrderDetail>("OrderDetails");
          oDataBuilder.EntitySet<ExportWithFilter.Models.Sample.Product>("Products");

          this.OnConfigureOData(oDataBuilder);

          var model = oDataBuilder.GetEdmModel();

          builder.MapODataServiceRoute("odata/Sample", "odata/Sample", model);

      });

      if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RADZEN")) && env.IsDevelopment())
      {
        app.UseSpa(spa =>
        {
          spa.Options.SourcePath = "../client";
          spa.UseAngularCliServer(npmScript: "start -- --port 8000 --open");
        });
      }

      OnConfigure(app);
    }
  }
}

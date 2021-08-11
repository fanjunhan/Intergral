using System;
using System.Text.Json;
using Autofac;
using Integral.Models;
using Integral.Actors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newbe.Claptrap;
using Newbe.Claptrap.Bootstrapper;
using OpenTelemetry.Trace;
using Newbe.Claptrap.StorageProvider.MySql;

namespace Integral.BackendServer
{
    public class Startup
    {
        private readonly AutofacClaptrapBootstrapper _claptrapBootstrapper;
        private readonly IClaptrapDesignStore _claptrapDesignStore;
        private readonly ClaptrapBootstrapperBuilderOptions _options;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var loggerFactory = new ServiceCollection()
                .AddLogging(logging => logging.AddConsole())
                .BuildServiceProvider()
                .GetRequiredService<ILoggerFactory>();
            
            var bootstrapperBuilder = new AutofacClaptrapBootstrapperBuilder(loggerFactory);
            _claptrapBootstrapper = (AutofacClaptrapBootstrapper) bootstrapperBuilder
                .ScanClaptrapModule()
                .AddConfiguration(configuration)
                .ScanClaptrapDesigns(new[] {typeof(IntegralActor).Assembly})
                .Build();
            _claptrapDesignStore = _claptrapBootstrapper.DumpDesignStore();
            _options = bootstrapperBuilder.Options;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {            
          /*   services.AddOpenTelemetryTracing(
                builder => builder
                    .AddSource(ClaptrapActivitySource.Instance.Name)
                    .SetSampler(new ParentBasedSampler(new AlwaysOnSampler()))
                    .AddAspNetCoreInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddZipkinExporter(options =>
                    {
                        options.Endpoint = new Uri(this.Configuration.GetValue<string>("Zipkin:Endpoint"));
                    })
            ); */
            services.AddClaptrapServerOptions();
            services.AddActors(options => { options.AddClaptrapDesign(_claptrapDesignStore); });
            services.AddControllers().AddJsonOptions(options =>
            {
          //      options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.IncludeFields = true;
            }
            ).AddDapr();
            services.AddSingleton(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            });
            services.AddSingleton<IIntegralConfigStore>(new IntegralConfigStore(new DbFactory(_options)));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Integral.BackendServer", Version = "v1"});
            });
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register your own things directly with Autofac here. Don't
            // call builder.Populate(), that happens in AutofacServiceProviderFactory
            // for you.

            _claptrapBootstrapper.Boot(builder);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Integral.BackendServer v1"));

            app.UseRouting();
            app.UseCloudEvents();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapActorsHandlers();
                endpoints.MapSubscribeHandler();
                endpoints.MapControllers();
            });
        }
    }
}
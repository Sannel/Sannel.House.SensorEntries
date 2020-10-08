/* Copyright 2019-2020 Sannel Software, L.L.C.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
      http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/

using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sannel.House.SensorLogging.Data;
using Sannel.House.Base.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Logging;
using Sannel.House.Base.Web;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Repositories;
using Microsoft.Extensions.Hosting;
using Sannel.House.SensorLogging.Services;

namespace Sannel.House.SensorLogging
{
	public class Startup
	{
		public Startup(IConfiguration configuration) 
			=> this.Configuration = configuration;

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContextPool<SensorLoggingContext>(o =>
			{
				var connectionString = Configuration.GetWithReplacement("Db:ConnectionString");
				switch(Configuration["Db:Provider"]?.ToLower())
				{
					case "mysql":
						throw new NotSupportedException("MySql is no longer supported");

					case "sqlserver":
						o.ConfigureSqlServer(connectionString);
						break;

					case "postgresql":
						o.ConfigurePostgreSQL(connectionString);
						break;

					case "sqlite":
					default:
						o.ConfigureSqlite(connectionString);
						break;
				};
			});

			services.AddControllers();


			services.AddAuthentication(Configuration["Authentication:Schema"])
				.AddIdentityServerAuthentication(Configuration["Authentication:Schema"], o =>
					{
						o.Authority = this.Configuration["Authentication:AuthorityUrl"];
						o.ApiName = this.Configuration["Authentication:ApiName"];

						var apiSecret = this.Configuration["Authentication:ApiSecret"];
						if (!string.IsNullOrWhiteSpace(apiSecret))
						{
							o.ApiSecret = apiSecret;
						}

						o.SupportedTokens = SupportedTokens.Both;

						if (Configuration.GetValue<bool?>("Authentication:DisableRequireHttpsMetadata") == true)
						{
							o.RequireHttpsMetadata = false;
						}
					});



			services.AddTransient<ISensorRepository, DbContextRepository>();

			services.AddOpenApiDocument((s, p) =>
			{
				s.Title = "Sensor Logging";
				s.Description = "Project for logging sensor data";
			});

			services.AddMqttService(Configuration);

			services.AddTransient<ISensorService, SensorService>();

			services.AddHealthChecks()
				.AddDbHealthCheck<SensorLoggingContext>("DbHealthCheck", async (context) =>
				{
					await context.SensorEntries.Take(1).CountAsync();
				});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider, ILogger<Startup> logger)
		{
			provider.CheckAndInstallTrustedCertificate();

			var p = Configuration["Db:Provider"];
			var db = provider.GetService<SensorLoggingContext>();

			if(db is null)
			{
				logger.LogError("Unable to get instance of {context}", nameof(SensorLoggingContext));
				throw new Exception("Unable to get database context");
			}

			if (string.Compare(p, "mysql", true) == 0
					|| string.Compare(p, "postgresql", true) == 0
					|| string.Compare(p, "sqlserver", true) == 0)
			{
				if (!db.WaitForServer(logger))
				{
					throw new Exception("Shutting down");
				}
			}

			db.Database.Migrate();


			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				//app.UseHsts();
			}

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();
			//app.UseHttpsRedirection();

			app.UseOpenApi();
			app.UseReDoc();

			app.UseEndpoints(i =>
			{
				i.MapControllers();
				i.MapHouseHealthChecks("/health");
				i.MapHouseRobotsTxt();
			});

		}
	}
}

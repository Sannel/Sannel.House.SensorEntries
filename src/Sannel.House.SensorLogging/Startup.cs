/* Copyright 2019 Sannel Software, L.L.C.
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
using Sannel.House.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Logging;
using Sannel.House.Web;
using Sannel.House.Devices.Client;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Repositories;

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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddDbContextPool<SensorLoggingContext>(o =>
			{
				switch (Configuration["Db:Provider"])
				{
					case "mysql":
						o.ConfigureMySql(Configuration["Db:ConnectionString"]);
						break;
					case "sqlserver":
						o.ConfigureSqlServer(Configuration["Db:ConnectionString"]);
						break;
					case "PostgreSQL":
					case "postgresql":
						o.ConfigurePostgreSQL(Configuration["Db:ConnectionString"]);
						break;
					case "sqlite":
					default:
						o.ConfigureSqlite(Configuration["Db:ConnectionString"]);
						break;
				}
			});


			services.AddAuthentication("houseapi")
			.AddIdentityServerAuthentication("houseapi", o =>
			{
				o.Authority = this.Configuration["Authentication:AuthorityUrl"];
				o.ApiName = this.Configuration["Authentication:ApiName"];
				o.SupportedTokens = SupportedTokens.Both;
#if DEBUG
				if (this.Configuration.GetValue<bool?>("Authentication:DisableRequireHttpsMetadata") == true)
				{
					o.RequireHttpsMetadata = false;
				}
#endif
			});

			services.AddDevicesHttpClientRegistration(new Uri(Configuration["Client:DevicesBaseUrl"]));

			services.AddTransient((p) =>
			{
				var d = new DevicesClient(p.GetService<IHttpClientFactory>(), p.GetService<ILogger<DevicesClient>>());
				return d;
			});

			services.AddTransient<ISensorRepository, DbContextRepository>();

			services.AddSwaggerDocument();

			services.AddHealthChecks()
				.AddDbHealthCheck<SensorLoggingContext>("DbHealthCheck", async (context) =>
				{
					await context.SensorEntries.Take(1).CountAsync();
				});


		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider provider, ILogger<Startup> logger)
		{
			provider.CheckAndInstallTrustedCertificate();

			var p = Configuration["Db:Provider"];
			var db = provider.GetService<SensorLoggingContext>();

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
				app.UseHsts();
			}

			app.UseHealthChecks("/health");

			app.UseAuthentication();
			app.UseHttpsRedirection();

			app.UseSwagger();
			app.UseSwaggerUi3();

			app.UseMvc();
		}
	}
}

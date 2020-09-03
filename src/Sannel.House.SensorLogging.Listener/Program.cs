/* Copyright 2020-2020 Sannel Software, L.L.C.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
      http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sannel.House.Base.Data;
using Sannel.House.Base.MQTT.Interfaces;
using Sannel.House.Base.Web;
using Sannel.House.SensorLogging.Data;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Repositories;
using Sannel.House.SensorLogging.Services;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Sannel.House.SensorLogging.Tests")]

namespace Sannel.House.SensorLogging.Listener
{
	public class Program
	{
		public static void Main(string[] args) 
			=> CreateHostBuilder(args).Build().Run();

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((builder) => BuildConfiguration(builder, args))
				.ConfigureServices((hostContext, services) =>
				{
					SetupDI(hostContext.Configuration, services);
				});
		
		public static void BuildConfiguration(IConfigurationBuilder builder, string[] args)
		{
			builder.AddJsonFile("appsettings.json", false)
				.AddJsonFile(Path.Combine("app_config", "appsettings.json"), true)
				.AddYamlFile(Path.Combine("app_config", "appsettings.yml"), false)
				.AddEnvironmentVariables()
				.AddCommandLine(args);
		}

		public static void SetupDI(IConfiguration configuration, IServiceCollection services)
		{
			services.AddSingleton(configuration);
			services.AddLogging(i =>
			{
				i.AddConsole();
			});

			services.AddDbContextPool<SensorLoggingContext>(o =>
			{
				var connectionString = configuration.GetWithReplacement("Db:ConnectionString");
				switch(configuration["Db:Provider"]?.ToLower())
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

			services.AddTransient<ISensorRepository, DbContextRepository>();
			services.AddTransient<ISensorService, SensorService>();

			services.AddTransient<IMqttTopicSubscriber, SensorDataSubscriber>();
			services.AddTransient<IMqttTopicSubscriber, DeviceSubscriber>();

			services.AddMqttService(configuration["MQTT:Server"],
				configuration["MQTT:DefaultTopic"],
				configuration.GetValue<int?>("MQTT:Port"));
		}
	}
}

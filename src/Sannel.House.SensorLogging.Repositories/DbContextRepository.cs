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
using Microsoft.Extensions.Logging;
using Sannel.House.SensorLogging.Data;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.SensorLogging.Repositories
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Sannel.House.SensorLogging.Interfaces.ISensorRepository" />
	public class DbContextRepository : ISensorRepository
	{
		private SensorLoggingContext context;
		private ILogger logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="DbContextRepository"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <exception cref="ArgumentNullException">context</exception>
		public DbContextRepository(SensorLoggingContext context, ILogger<DbContextRepository> logger)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Adds the Sensor Entry asynchronous.
		/// </summary>
		/// <param name="sensorEntry">The sensor entry.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">sensorEntry</exception>
		public async Task AddSensorEntryAsync(SensorEntry sensorEntry)
		{
			if(sensorEntry == null)
			{
				logger.LogError("Null SensorEntry Received");
				throw new ArgumentNullException(nameof(sensorEntry));
			}

			if(sensorEntry.SensorEntryId == Guid.Empty)
			{
				logger.LogWarning("Empty Id on Sensor Entry");
				sensorEntry.SensorEntryId = Guid.NewGuid();
			}

			if(logger.IsEnabled(LogLevel.Debug))
			{
				logger.LogDebug("SensorEntry received {0}", sensorEntry);
			}

			await context.SensorEntries.AddAsync(sensorEntry);
			await context.SaveChangesAsync();
			context.Entry(sensorEntry).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
		}
	}
}

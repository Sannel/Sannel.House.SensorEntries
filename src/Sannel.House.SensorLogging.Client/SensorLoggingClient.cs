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
using Sannel.House.Client;
using Sannel.House.Sensor;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.SensorLogging.Client
{
	public class SensorLoggingClient : ClientBase
	{
		/// <summary>Initializes a new instance of the <see cref="SensorLoggingClient"/> class.</summary>
		/// <param name="factory">The factory.</param>
		/// <param name="logger">The logger.</param>
		/// <exception cref="ArgumentNullException">factory
		/// or
		/// logger</exception>
		public SensorLoggingClient(IHttpClientFactory factory, ILogger<SensorLoggingClient> logger) : base(factory, logger)
		{

		}

		/// <summary>
		/// Gets the client.
		/// </summary>
		/// <returns></returns>
		protected override HttpClient GetClient()
			=> factory.CreateClient(nameof(SensorLoggingClient));

		/// <summary>
		/// Logs the reading asynchronous.
		/// </summary>
		/// <param name="deviceId">The device identifier.</param>
		/// <param name="sensor">The sensor.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public Task<Results<Guid>> LogReadingAsync(int deviceId, SensorTypes sensor, params KeyValuePair<string, double>[] values)
			=> LogReadingAsync(deviceId, sensor, DateTimeOffset.Now, values);

		/// <summary>
		/// Logs the reading asynchronous.
		/// </summary>
		/// <param name="deviceId">The device identifier.</param>
		/// <param name="sensor">The sensor.</param>
		/// <param name="createdDate">The created date.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">values</exception>
		/// <exception cref="ArgumentOutOfRangeException">values - You must pass in at least 1 value</exception>
		public Task<Results<Guid>> LogReadingAsync(int deviceId, SensorTypes sensor, DateTimeOffset createdDate, params KeyValuePair<string, double>[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			if(values.Length < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(values), "You must pass in at least 1 value");
			}

			var reading = new SensorReading
			{
				DeviceId = deviceId,
				SensorType = sensor,
				CreationDate = createdDate,
				Values = new Dictionary<string, double>()
			};

			foreach (var val in values)
			{
				reading.Values.Add(val.Key, val.Value);
			}

			return PostAsync<Results<Guid>>("SensorLogging", reading);
		}

		/// <summary>
		/// Logs the reading asynchronous.
		/// </summary>
		/// <param name="deviceId">The device identifier.</param>
		/// <param name="sensor">The sensor.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public Task<Results<Guid>> LogReadingAsync(int deviceId, SensorTypes sensor, params (string key, double value)[] values)
					=> LogReadingAsync(deviceId, sensor, DateTimeOffset.Now, values);

		/// <summary>
		/// Logs the reading asynchronous.
		/// </summary>
		/// <param name="deviceId">The device identifier.</param>
		/// <param name="sensor">The sensor.</param>
		/// <param name="createdDate">The created date.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public Task<Results<Guid>> LogReadingAsync(int deviceId, SensorTypes sensor, DateTimeOffset createdDate, params (string key, double value)[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			if (values.Length < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(values), "You must pass in at least 1 value");
			}

			var reading = new SensorReading
			{
				DeviceId = deviceId,
				SensorType = sensor,
				CreationDate = createdDate,
				Values = new Dictionary<string, double>()
			};

			foreach (var (key, value) in values)
			{
				reading.Values.Add(key, value);
			}

			return PostAsync<Results<Guid>>("SensorLogging", reading);
		}
	}
}

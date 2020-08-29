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

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sannel.House.Base.MQTT.Interfaces;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Listener.Models;

namespace Sannel.House.SensorLogging.Listener
{
	public class SensorDataSubscriber : IMqttTopicSubscriber
	{
		private readonly ILogger logger;
		private readonly ISensorService service;

		public SensorDataSubscriber(ISensorService service, ILogger<SensorDataSubscriber> logger, IConfiguration configuration)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			Topic = (configuration ?? throw new ArgumentNullException(nameof(configuration)))["Mqtt:SensorLoggingTopic"];
		}

		public string Topic
		{
			get;
			private set;
		}

		public async void Message(string topic, string message)
		{
			var options = new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true
			};

			options.Converters.Add(new JsonStringEnumConverter());

			var reading = JsonSerializer.Deserialize<FieldReading>(message, options);

			if(logger.IsEnabled(LogLevel.Debug))
			{
				logger.LogDebug($"topic={topic} message={message}");
			}

			if(reading is null)
			{
				logger.LogError($"Received invalid message on topic {topic}");
				return;
			}

			if(reading.MacAddress.HasValue)
			{
				await service.AddSensorEntryAsync(reading.SensorType, DateTimeOffset.Now, reading.Values, reading.MacAddress.Value);
			}
			else if(reading.Uuid.HasValue)
			{
				await service.AddSensorEntryAsync(reading.SensorType, DateTimeOffset.Now, reading.Values, reading.Uuid.Value);
			}
			else if(!string.IsNullOrWhiteSpace(reading.Manufacture) && !string.IsNullOrWhiteSpace(reading.ManufactureId))
			{
				await service.AddSensorEntryAsync(reading.SensorType, DateTimeOffset.Now,
					reading.Values, reading.Manufacture, reading.ManufactureId);
			}
			else
			{
				logger.LogError($"Invalid reading on topic {topic} message={message}");
			}
		}
	}
}

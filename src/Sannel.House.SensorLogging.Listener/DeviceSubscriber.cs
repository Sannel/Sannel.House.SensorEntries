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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sannel.House.Base.MQTT.Interfaces;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Listener.Models;
using Sannel.House.SensorLogging.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sannel.House.SensorLogging.Listener
{
	public class DeviceSubscriber : IMqttTopicSubscriber
	{
		private readonly ILogger logger;
		private readonly ISensorService service;

		public DeviceSubscriber(ISensorService service, ILogger<DeviceSubscriber> logger, IConfiguration configuration)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			Topic = (configuration ?? throw new ArgumentNullException(nameof(configuration)))["Mqtt:DevicesTopic"];
		}

		public string Topic
		{
			get;
			private set;
		}

		public async void Message(string topic, string message)
			=> await MessageAsync(topic, message);

		internal async Task MessageAsync(string topic, string message)
		{
			try
			{
				if(string.IsNullOrWhiteSpace(message))
				{
					return;
				}

				var options = new JsonSerializerOptions()
				{
					PropertyNameCaseInsensitive = true
				};

				options.Converters.Add(new JsonStringEnumConverter());
				var dmessage = System.Text.Json.JsonSerializer.Deserialize<DeviceMessage>(message, options);

				if(dmessage.DeviceId.HasValue)
				{
					await service.UpdateDeviceInformationFromMessageAsync(dmessage);
				}
			}
			catch(JsonException ex)
			{
				logger.LogError(ex, "Error reading device message on topic {0}", topic);
			}
		}
	}
}

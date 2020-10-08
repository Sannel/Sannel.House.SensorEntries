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
using Sannel.House.Base.Sensor;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.SensorLogging.Services
{
	public class SensorService : ISensorService
	{
		public readonly ISensorRepository repository;
		public readonly ILogger logger;
		public readonly IMqttClientPublishService mqttClient;
		public readonly string newReadingTopic;
		public readonly string unknownDeviceTopic;

		/// <summary>
		/// Initializes a new instance of the <see cref="SensorService"/> class.
		/// </summary>
		/// <param name="mqttClient">The MQTT client.</param>
		/// <param name="repository">The repository.</param>
		/// <param name="logger">The logger.</param>
		/// <exception cref="System.ArgumentNullException">
		/// mqttClient
		/// or
		/// repository
		/// or
		/// logger
		/// </exception>
		public SensorService(IMqttClientPublishService mqttClient, ISensorRepository repository, IConfiguration configuration, ILogger<SensorService> logger)
		{
			this.mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));
			this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			newReadingTopic = configuration["MQTT:NewReadingTopic"];
			unknownDeviceTopic = configuration["MQTT:UnknownDeviceTopic"];
		}

		/// <summary>
		/// Adds the sensor entry asynchronous.
		/// </summary>
		/// <param name="sensorType">Type of the sensor.</param>
		/// <param name="creationDate">The creation date.</param>
		/// <param name="values">The values.</param>
		/// <param name="deviceMacAddress">The device mac address.</param>
		/// <exception cref="NullReferenceException">Unable to create a device for some reason</exception>
		public async Task AddSensorEntryAsync(SensorTypes sensorType, Dictionary<string, double> values, long deviceMacAddress)
		{
			var device = await repository.GetDeviceByMacAddressAsync(deviceMacAddress);
			if(device is null)
			{
				if(logger.IsEnabled(LogLevel.Debug))
				{
					logger.LogDebug("Unknown Device with MacAddress {MacAddress} Adding", deviceMacAddress);
				}

				device = await repository.AddDeviceByMacAddressAsync(deviceMacAddress);
			}

			if(device is null)
			{
				logger.LogError("Unable to create device for macAddress {deviceMacAddress}", deviceMacAddress);
				throw new NullReferenceException("Unable to create a device for some reason");
			}

			if(device.DeviceId is null)
			{
				if(logger.IsEnabled(LogLevel.Debug))
				{
					logger.LogDebug("Unknown DeviceId for MacAddress = {MacAddress}", device.MacAddress);
				}

				mqttClient.Publish(unknownDeviceTopic,
					new UnknownDeviceMessage()
					{
						MacAddress = device.MacAddress
					});
			}

			var result = await repository.AddSensorEntryAsync(sensorType, device.LocalDeviceId, values);
			if(result != null)
			{
				if(logger.IsEnabled(LogLevel.Debug))
				{
					logger.LogDebug("Topic = {Topic} SensorType = {SensorType} CreationDate = {CreationDate} LocalDeviceId = {LocalDeviceId}",
						newReadingTopic,
						result.SensorType,
						result.CreationDate,
						result.LocalDeviceId);
				}
				mqttClient.Publish(newReadingTopic,
					new NewReadingMessage()
					{
						DeviceId = await repository.GetDeviceIdFromLocalDeviceId(result.LocalDeviceId),
						SensorType = result.SensorType,
						CreationDate = result.CreationDate,
						Values = result.Values.ToDictionary(i => i.Name, i => i.Value)
					});
			}
			else
			{
				logger.LogWarning("Results is null");
			}
		}

		/// <summary>
		/// Adds the sensor entry asynchronous.
		/// </summary>
		/// <param name="sensorType">Type of the sensor.</param>
		/// <param name="values">The values.</param>
		/// <param name="deviceUuid">The device UUID.</param>
		/// <exception cref="NullReferenceException">Unable to create a device for some reason</exception>
		public async Task AddSensorEntryAsync(SensorTypes sensorType, Dictionary<string, double> values, Guid deviceUuid)
		{
			var device = await repository.GetDeviceByUuidAsync(deviceUuid);
			if(device is null)
			{
				if(logger.IsEnabled(LogLevel.Debug))
				{
					logger.LogDebug("Unknown Device with Uuid {deviceUuid} Adding", deviceUuid);
				}

				device = await repository.AddDeviceByUuidAsync(deviceUuid);
			}

			if(device is null)
			{
				logger.LogError($"Unable to create device for uuid {deviceUuid}");
				throw new NullReferenceException("Unable to create a device for some reason");
			}

			if(device.DeviceId is null)
			{
				if(logger.IsEnabled(LogLevel.Debug))
				{
					logger.LogDebug("Unknown DeviceId for Uuid = {deviceUuid}", device.Uuid);
				}

				mqttClient.Publish(unknownDeviceTopic,
					new UnknownDeviceMessage()
					{
						Uuid = deviceUuid
					});
			}

			var result = await repository.AddSensorEntryAsync(sensorType, device.LocalDeviceId, values);
			if(result != null)
			{
				if(logger.IsEnabled(LogLevel.Debug))
				{
					logger.LogDebug("Topic = {Topic} SensorType = {SensorType} CreationDate = {CreationDate} LocalDeviceId = {LocalDeviceId}",
						newReadingTopic,
						result.SensorType,
						result.CreationDate,
						result.LocalDeviceId);
				}

				mqttClient.Publish(newReadingTopic,
					new NewReadingMessage()
					{
						DeviceId = await repository.GetDeviceIdFromLocalDeviceId(result.LocalDeviceId),
						SensorType = result.SensorType,
						CreationDate = result.CreationDate,
						Values = result.Values.ToDictionary(i => i.Name, i => i.Value)
					});
			}
			else
			{
				logger.LogWarning("Results is null");
			}
		}

		/// <summary>
		/// Adds the sensor entry asynchronous.
		/// </summary>
		/// <param name="sensorType">Type of the sensor.</param>
		/// <param name="values">The values.</param>
		/// <param name="manufacture">The manufacture.</param>
		/// <param name="manufactureId">The manufacture identifier.</param>
		/// <exception cref="NullReferenceException">Unable to create a device for some reason</exception>
		public async Task AddSensorEntryAsync(SensorTypes sensorType, Dictionary<string, double> values, string manufacture, string manufactureId)
		{
			var device = await repository.GetDeviceByManufactureIdAsync(manufacture, manufactureId);
			if(device is null)
			{
				if(logger.IsEnabled(LogLevel.Debug))
				{
					logger.LogDebug("Unknown Device with Manufacture {manufacture} ManufactureId {ManufactureId} Adding", manufacture, manufactureId);
				}

				device = await repository.AddDeviceByManufactureIdAsync(manufacture, manufactureId);
			}

			if(device is null)
			{
				logger.LogError($"Unable to create device for manufacture {manufacture} manufactureId {manufactureId}");
				throw new NullReferenceException("Unable to create a device for some reason");
			}

			if(device.DeviceId is null)
			{
				if(logger.IsEnabled(LogLevel.Debug))
				{
					logger.LogDebug("Unknown DeviceId for Manufacture = {manufacture} ManufactureId = {manufactureId", device.Manufacture, device.ManufactureId);
				}

				mqttClient.Publish(unknownDeviceTopic,
					new UnknownDeviceMessage()
					{
						Manufacture = manufacture,
						ManufactureId = manufactureId
					});
			}

			var result = await repository.AddSensorEntryAsync(sensorType, device.LocalDeviceId, values);
			if(result != null)
			{
				if(logger.IsEnabled(LogLevel.Debug))
				{
					logger.LogDebug("Topic = {Topic} SensorType = {SensorType} CreationDate = {CreationDate} LocalDeviceId = {LocalDeviceId}",
						newReadingTopic,
						result.SensorType,
						result.CreationDate,
						result.LocalDeviceId);
				}

				mqttClient.Publish(newReadingTopic,
					new NewReadingMessage()
					{
						DeviceId = await repository.GetDeviceIdFromLocalDeviceId(result.LocalDeviceId),
						SensorType = result.SensorType,
						CreationDate = result.CreationDate,
						Values = result.Values.ToDictionary(i => i.Name, i => i.Value)
					});
			}
			else
			{
				logger.LogWarning("Results is null");
			}
		}

		/// <summary>
		/// Updates the device information from message asynchronous.
		/// </summary>
		/// <param name="deviceMessage">The device message.</param>
		/// <returns></returns>
		public Task UpdateDeviceInformationFromMessageAsync(DeviceMessage deviceMessage)
			=> repository.UpdateDeviceInformationFromMessageAsync(deviceMessage ?? throw new ArgumentNullException(nameof(deviceMessage)));
	}
}

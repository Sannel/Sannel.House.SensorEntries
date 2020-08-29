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

using Sannel.House.Base.Sensor;
using Sannel.House.SensorLogging.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sannel.House.SensorLogging.Interfaces
{
	public interface ISensorService
	{
		/// <summary>
		/// Adds the sensor entry asynchronous.
		/// </summary>
		/// <param name="sensorType">Type of the sensor.</param>
		/// <param name="creationDate">The creation date.</param>
		/// <param name="values">The values.</param>
		/// <param name="deviceMacAddress">The device mac address.</param>
		/// <returns></returns>
		Task AddSensorEntryAsync(SensorTypes sensorType, DateTimeOffset creationDate, Dictionary<string, double> values, long deviceMacAddress);

		/// <summary>
		/// Adds the sensor entry asynchronous.
		/// </summary>
		/// <param name="sensorType">Type of the sensor.</param>
		/// <param name="creationDate">The creation date.</param>
		/// <param name="values">The values.</param>
		/// <param name="deviceUuid">The device UUID.</param>
		/// <returns></returns>
		Task AddSensorEntryAsync(SensorTypes sensorType, DateTimeOffset creationDate, Dictionary<string, double> values, Guid deviceUuid);
		/// <summary>
		/// Adds the sensor entry asynchronous.
		/// </summary>
		/// <param name="sensorType">Type of the sensor.</param>
		/// <param name="creationDate">The creation date.</param>
		/// <param name="values">The values.</param>
		/// <param name="manufacture">The manufacture.</param>
		/// <param name="manufactureId">The manufacture identifier.</param>
		/// <returns></returns>
		Task AddSensorEntryAsync(SensorTypes sensorType, DateTimeOffset creationDate, Dictionary<string, double> values, string manufacture, string manufactureId);

		/// <summary>
		/// Updates the device information from message asynchronous.
		/// </summary>
		/// <param name="deviceMessage">The device message.</param>
		/// <returns></returns>
		Task UpdateDeviceInformationFromMessageAsync(DeviceMessage deviceMessage);
	}
}

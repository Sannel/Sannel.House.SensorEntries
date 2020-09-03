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

using Sannel.House.Base.Sensor;
using Sannel.House.SensorLogging.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.SensorLogging.Interfaces
{
	public interface ISensorRepository
	{
		/// <summary>
		/// Adds the Sensor Entry asynchronous.
		/// </summary>
		/// <param name="sensorType">Type of the sensor.</param>
		/// <param name="localDeviceId">The local device identifier.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		Task<SensorEntry> AddSensorEntryAsync(SensorTypes sensorType, 
			Guid localDeviceId, 
			Dictionary<string, double> values);

		/// <summary>
		/// Gets the device by mac address.
		/// </summary>
		/// <param name="macAddress">The mac address.</param>
		/// <returns></returns>
		Task<Device> GetDeviceByMacAddressAsync(long macAddress);

		/// <summary>
		/// Adds the device by mac address.
		/// </summary>
		/// <param name="macAddress">The mac address.</param>
		/// <returns></returns>
		Task<Device> AddDeviceByMacAddressAsync(long macAddress);

		/// <summary>
		/// Gets the device by UUID.
		/// </summary>
		/// <param name="uuid">The UUID.</param>
		/// <returns></returns>
		Task<Device> GetDeviceByUuidAsync(Guid uuid);

		/// <summary>
		/// Adds the device by UUID.
		/// </summary>
		/// <param name="uuid">The UUID.</param>
		/// <returns></returns>
		Task<Device> AddDeviceByUuidAsync(Guid uuid);

		/// <summary>
		/// Gets the device by manufacture identifier.
		/// </summary>
		/// <param name="manufacture">The manufacture.</param>
		/// <param name="manufactureId">The manufacture identifier.</param>
		/// <returns></returns>
		Task<Device> GetDeviceByManufactureIdAsync(string manufacture, string manufactureId);
		/// <summary>
		/// Adds the device by manufacture identifier.
		/// </summary>
		/// <param name="manufacture">The manufacture.</param>
		/// <param name="manufactureId">The manufacture identifier.</param>
		/// <returns></returns>
		Task<Device> AddDeviceByManufactureIdAsync(string manufacture, string manufactureId);

		/// <summary>
		/// Updates the device identifier.
		/// </summary>
		/// <param name="localDeviceId">The local device identifier.</param>
		/// <param name="deviceId">The device identifier.</param>
		/// <returns></returns>
		Task<Device> UpdateDeviceIdAsync(Guid localDeviceId, int? deviceId);

		/// <summary>
		/// Gets the device identifier from local device identifier.
		/// </summary>
		/// <param name="localDeviceId">The local device identifier.</param>
		/// <returns></returns>
		Task<int?> GetDeviceIdFromLocalDeviceId(Guid localDeviceId);

		/// <summary>
		/// Updates the device information from message asynchronous.
		/// </summary>
		/// <param name="deviceMessage">The device message.</param>
		/// <returns></returns>
		Task UpdateDeviceInformationFromMessageAsync(DeviceMessage deviceMessage);
	}
}

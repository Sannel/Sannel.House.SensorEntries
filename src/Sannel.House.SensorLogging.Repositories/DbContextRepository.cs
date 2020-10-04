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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sannel.House.Base.Sensor;
using Sannel.House.SensorLogging.Data;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Models;
using System;
using System.Linq;
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
		private readonly SensorLoggingContext context;
		private readonly ILogger logger;

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
		/// Adds the device by mac address.
		/// </summary>
		/// <param name="macAddress">The mac address.</param>
		/// <returns></returns>
		public async Task<Device> AddDeviceByMacAddressAsync(long macAddress)
		{
			var result = await context.Devices.AddAsync(new Device()
			{
				LocalDeviceId = Guid.NewGuid(),
				MacAddress = macAddress
			});

			await context.SaveChangesAsync();
			result.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

			return result.Entity;
		}

		/// <summary>
		/// Adds the device by manufacture identifier.
		/// </summary>
		/// <param name="manufacture">The manufacture.</param>
		/// <param name="manufactureId">The manufacture identifier.</param>
		/// <returns></returns>
		public async Task<Device> AddDeviceByManufactureIdAsync(string manufacture, string manufactureId)
		{
			var result = await context.Devices.AddAsync(new Device()
			{
				LocalDeviceId = Guid.NewGuid(),
				Manufacture = manufacture,
				ManufactureId = manufactureId
			});

			await context.SaveChangesAsync();
			result.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

			return result.Entity;
		}

		/// <summary>
		/// Adds the device by UUID.
		/// </summary>
		/// <param name="uuid">The UUID.</param>
		/// <returns></returns>
		public async Task<Device> AddDeviceByUuidAsync(Guid uuid)
		{
			var result = await context.Devices.AddAsync(new Device()
			{
				LocalDeviceId = Guid.NewGuid(),
				Uuid = uuid
			});

			await context.SaveChangesAsync();
			result.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

			return result.Entity;
		}

		/// <summary>
		/// Adds the Sensor Entry asynchronous.
		/// </summary>
		/// <param name="sensorType">Type of the sensor.</param>
		/// <param name="localDeviceId">The local device identifier.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public async Task<SensorEntry> AddSensorEntryAsync(SensorTypes sensorType, Guid localDeviceId, Dictionary<string, double> values)
		{
			var entry = new SensorEntry()
			{
				SensorEntryId = Guid.NewGuid(),
				SensorType = sensorType,
				CreationDate = DateTimeOffset.Now,
				LocalDeviceId = localDeviceId,
				Values = new System.Collections.ObjectModel.Collection<SensorReading>()
			};

			foreach(var kvp in values)
			{
				entry.Values.Add(kvp);
			}

			var result = await context.SensorEntries.AddAsync(entry);

			await context.SaveChangesAsync();
			result.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

			return result.Entity;
		}

		/// <summary>
		/// Gets the device by mac address.
		/// </summary>
		/// <param name="macAddress">The mac address.</param>
		/// <returns></returns>
		public async Task<Device?> GetDeviceByMacAddressAsync(long macAddress) 
			=> await context.Devices.AsNoTracking().FirstOrDefaultAsync(i => i.MacAddress == macAddress);

		/// <summary>
		/// Gets the device by manufacture identifier.
		/// </summary>
		/// <param name="manufacture">The manufacture.</param>
		/// <param name="manufactureId">The manufacture identifier.</param>
		/// <returns></returns>
		public async Task<Device?> GetDeviceByManufactureIdAsync(string manufacture, string manufactureId)
			=> await context.Devices.AsNoTracking().FirstOrDefaultAsync(i => i.Manufacture == manufacture && i.ManufactureId == manufactureId);

		/// <summary>
		/// Gets the device by UUID.
		/// </summary>
		/// <param name="uuid">The UUID.</param>
		/// <returns></returns>
		public async Task<Device?> GetDeviceByUuidAsync(Guid uuid)
			=> await context.Devices.AsNoTracking().FirstOrDefaultAsync(i => i.Uuid == uuid);

		/// <summary>
		/// Gets the device identifier from local device identifier.
		/// </summary>
		/// <param name="localDeviceId">The local device identifier.</param>
		/// <returns></returns>
		public async Task<int?> GetDeviceIdFromLocalDeviceId(Guid localDeviceId)
		{
			var result = await context.Devices.AsNoTracking().FirstOrDefaultAsync(i => i.LocalDeviceId == localDeviceId);
			return result?.DeviceId;
		}

		/// <summary>
		/// Updates the device identifier.
		/// </summary>
		/// <param name="localDeviceId">The local device identifier.</param>
		/// <param name="deviceId">The device identifier.</param>
		/// <returns></returns>
		public async Task<Device?> UpdateDeviceIdAsync(Guid localDeviceId, int? deviceId)
		{
			var device = await context.Devices.FirstOrDefaultAsync(i => i.LocalDeviceId == localDeviceId);
			if(device is null)
			{
				logger.LogInformation($"No device with localDeviceId {localDeviceId}");
				return null;
			}

			device.DeviceId = deviceId;

			await context.SaveChangesAsync();

			return await context.Devices.AsNoTracking().FirstOrDefaultAsync(i => i.LocalDeviceId == localDeviceId);

		}

		public async Task UpdateDeviceInformationFromMessageAsync(DeviceMessage deviceMessage)
		{
			if(deviceMessage is null)
			{
				throw new ArgumentNullException(nameof(deviceMessage));
			}

			// If there are no alternate ids return 
			if(deviceMessage.AlternateIds is null || deviceMessage.AlternateIds.Count <= 0)
			{
				return;
			}

			foreach(var altId in deviceMessage.AlternateIds)
			{
				if(altId is null)
				{
					continue;
				}
				if(altId.MacAddress.HasValue 
					|| altId.Uuid.HasValue
					|| (!string.IsNullOrWhiteSpace(altId.Manufacture)
						&& !string.IsNullOrWhiteSpace(altId.ManufactureId)))
				{
					var query = context.Devices.Where(i =>
						(i.MacAddress == altId.MacAddress &&
						i.MacAddress != null) ||
						(i.Uuid == altId.Uuid &&
						i.Uuid != null) ||
						(i.Manufacture == altId.Manufacture &&
						i.Manufacture != null &&
						i.ManufactureId == altId.ManufactureId &&
						i.ManufactureId != null)
					);
					if (await query.AnyAsync())
					{
						await query.UpdateFromQueryAsync(i => new Device() { DeviceId = deviceMessage.DeviceId });
					}
					else
					{
						var device = new Device()
						{
							LocalDeviceId = Guid.NewGuid(),
							DeviceId = deviceMessage.DeviceId,
							MacAddress = altId.MacAddress,
							Uuid = altId.Uuid,
							Manufacture = altId.Manufacture,
							ManufactureId = altId.ManufactureId
						};

						await context.Devices.AddAsync(device);
						await context.SaveChangesAsync();
					}
				}
			}
		}
	}
}

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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sannel.House.Devices.Client;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Models;
using Sannel.House.SensorLogging.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sannel.House.SensorLogging.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SensorLoggingController : ControllerBase
	{
		private ISensorRepository repository;
		private DevicesClient client;
		private ILogger logger;

		public SensorLoggingController(ISensorRepository repository, DevicesClient client, ILogger<SensorLoggingController> logger)
		{
			this.repository = repository;
			this.logger = logger;
			this.client = client;
		}
		

		[HttpPost]
		[Authorize(Roles = "SensorReadingWrite,Admin")]
		public async Task<IActionResult> Post(SensorReading reading)
		{
			if(reading == null)
			{
				logger.LogError("Null reading sent");
				throw new ArgumentNullException(nameof(reading));
			}

			requestStart();

			var entry = new SensorEntry
			{
				SensorEntryId = Guid.NewGuid()
			};

			if (reading.DeviceId.HasValue)
			{
				var resp = await client.GetDeviceAsync(reading.DeviceId.Value);
				if(resp.Success)
				{
					entry.DeviceId = reading.DeviceId.Value;
				}
				else
				{
					logger.LogWarning("DeviceId {0} not found defaulting to default device", reading.DeviceId);
					entry.DeviceId = Sannel.House.Data.Defaults.DEFAULT_DEVICEID;
				}
			}
			else if(reading.DeviceMacAddress.HasValue)
			{
				var resp = await client.GetByMacAddressAsync(reading.DeviceMacAddress.Value);
				if(resp.Success)
				{
					entry.DeviceId = resp.Data.DeviceId;
				}
				else
				{
					logger.LogWarning("Device with Mac Address {0:X} not found defaulting to default device", reading.DeviceMacAddress);
					entry.DeviceId = Sannel.House.Data.Defaults.DEFAULT_DEVICEID;
				}
			}
			else if(reading.DeviceUuid.HasValue)
			{
				var resp = await client.GetByUuidAsync(reading.DeviceUuid.Value);
				if(resp.Success)
				{
					entry.DeviceId = resp.Data.DeviceId;
				}
				else
				{
					logger.LogWarning("Device with Uuid {0} not found defaulting to default device", reading.DeviceUuid);
					entry.DeviceId = Sannel.House.Data.Defaults.DEFAULT_DEVICEID;
				}
			}
			else if(!string.IsNullOrWhiteSpace(reading.Manufacture) && !string.IsNullOrWhiteSpace(reading.ManufactureId))
			{
				var resp = await client.GetByManufactureIdAsync(reading.Manufacture, reading.ManufactureId);
				if(resp.Success)
				{
					entry.DeviceId = resp.Data.DeviceId;
				}
				else
				{
					logger.LogWarning("Device with Manufacture {0} and ManufactureId {1} not found defaulting to default device", reading.Manufacture, reading.ManufactureId);
					entry.DeviceId = Sannel.House.Data.Defaults.DEFAULT_DEVICEID;
				}
			}
			else
			{
				logger.LogWarning("No valid device info passed defaulting to default device");
				entry.DeviceId = Sannel.House.Data.Defaults.DEFAULT_DEVICEID;
			}
			entry.CreationDate = reading.CreationDate;
			entry.SensorType = reading.SensorType;
			entry.Values = reading.Values;

			requestEnd();
			return Ok();
		}

		private void requestStart()
		{
			client.GetAuthenticationToken += this.Client_GetAuthenticationToken;
		}

		private void requestEnd()
		{
			client.GetAuthenticationToken -= this.Client_GetAuthenticationToken;
		}

		private void Client_GetAuthenticationToken(object sender, Client.AuthenticationTokenArgs e)
		{
			string auth = HttpContext.Request.Headers["Authorziation"];
			var segments = auth.Split(' ');
			if(segments?.Length == 2)
			{
				e.Token = segments[1];
			}
		}
	}
}

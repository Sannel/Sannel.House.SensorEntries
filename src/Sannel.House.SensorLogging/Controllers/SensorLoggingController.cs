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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sannel.House.Base.Models;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Models;
using Sannel.House.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Sannel.House.Base.Sensor;
using System.ComponentModel.DataAnnotations;
using Sannel.House.SensorLogging.ViewModel;

namespace Sannel.House.SensorLogging.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SensorLoggingController : ControllerBase
	{
		private readonly ISensorService service;
		private readonly ILogger logger;

		public SensorLoggingController(ISensorService service, ILogger<SensorLoggingController> logger)
		{
			this.service = service;
			this.logger = logger;
		}
		

		[HttpPost(nameof(AddWithMacAddress))]
		[Authorize(Roles = "SensorStoreWrite,Admin")]
		[ProducesResponseType(200, Type = typeof(ResponseModel))]
		[ProducesResponseType(400, Type = typeof(ErrorResponseModel))]
		public async Task<IActionResult> AddWithMacAddress([Required][FromBody]MacAddressReading reading)
		{
			if (ModelState.IsValid)
			{
				await service.AddSensorEntryAsync(reading.SensorType, DateTimeOffset.Now, reading.Values, reading.MacAddress);

				return Ok(new ResponseModel(HttpStatusCode.OK));
			}
			else
			{
				logger.LogInformation($"{nameof(AddWithMacAddress)}: Invalid Model for MacAddress {reading.MacAddress}");
				return BadRequest(new ErrorResponseModel(HttpStatusCode.BadRequest, "Invalid Model").FillWithStateDictionary(ModelState));
			}
		}

		[HttpPost(nameof(AddWithUuid))]
		[Authorize(Roles = "SensorStoreWrite,Admin")]
		[ProducesResponseType(200, Type = typeof(ResponseModel))]
		[ProducesResponseType(400, Type = typeof(ErrorResponseModel))]
		public async Task<IActionResult> AddWithUuid([Required][FromBody]UuidReading reading)
		{
			if (ModelState.IsValid)
			{
				await service.AddSensorEntryAsync(reading.SensorType, DateTimeOffset.Now, reading.Values, reading.Uuid);

				return Ok(new ResponseModel(HttpStatusCode.OK));
			}
			else
			{
				logger.LogInformation($"{nameof(AddWithUuid)}: Invalid Model for Uuid {reading.Uuid}");
				return BadRequest(new ErrorResponseModel(HttpStatusCode.BadRequest, "Invalid Model").FillWithStateDictionary(ModelState));
			}
		}


		[HttpPost(nameof(AddWithManufactureId))]
		[Authorize(Roles = "SensorStoreWrite,Admin")]
		[ProducesResponseType(200, Type = typeof(ResponseModel))]
		[ProducesResponseType(400, Type = typeof(ErrorResponseModel))]
		public async Task<IActionResult> AddWithManufactureId([Required][FromBody]ManufactureIdReading reading)
		{
			if (ModelState.IsValid)
			{
				await service.AddSensorEntryAsync(reading.SensorType, DateTimeOffset.Now, reading.Values, reading.Manufacture, reading.ManufactureId);

				return Ok(new ResponseModel(HttpStatusCode.OK));
			}
			else
			{
				logger.LogInformation($"{nameof(AddWithManufactureId)}: Invalid Model for Manufacture {reading.Manufacture} ManufactureId {reading.ManufactureId}");
				return BadRequest(new ErrorResponseModel(HttpStatusCode.BadRequest, "Invalid Model").FillWithStateDictionary(ModelState));
			}
		}
	}
}

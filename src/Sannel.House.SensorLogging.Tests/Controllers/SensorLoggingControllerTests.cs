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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sannel.House.SensorLogging.Controllers;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.ViewModel;
using Sannel.House.SensorLogging.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using Moq.Protected;
using System.Threading;
using Newtonsoft.Json;
using Sannel.House.SensorLogging.Models;
using Sannel.House.Base.Models;
using Sannel.House.Base.Sensor;

namespace Sannel.House.SensorLogging.Tests.Controllers
{
	public class SensorLoggingControllerTests : BaseTest
	{
		[Fact]
		public async Task AddWithMacAddressTest()
		{
			var service = new Mock<ISensorService>();
			var controller = new SensorLoggingController(service.Object, 
				CreateLogger<SensorLoggingController>());

			var model = new MacAddressReading()
			{
				MacAddress = (long)Math.Truncate(Random.NextDouble() * int.MaxValue),
				SensorType = Base.Sensor.SensorTypes.Pressure,
				Values = new Dictionary<string, double>()
				{
					{"Value1", Random.NextDouble() },
					{"Value2", Random.NextDouble() }
				}
			};

			var addSensorEntryCallback = 0;
			service.Setup(i => i.AddSensorEntryAsync(It.IsAny<SensorTypes>(), It.IsAny<Dictionary<string, double>>(),
				It.IsAny<long>()))
				.Callback((SensorTypes type, Dictionary<string, double> values, long macAddress) =>
				{
					addSensorEntryCallback++;
					Assert.Equal(model.SensorType, type);
					Assert.Equal(model.MacAddress, macAddress);
					Assert.Equal(model.Values.Count, values.Count);

					foreach(var kv in values)
					{
						Assert.True(model.Values.ContainsKey(kv.Key));
						Assert.Equal(model.Values[kv.Key], kv.Value);
					}
				});

			var result = await controller.AddWithMacAddress(model);
			var okObject = Assert.IsAssignableFrom<OkObjectResult>(result);
			Assert.Equal(200, okObject.StatusCode);
			var response = Assert.IsAssignableFrom<ResponseModel>(okObject.Value);
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

			controller.ModelState.AddModelError("test", "test");
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			result = await controller.AddWithMacAddress(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
			var badRequest = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
			Assert.Equal(400, badRequest.StatusCode);
			var error = Assert.IsAssignableFrom<ErrorResponseModel>(badRequest.Value);
			Assert.Equal("Invalid Model", error.Title);
			Assert.Equal(1, addSensorEntryCallback);
		}

		[Fact]
		public async Task AddWithUuidTest()
		{
			var service = new Mock<ISensorService>();
			var controller = new SensorLoggingController(service.Object, 
				CreateLogger<SensorLoggingController>());

			var model = new UuidReading()
			{
				Uuid = Guid.NewGuid(),
				SensorType = Base.Sensor.SensorTypes.Lux,
				Values = new Dictionary<string, double>()
				{
					{"Value1", Random.NextDouble() },
					{"Value2", Random.NextDouble() }
				}
			};

			var addSensorEntryCallback = 0;
			service.Setup(i => i.AddSensorEntryAsync(It.IsAny<SensorTypes>(), It.IsAny<Dictionary<string, double>>(),
				It.IsAny<Guid>()))
				.Callback((SensorTypes type, Dictionary<string, double> values, Guid uuid) =>
				{
					addSensorEntryCallback++;
					Assert.Equal(model.SensorType, type);
					Assert.Equal(model.Uuid, uuid);
					Assert.Equal(model.Values.Count, values.Count);

					foreach(var kv in values)
					{
						Assert.True(model.Values.ContainsKey(kv.Key));
						Assert.Equal(model.Values[kv.Key], kv.Value);
					}
				});

			var result = await controller.AddWithUuid(model);
			var okObject = Assert.IsAssignableFrom<OkObjectResult>(result);
			Assert.Equal(200, okObject.StatusCode);
			var response = Assert.IsAssignableFrom<ResponseModel>(okObject.Value);
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

			controller.ModelState.AddModelError("test", "test");
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			result = await controller.AddWithMacAddress(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
			var badRequest = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
			Assert.Equal(400, badRequest.StatusCode);
			var error = Assert.IsAssignableFrom<ErrorResponseModel>(badRequest.Value);
			Assert.Equal("Invalid Model", error.Title);
			Assert.Equal(1, addSensorEntryCallback);
		}

		[Fact]
		public async Task AddWithManufactureTest()
		{
			var service = new Mock<ISensorService>();
			var controller = new SensorLoggingController(service.Object, 
				CreateLogger<SensorLoggingController>());

			var model = new ManufactureIdReading()
			{
				Manufacture = Guid.NewGuid().ToString(),
				ManufactureId = Guid.NewGuid().ToString(),
				SensorType = Base.Sensor.SensorTypes.Rain,
				Values = new Dictionary<string, double>()
				{
					{"Value1", Random.NextDouble() },
					{"Value2", Random.NextDouble() }
				}
			};

			var addSensorEntryCallback = 0;
			service.Setup(i => i.AddSensorEntryAsync(It.IsAny<SensorTypes>(), It.IsAny<Dictionary<string, double>>(),
				It.IsAny<string>(),
				It.IsAny<string>()))
				.Callback((SensorTypes type, Dictionary<string, double> values, string manufacture, string manufactureId) =>
				{
					addSensorEntryCallback++;
					Assert.Equal(model.SensorType, type);
					Assert.Equal(model.Manufacture, manufacture);
					Assert.Equal(model.ManufactureId, manufactureId);
					Assert.Equal(model.Values.Count, values.Count);

					foreach(var kv in values)
					{
						Assert.True(model.Values.ContainsKey(kv.Key));
						Assert.Equal(model.Values[kv.Key], kv.Value);
					}
				});

			var result = await controller.AddWithManufactureId(model);
			var okObject = Assert.IsAssignableFrom<OkObjectResult>(result);
			Assert.Equal(200, okObject.StatusCode);
			var response = Assert.IsAssignableFrom<ResponseModel>(okObject.Value);
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

			controller.ModelState.AddModelError("test", "test");
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			result = await controller.AddWithMacAddress(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
			var badRequest = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
			Assert.Equal(400, badRequest.StatusCode);
			var error = Assert.IsAssignableFrom<ErrorResponseModel>(badRequest.Value);
			Assert.Equal("Invalid Model", error.Title);
			Assert.Equal(1, addSensorEntryCallback);
		}
	}
}

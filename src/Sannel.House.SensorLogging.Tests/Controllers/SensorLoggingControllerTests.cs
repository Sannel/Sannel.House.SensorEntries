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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sannel.House.Devices.Client;
using Sannel.House.Models;
using Sannel.House.SensorLogging.Controllers;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.ViewModel;
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
using Results_Device = Sannel.House.Client.Results<Sannel.House.Devices.Client.Device>;

namespace Sannel.House.SensorLogging.Tests.Controllers
{
	public class SensorLoggingControllerTests : BaseTest
	{
		private (Mock<ISensorRepository> repo,
			Mock<IHttpClientFactory> factory,
			Mock<DevicesClient> client)
			getCommon()
		{
			var repo = new Mock<ISensorRepository>();
			var factory = new Mock<IHttpClientFactory>();
			var messageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<DevicesClient>>();


			factory.Setup(i => i.CreateClient(nameof(Sannel.House.Devices.Client.DevicesClient)))
				.Returns(() => new HttpClient(messageHandler.Object));

			var client = new Mock<DevicesClient>(factory.Object, logger.Object);

			return (repo, factory, client);

		}



		[Fact]
		public async Task PostTestException()
		{
			var (repo, factory, client) = getCommon();
			var logger = this.CreateLogger<SensorLoggingController>();
			var controller = new SensorLoggingController(repo.Object, client.Object, logger);

			await Assert.ThrowsAsync<ArgumentNullException>(() => controller.Post(null));

		}


		[Fact]
		public async Task PostModelError()
		{
			var (repo, factory, client) = getCommon();
			var logger = this.CreateLogger<SensorLoggingController>();
			var controller = new SensorLoggingController(repo.Object, client.Object, logger);

			// Model State Error
			controller.ModelState.AddModelError("error", "errorMessage");

			var sensorReading = new SensorReading
			{
				DeviceId = 2,
				CreationDate = DateTime.Now,
				SensorType = Sensor.SensorTypes.Pressure,
				Values = new Dictionary<string, double>()
				{
					{ "current", 30.2d }
				}
			};

			var result = await controller.Post(sensorReading);
			var bror = Assert.IsAssignableFrom<BadRequestObjectResult>(result.Result);
			Assert.Equal(400, bror.StatusCode);
			var v = Assert.IsAssignableFrom<ErrorResponseModel>(bror.Value);
			Assert.NotNull(v);
			Assert.Equal("Invalid Model", v.Title);
			Assert.Single(v.Errors);
			var e = v.Errors.First();
			Assert.Equal("error", e.Key);
			Assert.Equal("errorMessage", string.Join(" ", e.Value));
		}

		[Theory]
		[ClassData(typeof(SensorReadingsGenerator))]
		public async Task PostTest(SensorReading sensorReading)
		{
			var (repo, factory, client) = getCommon();
			var logger = this.CreateLogger<SensorLoggingController>();
			var controller = new SensorLoggingController(repo.Object, client.Object, logger);

			var deviceId = sensorReading.DeviceId ?? 340;

			Func<Results_Device> ret = () => new Results_Device()
			{
				Success = false,
				Status = 200,
				Data = new Device()
				{
					DeviceId = deviceId
				}
			};

			if (sensorReading.DeviceId.HasValue)
			{
				client
					.Setup(i => i.GetDeviceAsync(sensorReading.DeviceId.Value))
					.ReturnsAsync(ret);
			}
			else if(sensorReading.DeviceMacAddress.HasValue)
			{
				client
					.Setup(i => i.GetByMacAddressAsync(sensorReading.DeviceMacAddress.Value))
					.ReturnsAsync(ret);
			}
			else if(sensorReading.DeviceUuid.HasValue)
			{
				client.Setup(i => i.GetByUuidAsync(sensorReading.DeviceUuid.Value))
					.ReturnsAsync(ret);
			}
			else if(!string.IsNullOrWhiteSpace(sensorReading.Manufacture) && !string.IsNullOrWhiteSpace(sensorReading.ManufactureId))
			{
				client
					.Setup(i => i.GetByManufactureIdAsync(sensorReading.Manufacture, sensorReading.ManufactureId))
					.ReturnsAsync(ret);
			}

			repo.Setup(i => i.AddSensorEntryAsync(It.IsAny<SensorEntry>()))
				.Callback<SensorEntry>(i =>
			{
				Assert.NotNull(i);
				Assert.Equal(sensorReading.CreationDate, i.CreationDate);
				Assert.Equal(sensorReading.SensorType, i.SensorType);
				Assert.Equal(sensorReading.Values.Count, i.Values.Count);

				foreach(var key in sensorReading.Values.Keys)
				{
					Assert.True(i.Values.ContainsKey(key));
					Assert.Equal(sensorReading.Values[key], i.Values[key]);
				}
			}).Returns(Task.Run(()=> { }));

			var result = await controller.Post(sensorReading);
			var okor = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
			Assert.Equal(200, okor.StatusCode);
			var responseModel = Assert.IsAssignableFrom<Sannel.House.Models.ResponseModel<Guid>>(okor.Value);
			Assert.NotNull(responseModel);
			Assert.NotEqual(Guid.Empty, responseModel.Data);

			ret = () => new Results_Device()
			{
				Success = true,
				Status = 200,
				Data = new Device()
				{
					DeviceId = deviceId
				}
			};

			if (sensorReading.DeviceId.HasValue)
			{
				client
					.Setup(i => i.GetDeviceAsync(sensorReading.DeviceId.Value))
					.ReturnsAsync(ret);
			}
			else if(sensorReading.DeviceMacAddress.HasValue)
			{
				client
					.Setup(i => i.GetByMacAddressAsync(sensorReading.DeviceMacAddress.Value))
					.ReturnsAsync(ret);
			}
			else if(sensorReading.DeviceUuid.HasValue)
			{
				client.Setup(i => i.GetByUuidAsync(sensorReading.DeviceUuid.Value))
					.ReturnsAsync(ret);
			}
			else if(!string.IsNullOrWhiteSpace(sensorReading.Manufacture) && !string.IsNullOrWhiteSpace(sensorReading.ManufactureId))
			{
				client
					.Setup(i => i.GetByManufactureIdAsync(sensorReading.Manufacture, sensorReading.ManufactureId))
					.ReturnsAsync(ret);
			}

			result = await controller.Post(sensorReading);
			okor = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
			Assert.Equal(200, okor.StatusCode);
			responseModel = Assert.IsAssignableFrom<Sannel.House.Models.ResponseModel<Guid>>(okor.Value);
			Assert.NotNull(responseModel);
			Assert.NotEqual(Guid.Empty, responseModel.Data);
		}

	}
}

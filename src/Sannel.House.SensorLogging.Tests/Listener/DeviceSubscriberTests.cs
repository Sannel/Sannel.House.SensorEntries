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
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sannel.House.Base.Sensor;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Listener;
using Sannel.House.SensorLogging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Sannel.House.SensorLogging.Tests.Listener
{
	public class DeviceSubscriberTests : BaseTest
	{
		[Fact]
		public async Task MessageTestAsync()
		{
			var sensorService = new Mock<ISensorService>();

			var collection = new ServiceCollection();
			collection.AddSingleton(sensorService.Object);

			var config = new Mock<IConfiguration>();
			var subscriber = new DeviceSubscriber(collection.BuildServiceProvider(),
				CreateLogger<DeviceSubscriber>(),
				config.Object);

			var updateDeviceInformationCalled = 0;
			var message = new DeviceMessage()
			{
				DeviceId = 2,
				DateCreated = DateTimeOffset.Now,
				AlternateIds = new List<AlternateIdMessage>()
				{
					new AlternateIdMessage()
					{
						MacAddress = (long)Math.Truncate(random.NextDouble() * int.MaxValue),
						DateCreated = DateTimeOffset.Now
					}
				}
			};

			sensorService.Setup(i => i.UpdateDeviceInformationFromMessageAsync(It.IsAny<DeviceMessage>()))
				.Callback((DeviceMessage deviceMessage) =>
				{
					updateDeviceInformationCalled++;
					Assert.NotNull(deviceMessage);
					Assert.Equal(message.DeviceId, deviceMessage.DeviceId);
					Assert.Equal(message.DateCreated, deviceMessage.DateCreated);
					Assert.Single(deviceMessage.AlternateIds);
					var first = deviceMessage.AlternateIds.First();
					Assert.Equal(message.AlternateIds[0].MacAddress, first.MacAddress);
					Assert.Equal(message.AlternateIds[0].DateCreated, first.DateCreated);
				});


			await subscriber.MessageAsync(null, null);

			Assert.Equal(0, updateDeviceInformationCalled);

			await subscriber.MessageAsync("test/topic", JsonSerializer.Serialize(message));

			Assert.Equal(1, updateDeviceInformationCalled);
		}

		[Fact]
		public async Task InvalidJsonAsyncTest()
		{
			var sensorService = new Mock<ISensorService>();
			var collection = new ServiceCollection();
			collection.AddSingleton(sensorService.Object);

			var config = new Mock<IConfiguration>();
			var subscriber = new DeviceSubscriber(collection.BuildServiceProvider(),
				CreateLogger<DeviceSubscriber>(),
				config.Object);

			await subscriber.MessageAsync("test", "{");
		}
	}
}

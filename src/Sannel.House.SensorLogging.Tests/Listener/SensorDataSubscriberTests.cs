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
using Moq;
using Sannel.House.Base.Sensor;
using Sannel.House.SensorLogging.Interfaces;
using Sannel.House.SensorLogging.Listener;
using Sannel.House.SensorLogging.Listener.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Sannel.House.SensorLogging.Tests.Listener
{
	public class SensorDataSubscriberTests : BaseTest
	{
		[Fact]
		public async Task MessageTestAsync()
		{
			var sensorService = new Mock<ISensorService>();
			var config = new Mock<IConfiguration>();
			var subscriber = new SensorDataSubscriber(sensorService.Object,
				CreateLogger<SensorDataSubscriber>(),
				config.Object);

			var addSensorEntryMacAddressCalled = 0;
			var addSensorEntryUuidCalled = 0;
			var addSensorEntryManufactureIdCalled = 0;
			var reading = new FieldReading();

			sensorService.Setup(i => i.AddSensorEntryAsync(
				It.IsAny<SensorTypes>(),
				It.IsAny<Dictionary<string, double>>(),
				It.IsAny<long>()))
				.Callback((SensorTypes type,
				Dictionary<string, double> values,
				long macAddress) =>
				{
					addSensorEntryMacAddressCalled++;
					Assert.Equal(reading.MacAddress, macAddress);
					Assert.Equal(reading.SensorType, type);

					Assert.Equal(reading.Values.Count, values.Count);

					foreach(var kv in values)
					{
						Assert.True(reading.Values.ContainsKey(kv.Key));
						Assert.Equal(reading.Values[kv.Key], kv.Value);
					}
				});

			sensorService.Setup(i => i.AddSensorEntryAsync(
				It.IsAny<SensorTypes>(),
				It.IsAny<Dictionary<string, double>>(),
				It.IsAny<Guid>()))
				.Callback((SensorTypes type,
				Dictionary<string, double> values,
				Guid uuid) =>
				{
					addSensorEntryUuidCalled++;
					Assert.Equal(reading.Uuid, uuid);
					Assert.Equal(reading.SensorType, type);

					Assert.Equal(reading.Values.Count, values.Count);

					foreach(var kv in values)
					{
						Assert.True(reading.Values.ContainsKey(kv.Key));
						Assert.Equal(reading.Values[kv.Key], kv.Value);
					}
				});

			sensorService.Setup(i => i.AddSensorEntryAsync(
				It.IsAny<SensorTypes>(),
				It.IsAny<Dictionary<string, double>>(),
				It.IsAny<string>(),
				It.IsAny<string>()))
				.Callback((SensorTypes type,
				Dictionary<string, double> values,
				string manufacture,
				string manufactureId) =>
				{
					addSensorEntryManufactureIdCalled++;
					Assert.Equal(reading.Manufacture, manufacture);
					Assert.Equal(reading.ManufactureId, manufactureId);
					Assert.Equal(reading.SensorType, type);

					Assert.Equal(reading.Values.Count, values.Count);

					foreach(var kv in values)
					{
						Assert.True(reading.Values.ContainsKey(kv.Key));
						Assert.Equal(reading.Values[kv.Key], kv.Value);
					}
				});


			await subscriber.MessageAsync(null, null);

			Assert.Equal(0, addSensorEntryMacAddressCalled);
			Assert.Equal(0, addSensorEntryUuidCalled);
			Assert.Equal(0, addSensorEntryManufactureIdCalled);

			reading.MacAddress = (long)Math.Truncate(random.NextDouble() * int.MaxValue);
			reading.Values = new Dictionary<string, double>()
			{
				{ "Value1", random.NextDouble() },
				{ "Value2", random.NextDouble() }
			};
			reading.SensorType = SensorTypes.SoilMoisture;

			await subscriber.MessageAsync("test/topic", JsonSerializer.Serialize(reading));

			Assert.Equal(1, addSensorEntryMacAddressCalled);
			Assert.Equal(0, addSensorEntryUuidCalled);
			Assert.Equal(0, addSensorEntryManufactureIdCalled);

			addSensorEntryMacAddressCalled = 0;

			reading = new FieldReading()
			{
				Uuid = Guid.NewGuid(),
				SensorType = SensorTypes.WindDirection,
				Values = new Dictionary<string, double>()
				{
					{ "Value1", random.NextDouble() },
					{ "Value2", random.NextDouble() }
				}
			};


			await subscriber.MessageAsync("test/topic", JsonSerializer.Serialize(reading));

			Assert.Equal(0, addSensorEntryMacAddressCalled);
			Assert.Equal(1, addSensorEntryUuidCalled);
			Assert.Equal(0, addSensorEntryManufactureIdCalled);

			addSensorEntryUuidCalled = 0;

			reading = new FieldReading()
			{
				Manufacture = Guid.NewGuid().ToString(),
				ManufactureId = Guid.NewGuid().ToString(),
				SensorType = SensorTypes.WindSpeed,
				Values = new Dictionary<string, double>()
				{
					{ "Value1", random.NextDouble() },
					{ "Value2", random.NextDouble() }
				}
			};


			await subscriber.MessageAsync("test/topic", JsonSerializer.Serialize(reading));

			Assert.Equal(0, addSensorEntryMacAddressCalled);
			Assert.Equal(0, addSensorEntryUuidCalled);
			Assert.Equal(1, addSensorEntryManufactureIdCalled);

			addSensorEntryManufactureIdCalled = 0;

			reading = new FieldReading()
			{
				SensorType = SensorTypes.WindSpeed,
				Values = new Dictionary<string, double>()
				{
					{ "Value1", random.NextDouble() },
					{ "Value2", random.NextDouble() }
				}
			};


			await subscriber.MessageAsync("test/topic", JsonSerializer.Serialize(reading));

			Assert.Equal(0, addSensorEntryMacAddressCalled);
			Assert.Equal(0, addSensorEntryUuidCalled);
			Assert.Equal(0, addSensorEntryManufactureIdCalled);
		}

		[Fact]
		public async Task InvalidJsonAsyncTest()
		{
			var sensorService = new Mock<ISensorService>();
			var config = new Mock<IConfiguration>();
			var subscriber = new SensorDataSubscriber(sensorService.Object,
				CreateLogger<SensorDataSubscriber>(),
				config.Object);

			await subscriber.MessageAsync("test", "{");
		}
	}
}

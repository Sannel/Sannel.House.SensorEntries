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

using Sannel.House.Base.MQTT.Interfaces;
using Sannel.House.SensorLogging.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Sannel.House.SensorLogging.Services;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Threading.Tasks;
using Sannel.House.SensorLogging.Models;
using Sannel.House.Base.Sensor;
using System.Collections.ObjectModel;
using Xunit.Sdk;
using NuGet.Frameworks;
using System.Linq;

namespace Sannel.House.SensorLogging.Tests.Services
{
	public class SensorServiceTests : BaseTest
	{
		protected ISensorService CreateSensorService(
			Dictionary<string, string> configurationValues,
			out Mock<IMqttClientPublishService> mqttClient,
			out Mock<ISensorRepository> repository)
		{
			mqttClient = new Mock<IMqttClientPublishService>();
			repository = new Mock<ISensorRepository>();
			var builder = new ConfigurationBuilder();
			builder.AddInMemoryCollection(configurationValues);

			return new SensorService(mqttClient.Object,
				repository.Object,
				builder.Build(),
				CreateLogger<SensorService>());
		}

		[Fact]
		public void SensorServiceConstructorTest()
		{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			Assert.Throws<ArgumentNullException>("mqttClient", () => new SensorService(null, null, null, null));
			var mqttClient = new Mock<IMqttClientPublishService>();
			Assert.Throws<ArgumentNullException>("repository", () => new SensorService(mqttClient.Object, null, null, null));
			var repository = new Mock<ISensorRepository>();
			Assert.Throws<ArgumentNullException>("configuration", () => new SensorService(mqttClient.Object, repository.Object, null, null));
			var configuration = new ConfigurationBuilder().Build();
			Assert.Throws<ArgumentNullException>("logger", () => new SensorService(mqttClient.Object, repository.Object, configuration, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		}

		[Fact]
		public async Task AddSensorEntryByMacAddressTestAsync()
		{
			var unknownDeviceTopic = "test/unknown";
			var newReadingTopic = "test/reading";
			var service = CreateSensorService(new Dictionary<string, string>()
			{
				{"MQTT:NewReadingTopic", newReadingTopic },
				{"MQTT:UnknownDeviceTopic", unknownDeviceTopic }
			}, out var mqttClient,
			out var repository);

			var testMacAddress = (long)Math.Truncate(Random.NextDouble() * int.MaxValue);
			var testCreationDate = DateTimeOffset.Now;
			var getDeviceCalled = 0;
			var addDeviceCalled = 0;
			var addSensorEntryCalled = 0;
			var publishCalled = 0;
			repository.Setup(i => i.GetDeviceByMacAddressAsync(It.IsAny<long>()))
				.ReturnsAsync((long macAddress) =>
				{
					getDeviceCalled++;
					Assert.Equal(testMacAddress, macAddress);
					return null;
				});

			var testDevice = new Device()
			{
				LocalDeviceId = Guid.NewGuid(),
				MacAddress = testMacAddress
			};

			repository.Setup(i => i.AddDeviceByMacAddressAsync(It.IsAny<long>()))
				.ReturnsAsync((long macAddress) =>
				{
					addDeviceCalled++;
					Assert.Equal(testMacAddress, macAddress);
					return testDevice;
				});

			var testSensorType = (SensorTypes)Random.Next((int)SensorTypes.Unknown, (int)SensorTypes.SoilTemperature);
			var testValues = new Dictionary<string, double>()
			{
				{"Value1", Random.NextDouble() * 10000 },
				{"Value2", Random.NextDouble() * 10000 }
			};

			repository.Setup(i => i.AddSensorEntryAsync(It.IsAny<SensorTypes>(),
				It.IsAny<Guid>(),
				It.IsAny<Dictionary<string, double>>()))
				.ReturnsAsync((SensorTypes type, Guid localDeviceId, Dictionary<string, double> values) =>
				{
					addSensorEntryCalled++;
					Assert.Equal(testSensorType, type);
					Assert.Equal(testDevice.LocalDeviceId, localDeviceId);
					Assert.Equal(testValues.Count, values.Count);

					var collection = new Collection<SensorReading>();

					foreach (var value in values)
					{
						Assert.True(testValues.ContainsKey(value.Key));
						Assert.Equal(testValues[value.Key], value.Value);
						collection.Add(new SensorReading()
						{
							SensorReadingId = Guid.NewGuid(),
							Name = value.Key,
							Value = value.Value
						});
					}

					return new SensorEntry()
					{
						SensorEntryId = Guid.NewGuid(),
						LocalDeviceId = localDeviceId,
						CreationDate = testCreationDate,
						SensorType = type,
						Values = collection
					};
				});

			mqttClient.Setup(i => i.Publish(It.IsAny<string>(), It.IsAny<object>()))
				.Callback((string topic, object message) =>
				{
					publishCalled++;
					if (topic == unknownDeviceTopic)
					{
						var uMessage = Assert.IsAssignableFrom<UnknownDeviceMessage>(message);
						Assert.NotNull(uMessage);
						Assert.Null(uMessage.Manufacture);
						Assert.Null(uMessage.ManufactureId);
						Assert.Null(uMessage.Uuid);
						Assert.Equal(testMacAddress, uMessage.MacAddress);
					}
					else if (topic == newReadingTopic)
					{
						var nrMessage = Assert.IsAssignableFrom<NewReadingMessage>(message);
						Assert.NotNull(nrMessage);
						Assert.Null(nrMessage.DeviceId);
						Assert.Equal(testSensorType, nrMessage.SensorType);
						Assert.Equal(testCreationDate, nrMessage.CreationDate);
						Assert.Equal(testValues.Count, nrMessage.Values.Count);

						foreach(var value in nrMessage.Values)
						{
							Assert.True(testValues.ContainsKey(value.Key));
							Assert.Equal(testValues[value.Key], value.Value);
						}
					}
					else
					{
						throw new Exception($"unexpected topic {topic}");
					}
				});

			await service.AddSensorEntryAsync(testSensorType, 
				testValues, 
				testMacAddress);

			Assert.Equal(1, getDeviceCalled);
			Assert.Equal(1, addDeviceCalled);
			Assert.Equal(2, publishCalled);
			Assert.Equal(1, addSensorEntryCalled);
		}

		[Fact]
		public async Task AddSensorEntryByUuidTestAsync()
		{
			var unknownDeviceTopic = "test/unknown";
			var newReadingTopic = "test/reading";
			var service = CreateSensorService(new Dictionary<string, string>()
			{
				{"MQTT:NewReadingTopic", newReadingTopic },
				{"MQTT:UnknownDeviceTopic", unknownDeviceTopic }
			}, out var mqttClient,
			out var repository);

			var testUuid = Guid.NewGuid();
			var testCreationDate = DateTimeOffset.Now;
			var getDeviceCalled = 0;
			var addDeviceCalled = 0;
			var addSensorEntryCalled = 0;
			var publishCalled = 0;
			repository.Setup(i => i.GetDeviceByUuidAsync(It.IsAny<Guid>()))
				.ReturnsAsync((Guid uuid) =>
				{
					getDeviceCalled++;
					Assert.Equal(testUuid, uuid);
					return null;
				});

			var testDevice = new Device()
			{
				LocalDeviceId = Guid.NewGuid(),
				Uuid = testUuid
			};

			repository.Setup(i => i.AddDeviceByUuidAsync(It.IsAny<Guid>()))
				.ReturnsAsync((Guid uuid) =>
				{
					addDeviceCalled++;
					Assert.Equal(testUuid, uuid);
					return testDevice;
				});

			var testSensorType = (SensorTypes)Random.Next((int)SensorTypes.Unknown, (int)SensorTypes.SoilTemperature);
			var testValues = new Dictionary<string, double>()
			{
				{"Value1", Random.NextDouble() * 10000 },
				{"Value2", Random.NextDouble() * 10000 }
			};

			repository.Setup(i => i.AddSensorEntryAsync(It.IsAny<SensorTypes>(),
				It.IsAny<Guid>(),
				It.IsAny<Dictionary<string, double>>()))
				.ReturnsAsync((SensorTypes type, Guid localDeviceId, Dictionary<string, double> values) =>
				{
					addSensorEntryCalled++;
					Assert.Equal(testSensorType, type);
					Assert.Equal(testDevice.LocalDeviceId, localDeviceId);
					Assert.Equal(testValues.Count, values.Count);

					var collection = new Collection<SensorReading>();

					foreach (var value in values)
					{
						Assert.True(testValues.ContainsKey(value.Key));
						Assert.Equal(testValues[value.Key], value.Value);
						collection.Add(new SensorReading()
						{
							SensorReadingId = Guid.NewGuid(),
							Name = value.Key,
							Value = value.Value
						});
					}

					return new SensorEntry()
					{
						SensorEntryId = Guid.NewGuid(),
						LocalDeviceId = localDeviceId,
						CreationDate = testCreationDate,
						SensorType = type,
						Values = collection
					};
				});

			mqttClient.Setup(i => i.Publish(It.IsAny<string>(), It.IsAny<object>()))
				.Callback((string topic, object message) =>
				{
					publishCalled++;
					if (topic == unknownDeviceTopic)
					{
						var uMessage = Assert.IsAssignableFrom<UnknownDeviceMessage>(message);
						Assert.NotNull(uMessage);
						Assert.Null(uMessage.Manufacture);
						Assert.Null(uMessage.ManufactureId);
						Assert.Equal(testUuid, uMessage.Uuid);
						Assert.Null(uMessage.MacAddress);
					}
					else if (topic == newReadingTopic)
					{
						var nrMessage = Assert.IsAssignableFrom<NewReadingMessage>(message);
						Assert.NotNull(nrMessage);
						Assert.Null(nrMessage.DeviceId);
						Assert.Equal(testSensorType, nrMessage.SensorType);
						Assert.Equal(testCreationDate, nrMessage.CreationDate);
						Assert.Equal(testValues.Count, nrMessage.Values.Count);

						foreach(var value in nrMessage.Values)
						{
							Assert.True(testValues.ContainsKey(value.Key));
							Assert.Equal(testValues[value.Key], value.Value);
						}
					}
					else
					{
						throw new Exception($"unexpected topic {topic}");
					}
				});

			await service.AddSensorEntryAsync(testSensorType, 
				testValues, 
				testUuid);

			Assert.Equal(1, getDeviceCalled);
			Assert.Equal(1, addDeviceCalled);
			Assert.Equal(2, publishCalled);
			Assert.Equal(1, addSensorEntryCalled);
		}

		[Fact]
		public async Task AddSensorEntryByManufactureIdTestAsync()
		{
			var unknownDeviceTopic = "test/unknown";
			var newReadingTopic = "test/reading";
			var service = CreateSensorService(new Dictionary<string, string>()
			{
				{"MQTT:NewReadingTopic", newReadingTopic },
				{"MQTT:UnknownDeviceTopic", unknownDeviceTopic }
			}, out var mqttClient,
			out var repository);

			var testManufacture = Guid.NewGuid().ToString();
			var testManufactureId = Guid.NewGuid().ToString();
			var testCreationDate = DateTimeOffset.Now;
			var getDeviceCalled = 0;
			var addDeviceCalled = 0;
			var addSensorEntryCalled = 0;
			var publishCalled = 0;
			repository.Setup(i => i.GetDeviceByManufactureIdAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync((string manufacture, string manufactureId) =>
				{
					getDeviceCalled++;
					Assert.Equal(testManufacture, manufacture);
					Assert.Equal(testManufactureId, manufactureId);
					return null;
				});

			var testDevice = new Device()
			{
				LocalDeviceId = Guid.NewGuid(),
				Manufacture = testManufacture,
				ManufactureId = testManufactureId
			};

			repository.Setup(i => i.AddDeviceByManufactureIdAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync((string manufacture, string manufactureId) =>
				{
					addDeviceCalled++;
					Assert.Equal(testManufacture, manufacture);
					Assert.Equal(testManufactureId, manufactureId);
					return testDevice;
				});

			var testSensorType = (SensorTypes)Random.Next((int)SensorTypes.Unknown, (int)SensorTypes.SoilTemperature);
			var testValues = new Dictionary<string, double>()
			{
				{"Value1", Random.NextDouble() * 10000 },
				{"Value2", Random.NextDouble() * 10000 }
			};

			repository.Setup(i => i.AddSensorEntryAsync(It.IsAny<SensorTypes>(),
				It.IsAny<Guid>(),
				It.IsAny<Dictionary<string, double>>()))
				.ReturnsAsync((SensorTypes type, Guid localDeviceId, Dictionary<string, double> values) =>
				{
					addSensorEntryCalled++;
					Assert.Equal(testSensorType, type);
					Assert.Equal(testDevice.LocalDeviceId, localDeviceId);
					Assert.Equal(testValues.Count, values.Count);

					var collection = new Collection<SensorReading>();

					foreach (var value in values)
					{
						Assert.True(testValues.ContainsKey(value.Key));
						Assert.Equal(testValues[value.Key], value.Value);
						collection.Add(new SensorReading()
						{
							SensorReadingId = Guid.NewGuid(),
							Name = value.Key,
							Value = value.Value
						});
					}

					return new SensorEntry()
					{
						SensorEntryId = Guid.NewGuid(),
						LocalDeviceId = localDeviceId,
						CreationDate = testCreationDate,
						SensorType = type,
						Values = collection
					};
				});

			mqttClient.Setup(i => i.Publish(It.IsAny<string>(), It.IsAny<object>()))
				.Callback((string topic, object message) =>
				{
					publishCalled++;
					if (topic == unknownDeviceTopic)
					{
						var uMessage = Assert.IsAssignableFrom<UnknownDeviceMessage>(message);
						Assert.NotNull(uMessage);
						Assert.Equal(testManufacture, uMessage.Manufacture);
						Assert.Equal(testManufactureId, uMessage.ManufactureId);
						Assert.Null(uMessage.Uuid);
						Assert.Null(uMessage.MacAddress);
					}
					else if (topic == newReadingTopic)
					{
						var nrMessage = Assert.IsAssignableFrom<NewReadingMessage>(message);
						Assert.NotNull(nrMessage);
						Assert.Null(nrMessage.DeviceId);
						Assert.Equal(testSensorType, nrMessage.SensorType);
						Assert.Equal(testCreationDate, nrMessage.CreationDate);
						Assert.Equal(testValues.Count, nrMessage.Values.Count);

						foreach(var value in nrMessage.Values)
						{
							Assert.True(testValues.ContainsKey(value.Key));
							Assert.Equal(testValues[value.Key], value.Value);
						}
					}
					else
					{
						throw new Exception($"unexpected topic {topic}");
					}
				});

			await service.AddSensorEntryAsync(testSensorType, 
				testValues, 
				testManufacture,
				testManufactureId);

			Assert.Equal(1, getDeviceCalled);
			Assert.Equal(1, addDeviceCalled);
			Assert.Equal(2, publishCalled);
			Assert.Equal(1, addSensorEntryCalled);
		}

		[Fact]
		public async Task UpdateDeviceInformationFromMessageTestAsync()
		{
			var unknownDeviceTopic = "test/unknown";
			var newReadingTopic = "test/reading";
			var service = CreateSensorService(new Dictionary<string, string>()
			{
				{"MQTT:NewReadingTopic", newReadingTopic },
				{"MQTT:UnknownDeviceTopic", unknownDeviceTopic }
			}, out var mqttClient,
			out var repository);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			await Assert.ThrowsAsync<ArgumentNullException>("deviceMessage", async () => await service.UpdateDeviceInformationFromMessageAsync(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

			var testMessage = new DeviceMessage()
			{
				DeviceId = Random.Next(100, 30000),
				DateCreated = DateTimeOffset.Now,
				AlternateIds = new List<AlternateIdMessage>()
				{
					new AlternateIdMessage()
					{
						AlternateId = Random.Next(100, 40000),
						DateCreated = DateTimeOffset.Now.AddDays(1),
						MacAddress = (long)Math.Truncate(Random.NextDouble() * int.MaxValue),
						Uuid = Guid.NewGuid(),
						Manufacture = Guid.NewGuid().ToString(),
						ManufactureId = Guid.NewGuid().ToString()
					}
				}
			};

			var updateCalled = 0;
			repository.Setup(i => i.UpdateDeviceInformationFromMessageAsync(It.IsAny<DeviceMessage>()))
				.Callback((DeviceMessage message) =>
				Task.Run(() =>
				{
					updateCalled++;

					Assert.NotNull(message);
					Assert.Equal(testMessage.DeviceId, message.DeviceId);
					Assert.Equal(testMessage.DateCreated, message.DateCreated);
					Assert.Single(message.AlternateIds);
					var first = message.AlternateIds.First();
					Assert.Equal(testMessage.AlternateIds[0].AlternateId, first.AlternateId);
					Assert.Equal(testMessage.AlternateIds[0].DateCreated, first.DateCreated);
					Assert.Equal(testMessage.AlternateIds[0].MacAddress, first.MacAddress);
					Assert.Equal(testMessage.AlternateIds[0].Uuid, first.Uuid);
					Assert.Equal(testMessage.AlternateIds[0].Manufacture, first.Manufacture);
					Assert.Equal(testMessage.AlternateIds[0].ManufactureId, first.ManufactureId);
				}));

			await service.UpdateDeviceInformationFromMessageAsync(testMessage);
			await Task.Delay(500);
			Assert.Equal(1, updateCalled);
		}
	}
}

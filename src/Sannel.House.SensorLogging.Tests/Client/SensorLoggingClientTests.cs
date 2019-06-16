using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Sannel.House.Sensor;
using Sannel.House.SensorLogging.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sannel.House.SensorLogging.Tests.Client
{
	public class SensorLoggingClientTests : BaseTest
	{
		[Fact]
		public async Task LogReadingAsyncTest()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var deviceId = 1;
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceId=1,
					SensorType=type,
					CreationDate=dateTime,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			await Assert.ThrowsAsync<ArgumentNullException>("values", () => sensorLoggingClient.LogReadingAsync(deviceId,
				type,
				dateTime,
				(KeyValuePair<string,double>[])null));

			await Assert.ThrowsAsync<ArgumentOutOfRangeException>("values", () => sensorLoggingClient.LogReadingAsync(deviceId,
				type,
				dateTime,new KeyValuePair<string, double>[0]));

			var result = await sensorLoggingClient.LogReadingAsync(deviceId, 
				type, 
				dateTime, 
				new KeyValuePair<string, double>("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsync2Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var deviceId = 1;
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				dynamic d = JsonConvert.DeserializeObject(s);

				var n = DateTimeOffset.Now;
				Assert.True(d.CreationDate > dateTime && d.CreationDate < n, "date out of range");
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceId=1,
					SensorType=type,
					d.CreationDate,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			await Assert.ThrowsAsync<ArgumentNullException>("values", () => sensorLoggingClient.LogReadingAsync(deviceId,
				type,
				(KeyValuePair<string,double>[])null));

			await Assert.ThrowsAsync<ArgumentOutOfRangeException>("values", () => sensorLoggingClient.LogReadingAsync(deviceId,
				type,
				new KeyValuePair<string, double>[0]));

			var result = await sensorLoggingClient.LogReadingAsync(deviceId,
				type,
				new KeyValuePair<string, double>("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsync3Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var deviceId = 1;
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceId=1,
					SensorType=type,
					CreationDate=dateTime,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			await Assert.ThrowsAsync<ArgumentNullException>("values", () => sensorLoggingClient.LogReadingAsync(deviceId,
				type,
				dateTime,
				((string, double)[])null));

			await Assert.ThrowsAsync<ArgumentOutOfRangeException>("values", () => sensorLoggingClient.LogReadingAsync(deviceId,
				type,
				dateTime,
				(new (string, double)[0])));

			var result = await sensorLoggingClient.LogReadingAsync(deviceId, 
				type, 
				dateTime, 
				("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsync4Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var deviceId = 1;
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				dynamic d = JsonConvert.DeserializeObject(s);

				var n = DateTimeOffset.Now;
				Assert.True(d.CreationDate > dateTime && d.CreationDate < n, "date out of range");
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceId=1,
					SensorType=type,
					d.CreationDate,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			await Assert.ThrowsAsync<ArgumentNullException>("values", () => sensorLoggingClient.LogReadingAsync(deviceId,
				type,
				((string, double)[])null));

			await Assert.ThrowsAsync<ArgumentOutOfRangeException>("values", () => sensorLoggingClient.LogReadingAsync(deviceId,
				type,
				(new (string, double)[0])));

			var result = await sensorLoggingClient.LogReadingAsync(deviceId, 
				type, 
				("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsync5Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var macAddress = 1245421L;
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceMacAddress=macAddress,
					SensorType=type,
					CreationDate=dateTime,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(macAddress, 
				type, 
				dateTime, 
				new KeyValuePair<string, double>("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsync5bTest()
		{
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};


			var sensorLoggingClient = new SensorLoggingClient(httpClient, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var macAddress = 1245421L;
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceMacAddress=macAddress,
					SensorType=type,
					CreationDate=dateTime,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(macAddress, 
				type, 
				dateTime, 
				new KeyValuePair<string, double>("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsync6Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var macAddress = 1245421L;
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				dynamic d = JsonConvert.DeserializeObject(s);

				var n = DateTimeOffset.Now;
				Assert.True(d.CreationDate > dateTime && d.CreationDate < n, "date out of range");
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceMacAddress=macAddress,
					SensorType=type,
					CreationDate=d.CreationDate,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(macAddress, 
				type, 
				new KeyValuePair<string, double>("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsync7Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var macAddress = 1245421L;
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceMacAddress=macAddress,
					SensorType=type,
					CreationDate=dateTime,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(macAddress, 
				type, 
				dateTime, 
				("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsync8Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var macAddress = 1245421L;
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				dynamic d = JsonConvert.DeserializeObject(s);

				var n = DateTimeOffset.Now;
				Assert.True(d.CreationDate > dateTime && d.CreationDate < n, "date out of range");
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceMacAddress=macAddress,
					SensorType=type,
					CreationDate=d.CreationDate,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(macAddress, 
				type, 
				("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsyncDeviceUuid1Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var deviceUuid = Guid.NewGuid();
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceUuid=deviceUuid,
					SensorType=type,
					CreationDate=dateTime,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(deviceUuid, 
				type, 
				dateTime, 
				new KeyValuePair<string, double>("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsyncDeviceUuid2Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var deviceUuid = Guid.NewGuid();
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				dynamic d = JsonConvert.DeserializeObject(s);

				var n = DateTimeOffset.Now;
				Assert.True(d.CreationDate > dateTime && d.CreationDate < n, "date out of range");
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceUuid=deviceUuid,
					SensorType=type,
					CreationDate=d.CreationDate,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(deviceUuid, 
				type, 
				new KeyValuePair<string, double>("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsyncDeviceUuid3Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var deviceUuid = Guid.NewGuid();
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceUuid=deviceUuid,
					SensorType=type,
					CreationDate=dateTime,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(deviceUuid, 
				type, 
				dateTime, 
				("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsyncDeviceUuid4Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var deviceUuid = Guid.NewGuid();
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				dynamic d = JsonConvert.DeserializeObject(s);

				var n = DateTimeOffset.Now;
				Assert.True(d.CreationDate > dateTime && d.CreationDate < n, "date out of range");
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					DeviceUuid=deviceUuid,
					SensorType=type,
					CreationDate=d.CreationDate,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(deviceUuid, 
				type, 
				("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsyncManufactureId1Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var manufacture = "testman";
			var manufactureId = "testid";
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					Manufacture = manufacture,
					ManufactureId = manufactureId,
					SensorType=type,
					CreationDate=dateTime,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(manufacture,
				manufactureId,
				type, 
				dateTime, 
				new KeyValuePair<string, double>("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsyncManufactureId2Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var manufacture = "testman";
			var manufactureId = "testid";
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				dynamic d = JsonConvert.DeserializeObject(s);

				var n = DateTimeOffset.Now;
				Assert.True(d.CreationDate > dateTime && d.CreationDate < n, "date out of range");
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					Manufacture = manufacture,
					ManufactureId = manufactureId,
					SensorType=type,
					CreationDate=d.CreationDate,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(manufacture,
				manufactureId,
				type, 
				new KeyValuePair<string, double>("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsyncManufactureId3Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var manufacture = "testman";
			var manufactureId = "testid";
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					Manufacture = manufacture,
					ManufactureId = manufactureId,
					SensorType=type,
					CreationDate=dateTime,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(manufacture,
				manufactureId,
				type, 
				dateTime, 
				("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsyncManufactureId4Test()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var manufacture = "testman";
			var manufactureId = "testid";
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				dynamic d = JsonConvert.DeserializeObject(s);

				var n = DateTimeOffset.Now;
				Assert.True(d.CreationDate > dateTime && d.CreationDate < n, "date out of range");
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					Manufacture = manufacture,
					ManufactureId = manufactureId,
					SensorType=type,
					CreationDate=d.CreationDate,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(manufacture,
				manufactureId,
				type, 
				("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}

		[Fact]
		public async Task LogReadingAsyncObjectTest()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());
			var token = "test123";
			sensorLoggingClient.AuthToken = token;

			var manufacture = "testman";
			var manufactureId = "testid";
			var type = SensorTypes.Humidity;
			var dateTime = DateTimeOffset.Now;
			var reading = 5.3;
			var readingId = Guid.NewGuid();

			client.Protected().Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			).ReturnsAsync((HttpRequestMessage r, CancellationToken c) =>
			{
				Assert.Equal(HttpMethod.Post, r.Method);
				Assert.Equal(new Uri("https://gateway.dev.local/api/v1/SensorLogging"), r.RequestUri);
				Assert.NotNull(r.Headers.Authorization);
				Assert.Equal("Bearer", r.Headers.Authorization.Scheme);
				Assert.Equal(token, r.Headers.Authorization.Parameter);

				var s = r.Content.ReadAsStringAsync().Result;
				dynamic d = JsonConvert.DeserializeObject(s);

				var n = DateTimeOffset.Now;
				Assert.True(d.CreationDate > dateTime && d.CreationDate < n, "date out of range");
				/*{"DeviceId":1,
				 * "DeviceMacAddress":null,
				 * "DeviceUuid":null,
				 * "Manufacture":null,
				 * "ManufactureId":null,
				 * "SensorType":2,
				 * "CreationDate":"2019-05-27T23:02:19.6626549-06:00",
				 * "Values":
				 *    {"value1":5.3}}*/
				Assert.Equal(JsonConvert.SerializeObject(new {
					Manufacture = manufacture,
					ManufactureId = manufactureId,
					SensorType=type,
					d.CreationDate,
					Values = new 
					{
						value1=reading
					}
				}),
					s);

				return new HttpResponseMessage()
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Content = new StringContent($"{{ 'success':true,status: 200,'Data':'{readingId}'}}")
				};
			}).Verifiable();

			var result = await sensorLoggingClient.LogReadingAsync(new SensorReading()
			{
				Manufacture = manufacture,
				ManufactureId = manufactureId,
				SensorType = type,
				Values = new Dictionary<string, double>()
				{
					{"value1", reading }
				}
			});

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);
		}

		[Fact]
		public async Task LogReadingAsyncObjectExceptionsTest()
		{
			var clientFactory = new Mock<IHttpClientFactory>();
			var client = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var httpClient = new HttpClient(client.Object)
			{
				BaseAddress = new Uri("https://gateway.dev.local/api/v1/")
			};
			clientFactory.Setup(i => i.CreateClient(nameof(SensorLoggingClient))).Returns(httpClient);


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, new Uri("https://gateway.dev.local"), CreateLogger<SensorLoggingClient>());

			await Assert.ThrowsAsync<ArgumentNullException>(() => sensorLoggingClient.LogReadingAsync(null));
			await Assert.ThrowsAsync<NullReferenceException>(() => sensorLoggingClient.LogReadingAsync(new SensorReading()
			{
				Values = null
			}));

			await Assert.ThrowsAsync<IndexOutOfRangeException>(() => sensorLoggingClient.LogReadingAsync(new SensorReading()
			{
				Values =new Dictionary<string, double>()
			}));
		}

	}
}

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


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, CreateLogger<SensorLoggingClient>());
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
				null));

			await Assert.ThrowsAsync<ArgumentOutOfRangeException>("values", () => sensorLoggingClient.LogReadingAsync(deviceId,
				type,
				dateTime));

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


			var sensorLoggingClient = new SensorLoggingClient(clientFactory.Object, CreateLogger<SensorLoggingClient>());
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
				null));

			await Assert.ThrowsAsync<ArgumentOutOfRangeException>("values", () => sensorLoggingClient.LogReadingAsync(deviceId,
				type));

			var result = await sensorLoggingClient.LogReadingAsync(deviceId, 
				type, 
				dateTime, 
				new KeyValuePair<string, double>("value1", reading));

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Equal(HttpStatusCode.OK, result.StatusCode);
			Assert.Equal(readingId, result.Data);

		}
	}
}

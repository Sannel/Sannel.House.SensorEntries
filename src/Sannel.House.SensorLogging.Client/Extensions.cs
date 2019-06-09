using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Sannel.House.SensorLogging.Client
{
	public static class Extensions
	{
		/// <summary>
		/// Adds the sensor logging HTTP client registration.
		/// </summary>
		/// <param name="provider">The provider.</param>
		/// <param name="baseUrl">The base URL.</param>
		/// <returns></returns>
		public static IServiceCollection AddSensorLoggingHttpClientRegistration(this IServiceCollection provider, Uri baseUrl)
		{
			Sannel.House.Client.Helpers.RegisterClient(provider, baseUrl, "/api/v1/", nameof(SensorLoggingClient), "1.0");
			return provider;
		}

		/// <summary>
		/// Adds the sensor logging client registration.
		/// </summary>
		/// <param name="services">The services.</param>
		/// <returns></returns>
		public static IServiceCollection AddSensorLoggingClientRegistration(this IServiceCollection services) 
			=> services.AddTransient(g => new SensorLoggingClient(
				g.GetService<IHttpClientFactory>(),
				g.GetService<ILogger<SensorLoggingClient>>()
			));

		/// <summary>
		/// Adds the sensor logging client and Http Client Factory Registration.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="baseUrl">The base URL.</param>
		/// <returns></returns>
		public static IServiceCollection AddSensorLoggingClient(this IServiceCollection service, Uri baseUrl)
			=> service.AddSensorLoggingHttpClientRegistration(baseUrl)
				.AddSensorLoggingClientRegistration();
	}
}

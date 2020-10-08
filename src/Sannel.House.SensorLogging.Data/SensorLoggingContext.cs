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
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Sannel.House.SensorLogging.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sannel.House.SensorLogging.Data
{
	public class SensorLoggingContext : DbContext
	{
		/// <summary>
		/// Gets the sensor entries.
		/// </summary>
		/// <value>
		/// The sensor entries.
		/// </value>
		public DbSet<SensorEntry> SensorEntries => Set<SensorEntry>();
		/// <summary>
		/// Gets the devices.
		/// </summary>
		/// <value>
		/// The devices.
		/// </value>
		public DbSet<Device> Devices => Set<Device>();

		/// <summary>
		/// Gets the sensor readings.
		/// </summary>
		/// <value>
		/// The sensor readings.
		/// </value>
		public DbSet<SensorReading> SensorReadings => Set<SensorReading>();

		/// <summary>
		/// Initializes a new instance of the <see cref="SensorLoggingContext"/> class.
		/// </summary>
		/// <param name="options">The options for this context.</param>
		public SensorLoggingContext(DbContextOptions options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			var se = modelBuilder.Entity<SensorEntry>();
			//se.Property(i => i.Values)
				//.HasConversion(
				//	j => (j == null)?null:JsonSerializer.Serialize(j, null),
				//	k => (k == null)?null:JsonSerializer.Deserialize<Dictionary<string, double>>(k, null));
			se.Property(i => i.SensorType)
				.HasConversion(
					j => (int)j,
					k => (Base.Sensor.SensorTypes)k
				);

			se.HasMany(i => i.Values).WithOne("SensorEntry");

			se.HasIndex(i => i.LocalDeviceId);
			se.HasIndex(i => i.SensorType);

			var sr = modelBuilder.Entity<SensorReading>();
			sr.Property(i => i.Name).IsRequired();
			sr.Property(i => i.Value).IsRequired();
			sr.HasIndex(i => i.Name);

			var d = modelBuilder.Entity<Device>();
			d.HasIndex(i => i.DeviceId);
			d.HasIndex(i => i.Uuid);
			d.HasIndex(i => i.MacAddress);
			d.HasIndex(nameof(Device.Manufacture), nameof(Device.ManufactureId));
		}
	}
}

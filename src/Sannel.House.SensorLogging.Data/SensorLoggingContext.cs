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
using Newtonsoft.Json;
using Sannel.House.SensorLogging.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sannel.House.SensorLogging.Data
{
	public class SensorLoggingContext : DbContext
	{
		public DbSet<SensorEntry> SensorEntries { get; set; }

		public SensorLoggingContext(DbContextOptions options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			var se = modelBuilder.Entity<SensorEntry>();
			se.Property(i => i.Values)
				.HasConversion(
					j => (j == null)?null:JsonConvert.SerializeObject(j),
					k => (k == null)?null:JsonConvert.DeserializeObject<Dictionary<string, double>>(k));
			se.Property(i => i.SensorType)
				.HasConversion(
					j => (int)j,
					k => (Sensor.SensorTypes)k
				);

			se.HasIndex(i => i.DeviceId);
			se.HasIndex(i => i.SensorType);
		}
	}
}

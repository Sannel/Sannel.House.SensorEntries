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

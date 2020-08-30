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
using Microsoft.EntityFrameworkCore;
using Sannel.House.Base.Sensor;
using Sannel.House.SensorLogging.Models;
using Sannel.House.SensorLogging.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sannel.House.SensorLogging.Tests.Repository
{
	public class DbContextRepositoryTests : BaseTest
	{
		[Fact]
		public void ContructorTest()
		{
			using (var context = CreateTestDB())
			{
				Assert.Throws<ArgumentNullException>(() => new DbContextRepository(null, null));
				Assert.Throws<ArgumentNullException>(() => new DbContextRepository(context, null));
				Assert.NotNull(new DbContextRepository(context, CreateLogger<DbContextRepository>()));
			}
		}

		[Fact]
		public async Task AddSensoryEntryAsyncTest()
		{
			using (var context = CreateTestDB())
			{
				var repo = new DbContextRepository(context, CreateLogger<DbContextRepository>());

				context.SensorEntries.RemoveRange(context.SensorEntries);
				await context.SaveChangesAsync();

				var device = await repo.AddDeviceByUuidAsync(Guid.NewGuid());

				var values = new Dictionary<string, double>()
				{
					{"Value", 13.5 }
				};
				await repo.AddSensorEntryAsync(Base.Sensor.SensorTypes.Temperature,
					device.LocalDeviceId,
					values);

				Assert.Single(context.SensorEntries);
				var first = context.SensorEntries.Include(i => i.Values).First();
				Assert.NotEqual(Guid.Empty, first.SensorEntryId);
				Assert.Equal(device.LocalDeviceId, first.LocalDeviceId);
				Assert.Equal(SensorTypes.Temperature, first.SensorType);
				Assert.True(first.CreationDate >= DateTimeOffset.UtcNow.AddMinutes(-1) &&
					first.CreationDate <= DateTimeOffset.UtcNow.AddMinutes(1));
				Assert.Single(first.Values);

				context.Devices.RemoveRange(context.Devices);
				context.SensorEntries.RemoveRange(context.SensorEntries);
				await context.SaveChangesAsync();
			}
		}
	}
}

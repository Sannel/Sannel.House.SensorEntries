/* Copyright 2019-2020 Sannel Software, L.L.C.
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
using Microsoft.EntityFrameworkCore.Design;

namespace Sannel.House.SensorLogging.Data.Migrations.PostgreSQL
{
	public class DesignTimeFactory : IDesignTimeDbContextFactory<SensorLoggingContext>
	{
		public SensorLoggingContext CreateDbContext(string[] args)
		{
			var builder = new DbContextOptionsBuilder<SensorLoggingContext>();


			builder.UseNpgsql("Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;",
					o => o.MigrationsAssembly(this.GetType().Assembly.GetName().FullName));

			return new SensorLoggingContext(builder.Options);

		}
	}
}

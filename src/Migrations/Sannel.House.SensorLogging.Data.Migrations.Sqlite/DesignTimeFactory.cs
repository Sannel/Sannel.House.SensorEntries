using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sannel.House.SensorLogging.Data.Migrations.Sqlite
{
	public class DesignTimeFactory : IDesignTimeDbContextFactory<SensorLoggingContext>
	{
		public SensorLoggingContext CreateDbContext(string[] args)
		{
			var builder = new DbContextOptionsBuilder<SensorLoggingContext>();


			builder.UseSqlite("data source=db.db", o => o.MigrationsAssembly(this.GetType().Assembly.GetName().FullName));

			return new SensorLoggingContext(builder.Options);

		}
	}
}

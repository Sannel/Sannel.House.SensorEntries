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

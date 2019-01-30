using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sannel.House.SensorLogging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sannel.House.SensorLogging.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SensorLoggingController : ControllerBase
	{
		private ISensorRepository repository;
		private ILogger logger;

		public SensorLoggingController(ISensorRepository repository, ILogger<SensorLoggingController> logger)
		{
			this.repository = repository;
			this.logger = logger;
		}
	}
}

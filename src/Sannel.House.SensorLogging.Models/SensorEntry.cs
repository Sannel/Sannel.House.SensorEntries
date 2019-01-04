using Sannel.House.Sensor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sannel.House.SensorLogging.Models
{
	public class SensorEntry
	{
		[Key]
		public Guid SensorEntryId { get; set; }

		[Required]
		public int DeviceId { get; set; }

		[Required]
		public SensorTypes SensorType { get; set; }

		public DateTime CreationDate { get; set; }

		public Dictionary<string, double> Values { get; set; }
	}
}

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
using Sannel.House.Base.Sensor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;

namespace Sannel.House.SensorLogging.Models
{
	public class SensorEntry
	{
		/// <summary>
		/// Gets or sets the sensor entry identifier.
		/// </summary>
		/// <value>
		/// The sensor entry identifier.
		/// </value>
		[Key]
		public Guid SensorEntryId { get; set; }

		/// <summary>
		/// Gets or sets the local device identifier.
		/// </summary>
		/// <value>
		/// The local device identifier.
		/// </value>
		[Required]
		public Guid LocalDeviceId { get; set; }

		/// <summary>
		/// Gets or sets the type of the sensor.
		/// </summary>
		/// <value>
		/// The type of the sensor.
		/// </value>
		[Required]
		public SensorTypes SensorType { get; set; }

		/// <summary>
		/// Gets or sets the creation date.
		/// </summary>
		/// <value>
		/// The creation date.
		/// </value>
		public DateTimeOffset CreationDate { get; set; }

		/// <summary>
		/// Gets or sets the values.
		/// </summary>
		/// <value>
		/// The values.
		/// </value>
		public Collection<SensorReading> Values { get; set; } = new Collection<SensorReading>();

		/// <summary>
		/// Converts to string.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString() 
			=> $@"SensorEntryId={SensorEntryId}
LocalDeviceId={LocalDeviceId}
SensorType={SensorType}
CreationDate={CreationDate}
Values={JsonSerializer.Serialize(Values)}
";
	}
}

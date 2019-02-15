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
using Sannel.House.Sensor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sannel.House.SensorLogging.ViewModel
{
	public class SensorReading
	{
		/// <summary>
		/// Gets or sets the device identifier.
		/// </summary>
		/// <value>
		/// The device identifier.
		/// </value>
		public int? DeviceId { get; set; }
		/// <summary>
		/// Gets or sets the device mac address.
		/// </summary>
		/// <value>
		/// The device mac address.
		/// </value>
		public long? DeviceMacAddress { get; set; }
		/// <summary>
		/// Gets or sets the device UUID.
		/// </summary>
		/// <value>
		/// The device UUID.
		/// </value>
		public Guid? DeviceUuid { get; set; }
		/// <summary>
		/// Gets or sets the manufacture.
		/// </summary>
		/// <value>
		/// The manufacture.
		/// </value>
		public string Manufacture { get; set; }
		/// <summary>
		/// Gets or sets the manufacture identifier.
		/// </summary>
		/// <value>
		/// The manufacture identifier.
		/// </value>
		public string ManufactureId { get; set; }

		/// <summary>
		/// Gets or sets the type of the sensor.
		/// </summary>
		/// <value>
		/// The type of the sensor.
		/// </value>
		[Required]
		public SensorTypes SensorType { get;set; }
		/// <summary>
		/// Gets or sets the creation date.
		/// </summary>
		/// <value>
		/// The creation date.
		/// </value>
		[Required]
		public DateTime CreationDate { get; set; }
		/// <summary>
		/// Gets or sets the values.
		/// </summary>
		/// <value>
		/// The values.
		/// </value>
		[Required]
		public Dictionary<string, double> Values { get; set; }
	}
}
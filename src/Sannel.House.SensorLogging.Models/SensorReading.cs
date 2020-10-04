/* Copyright 2020-2020 Sannel Software, L.L.C.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
      http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Sannel.House.SensorLogging.Models
{
	public class SensorReading
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SensorReading"/> class.
		/// </summary>
		public SensorReading()
		{
			Name = Name ?? string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SensorReading"/> class.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="name">The name.</param>
		/// <exception cref="ArgumentNullException">
		/// entry
		/// or
		/// name
		/// </exception>
		public SensorReading(SensorEntry entry, string name)
		{
			SensorEntry = entry ?? throw new ArgumentNullException(nameof(entry));
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		/// <summary>
		/// Gets or sets the sensor reading identifier.
		/// </summary>
		/// <value>
		/// The sensor reading identifier.
		/// </value>
		[Key]
		public Guid? SensorReadingId { get; set; }

		/// <summary>
		/// Gets or sets the sensor entry.
		/// </summary>
		/// <value>
		/// The sensor entry.
		/// </value>
		[Required]
		public SensorEntry? SensorEntry { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[MaxLength(256)]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		public double Value { get; set; }

		/// <summary>
		/// Performs an implicit conversion from <see cref="KeyValuePair{System.String, System.Double}"/> to <see cref="SensorReading"/>.
		/// </summary>
		/// <param name="keyValuePair">The key value pair.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		public static implicit operator SensorReading(KeyValuePair<string, double> keyValuePair)
			=> new SensorReading()
			{
				Name = keyValuePair.Key,
				Value = keyValuePair.Value
			};
	}
}

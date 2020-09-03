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
		[Key]
		public Guid? SensorReadingId { get; set; }

		[Required]
		public SensorEntry SensorEntry { get; set; }

		[MaxLength(256)]
		public string Name { get; set; }

		public double Value { get; set; }

		public static implicit operator SensorReading(KeyValuePair<string, double> keyValuePair)
			=> new SensorReading()
			{
				Name = keyValuePair.Key,
				Value = keyValuePair.Value
			};
	}
}

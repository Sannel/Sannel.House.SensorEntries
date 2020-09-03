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
		[Key]
		public Guid SensorEntryId { get; set; }

		[Required]
		public Guid LocalDeviceId { get; set; }

		[Required]
		public SensorTypes SensorType { get; set; }

		public DateTimeOffset CreationDate { get; set; }

		public Collection<SensorReading> Values{get;set;}

		public override string ToString() 
			=> $@"SensorEntryId={SensorEntryId}
LocalDeviceId={LocalDeviceId}
SensorType={SensorType}
CreationDate={CreationDate}
Values={JsonSerializer.Serialize(Values)}
";
	}
}

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

using Sannel.House.Base.Sensor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sannel.House.SensorLogging.Models
{
	public class NewReadingMessage
	{
		public int? DeviceId { get; set; }
		public SensorTypes SensorType { get; set; }
		public DateTimeOffset CreationDate { get; set; }
		public Dictionary<string, double> Values { get; set; }
	}
}

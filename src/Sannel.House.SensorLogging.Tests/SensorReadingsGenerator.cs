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
using Sannel.House.SensorLogging.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sannel.House.SensorLogging.Tests
{
	public class SensorReadingsGenerator : IEnumerable<object[]>
	{
		//private Random rand = new Random();
		public IEnumerator<object[]> GetEnumerator()
			=> null; // GetSensorReadings().Select(i => new object[] { i }).GetEnumerator();

		//public IEnumerable<SensorReading> GetSensorReadings()
		//{ 
		//	foreach(Sensor.SensorTypes value in Enum.GetValues(typeof(Sensor.SensorTypes)))
		//	{
		//		yield return new SensorReading()
		//		{
		//			DeviceId = rand.Next(),
		//			CreationDate = DateTime.Now.AddSeconds(rand.Next(4000, 100000) * -1),
		//			SensorType = value,
		//			Values = new Dictionary<string, double>()
		//			{
		//				{"Value1", rand.NextDouble() },
		//				{"Value2", rand.NextDouble() }
		//			}
		//		};

		//		yield return new SensorReading()
		//		{
		//			DeviceMacAddress = rand.Next(),
		//			CreationDate = DateTime.Now.AddSeconds(rand.Next(4000, 100000) * -1),
		//			SensorType = value,
		//			Values = new Dictionary<string, double>()
		//			{
		//				{"Value1", rand.NextDouble() },
		//				{"Value2", rand.NextDouble() }
		//			}
		//		};

		//		yield return new SensorReading()
		//		{
		//			DeviceUuid = Guid.NewGuid(),
		//			CreationDate = DateTime.Now.AddSeconds(rand.Next(4000, 100000) * -1),
		//			SensorType = value,
		//			Values = new Dictionary<string, double>()
		//			{
		//				{"Value1", rand.NextDouble() },
		//				{"Value2", rand.NextDouble() }
		//			}
		//		};

		//		yield return new SensorReading()
		//		{
		//			Manufacture = "Manufacture1",
		//			ManufactureId = "Man1",
		//			CreationDate = DateTime.Now.AddSeconds(rand.Next(4000, 100000) * -1),
		//			SensorType = value,
		//			Values = new Dictionary<string, double>()
		//			{
		//				{"Value1", rand.NextDouble() },
		//				{"Value2", rand.NextDouble() }
		//			}
		//		};
		//	}

		//	yield return new SensorReading()
		//	{
		//		CreationDate = DateTime.Now.AddSeconds(rand.Next(4000, 100000) * -1),
		//		SensorType = Sensor.SensorTypes.Unknown,
		//		Values = new Dictionary<string, double>()
		//		{
		//			{"Value1", rand.NextDouble() }
		//		}
		//	};
		//}

		IEnumerator IEnumerable.GetEnumerator()
			=> null;// GetEnumerator();
	}
}

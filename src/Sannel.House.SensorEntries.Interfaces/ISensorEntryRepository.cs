/* Copyright 2018 Sannel Software, L.L.C.
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
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.SensorEntries.Interfaces
{
	public interface ISensorEntryRepository
	{
		/// <summary>
		/// Adds the sensor entry asynchronous.
		/// </summary>
		/// <param name="deviceId">The device identifier.</param>
		/// <param name="values">The values for then entry.</param>
		/// <returns>The id of the entry</returns>
		Task<long> AddSensorEntryAsync(int deviceId, DateTime entryDate, IReadOnlyDictionary<string, object> values);
	}
}

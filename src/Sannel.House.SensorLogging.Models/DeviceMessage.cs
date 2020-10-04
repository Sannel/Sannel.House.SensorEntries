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
using System.Text;

namespace Sannel.House.SensorLogging.Models
{
	public class DeviceMessage
	{
		/// <summary>
		/// Gets or sets the device identifier.
		/// </summary>
		/// <value>
		/// The device identifier.
		/// </value>
		public int? DeviceId { get; set; }

		/// <summary>
		/// Gets or sets the date created.
		/// </summary>
		/// <value>
		/// The date created.
		/// </value>
		public DateTimeOffset DateCreated { get; set; }

		/// <summary>
		/// Gets or sets the alternate ids.
		/// </summary>
		/// <value>
		/// The alternate ids.
		/// </value>
		public IList<AlternateIdMessage> AlternateIds { get; set; } = new List<AlternateIdMessage>();

	}
}

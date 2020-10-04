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
	public class AlternateIdMessage
	{
		/// <summary>
		/// Gets or sets the alternate identifier.
		/// </summary>
		/// <value>
		/// The alternate identifier.
		/// </value>
		public int? AlternateId { get; set; }
		/// <summary>
		/// Gets or sets the date created.
		/// </summary>
		/// <value>
		/// The date created.
		/// </value>
		public DateTimeOffset DateCreated { get; set; }
		/// <summary>
		/// Gets or sets the UUID.
		/// </summary>
		/// <value>
		/// The UUID.
		/// </value>
		public Guid? Uuid { get; set; }
		/// <summary>
		/// Gets or sets the mac address.
		/// </summary>
		/// <value>
		/// The mac address.
		/// </value>
		public long? MacAddress { get; set; }
		/// <summary>
		/// Gets or sets the manufacture.
		/// </summary>
		/// <value>
		/// The manufacture.
		/// </value>
		public string? Manufacture { get; set; }
		/// <summary>
		/// Gets or sets the manufacture identifier.
		/// </summary>
		/// <value>
		/// The manufacture identifier.
		/// </value>
		public string? ManufactureId { get; set; }

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Linux.src.models
{
	class StaticsModel: Model
	{
		public string Name { get; set; }
		public float CurrentValue { get; set; }
		public float MinValue { get; set; }
		public float MaxValue { get; set; }
		public float ScalingFactor { get; set; }
		public string MeasurementUnit { get; set; }
		public Dictionary<string, string> Details { get; set; }
	}
}

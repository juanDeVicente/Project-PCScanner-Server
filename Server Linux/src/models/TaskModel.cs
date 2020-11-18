using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Linux.src.models
{
	class TaskModel: Model
	{
		public string Name { get; set; }
		public int PID { get; set; }
		public bool Responding { get; set; }
		public string Username { get; set; }
		public string MemoryUse { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
	}
}

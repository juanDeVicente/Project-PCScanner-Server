using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Windows.src.models
{
	class InformationModel : Model
	{
		public string Name { get; set; }
		public string IP { get; set; }
		public string Port { get; set; }
		public string MacAddress { get; set; }
	}
}


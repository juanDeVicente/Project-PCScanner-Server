using Server_Windows.src.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Windows.src.controllers
{
	abstract class Controller
	{
		public abstract byte[] HandlePath(string path);
	}
}

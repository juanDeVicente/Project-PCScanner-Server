
using Server_Linux.src.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Linux.src.controllers 
{ 
	abstract class Controller
	{
		public abstract Model[] HandlePath(string path);
	}
}

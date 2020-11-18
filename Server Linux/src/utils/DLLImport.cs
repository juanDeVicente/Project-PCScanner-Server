using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Server_Linux.src.utils
{
	class DLLImport
	{
		[DllImport("libc")]
		public static extern uint getuid();
	}
}

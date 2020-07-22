using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Server_Windows.src.utils
{
	class DLLImport
	{
		public static string GetProcessUser(Process process)
		{
			IntPtr processHandle = IntPtr.Zero;
			try
			{
				OpenProcessToken(process.Handle, 8, out processHandle);
				WindowsIdentity wi = new WindowsIdentity(processHandle);
				string user = wi.Name;
				return user.Contains(@"\") ? user.Substring(user.IndexOf(@"\") + 1) : user;
			}
			catch
			{
				return "unknown";
			}
			finally
			{
				if (processHandle != IntPtr.Zero)
				{
					CloseHandle(processHandle);
				}
			}
		}
		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle(IntPtr hObject);
	}
}

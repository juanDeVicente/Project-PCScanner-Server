﻿using Newtonsoft.Json;
using Server_Windows.src.utils;
using Server_Windows.src.models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Windows.src.controllers
{
	class TaskController : Controller
	{
		private bool IsApplicationProcess(Process p)
		{
			return !string.IsNullOrEmpty(p.MainWindowTitle);
		}
		private string BytesToReadableValue(long number)
		{
			List<string> suffixes = new List<string> { " B", " KB", " MB", " GB", " TB", " PB" };

			for (int i = 0; i < suffixes.Count; i++)
			{
				long temp = number / (int)Math.Pow(1024, i + 1);

				if (temp == 0)
				{
					return (number / (int)Math.Pow(1024, i)) + suffixes[i];
				}
			}

			return number.ToString();
		}

		private byte[] GetAllTasks()
		{
			var applications = new List<TaskModel>();
			var process = new List<TaskModel>();

			Parallel.ForEach(Process.GetProcesses(), new ParallelOptions { MaxDegreeOfParallelism = 4 }, (p) => //Esto se come la CPU
			//foreach (var p in Process.GetProcesses())
			{
				//dynamic extraInfo = GetProcessExtraInformation(p.Id);
				var model = new TaskModel()
				{
					Name = p.ProcessName,
					PID = p.Id,
					Responding = p.Responding,
					Username = DLLImport.GetProcessUser(p),
					MemoryUse = BytesToReadableValue(p.PeakWorkingSet64),
				};
				try
				{
					model.Description = p.MainModule.FileVersionInfo.FileDescription;
				}
				catch
				{
					model.Description = "no_description";
				}
				try
				{
					model.Icon = Convert.ToBase64String(ImageUtil.ImageToByte(Icon.ExtractAssociatedIcon(p.MainModule.FileName).ToBitmap(), ImageFormat.Bmp));
				}
				catch
				{
					model.Icon = Convert.ToBase64String(ImageUtil.ImageToByte(Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToBitmap(), ImageFormat.Bmp));
				}
				if (model != null)
				{
					if (IsApplicationProcess(p))
						applications.Add(model);
					else
						process.Add(model);
				}
				
			});

			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
				new Dictionary<string, List<TaskModel>>()
				{
					{ "applications" , applications},
					{ "process", process }
				}
			));
		}

		private byte[] DeleteTask(int pid)
		{
			try
			{
				Process.GetProcessById(pid).Kill();
			}
			catch
			{
				return Encoding.UTF8.GetBytes("error");
			}
			return Encoding.UTF8.GetBytes("deleted");

		}

		public override byte[] HandlePath(params string[] values)
		{
			if (values.Length == 0)
				return GetAllTasks();
			return DeleteTask(int.Parse(values[0]));
		}
		
		/*
		private ExpandoObject GetProcessExtraInformation(int processId)
		{
			string query = "Select * From Win32_Process Where ProcessID = " + processId;
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
			ManagementObjectCollection processList = searcher.Get();

			dynamic response = new ExpandoObject();
			response.Description = "no_description";
			response.Username = "unknown";

			foreach (ManagementObject obj in processList)
			{
				string[] argList = new string[] { string.Empty, string.Empty };
				int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
				if (returnVal == 0)
				{
					response.Username = argList[0];
				}
				if (obj["ExecutablePath"] != null)
				{
					try
					{
						FileVersionInfo info = FileVersionInfo.GetVersionInfo(obj["ExecutablePath"].ToString());
						response.Description = info.FileDescription;
					}
					catch { }
				}
			}

			return response;
		}
		*/
	}
}

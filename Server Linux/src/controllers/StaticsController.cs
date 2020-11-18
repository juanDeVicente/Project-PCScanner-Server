using Newtonsoft.Json;
using Server_Linux.src.models;
using Server_Linux.src.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Server_Linux.src.controllers
{
	class StaticsController : Controller
	{

		public StaticsModel GetCPUStatics()
		{
			return new StaticsModel
			{
				Name = "CPU",
				MinValue = 0,
				CurrentValue = float.Parse("grep \'cpu \' /proc/stat | awk \'{usage=($2+$4)*100/($2+$4+$5)} END {print usage }\'".Exec()),
				MaxValue = 100,
				ScalingFactor = 1,
				MeasurementUnit = "%",
				Details = new Dictionary<string, string>
				{
					{"name", ("cat /proc/cpuinfo | grep 'model name' | uniq").Exec().Replace("model name", "").Replace(':', '\0').Trim().RemoveSpecialCharacters()},
					{"number_of_process", System.Diagnostics.Process.GetProcesses().Length.ToString() },
					{"number_of_cores", "nproc --all".Exec() }
				}
			};
		}

		public StaticsModel GetRAMStatics()
		{
			var raminfo = Regex.Split("free | grep Mem:".Exec(), @"\s{1,}");
			return new StaticsModel
			{
				Name = "RAM",
				MinValue = 0,
				CurrentValue = int.Parse(raminfo[2].Trim()),
				MaxValue = int.Parse(raminfo[1].Trim()),
				ScalingFactor = 1024 * 1024, //El comando free lo devuelve en kb
				MeasurementUnit = "GB",
				Details = new Dictionary<string, string>()
			};
		}

		public List<StaticsModel> GetDiskStatics()
		{
			var models = new List<StaticsModel>();
			DriveInfo[] drives = DriveInfo.GetDrives();

			for (int i = 0; i < drives.Length; i++)
			{
				DriveInfo drive = drives[i];
				if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable || drive.DriveType == DriveType.CDRom) && drive.TotalSize > 0)
				{
					var model = new StaticsModel
					{
						Name = drive.Name,
						CurrentValue = drive.TotalSize - drive.TotalFreeSpace,
						MinValue = 0,
						MaxValue = drive.TotalSize,
						ScalingFactor = (1024 * 1024 * 1024),
						MeasurementUnit = "GB",
						Details = new Dictionary<string, string>
						{
							{ "drive_type", drive.DriveType.ToString().ToLower() },
							{ "drive_format", drive.DriveFormat }
						}
					};
					models.Add(model);
				}
			}
			return models;
		}

		public List<StaticsModel> GetGPUStaticsNvidia()
		{
			var commandResult = "nvidia-smi --query-gpu=gpu_name,temperature.gpu,utilization.gpu,memory.used,memory.total,driver_version --format=csv,noheader,nounits".Exec();
			if (commandResult == null)
				return new List<StaticsModel>(0);

			var gpuinfo = commandResult.Split('\n');
			if (gpuinfo.Length != 6)
				return new List<StaticsModel>(0);

			var models = new List<StaticsModel>(gpuinfo.Length);

			foreach (var gpu in gpuinfo)
			{
				var info = gpu.Split(", ");
				models.Add(new StaticsModel() 
				{
					Name = "GPU",
					CurrentValue = int.Parse(info[2]),
					MinValue = 0,
					MaxValue = 100,
					ScalingFactor = 1,
					MeasurementUnit = "%",
					Details = new Dictionary<string, string>
					{
						{"name", info[0] },
						{"builder", "NVIDIA" },
						{"temperature", info[1] + "ºC"},
						{"memory_used", info[3] + "MB"},
						{"memory_total", info[4] + "MB"},
						{"driver_vesion", info[5] },

					}
				});
			}

			return models;
		}

		public override byte[] HandlePath(params string[] values)
		{
			var statics = new List<StaticsModel>
			{
				GetCPUStatics(),
				GetRAMStatics()
			};
			statics.AddRange(GetDiskStatics());
			statics.AddRange(GetGPUStaticsNvidia());

			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(statics.ToArray()));
		}
	}
}

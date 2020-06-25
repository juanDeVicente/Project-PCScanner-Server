using LibreHardwareMonitor.Hardware;
using Server_Windows.src.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server_Windows.src.controllers
{
	class StaticsController: Controller
	{
		Computer Computer { get; set; }
		int HardwareSearched { get; set; }

		public StaticsController()
		{
			Computer = new Computer
			{
				IsCpuEnabled = true,
				IsGpuEnabled = true,
				IsNetworkEnabled = true
			};

			Computer.Open();
			HardwareSearched = Computer.Hardware.Length; //Cojo a ver cuantos componentes tengo que recorrer la primera vez
		}

		public Model GetCPUStatics(IHardware hardware)
		{
			var model = new StaticsModel
			{
				Name = "CPU",
				MinValue = 0,
				MaxValue = 100,
				ScalingFactor = 1,
				MeasurementUnit = "%",
				Details = new Dictionary<string, string>
				{
					{"name", hardware.Name},
					{"number_of_process", System.Diagnostics.Process.GetProcesses().Length.ToString() }
				}
				
			};
			int sensorsSearched = 2;
			for(int i = 0; i < hardware.Sensors.Length && sensorsSearched > 0; i++)
			{
				ISensor sensor = hardware.Sensors[i];
				if (sensor.Name == "CPU Total" && sensor.SensorType == SensorType.Load)
				{
					model.CurrentValue = (float)sensor.Value;
					sensorsSearched--;
				}
				else if (sensor.Name == "Core Average" && sensor.SensorType == SensorType.Temperature)
				{
					model.Details.Add("temperature", sensor.Value.ToString() + " \u00B0C");
					sensorsSearched--;
				}
				
			}

			return model;
		}
		public Model GetGPUNVidiaStatics(IHardware hardware)
		{
			var model = new StaticsModel
			{
				Name = "GPU",
				MinValue = 0,
				MaxValue = 100,
				ScalingFactor = 1,
				MeasurementUnit = "%",
				Details = new Dictionary<string, string>
				{
					{ "name", hardware.Name },
					{ "builder", "NVIDIA" }
				}

			};
			int sensorsSearched = 2;
			for (int i = 0; i < hardware.Sensors.Length && sensorsSearched > 0; i++)
			{
				ISensor sensor = hardware.Sensors[i];
				if (sensor.Name == "GPU Core" && sensor.SensorType == SensorType.Load)
				{
					model.CurrentValue = (float)sensor.Value;
					sensorsSearched--;
				}
				else if (sensor.Name == "GPU Core" && sensor.SensorType == SensorType.Temperature)
				{
					model.Details.Add("temperature", sensor.Value.ToString() + " \u00B0C");
					sensorsSearched--;
				}

			}

			return model;
		}
		public Model GetGPUAMDStatics(IHardware hardware)
		{
			var model = new StaticsModel
			{
				Name = "GPU",
				MinValue = 0,
				MaxValue = 100,
				ScalingFactor = 1,
				MeasurementUnit = "%",
				Details = new Dictionary<string, string>
				{
					{ "name", hardware.Name },
					{ "builder", "AMD" }
				}

			};
			int sensorsSearched = 2;
			for (int i = 0; i < hardware.Sensors.Length && sensorsSearched > 0; i++)
			{
				ISensor sensor = hardware.Sensors[i];
				if (sensor.Name == "GPU Core" && sensor.SensorType == SensorType.Load)
				{
					model.CurrentValue = (float)sensor.Value;
					sensorsSearched--;
				}
				else if (sensor.Name == "GPU Core" && sensor.SensorType == SensorType.Temperature)
				{
					model.Details.Add("temperature", sensor.Value.ToString() + " \u00B0C");
					sensorsSearched--;
				}

			}

			return model;
		}

		public Model GetNetworkStatics(IHardware hardware)
		{
			var model = new StaticsModel
			{
				Name = hardware.Name.Contains("Ethernet") ? "Ethernet" : hardware.Name.Contains("Wi-fi") ? "Wi-fi" : hardware.Name.Contains("Bluetooth") ? "Bluetooth" : hardware.Name.Contains("VirtualBox") ? "VirtualBox" : hardware.Name,
				MinValue = 0,
				MaxValue = 100,
				ScalingFactor = 1,
				MeasurementUnit = "%",
				Details = new Dictionary<string, string>()
			};
			int sensorsSearched = 3;
			for (int i = 0; i < hardware.Sensors.Length && sensorsSearched > 0; i++)
			{
				ISensor sensor = hardware.Sensors[i];
				if (sensor.Name == "Network Utilization" && sensor.SensorType == SensorType.Load) //FIXME no tiene sentido lo que devuelve y el admin de windows muestra la velocidad de subida y bajada en la misma gráfica
				{
					model.CurrentValue = (float)sensor.Value;
					sensorsSearched--;
				}
				else if (sensor.Name == "Upload Speed" && sensor.SensorType == SensorType.Throughput)
				{
					model.Details.Add("upload_speed", sensor.Value.ToString() + " KB/s");
					sensorsSearched--;
				}
				else if (sensor.Name == "Download Speed" && sensor.SensorType == SensorType.Throughput)
				{
					model.Details.Add("download_speed", sensor.Value.ToString() + " KB/s");
					sensorsSearched--;
				}
			}
			return model;
		}

		public Model GetRAMStatics()
		{
			var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
			return new StaticsModel
			{
				Name = "RAM",
				MinValue = 0,
				CurrentValue = computerInfo.TotalPhysicalMemory - computerInfo.AvailablePhysicalMemory, //No es verdad xd
				MaxValue = computerInfo.TotalPhysicalMemory,
				ScalingFactor = 1024 * 1024 * 1024,
				MeasurementUnit = "GB",
				Details = new Dictionary<string, string>()
			};
		}

		public List<Model> GetDiskStatics()
		{
			var models = new List<Model>();
			DriveInfo[] drives = DriveInfo.GetDrives();

			for (int i = 0; i < drives.Count(); i++)
			{
				DriveInfo drive = drives[i];
				if (drive.IsReady)
				{
					var model = new StaticsModel
					{
						Name = drive.Name.Remove(drive.Name.Length - 1),
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

		public override Model[] HandlePath(string path)
		{
			List<Model> statics = new List<Model>();
			List<Model> cpuStatics = new List<Model>();
			List<Model> gpuStatics = new List<Model>();
			List<Model> networkStatics = new List<Model>();
			int hardwareVisited = 0;

			for(int i = 0; i < Computer.Hardware.Length && hardwareVisited < HardwareSearched; i++)
			{
				IHardware hardware = Computer.Hardware[i];
				if (hardware.HardwareType == HardwareType.Cpu)
				{
					hardware.Update();
					cpuStatics.Add(GetCPUStatics(hardware));
					hardwareVisited++;
				}
				else if (hardware.HardwareType == HardwareType.GpuNvidia)
				{
					hardware.Update();
					gpuStatics.Add(GetGPUNVidiaStatics(hardware));
					hardwareVisited++;  
				}
				else if (hardware.HardwareType == HardwareType.GpuAmd)
				{
					hardware.Update();
					gpuStatics.Add(GetGPUAMDStatics(hardware));
					hardwareVisited++;
				}
				else if (hardware.HardwareType == HardwareType.Network)
				{
					hardware.Update();
					networkStatics.Add(GetNetworkStatics(hardware));
					hardwareVisited++;
				}
			}
			HardwareSearched = hardwareVisited; //Puede ser que el usuario no tenga ningún tipo de GPU, o varias GPU, o varias CPU
			//Meto en varias listas para imponer orden y no tardar en ejecutar (se podria implementar una lista que se ordene sola, pero es un bulce for mas por cada hardware)
			statics.AddRange(cpuStatics);
			statics.AddRange(gpuStatics);
			statics.Add(GetRAMStatics());
			statics.AddRange(GetDiskStatics());
			statics.AddRange(networkStatics);
			return statics.ToArray();
		}
	}
   
}

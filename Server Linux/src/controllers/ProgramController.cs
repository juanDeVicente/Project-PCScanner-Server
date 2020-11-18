using Newtonsoft.Json;
using Server_Linux.src.models;
using Server_Linux.src.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Linux.src.controllers
{
	class ProgramController : Controller
	{
		private readonly string programJsonPath;
		private List<ProgramExecutable> programs;
		public ProgramController(string programJsonPath)
		{
			this.programJsonPath = programJsonPath;
			programs = new List<ProgramExecutable>();
			LoadPrograms();
		}
		public override byte[] HandlePath(params string[] values)
		{
			if (values.Length == 0)
				return GetAllPrograms();
			ExecuteProgram(int.Parse(values[0]));
			return new byte[0];
		}

		private void ExecuteProgram(int programPos)
		{
			programs[programPos].StartProgram();
		}

		private byte[] GetAllPrograms()
		{
			LoadPrograms();
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(programs.ConvertAll(obj => obj.ToProgramModel())));
		}

		private void LoadPrograms()
		{
			string jsonString = System.IO.File.ReadAllText(programJsonPath);
			programs = JsonConvert.DeserializeObject<List<ProgramExecutable>>(jsonString);
			if (programs == null)
				programs = new List<ProgramExecutable>();
		}

		private class ProgramExecutable
		{
			public string ExecutablePath;
			public string Name;
			public string Arguments;

			public void StartProgram()
			{
				var startInfo = new ProcessStartInfo
				{
					FileName = this.ExecutablePath,
					Arguments = this.Arguments
				};
				Process.Start(startInfo);
			}
			public ProgramModel ToProgramModel()
			{
				return new ProgramModel()
				{
					Name = this.Name,
					Icon = Convert.ToBase64String(ImageUtil.ImageToByte(Icon.ExtractAssociatedIcon(this.ExecutablePath).ToBitmap(), ImageFormat.Bmp))
				};
			}
		}
	}
}

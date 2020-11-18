using Newtonsoft.Json;
using Server_Windows.src.models;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Server_Windows.src.controllers
{
	class InformationController : Controller
	{
		private readonly InformationModel model;

		public InformationController(string port)
		{
			model = new InformationModel
			{
				Name = Dns.GetHostName(),
				IP = PropertiesController.GetLocalIPv4Address(),
				Port = port,
				MacAddress = NetworkInterface.GetAllNetworkInterfaces()
				.Where(nic => nic.OperationalStatus == OperationalStatus.Up)
				.Select(nic => nic.GetPhysicalAddress().ToString())
				.FirstOrDefault()
			};
		}

		public override byte[] HandlePath(params string[] values)
		{
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
		}
	}
}

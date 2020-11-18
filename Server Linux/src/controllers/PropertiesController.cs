using Newtonsoft.Json;
using Server_Linux.src.models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server_Linux.src.controllers
{
	class PropertiesController : Controller
	{
		public Model GetPCName()
		{
			return new PropertiesModel
			{
				Name = "pc_name",
				Value = Dns.GetHostName()
			};
		}

		public Model GetLocalIPV4()
		{
			return new PropertiesModel
			{
				Name = "ipv4",
				Value = GetLocalIPv4Address()
			};
		}
		public Model GetLocalIPV6()
		{
			return new PropertiesModel
			{
				Name = "ipv6",
				Value = GetLocalIPv6Address()
			};
		}


		public Model GetOSName()
		{
			return new PropertiesModel
			{
				Name = "os",
				Value = System.Runtime.InteropServices.RuntimeInformation.OSDescription
		};
		}

		private string GetLocalIPv4Address()
		{
			string localIP = "127.0.0.1";
			Socket socket = null;
			try
			{
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
				socket.Connect("8.8.8.8", 65530);
				IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
				localIP = endPoint.Address.ToString();
			}
			catch (Exception)
			{

			}
			finally
			{
				if (socket != null)
					socket.Dispose();
			}

			return localIP;
		}

		//FIXME Hay errores al obtener la IPv6 a google
		private string GetLocalIPv6Address()
		{
			string localIP = "::1";
			Socket socket = null;
			try
			{
				socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, 0);
				socket.Connect("2001:4860:4860::8844", 65530);
				IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
				localIP = endPoint.Address.ToString();
			}
			catch (Exception)
			{

			}
			finally
			{
				if (socket != null)
					socket.Dispose();
			}

			return localIP;
		}

		public override byte[] HandlePath(params string[] values)
		{
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Model[]
			{
				GetPCName(),
				GetLocalIPV4(),
				//GetLocalIPV6(),
				GetOSName()
			}));
		}
	}
}

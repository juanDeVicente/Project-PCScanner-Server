using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Server_Windows.src.controllers;
using Server_Windows.src.models;
using NetFwTypeLib;
using System.Runtime.InteropServices;

namespace Server_Windows
{
	class Program
	{
		static HttpListener listener;
		static void Main(string[] args)
		{
            string port = args.Length >= 1 ? args[0] : "5000";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateFirewallRuleWindows(port);

			listener = new HttpListener();
			listener.Prefixes.Add("http://*:" + port + "/");
			listener.Start();

			var listenTask = HandleIncomingConnections();
			listenTask.GetAwaiter().GetResult();

            listener.Close();
		}

		public static async Task HandleIncomingConnections()
		{
            bool runServer = true;
            var staticsController = new StaticsController();
            var propertiesController = new PropertiesController();

            while (runServer)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();

                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/statics"))
                    WriteResponse(resp, staticsController.HandlePath(""));
                else if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/properties"))
                    WriteResponse(resp, propertiesController.HandlePath(""));
                else if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/shutdown"))
                    runServer = false;
                else if(req.HttpMethod == "GET")
                    WriteResponse(resp, new Model[0]);
                else
                {
                    resp.StatusCode = (int) HttpStatusCode.NotFound;
                    resp.Close();
                } 
            }
        }

        public static async void WriteResponse(HttpListenerResponse resp, Model[] models)
		{
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(models));
            resp.ContentType = "application/json";
            resp.ContentEncoding = Encoding.UTF8;
            //resp.AppendHeader("Cache-Control", "no-cache");
            resp.ContentLength64 = data.LongLength;

            await resp.OutputStream.WriteAsync(data, 0, data.Length);
            resp.Close();
        }

        /**
         * HttpListener no pide permiso al Firewall..., pero tengo permiso de administrador....
         */
        ///<summary>
        /// Función para crear la regla pertinente en el Firewall de Windows
        ///</summary>
        public static void CreateFirewallRuleWindows(string port)
		{
            Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);
            var currentProfiles = fwPolicy2.CurrentProfileTypes;

            INetFwRule2 inboundRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            inboundRule.Enabled = true;

            inboundRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;

            inboundRule.Protocol = 6;

            inboundRule.LocalPorts = port;
            
            inboundRule.Name = "ProjectPCScanner Rule";
            
            inboundRule.Profiles = currentProfiles;

            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallPolicy.Rules.Add(inboundRule);
        }
	}
}

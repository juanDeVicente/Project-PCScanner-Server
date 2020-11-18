using System;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Server_Windows.src.controllers;
using NetFwTypeLib;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using Server_Windows.src.security;

namespace Server_Windows
{
	class Program
	{
		static HttpListener listener;
        static SecurityModule securityModule;
        static string port;
		static void Main(string[] args)
		{
            try
            {
                port = args.Length >= 1 ? args[0] : "5000";

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    CreateFirewallRuleWindows(port);

                if (args.Length >= 2 && args[1] == "security")
                    securityModule = new SecurityModule();

                listener = new HttpListener();
                listener.Prefixes.Add("http://*:" + port + "/");
                listener.Start();

                var listenTask = HandleIncomingConnections();
                listenTask.GetAwaiter().GetResult();

                listener.Close();
            }
            catch (Exception e)
			{
                Console.WriteLine("Error:");
                Console.WriteLine(e.Source);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
			}
		}

		public static async Task HandleIncomingConnections()
		{
            bool runServer = true;
            var staticsController = new StaticsController();
            var propertiesController = new PropertiesController();
            var taskController = new TaskController();
            var programController = new ProgramController("./programs.json");
            var informationController = new InformationController(port);

            while (runServer)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                string[] urlParams = req.Url.AbsolutePath.TrimEnd('/').Split('/').Skip(1).ToArray();
                var address = req.RemoteEndPoint.Address;

                if (securityModule != null && !securityModule.IsIpLogged(address))
                {
                    if (req.HttpMethod == "POST")
                    {
                        var reqPassword = req.QueryString.Get("password");
                        if (reqPassword != null && securityModule.VerifyHash(reqPassword, address))
                        {
                            resp.StatusCode = (int)HttpStatusCode.OK;
                            resp.Close();
                        }
                        else
                        {
                            resp.StatusCode = (int)HttpStatusCode.Unauthorized;
                            resp.Close();
                        }
                    }
                    else
                    {
                        resp.StatusCode = (int)HttpStatusCode.Unauthorized;
                        resp.Close();
                    }
                }
                else if (req.HttpMethod == "GET")
                {
                    if (urlParams.Length == 0)
                        WriteResponse(resp, informationController.HandlePath(""));
                    else if (urlParams[0] == "statics")
                        WriteResponse(resp, staticsController.HandlePath(""));
                    else if (urlParams[0] == "properties")
                        WriteResponse(resp, propertiesController.HandlePath(""));
                    else if (urlParams[0] == "tasks")
                        WriteResponse(resp, taskController.HandlePath(urlParams.Skip(1).Take(urlParams.Length - 1).ToArray()));
                    else if (urlParams[0] == "programs")
                        WriteResponse(resp, programController.HandlePath(urlParams.Skip(1).Take(urlParams.Length - 1).ToArray()));
                    else if (urlParams[0] == "shutdown")
                    {
                        WriteResponse(resp, new byte[0]);
                        runServer = false;
                        InvokeWin32ShutdownMethod("1"); //FIXME puede ser que tenga que poner lo de force shutdown (5)
                    }
                    else if (urlParams[0] == "reboot")
                    {
                        WriteResponse(resp, new byte[0]);
                        runServer = false;
                        InvokeWin32ShutdownMethod("2");
                    }
                    else
					{
                        resp.StatusCode = (int)HttpStatusCode.NotFound;
                        resp.Close();
                    }
                }
                else
                {
                    resp.StatusCode = (int)HttpStatusCode.NotFound;
                    resp.Close();
                }
            }
        }

        public static async void WriteResponse(HttpListenerResponse resp, byte[] data)
		{
            resp.ContentType = "application/json";
            resp.ContentEncoding = Encoding.UTF8;
            //resp.AppendHeader("Cache-Control", "no-cache");
            resp.ContentLength64 = data.LongLength;

            try
            {
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
            }
			catch
			{
                
			}
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

            inboundRule.Protocol = 6; //TCP

            inboundRule.LocalPorts = port;
            
            inboundRule.Name = "ProjectPCScanner Rule";
            
            inboundRule.Profiles = currentProfiles;

            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallPolicy.Rules.Add(inboundRule);
        }
        public static void InvokeWin32ShutdownMethod(string flag)
        {
			ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();

            mcWin32.Scope.Options.EnablePrivileges = true;
            ManagementBaseObject mboShutdownParams =
                     mcWin32.GetMethodParameters("Win32Shutdown");

            mboShutdownParams["Flags"] = flag; 
            mboShutdownParams["Reserved"] = "0";
            foreach (ManagementObject manObj in mcWin32.GetInstances())
				_ = manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
        }
    }
}

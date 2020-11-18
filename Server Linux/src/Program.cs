using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Server_Linux.src.controllers;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using Server_Linux.src.utils;
using Server_Linux.src.security;

namespace Server_Linux
{
    class Program
    {
        static HttpListener listener;
        static SecurityModule securityModule;
        static void Main(string[] args)
        {
            if (!IsAdmin())
            {
                Console.WriteLine("The application must be initialized with sudo, please call this application with sudo");
                return;
            }
            try
            {
                string port = args.Length >= 1 ? args[0] : "5000";

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
                if (req.HttpMethod == "GET")
                {
                    if (urlParams.Length == 0)
                        WriteResponse(resp, Encoding.UTF8.GetBytes("hello!!"));
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
                        "shutdown now".Exec();
                        runServer = false;
                    }
                    else if (urlParams[0] == "reboot")
                    {
                        WriteResponse(resp, new byte[0]);
                        "reboot now".Exec();
                        runServer = false;
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

        /// <summary>
        /// Devuelve si el usuario tiene permisos de administracion
        /// </summary>
        public static bool IsAdmin()
        { 
            try
            {
                if (DLLImport.getuid() != 0)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
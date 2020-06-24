using Newtonsoft.Json;
using Server_Linux.src.controllers;
using Server_Linux.src.models;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server_Linux
{
	class Program
	{
        static HttpListener listener;
        static void Main(string[] args)
		{
			string port = args.Length >= 1 ? args[0] : "5000";

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
                else if (req.HttpMethod == "GET")
                    WriteResponse(resp, new Model[0]);
                else
                {
                    resp.StatusCode = (int)HttpStatusCode.NotFound;
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
    }
}
 
using System;
using XR.Server.Http;
using XR.Server.Json;
using Jayrock.JsonRpc;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using XR.Client.Json;

namespace TestServer
{

	class TestWebJsonServer
	{
		public static HttpServer StartServer (int port, string jsonprefix)
		{
			var web = new HttpServer () { Port = port };
			
			web.UriRequested += (object sender, UriRequestEventArgs args) => {
				if (args.Request.Url.AbsolutePath == "/") {
					args.Handled = true;
					args.SetResponseState (200);
					args.SetResponseType ("text/html");
					args.ResponsStream.Write ("<html><head></head><body><h1>Hello!</h1></body></html>");
				}            
			};
			
			var jh = new JsonServer<TestJsonRPCService> ();
			jh.PathMatch = new Regex (jsonprefix);
			web.UriRequested += jh.HandleJsonRequest;
			
			web.BeginListen ();

			return web;
		}

		static int serverPort = 9988;
		static string serverPrefix = "/jsonrpc/test";

		public static void Main (string[] argv)
		{

			var web = StartServer (serverPort, serverPrefix);

			var fact = new XR.Client.Json.JsonRpcClientFactory ();
			var c = fact.GetClient<ITestJsonRPCContract> (new UriBuilder () { 
				Scheme = "http",
				Host = "localhost",
				Port = serverPort,
				Path = serverPrefix
			}.Uri);
			var start = DateTime.Now;
			int i = 0;
			while (DateTime.Now.Subtract(start).TotalSeconds < 20) {
				var rv = c.GetDictionary (10);
				Console.WriteLine (rv.Count);
				foreach (var x in rv.Keys)
					Console.WriteLine ("{0} = {1}", x, rv [x]);
                     
				var rv2 = c.GetSomeClass ("fred" + i++);
				Console.WriteLine (rv2.Name);

				var rv3 = c.GetSomeContainer ("bob" + i++);
				Console.WriteLine (rv3.Items [0].Name);

			}

			// throwers
			try {
				var err = c.ThrowException ("hello world");
				Console.Error.WriteLine (err);
			} catch (JsonServerErrorException e) {
				Console.Error.WriteLine ("Server threw an error:");
				Console.Error.WriteLine ("{0}", e);
			}

			web.StopServer ();
		}
	}
}

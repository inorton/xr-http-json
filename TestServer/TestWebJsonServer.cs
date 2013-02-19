using System;
using XR.Server.Http;
using XR.Server.Json;
using Jayrock.JsonRpc;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace TestServer
{
    public class SomeClass {
        public string Name { get; set; }
    }

    public interface ITestJsonRPCContract {
        IDictionary<string,Guid> GetDictionary(int count);
        SomeClass GetSomeClass( string name );
    }

    public class TestJsonRPCService : JsonRpcService, ITestJsonRPCContract
    {
        object sync = new object();
        public int Requests { get;  set;} 

        [JsonRpcMethod]
        public IDictionary<string,Guid> GetDictionary(int count)
        {
            lock ( sync )
                Requests++;
            var d = new Dictionary<string,Guid>();
            while (d.Count < count)
            {
                var x = Guid.NewGuid();
                d.Add( x.ToString(), x );
            }
            return d;
        }

        [JsonRpcMethod]
        public SomeClass GetSomeClass( string name )
        {
            return new SomeClass() { Name = name };
        }
    }

    public class TestJsonRPCClient : JsonRpcClient, ITestJsonRPCContract
    {
        static MethodBase GetCallerMethod(int offset)
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1 + offset);
            return sf.GetMethod();
        }

        static string GetCallerMethodName()
        {
            return GetCallerMethod(1).Name;
        }

        static Type GetCallerMethodReturnType()
        {
            return ((MethodInfo)GetCallerMethod(1)).ReturnType;
        }

        #region ITestJsonRPCContract implementation   
        public IDictionary<string, Guid> GetDictionary(int count)
        {
            return (IDictionary<string, Guid>)InvokeVargs( GetCallerMethodReturnType(), GetCallerMethodName(), count );
        }

        public SomeClass GetSomeClass(string name)
        {
            return (SomeClass)InvokeVargs(GetCallerMethodReturnType(), GetCallerMethodName(), name);
        }
        #endregion
    }

    class TestWebJsonServer
    {
        public static void Main(string[] argv)
        {
            var web = new HttpServer() { Port = 9988 };

            web.UriRequested += (object sender, UriRequestEventArgs args) => {
                if (args.Request.Url.AbsolutePath == "/")
                {
                    args.Handled = true;
                    args.SetResponseState(200);
                    args.SetResponseType("text/html");
                    args.ResponsStream.Write("<html><head></head><body><h1>Hello!</h1></body></html>");
                }            
            };

            var jh = new JsonServer<TestJsonRPCService>();
            jh.PathMatch = new Regex("/jsonrpc/test");
            web.UriRequested += jh.HandleJsonRequest;

            var c = new TestJsonRPCClient() { Url = "http://127.0.0.1:9988/jsonrpc/test" };

            ThreadPool.QueueUserWorkItem((w) => {
                ((HttpServer)w).Listen();
            }, web);

            while (jh.Service.Requests < 100)
            {
                var rv = c.GetDictionary(10);
                Console.WriteLine( rv.Count );
                foreach ( var x in rv.Keys )
                    Console.WriteLine( "{0} = {1}", x, rv[x] );
                     
                var rv2 = c.GetSomeClass("fred" + jh.Service.Requests );
                Console.WriteLine( rv2.Name );
            }

            web.StopServer();
        }
    }
}

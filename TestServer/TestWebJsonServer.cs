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

    public class SomeContainer {
        public IList<SomeClass> Items { get; set; }
    }

    public interface ITestJsonRPCContract {
        IDictionary<string,Guid> GetDictionary(int count);
        SomeClass GetSomeClass( string name );
        SomeContainer GetSomeContainer( string name );
        string ThrowException( string msg );
    }

    public class TestJsonRPCService : JsonRpcService, ITestJsonRPCContract
    {
        object sync = new object();
        public int Requests { get;  set;} 

        [JsonRpcMethod]
        public string ThrowException(string msg)
        {
            throw new InvalidOperationException(msg);
        }

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

        [JsonRpcMethod]
        public SomeContainer GetSomeContainer( string name )
        {
            var c = new SomeContainer() {
                Items = new List<SomeClass>() { new SomeClass() { Name = name } },
            };
            return c;
        }
    }

    public class TestJsonRPCClient : JsonRpcClient, ITestJsonRPCContract
    {
        protected override void OnError(object errorObject)
        {
            Console.Error.WriteLine("server raised error : {0}", errorObject );
            throw new InvalidProgramException("blaaaa");
        }

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


        public string ThrowException(string msg)
        {
            return (string)InvokeVargs( GetCallerMethodReturnType(), GetCallerMethodName(), msg );
        }

        public IDictionary<string, Guid> GetDictionary(int count)
        {
            return (IDictionary<string, Guid>)InvokeVargs( GetCallerMethodReturnType(), GetCallerMethodName(), count );
        }

        public SomeClass GetSomeClass(string name)
        {
            return (SomeClass)InvokeVargs(GetCallerMethodReturnType(), GetCallerMethodName(), name);
        }

        public SomeContainer GetSomeContainer(string name)
        {
            return (SomeContainer)InvokeVargs(GetCallerMethodReturnType(), GetCallerMethodName(), name );
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
                Console.WriteLine(rv.Count);
                foreach (var x in rv.Keys)
                    Console.WriteLine("{0} = {1}", x, rv [x]);
                     
                var rv2 = c.GetSomeClass("fred" + jh.Service.Requests);
                Console.WriteLine(rv2.Name);

                var rv3 = c.GetSomeContainer("bob" + jh.Service.Requests);
                Console.WriteLine(rv3.Items [0].Name);
            }

            // throwers
            try
            {
                var err = c.ThrowException("hello world");
                Console.Error.WriteLine(err);
            } catch (Exception e)
            {
                Console.Error.WriteLine("caught:");
                Console.Error.WriteLine(e.GetType().Name);
                Console.Error.WriteLine(e);
            }

            web.StopServer();
        }
    }
}

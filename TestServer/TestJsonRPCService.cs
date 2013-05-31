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
    
}

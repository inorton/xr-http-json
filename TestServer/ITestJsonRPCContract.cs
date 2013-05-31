using System;
using XR.Server.Json;
using System.Collections.Generic;

namespace TestServer
{

	public interface ITestJsonRPCContract : IJsonRpcServiceContract
	{
		IDictionary<string,Guid> GetDictionary (int count);

		SomeClass GetSomeClass (string name);

		SomeContainer GetSomeContainer (string name);

		string ThrowException (string msg);
	}

	public class SomeClass
	{
		public string Name { get; set; }
	}

	public class SomeContainer
	{
		public IList<SomeClass> Items { get; set; }
	}	
}

using System;
using Castle.DynamicProxy;
using Jayrock.JsonRpc;

namespace XR.Client.Json
{
	internal sealed class JsonRpcMethodInterceptor<T> : IInterceptor 
		where T : DynamicJsonRpcClientBase
	{

		#region IInterceptor implementation

		public void Intercept (IInvocation invocation)
		{
			T client = invocation.Proxy as T;
			if ( invocation.Method.ReturnType == typeof(void) ) {
				client.InvokeVargs( invocation.Method.Name, invocation.Arguments );
			} else {
				invocation.ReturnValue = client.InvokeVargs( invocation.Method.ReturnType, invocation.Method.Name, invocation.Arguments );
			}
		}

		#endregion


	}

	internal sealed class JsonRpcProxyGenerationHook : IProxyGenerationHook 
	{
		#region IProxyGenerationHook implementation

		public void MethodsInspected ()
		{
		}

		public void NonProxyableMemberNotification (Type type, System.Reflection.MemberInfo memberInfo)
		{
		}

		public bool ShouldInterceptMethod (Type type, System.Reflection.MethodInfo methodInfo)
		{
			return methodInfo.IsAbstract;
		}

		#endregion

	}

	public class JsonRpcClientFactory
	{
		ProxyGenerator generator;

		public JsonRpcClientFactory ()
		{
			generator = new ProxyGenerator();
		}

		public TInterface GetClient<TInterface>( Uri address )
			where TInterface : class, XR.Server.Json.IJsonRpcServiceContract
		{
			var tlist = new Type[] { typeof(TInterface) };
			var opts = new ProxyGenerationOptions( new JsonRpcProxyGenerationHook() );
			var client = generator.CreateClassProxy( typeof(DynamicJsonRpcClientBase), tlist, opts, new JsonRpcMethodInterceptor<DynamicJsonRpcClientBase>() ) as TInterface;
			(client as JsonRpcClient).Url = address.ToString();
			return client;
		}
	}
}


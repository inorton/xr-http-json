using System;
using System.Linq;
using Jayrock.JsonRpc;
using System.Collections;
using System.Collections.Generic;
using Jayrock.Json;

namespace XR.Client.Json
{
	public class DynamicJsonRpcClientBase : JsonRpcClient
	{
		protected override void OnError (object errorObject)
		{
			Exception throwme = null;
			try {
				var je = errorObject as IDictionary;
				if ( je.Contains( "errors" ) ) {
					var errlist = je["errors"] as JsonArray;
					List<IDictionary> errors = new List<IDictionary>();
					string errmsg = je["message"] as String;
					string firstetype = null;
					foreach ( JsonObject e in errlist )
					{
						if ( e.Contains("name") && e.Contains("message") ) {
							errors.Add( e as IDictionary );
							if ( firstetype == null ) {
								firstetype = e["name"] as String;
							}
						}
					}

					var x = new JsonServerErrorException( errmsg, errors.ToArray() );
					x.FirstErrorType = firstetype;
					x.FirstErrorMessage = errmsg;
					throwme = x;
				}	
			} catch { }

			if ( throwme != null ) throw throwme;

			base.OnError (errorObject);
		}
	}
}


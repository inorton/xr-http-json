using System;
using System.Net;
using System.Web;
using Jayrock.JsonRpc;
using System.IO;
using XR.Server.Http;
using System.Text.RegularExpressions;
using Jayrock.Services;

namespace XR.Server.Json
{
    public class JsonServerErrorEventArgs : EventArgs {
        public Exception Exception { get; set; }
    }

    public delegate void JsonServerError( object sender, JsonServerErrorEventArgs args );

    public class LoggingJsonRpcDispatcher : JsonRpcDispatcher {
        public LoggingJsonRpcDispatcher( IService service ) : base ( service ) {

        }

        protected override object OnError(Exception e, System.Collections.IDictionary request)
        {
            if ( ServerError != null ) {
                ServerError( Service, new JsonServerErrorEventArgs() { Exception = e.InnerException } );                     
            }
            return base.OnError(e, request);
        }

        public event JsonServerError ServerError;
    }

    public class JsonServer<T> 
        where T : JsonRpcService, new() 
    {

        public T Service { get; private set; }
        LoggingJsonRpcDispatcher dispatcher; 

        public Regex PathMatch { get; set; }

        public JsonServer(T serviceObject)
        {
            Service = serviceObject;
            dispatcher = new LoggingJsonRpcDispatcher( Service );
            dispatcher.ServerError += (object sender, JsonServerErrorEventArgs args) => {
                Console.Error.WriteLine("Server Error: {0}", args.Exception);
            };
        }

        public JsonServer() : this( new T() )
        {
        }

        public void HandleJsonRequest(object sender, UriRequestEventArgs args)
        {
            if (args == null || args.Handled) return;

            if ((PathMatch == null) || (PathMatch.IsMatch(args.Request.Url.PathAndQuery)))
            {
                args.Handled = true;
                args.SetResponseType( "application/json" );
                args.SetResponseState( 200 );
                dispatcher.Process( new StreamReader(args.Request.InputStream),
                                   args.ResponsStream );
            }
        }
    }
}



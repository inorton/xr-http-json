using System;
using System.Net;
using System.Web;
using Jayrock.JsonRpc;
using System.IO;
using XR.Server.Http;
using System.Text.RegularExpressions;
using Jayrock.Services;
using System.Collections;

using System.Reflection;

namespace XR.Server.Json
{
    public class JsonServerErrorEventArgs : EventArgs {
        public Exception Exception { get; set; }
        public IDictionary Request { get; set; }
    }

    public delegate void JsonServerError( object sender, JsonServerErrorEventArgs args );

    public class LoggingJsonRpcDispatcher : JsonRpcDispatcher {
        public LoggingJsonRpcDispatcher( IService service ) : base ( service ) {

        }
        public override IDictionary Invoke(IDictionary request)
        {
            try
            {
                return base.Invoke(request);
            }
            catch (Exception e)
            {
                OnError(e, request);
                throw;
            }
        }

        protected override object OnError(Exception e, IDictionary request)
        {
            var ex = new JsonServerErrorEventArgs() { 
                Exception = e.InnerException, 
                Request = request };
            if ( ex.Exception == null ) ex.Exception = e;

            if ( ServerError != null ) {
                ServerError( Service, ex );
            } else {
                Console.Error.WriteLine("Server Error: {0}", ex.Exception);
            }
            return base.OnError(e, request);
        }

        public event JsonServerError ServerError;
    }

    public class JsonServer<T> 
        where T : JsonRpcService, IJsonRpcServiceContract, new() 
    {

        public T Service { get; private set; }
        public LoggingJsonRpcDispatcher Dispatcher { get; private set; } 

        public Regex PathMatch { get; set; }

        public JsonServer(T serviceObject)
        {
            Service = serviceObject;

            Dispatcher = new LoggingJsonRpcDispatcher( Service );
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
                Dispatcher.Process( new StreamReader(args.Request.InputStream),
                                   args.ResponsStream );
            }
        }
    }
}



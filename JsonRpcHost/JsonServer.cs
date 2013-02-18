using System;
using System.Net;
using System.Web;
using Jayrock.JsonRpc;
using System.IO;
using XR.Server.Http;
using System.Text.RegularExpressions;

namespace XR.Server.Json
{
    public class JsonServer<T> 
        where T : Jayrock.Services.IService, new() 
    {

        T service;
        JsonRpcDispatcher dispatcher; 

        public HttpServer HttpServer { get; set; }

        public Regex PathMatch { get; set; }

        public JsonServer()
        {
            service = new T();
            dispatcher = JsonRpcDispatcherFactory.CreateDispatcher( service );
            HttpServer.UriRequested += HandleJsonRequest;
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



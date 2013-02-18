using System;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace XR.Server.Http
{
    public class UriRequestEventArgs : EventArgs {
        public HttpListenerRequest Request { get; set; }
        HttpListenerResponse Response { get; set; }

        public StreamWriter ResponsStream { get; set; }
        public bool Handled { get; set; }

        public UriRequestEventArgs( HttpListenerResponse resp ) {
            Response = resp;
        }

        public void SetResponseType( string type ) {
            Response.ContentType = type;
        }

        public void SetResponseState( int status ) {
            Response.StatusCode = status;
        }

    }

    public delegate void UriRequestHandler( object sender, UriRequestEventArgs args );
    
}

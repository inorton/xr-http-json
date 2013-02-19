using System;
using System.Web;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace XR.Server.Http
{
    public class HttpServer
    {
        bool stopServer;
        
        public HttpServer()
        {
            Port = 16000;
        }
        
        public event UriRequestHandler UriRequested;
        
        void ProcessRequest(IAsyncResult res)
        {
            var httpd = (HttpListener)res.AsyncState;
            var ctx = httpd.EndGetContext(res);
            var req = ctx.Request;
            Console.Error.WriteLine("request from {0}", req.RemoteEndPoint.Address);
            var sw = new StreamWriter(ctx.Response.OutputStream);
            try
            {
                bool handled = false;
      
                if ( UriRequested != null ){
                    var evtargs = new UriRequestEventArgs( ctx.Response ) 
                    { 
                        Request = ctx.Request,
                        ResponsStream = sw

                    };

                    UriRequested( this, evtargs );
                    handled = evtargs.Handled;
                }

                if ( !handled ){
                    ctx.Response.ContentType = "text/plain";
                    ctx.Response.StatusCode = 404;
                    sw.WriteLine("404 Not found");
                    sw.WriteLine(req.Url);
                }
            } catch (Exception e)
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.ContentType = "text/plain";
                sw.WriteLine("500 Server error");
                sw.WriteLine( e.ToString() );
            }
            
            
            
            sw.Close();
        }
        
        
        public int Port { get; set; }
        
        
        public void StopServer() {
            stopServer = true;
            closing = true;
            while ( closing )
                System.Threading.Thread.Sleep(500);
        }

        bool closing = false;
        public void Listen() {
            stopServer = false;

            var httpd = new HttpListener();
            httpd.Prefixes.Add(string.Format("http://*:{0}/",Port));
            httpd.Start();
            
            while ( !stopServer ){
                var iar = httpd.BeginGetContext( new AsyncCallback(ProcessRequest), httpd );
                do {
                } while ( !iar.AsyncWaitHandle.WaitOne(1000) && !stopServer );
            }
            httpd.Close();
            closing = false;
        }
        
    }
}


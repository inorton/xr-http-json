using System;
using System.Web;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading;

namespace XR.Server.Http
{
    public class HttpServer
    {
        bool stopServer;

        public HttpServer ()
        {
            Port = 16000;
        }

        public bool Localhostonly { get; set; }

        public event UriRequestHandler UriRequested;

        void ProcessRequest (IAsyncResult res)
        {
            if (closing || stopServer) return;
            HttpListenerContext ctx;
            try
            {
                var httpd = (HttpListener)res.AsyncState;
                ctx = httpd.EndGetContext(res);
            }
            catch (ObjectDisposedException) {
                return;
            }
            var req = ctx.Request;

            var sw = new StreamWriter (ctx.Response.OutputStream);
            try {
                bool handled = false;

                if (UriRequested != null) {
                    var evtargs = new UriRequestEventArgs (ctx.Response) 
                    { 
                        Request = ctx.Request,
                                ResponsStream = sw

                    };

                    UriRequested (this, evtargs);
                    handled = evtargs.Handled;
                }

                if (!handled) {
                    ctx.Response.ContentType = "text/plain";
                    ctx.Response.StatusCode = 404;
                    sw.WriteLine ("404 Not found");
                    sw.WriteLine (req.Url);
                }
            } catch (Exception e) {
                ctx.Response.StatusCode = 500;
                ctx.Response.ContentType = "text/plain";
                sw.WriteLine ("500 Server error");
                sw.WriteLine (e.ToString ());
            }



            sw.Close ();
        }


        public int Port { get; set; }


        public void StopServer ()
        {
            stopServer = true;
            closing = true;
            while (closing)
                System.Threading.Thread.Sleep (500);
        }

        bool closing = false;
        HttpListener httpd;
        public void Listen ()
        {
            stopServer = false;

            var addr = Localhostonly ? "localhost" : "*";

            httpd = new HttpListener ();
            httpd.Prefixes.Add (string.Format ("http://{0}:{1}/", addr, Port));
            httpd.Start ();

            while (!stopServer) {
                var iar = httpd.BeginGetContext (new AsyncCallback (ProcessRequest), httpd);
                do {
                } while ( !iar.AsyncWaitHandle.WaitOne(1000) && !stopServer );
            }
            httpd.Close ();
            closing = false;
        }

        Thread serverThread = null;
        public void BeginListen ()
        {
            serverThread = new Thread(Listen);
            serverThread.Start();            
        }
    }
}


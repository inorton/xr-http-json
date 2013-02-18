using System;

using XR.Server.Http;

namespace TestServer
{
    class MainClass
    {
        public static void Main(string[] argv)
        {
            var web = new HttpServer() { Port = 9988 };

            web.UriRequested += (object sender, UriRequestEventArgs args) =>  {
                args.Handled = true;
                args.SetResponseState(200);
                args.SetResponseType("text/html");
                args.ResponsStream.Write(@"
<html>
    <head>
        <title>Hello</title>
    </head>
    <body>
        <h1>Hello</h1>
        It Works!
    </body>
</html>");
            };

            web.Listen();
        }
    }
}

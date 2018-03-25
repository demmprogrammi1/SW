using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading;
using System.Text;
namespace SimpleHttpServ
{
    class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;
 
      public WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> method)
      {
         if (!HttpListener.IsSupported)
         {
            throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
         }
             
         // URI prefixes are required eg: "http://localhost:8080/test/"
         if (prefixes == null || prefixes.Count == 0)
         {
            throw new ArgumentException("URI prefixes are required");
         }
         
         if (method == null)
         {
            throw new ArgumentException("responder method required");
         }
 
         foreach (var s in prefixes)
         {
            _listener.Prefixes.Add(s);
         }
 
         _responderMethod = method;
         _listener.Start();
      }
 
      public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
         : this(prefixes, method)
      {
      }
 
      public void Run()
      {
         ThreadPool.QueueUserWorkItem(o =>
         {
            Console.WriteLine("Webserver running...");
            try
            {
               while (_listener.IsListening)
               {
                  ThreadPool.QueueUserWorkItem(c =>
                  {
                     var ctx = c as HttpListenerContext;
                     try
                     {
                        if (ctx == null)
                        {
                           return;
                        }
                            
                        var rstr = _responderMethod(ctx.Request);
                        var buf = Encoding.UTF8.GetBytes(rstr);
                        ctx.Response.ContentLength64 = buf.Length;
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                     }
                     catch
                     {
                        // ignored
                     }
                     finally
                     {
                        // always close the stream
                        if (ctx != null)
                        {
                           ctx.Response.OutputStream.Close();
                        }
                     }
                  }, _listener.GetContext());
               }
            }
            catch (Exception ex)
            {
               // ignored
            }
         });
      }
 
      public void Stop()
      {
         _listener.Stop();
         _listener.Close();
      }
   }
    
    internal class Progra {
        public static string Response(HttpListenerRequest request) {
            return string.Format("<HTML><BODY>My web page.<br>{0}</BODY></HTML>", DateTime.Now);
        }
        static void Main(string[] args)
        {
            var serv = new WebServer(Response,"https://localhost:10000/");
            serv.Run();
            Console.WriteLine("Simple Http server is started,press any key to stop it");
            Console.ReadKey();
            serv.Stop();
        }
    }
}

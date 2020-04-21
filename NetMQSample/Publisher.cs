using NetMQ;
using NetMQ.Sockets;
using System;
using System.Threading;

namespace NetMQSample
{
    public class Publisher
    {
        public Publisher()
        {
            using (var server = new ResponseSocket())
            {
                server.Bind("tcp://*:5555");
                while (true)
                {
                    var message = server.ReceiveFrameString();
                    Console.WriteLine("Received {0}", message);
                    // processing the request
                    Thread.Sleep(100);
                    Console.WriteLine("Sending World");
                    server.SendFrame("World");
                }
            }
        }
    }
}

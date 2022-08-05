using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebSocketServerTest
{
    public class EchoWebSocketBehaviorFactory
    {
        private static EchoWebSocketBehaviorFactory instance;

        private EchoWebSocketBehaviorFactory() { }

        public static EchoWebSocketBehaviorFactory GetInstance()
        {
            return instance ?? (instance = new EchoWebSocketBehaviorFactory());
        }

        private readonly List<Echo> _connections = new List<Echo>();
        
        public Echo Create()
        {
            var echo = new Echo();
            _connections.Add(echo);
            return echo;
        }

        public void SendMessageToAll(string message)
        {
            foreach (var connection in _connections)
            {
                connection.SendMessage(message);
            }
        }

        public void DeleteFromConnections(Echo echoInstance)
        {
            _connections.Remove(echoInstance);
        }
    }

    public class Echo : WebSocketBehavior
    {
        public void SendMessage(string message)
        {
            Send(message);
        }
        
        protected override void OnMessage(MessageEventArgs e)
        {
            Send(e.Data);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            EchoWebSocketBehaviorFactory.GetInstance().DeleteFromConnections(this);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            EchoWebSocketBehaviorFactory.GetInstance().DeleteFromConnections(this);
        }
    }
    
    internal class Program
    {
        public static void Main(string[] args)
        {
            var websocketServer = new WebSocketServer("ws://0.0.0.0:7890");
            websocketServer.AddWebSocketService("/", () => EchoWebSocketBehaviorFactory.GetInstance().Create());

            websocketServer.Start();
            Console.WriteLine("Started server on 0.0.0.0:7890");
            Console.ReadKey();
            EchoWebSocketBehaviorFactory.GetInstance().SendMessageToAll("TEST 1");
            Console.ReadKey();
            EchoWebSocketBehaviorFactory.GetInstance().SendMessageToAll("TEST 2");
            Console.ReadKey();
            websocketServer.Stop();
        }
    }
}
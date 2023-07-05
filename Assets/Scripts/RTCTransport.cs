using System;
using System.Collections.Concurrent;
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.SceneManagement;
namespace Mirror.SimpleWeb
{
    [DisallowMultipleComponent]
    public class RTCTransport : Transport
    {

        [DllImport("__Internal")] private static extern void JSFindMatch(string matchmakingServer, Action callbackBecomeHost, Action callbackBecomClient);

        [DllImport("__Internal")] private static extern void JSClientConnect(Action<IntPtr, int> callbackOnMessage, Action callbackOnGameStart);
        [DllImport("__Internal")] private static extern void JSClientDisconnect();
        [DllImport("__Internal")] private static extern void JSCLientSend(byte[] bufferPtr, int offset, int length);
        [DllImport("__Internal")] private static extern void JSServerStart(Action<IntPtr, int> callbackOnMessage, Action callbackOnClientConnect, Action callbackOnGameStart);
        [DllImport("__Internal")] private static extern void JSServerDisconnect();
        [DllImport("__Internal")] private static extern void JSServerStop();
        [DllImport("__Internal")] private static extern void JSServerSend(byte[] bufferPtr, int offset, int length);
        public static NetworkManager manager;
        public string signalingServer;
        bool server;
        bool client;
        public int maxMessagesPerTick = 100;
        // public int maxMessageSize = 16 * 1024;
        protected static BufferPool bufferPool = new BufferPool(5, 20, 64 * 1024);
        public static readonly ConcurrentQueue<Message> receiveQueue = new ConcurrentQueue<Message>();

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
        }

        void OnValidate()
        {
        }

        public void FindMatch()
        {
            string server = "ws://195.43.244.83:7778";
            JSFindMatch(server, BecomeHost, BecomeClient);
        }

        [MonoPInvokeCallback(typeof(Action))]
        static void BecomeHost()
        {
            manager.StartHost();
        }

        [MonoPInvokeCallback(typeof(Action))]
        static void BecomeClient()
        {
            manager.StartClient();
        }

        [MonoPInvokeCallback(typeof(Action))]
        static void CallbackOnGameStart()
        {
            manager.ServerChangeScene("GameScene");
            // SceneManager.LoadScene("GameScene");
        }


        public override bool Available()
        {
            return true;
        }
        public override int GetMaxPacketSize(int channelId = 0)
        {
            // TODO: What should this number be?
            return 1500;
        }

        public override void Shutdown()
        {
        }

        #region Client

        public override bool ClientConnected()
        {
            return client;
        }

        public void CallbackOnClientConnected()
        {
            OnClientConnected.Invoke();
        }

        public void CallbackOnClientData(IntPtr bufferPtr, int count)
        {

            ArrayBuffer buffer = bufferPool.Take(count);
            buffer.CopyFrom(bufferPtr, count);

            receiveQueue.Enqueue(new Message(buffer));
        }

        [MonoPInvokeCallback(typeof(Action<IntPtr, int>))]
        static void CallbackOnMessage(IntPtr bufferPtr, int count)
        {
            ArrayBuffer buffer = bufferPool.Take(count);
            buffer.CopyFrom(bufferPtr, count);
            receiveQueue.Enqueue(new Message(buffer));
        }

        [MonoPInvokeCallback(typeof(Action))]
        static void CallbackOnNewClientConnect()
        {
            receiveQueue.Enqueue(new Message(EventType.Connected));
        }

        public override void ClientConnect(string hostname)
        {
            JSClientConnect(CallbackOnMessage, CallbackOnGameStart);
            client = true;
        }

        public override void ClientDisconnect()
        {
            JSClientDisconnect();
        }

        public override void ClientSend(ArraySegment<byte> segment, int channelId)
        {
            JSCLientSend(segment.Array, segment.Offset, segment.Count);
        }

        // messages should always be processed in early update
        public override void ClientEarlyUpdate()
        {
            ProcessMessageQueue(this);
        }

        #endregion

        #region Server

        public override bool ServerActive()
        {
            return server;
        }

        public override void ServerStart()
        {
            JSServerStart(CallbackOnMessage, CallbackOnNewClientConnect, CallbackOnGameStart);
            server = true;
        }

        public override void ServerStop()
        {
            Debug.Log($"{System.Reflection.MethodBase.GetCurrentMethod().Name}");
            JSServerStop();
        }

        public override void ServerDisconnect(int connectionId)
        {
            JSServerDisconnect();
        }

        public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
        {
            JSServerSend(segment.Array, segment.Offset, segment.Count);
        }

        public override string ServerGetClientAddress(int connectionId)
        {
            return "WebRTC";
        }

        public override Uri ServerUri()
        {
            UriBuilder builder = new UriBuilder { };
            return builder.Uri;
        }

        // messages should always be processed in early update
        public override void ServerEarlyUpdate()
        {
            ProcessMessageQueue(this);
        }

        #endregion

        public void ProcessMessageQueue()
        {
            ProcessMessageQueue(null);
        }

        public void ProcessMessageQueue(MonoBehaviour behaviour)
        {
            int processedCount = 0;
            bool skipEnabled = behaviour == null;
            // check enabled every time in case behaviour was disabled after data
            while (
                (skipEnabled || behaviour.enabled) &&
                processedCount < maxMessagesPerTick &&
                // Dequeue last
                receiveQueue.TryDequeue(out Message next)
                )
            {
                processedCount++;

                switch (next.type)
                {
                    case EventType.Connected:
                        if (server)
                            OnServerConnected.Invoke(1);
                        break;
                    case EventType.Data:
                        if (client)
                            OnClientDataReceived.Invoke(next.data.ToSegment(), Channels.Reliable);
                        if (server)
                            OnServerDataReceived.Invoke(1, next.data.ToSegment(), Channels.Reliable);

                        next.data.Release();
                        break;
                    case EventType.Disconnected:
                        break;
                    case EventType.Error:
                        break;
                }
            }
        }
    }
}

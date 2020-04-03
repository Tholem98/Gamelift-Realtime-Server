//using System;
//using System.Text;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Aws.GameLift.Realtime;
//using Aws.GameLift.Realtime.Event;
//using Aws.GameLift.Realtime.Types;
//using Amazon.Lambda;
//using Amazon.Lambda.Model;
//using Amazon.Runtime;
//using Amazon.CognitoIdentity;
//using Amazon;
//using System.Net.NetworkInformation;
//using System.Net;
//using System.Linq;


////namespace Example
////{
///**
// * An example client that wraps the GameLift Realtime client SDK
// * 
// * You can redirect logging from the SDK by setting up the LogHandler as such:
// * ClientLogger.LogHandler = (x) => Console.WriteLine(x);
// *
// */
//public class RealTimeClient: MonoBehaviour
//    {
//        public Aws.GameLift.Realtime.Client Client { get; private set; }

//        // An opcode defined by client and your server script that represents a custom message type
//        private const int MY_TEST_OP_CODE = 10;

//        /// Initialize a client for GameLift Realtime and connect to a player session.
//        /// <param name="endpoint"></param>
//        /// <param name="remoteTcpPort">A TCP port for the Realtime server</param>
//        /// <param name="listeningUdpPort">A local port for listening to UDP traffic</param>
//        /// <param name="connectionType">Type of connection to establish between client and the Realtime server</param>
//        /// <param name="playerSessionId">The player session ID that is assigned to the game client for a game session </param>
//        /// <param name="connectionPayload">Developer-defined data to be used during client connection, such as for player authentication</param>
//        public RealTimeClient(string endpoint, int remoteTcpPort, int listeningUdpPort,
//                     string playerSessionId, byte[] connectionPayload)
//        {
//            // Create a client configuration to specify a secure or unsecure connection type
//            // Best practice is to set up a secure connection using the connection type RT_OVER_WSS_DTLS_TLS12.
//            ClientConfiguration clientConfiguration = new ClientConfiguration();
            

//            // Create a Realtime client with the client configuration            
//            Client = new Client(clientConfiguration);

//            // Initialize event handlers for the Realtime client
//            Client.ConnectionOpen += OnOpenEvent;
//            Client.ConnectionClose += OnCloseEvent;
//            Client.GroupMembershipUpdated += OnGroupMembershipUpdate;
//            Client.DataReceived += OnDataReceived;


//        ConnectToGameLiftServer();
//        }

//    public bool IsConnectedToServer { get; set; }

//    public void Start()
//    {
//        IsConnectedToServer = false;
//    }

//    public void Disconnect()
//        {
//            if (Client.Connected)
//            {
//                Client.Disconnect();
//            }
//        }

//        public bool IsConnected()
//        {
//            return Client.Connected;
//        }

//        /// <summary>
//        /// Example of sending to a custom message to the server.
//        /// 
//        /// Server could be replaced by known peer Id etc.
//        /// </summary>
//        /// <param name="intent">Choice of delivery intent ie Reliable, Fast etc. </param>
//        /// <param name="payload">Custom payload to send with message</param>
//        public void SendMessage(DeliveryIntent intent, string payload)
//        {
//            Client.SendMessage(Client.NewMessage(MY_TEST_OP_CODE)
//                .WithDeliveryIntent(intent)
//                .WithTargetPlayer(Constants.PLAYER_ID_SERVER)
//                .WithPayload(StringToBytes(payload)));
//        }

//        /**
//         * Handle connection open events
//         */
//        public void OnOpenEvent(object sender, EventArgs e)
//        {
//        }

//        /**
//         * Handle connection close events
//         */
//        public void OnCloseEvent(object sender, EventArgs e)
//        {
//        }

//        /**
//         * Handle Group membership update events 
//         */
//        public void OnGroupMembershipUpdate(object sender, GroupMembershipEventArgs e)
//        {
//        }

//        /**
//         *  Handle data received from the Realtime server 
//         */
//        public virtual void OnDataReceived(object sender, DataReceivedEventArgs e)
//        {
//            switch (e.OpCode)
//            {
//                // handle message based on OpCode
//                default:
//                    break;
//            }
//        }

//        /**
//         * Helper method to simplify task of sending/receiving payloads.
//         */
//        public static byte[] StringToBytes(string str)
//        {
//            return Encoding.UTF8.GetBytes(str);
//        }

//        /**
//         * Helper method to simplify task of sending/receiving payloads.
//         */
//        public static string BytesToString(byte[] bytes)
//        {
//            return Encoding.UTF8.GetString(bytes);
//        }

//        private void ConnectToGameLiftServer()
//        {
//            Debug.Log("Reaching out to client service Lambda function");

//            AWSConfigs.AWSRegion = "sa-east-1"; // Your region here
//            AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
//            // paste this in from the Amazon Cognito Identity Pool console
//            CognitoAWSCredentials credentials = new CognitoAWSCredentials(
//                "sa-east-1:fleet-5a55bd4e-c7a7-441b-9daa-354cd3a8c0ff", // Your identity pool ID here
//                RegionEndpoint.USEast1 // Your region here
//            );

//            AmazonLambdaClient client = new AmazonLambdaClient(credentials, RegionEndpoint.USEast1);
//            InvokeRequest request = new InvokeRequest
//            {
//                FunctionName = "ConnectClientToServer",
//                InvocationType = InvocationType.RequestResponse
//            };


//            client.InvokeAsync(request,
//                (response) =>
//                {
//                    if (response.Exception == null)
//                    {
//                        if (response.Response.StatusCode == 200)
//                        {
//                            var payload = Encoding.ASCII.GetString(response.Response.Payload.ToArray()) + "\n";
//                            var playerSessionObj = JsonUtility.FromJson<PlayerSessionObject>(payload);

//                            if (playerSessionObj.FleetId == null)
//                            {
//                                Debug.Log($"Error in Lambda: {payload}");
//                            }
//                            else
//                            {
//                                QForMainThread(ActionConnectToServer, playerSessionObj.IpAddress, Int32.Parse(playerSessionObj.Port), playerSessionObj.PlayerSessionId);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        Debug.LogError(response.Exception);
//                    }
//                });
//        }
//    private Queue<Action> _mainThreadQueue = new Queue<Action>();

//    private void QForMainThread(Action fn)
//    {
//        lock (_mainThreadQueue)
//        {
//            _mainThreadQueue.Enqueue(() => { fn(); });
//        }
//    }

//    private void QForMainThread<T1>(Action<T1> fn, T1 p1)
//    {
//        lock (_mainThreadQueue)
//        {
//            _mainThreadQueue.Enqueue(() => { fn(p1); });
//        }
//    }

//    private void QForMainThread<T1, T2>(Action<T1, T2> fn, T1 p1, T2 p2)
//    {
//        lock (_mainThreadQueue)
//        {
//            _mainThreadQueue.Enqueue(() => { fn(p1, p2); });
//        }
//    }

//    private void QForMainThread<T1, T2, T3>(Action<T1, T2, T3> fn, T1 p1, T2 p2, T3 p3)
//    {
//        lock (_mainThreadQueue)
//        {
//            _mainThreadQueue.Enqueue(() => { fn(p1, p2, p3); });
//        }
//    }

//    public void ActionConnectToServer(string ipAddr, int port, string tokenUID)
//    {
//        StartCoroutine(ConnectToServer(ipAddr, port, tokenUID));
//    }

//    private const string DEFAULT_ENDPOINT = "127.0.0.1";
//    private const int DEFAULT_TCP_PORT = 3001;
//    private const int DEFAULT_UDP_PORT = 8921;

//    private int FindAvailableUDPPort(int firstPort, int lastPort)
//    {
//        var UDPEndPoints = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
//        List<int> usedPorts = new List<int>();
//        usedPorts.AddRange(from n in UDPEndPoints where n.Port >= firstPort && n.Port <= lastPort select n.Port);
//        usedPorts.Sort();
//        for (int testPort = firstPort; testPort <= lastPort; ++testPort)

//        {
//            if (!usedPorts.Contains(testPort))
//            {
//                return testPort;
//            }
//        }
//        return -1;
//    }

//    private void OnConnectionErrorEvent(object sender, Aws.GameLift.Realtime.Event.ErrorEventArgs e)
//    {
//        Debug.Log($"[client] Connection Error! : ");
//        //GameController.QuitToMainMenu();
//    }

//    private IEnumerator ConnectToServer(string ipAddr, int port, string tokenUID)
//    {
//        ClientLogger.LogHandler = (x) => Debug.Log(x);
//        ConnectionToken token = new ConnectionToken(tokenUID, null);

//        ClientConfiguration clientConfiguration = ClientConfiguration.Default();

//        Client = new Aws.GameLift.Realtime.Client(clientConfiguration);
//        Client.ConnectionOpen += new EventHandler(OnOpenEvent);
//        Client.ConnectionClose += new EventHandler(OnCloseEvent);
//        Client.DataReceived += new EventHandler<DataReceivedEventArgs>(OnDataReceived);
//        Client.ConnectionError += new EventHandler<Aws.GameLift.Realtime.Event.ErrorEventArgs>(OnConnectionErrorEvent);

//        int UDPListenPort = FindAvailableUDPPort(DEFAULT_UDP_PORT, DEFAULT_UDP_PORT + 20);
//        if (UDPListenPort == -1)
//        {
//            Debug.Log("Unable to find an open UDP listen port");
//            yield break;
//        }
//        else
//        {
//            Debug.Log($"UDP listening on port: {UDPListenPort}");
//        }

//        Debug.Log($"[client] Attempting to connect to server ip: {ipAddr} TCP port: {port} Player Session ID: {tokenUID}");
//        Client.Connect(string.IsNullOrEmpty(ipAddr) ? DEFAULT_ENDPOINT : ipAddr, port, UDPListenPort, token);

//        while (true)
//        {
//            if (Client.ConnectedAndReady)
//            {
//                IsConnectedToServer = true;
//                Debug.Log("[client] Connected to server");
//                break;
//            }
//            yield return null;
//        }
//    }
//}
////}
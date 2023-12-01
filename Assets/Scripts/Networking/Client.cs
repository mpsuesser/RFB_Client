using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance; // singleton
    public static int dataBufferSize = 4096;

    public string ip;
    public int port = Constants.SERVER_LISTEN_PORT;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
        }


        tcp = new TCP();
        udp = new UDP();

        Debug.Log("Client is up.");
    }

    private void Start() {
        PreloadManager.instance.Ready(this);
    }

    private void OnApplicationQuit() {
        Disconnect();
    }

    public void ConnectToServer() {
        InitializeClientData();

        isConnected = true;
        Debug.Log("Attempting to connect via TCP...");
        tcp.Connect();
    }

    public class TCP {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect() {
            socket = new TcpClient(new IPEndPoint(GetLocalIPAddress(), /* Constants.CLIENT_LISTEN_PORT */ 0)) {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];

            Debug.Log("Beginning TCP connection, waiting for response...");
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        // https://stackoverflow.com/questions/6803073/get-local-ip-address
        private static IPAddress GetLocalIPAddress() {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address;
            }
        }

        private void ConnectCallback(IAsyncResult _result) {
            Debug.Log("ConnectCallback, port: " + ((IPEndPoint)socket.Client.LocalEndPoint).Port.ToString());
            socket.EndConnect(_result);

            if (!socket.Connected) {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet _packet) {
            try {
                if (socket != null) {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            } catch (Exception _ex) {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result) {
            try {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0) {
                    instance.Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            } catch (Exception _ex) {
                Console.WriteLine($"Error receiving TCP data: {_ex}");
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data) {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4) {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0) {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength()) {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet _packet = new Packet(_packetBytes)) {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4) {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0) {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1) {
                return true;
            }

            return false;
        }

        private void Disconnect() {
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP() {}

        public void Connect() {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
            socket = new UdpClient(/*Constants.CLIENT_LISTEN_PORT*/);

            Debug.Log($"Connecting UDP socket to endpoint {endPoint}");
            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);
            Debug.Log($"socket.BeginReceive() called");

            using (Packet _packet = new Packet()) {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet) {
            try {
                _packet.InsertInt(instance.myId);
                if (socket != null) {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            } catch (Exception _ex) {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result) {
            try {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4) {
                    instance.Disconnect();
                    Debug.Log("Disconnected because _data.Length was < 4!");
                    return;
                }

                HandleData(_data);
            } catch (Exception _ex) {
                Debug.Log($"Error in ReceiveCallback for UDP data: {_ex}");
                Disconnect();
            }
        }

        private void HandleData(byte[] _data) {
            using (Packet _packet = new Packet(_data)) {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() => {
                using (Packet _packet = new Packet(_data)) {
                    int _packetId = _packet.ReadInt();
                    // Debug.Log($"[UDP] Packet ID: {_packetId}");
                    packetHandlers[_packetId](_packet);
                }
            });
        }

        private void Disconnect() {
            Debug.Log($"UDP Disconnect() called.");
            instance.Disconnect();

            endPoint = null;
            socket = null;
        }
    }

    private void InitializeClientData() {
        packetHandlers = new Dictionary<int, PacketHandler>() {
            { (int)ServerPackets.welcomeToServer, ClientHandle.WelcomeToServer },
            { (int)ServerPackets.udpHandshakeReceived, ClientHandle.UDPHandshakeReceived },
            { (int)ServerPackets.welcomeToLobby, ClientHandle.WelcomeToLobby },
            { (int)ServerPackets.gameInProgress, ClientHandle.GameInProgress },
            { (int)ServerPackets.versionOutOfDate, ClientHandle.VersionOutOfDate },
            { (int)ServerPackets.updateLobby, ClientHandle.UpdateLobby },
            { (int)ServerPackets.gameStarting, ClientHandle.GameStarting },
            { (int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected },
            { (int)ServerPackets.teamMessage, ClientHandle.TeamMessageReceived },
            { (int)ServerPackets.allMessage, ClientHandle.AllMessageReceived },
            { (int)ServerPackets.newSnapFormation, ClientHandle.NewSnapFormation },
            { (int)ServerPackets.gsQuarter, ClientHandle.GameState_Quarter },
            { (int)ServerPackets.gsStopClock, ClientHandle.GameState_StopClock },
            { (int)ServerPackets.gsTimeLeftInQuarter, ClientHandle.GameState_TimeLeftInQuarter },
            { (int)ServerPackets.gsScore, ClientHandle.GameState_Score },
            { (int)ServerPackets.gsPossession, ClientHandle.GameState_Possession },
            { (int)ServerPackets.gsDown, ClientHandle.GameState_Down },
            { (int)ServerPackets.gsFieldPosition, ClientHandle.GameState_FieldPosition },
            { (int)ServerPackets.hiked, ClientHandle.Hiked },
            { (int)ServerPackets.turnoverOnDowns, ClientHandle.TurnoverOnDowns },
            { (int)ServerPackets.touchback, ClientHandle.Touchback },
            { (int)ServerPackets.safety, ClientHandle.Safety },
            { (int)ServerPackets.falseStart, ClientHandle.FalseStart },
            { (int)ServerPackets.unitCharged, ClientHandle.UnitCharged },
            { (int)ServerPackets.unitJuked, ClientHandle.UnitJuked },
            { (int)ServerPackets.unitTackled, ClientHandle.UnitTackled },
            { (int)ServerPackets.unitStiffed, ClientHandle.UnitStiffed },
            { (int)ServerPackets.unitPositionUpdate, ClientHandle.UnitPositionUpdate },
            { (int)ServerPackets.unitRotationUpdate, ClientHandle.UnitRotationUpdate },
            { (int)ServerPackets.ballThrown, ClientHandle.BallThrown },
            { (int)ServerPackets.ballUpdate, ClientHandle.BallUpdate },
            { (int)ServerPackets.ballCaught, ClientHandle.BallCaught },
            { (int)ServerPackets.hikeCaught, ClientHandle.HikeCaught },
            { (int)ServerPackets.ballIncomplete, ClientHandle.BallIncomplete },
            { (int)ServerPackets.ballIntercepted, ClientHandle.BallIntercepted },
            { (int)ServerPackets.unitCantThrow, ClientHandle.UnitCantThrow },
            { (int)ServerPackets.unitScoredTouchdown, ClientHandle.UnitScoredTouchdown },
            { (int)ServerPackets.unitTackledWithBall, ClientHandle.UnitTackledWithBall },
            { (int)ServerPackets.unitWentOutWithBall, ClientHandle.UnitWentOutWithBall },
            { (int)ServerPackets.quarterEnding, ClientHandle.QuarterEnding }
        };

        Debug.Log("Initialized packets.");
    }

    public void Disconnect() {
        if (isConnected) {
            isConnected = false;
            tcp.socket.Close();

            // in case we try connecting and the server is not up
            if (udp != null && udp.socket != null) {
                Debug.Log("Client.Disconnect() closed UDP connection as expected");
                udp.socket.Close();
            }

            Debug.Log("Disconnected from server.");
        }
    }
}

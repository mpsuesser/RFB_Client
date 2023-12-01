using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager instance;

    public string username;

    private Text connectionStatusText;
    public enum ConnectionState {
        REQUESTING = 1,
        ESTABLISHING,
        CONNECTED,
        JOINED,
        FAILED
    }
    public ConnectionState Connection { get; private set; }

    private Dictionary<int, string> slotsState = null;
    public Dictionary<int, string> Slots {
        get {
            return slotsState;
        }
    }

    public int MySlot {
        get {
            foreach (int _slot in Slots.Keys) {
                if (slotsState[_slot] == username) {
                    return _slot;
                }
            }

            return -1;
        }
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        PreloadManager.instance.Ready(this);
    }

    #region Main Menu
    public void ConnectionRequested(string _username) {
        username = _username;

        Client.instance.ConnectToServer();
        Connection = ConnectionState.REQUESTING;

        connectionStatusText = GameObject.Find("StatusText").GetComponent<Text>();
        UpdateStatusText("Establishing TCP connection...");
        Invoke("CheckOnResponse", 2f);
    }

    public void WelcomeToServerReceived() {
        Debug.Log("WelcomeToServer message received!");
        UpdateStatusText("Establishing UDP connection...");
        Connection = ConnectionState.ESTABLISHING;
    }

    public void UDPHandshakeReceived() {
        Debug.Log("UDPHandshake message received!");
        UpdateStatusText("Connected. Requesting to join lobby...");
        Connection = ConnectionState.CONNECTED;

        ClientSend.RequestToJoinLobby();
    }

    private void CheckOnResponse() {
        switch (Connection) {
            case ConnectionState.JOINED:
            case ConnectionState.FAILED:
                return;

            case ConnectionState.REQUESTING:
                UpdateStatusText("Failed to establish a TCP connection. Make sure port 26950 is open for sending and 26951 is open for receiving.");
                break;

            case ConnectionState.ESTABLISHING:
                UpdateStatusText("Failed to establish a UDP connection. Make sure port 26950 is open for sending and 26951 is open for receiving.");
                break;

            case ConnectionState.CONNECTED:
                UpdateStatusText("Failed to join the lobby in the generic case. This shouldn't happen - contact Arold to report this issue.");
                break;

            default:
                Debug.Log("Default switch case reached, this shouldn't happen.");
                break;
        }

        Connection = ConnectionState.FAILED;
        Client.instance.Disconnect();
    }

    private void UpdateStatusText(string _text) {
        if (connectionStatusText == null) {
            return;
        }

        if (SceneManager.GetActiveScene().buildIndex != 1) { // main menu scene
            return;
        }

        connectionStatusText.text = _text;
    }
    #endregion

    #region Lobby
    public void WelcomeToLobbyReceived(Dictionary<int, string> _slotsState) {
        Debug.Log("WelcomeToLobby message received!");
        UpdateStatusText("Joining lobby...");
        Connection = ConnectionState.JOINED;

        slotsState = _slotsState;

        // load the lobby scene
        SceneManager.LoadScene(2);
    }

    public void GameInProgressReceived() {
        Debug.Log("GameInProgress messaged received!");
        UpdateStatusText("A game is already in progress! Try again later.");
        Connection = ConnectionState.FAILED;

        Client.instance.Disconnect();
    }

    public void VersionOutOfDateReceived() {
        Debug.Log("VersionOutOfDate message received!");
        UpdateStatusText("This game client is out of date! Try again after updating.");
        Connection = ConnectionState.FAILED;

        Client.instance.Disconnect();
    }

    public void UpdateLobbyReceived(Dictionary<int, string> _slotsState) {
        Debug.Log("UpdateLobby message received!");
        slotsState = _slotsState;

        if (LobbyManager.instance == null) {
            Debug.Log("LobbyManager.instance was null when UpdateLobby was received.");
            return;
        }

        LobbyManager.instance.UpdateSlots(_slotsState);
    }
    #endregion

    public void Disconnect() {
        Client.instance.Disconnect();

        // load the main menu scene
        SceneManager.LoadScene(1);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour {
    public static void WelcomeToServer(Packet _packet) {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"[WelcomeToServer] Message from server: {_msg}");
        Client.instance.myId = _myId;

        // Received via TCP, now time to hook up our UDP
        Client.instance.udp.Connect();

        // Update the ConnectionManager's connection state
        ConnectionManager.instance.WelcomeToServerReceived();

        // Send request to handshake via UDP
        ClientSend.UDPHandshakeRequest();
    }

    public static void UDPHandshakeReceived(Packet _packet) {
        if (ConnectionManager.instance.Connection != ConnectionManager.ConnectionState.ESTABLISHING) {
            return;
        }

        string _msg = _packet.ReadString();

        Debug.Log($"[UDPHandshakeReceived] Message from server: {_msg}");

        Debug.Log("Connection can be considered established at this point.");
        int _tcpLocalPort = ((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port;
        int _udpLocalPort = ((IPEndPoint)Client.instance.udp.socket.Client.LocalEndPoint).Port;
        Debug.Log("TCP port: " + _tcpLocalPort);
        Debug.Log("UDP port: " + _udpLocalPort);

        // Pass this on to our connection manager since we can consider the connection established at this point.
        // From the connection manager, we'll initiate our request to join the lobby.
        ConnectionManager.instance.UDPHandshakeReceived();
    }

    public static void WelcomeToLobby(Packet _packet) {
        int _userCount = _packet.ReadInt();

        Dictionary<int, string> slotsState = new Dictionary<int, string>();
        for (int i = 0; i < _userCount; i++) {
            int _slot = _packet.ReadInt();
            string _username = _packet.ReadString();

            slotsState.Add(_slot, _username);
        }

        ConnectionManager.instance.WelcomeToLobbyReceived(slotsState);
    }

    public static void GameInProgress(Packet _packet) {
        ConnectionManager.instance.GameInProgressReceived();
    }

    public static void VersionOutOfDate(Packet _packet) {
        ConnectionManager.instance.VersionOutOfDateReceived();
    }

    public static void UpdateLobby(Packet _packet) {
        Debug.Log("UpdateLobby received!");
        int _userCount = _packet.ReadInt();

        Dictionary<int, string> slotsState = new Dictionary<int, string>();
        for (int i = 0; i < _userCount; i++) {
            int _slot = _packet.ReadInt();
            string _username = _packet.ReadString();

            slotsState.Add(_slot, _username);
        }

        ConnectionManager.instance.UpdateLobbyReceived(slotsState);
    }

    public static void GameStarting(Packet _packet) {
        LobbyManager.instance.GameStartSignaled();
    }

    public static void PlayerDisconnected(Packet _packet) {
        int _id = _packet.ReadInt();

        GameMaster.instance.PlayerDisconnected(_id);
    }

    public static void TeamMessageReceived(Packet _packet) {
        int _fromSlot = _packet.ReadInt();
        string _msg = _packet.ReadString();

        string fromUser = ConnectionManager.instance.Slots[_fromSlot];
        ChatHandler.instance.NewMessage($"[TEAM] {fromUser}: {_msg}");
    }

    public static void AllMessageReceived(Packet _packet) {
        int _fromSlot = _packet.ReadInt();
        string _msg = _packet.ReadString();

        string fromUser = ConnectionManager.instance.Slots[_fromSlot];
        ChatHandler.instance.NewMessage($"[ALL] {fromUser}: {_msg}");
    }

    public static void UnitPositionUpdate(Packet _packet) {
        int _unitId = _packet.ReadInt();
        Vector3 _pos = _packet.ReadVector3();

        Unit unit = GameMaster.instance.GetUnitById(_unitId);
        if (unit == null) {
            Debug.Log("Unit was null! Returning");
            return;
        }

        unit.UpdatePosition(_pos);
    }

    public static void UnitRotationUpdate(Packet _packet) {
        int _unitId = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        Unit unit = GameMaster.instance.GetUnitById(_unitId);
        if (unit == null) {
            return;
        }

        unit.UpdateRotation(_rotation);
    }

    public static void NewSnapFormation(Packet _packet) {
        float _lineOfScrimmageLocation = _packet.ReadFloat();
        float _firstDownLocation = _packet.ReadFloat();
        GameState.Team _team = (GameState.Team)_packet.ReadInt();

        GameState.instance.hiked = false;
        GameMaster.instance.CreateSnapFormation(_lineOfScrimmageLocation, _firstDownLocation, _team);
    }

    public static void Hiked(Packet _packet) {
        int _unitId = _packet.ReadInt();
        Vector3 _origin = _packet.ReadVector3();
        Vector3 _dest = _packet.ReadVector3();

        GameState.instance.hiked = true;

        GameMaster.instance.HikeBall(_unitId, _origin, _dest);
    }

    public static void TurnoverOnDowns(Packet _packet) {
        PopupText.instance.Show("Turnover on downs!", 2f);
    }

    public static void GameState_Quarter(Packet _packet) {
        int _quarter = _packet.ReadInt();

        GameState.instance.SetQuarter(_quarter);
    }

    public static void GameState_StopClock(Packet _packet) {
        bool _stopClock = _packet.ReadBool();

        GameState.instance.SetClockStopped(_stopClock);
    }

    public static void GameState_TimeLeftInQuarter(Packet _packet) {
        float _timeLeftInQuarter = _packet.ReadFloat();

        GameState.instance.SetTimeLeftInQuarter(_timeLeftInQuarter);
    }

    public static void GameState_Score(Packet _packet) {
        int _redScore = _packet.ReadInt();
        int _greenScore = _packet.ReadInt();

        GameState.instance.SetScore(_redScore, _greenScore);
    }

    public static void GameState_Possession(Packet _packet) {
        GameState.Team _possession = (GameState.Team)_packet.ReadInt();

        GameState.instance.SetPossession(_possession);
    }

    public static void GameState_Down(Packet _packet) {
        int _down = _packet.ReadInt();

        GameState.instance.SetDown(_down);
    }

    public static void GameState_FieldPosition(Packet _packet) {
        float _lineOfScrimmageLocation = _packet.ReadFloat();
        float _firstDownLocation = _packet.ReadFloat();

        GameState.instance.SetFieldPosition(_lineOfScrimmageLocation, _firstDownLocation);
    }

    public static void UnitCharged(Packet _packet) {
        int _unitId = _packet.ReadInt();
        float _cooldown = _packet.ReadFloat();
        float _duration = _packet.ReadFloat();

        Unit unit = GameMaster.instance.GetUnitById(_unitId);
        if (unit == null) {
            return;
        }

        unit.ChargeIssued(_cooldown, _duration);
    }

    public static void UnitJuked(Packet _packet) {
        int _unitId = _packet.ReadInt();
        float _cooldown = _packet.ReadFloat();
        float _duration = _packet.ReadFloat();

        Unit unit = GameMaster.instance.GetUnitById(_unitId);
        if (unit == null) {
            return;
        }

        unit.JukeIssued(_cooldown, _duration);
    }

    public static void UnitTackled(Packet _packet) {
        int _unitTackling = _packet.ReadInt();
        int _unitBeingTackled = _packet.ReadInt();
        float _cooldown = _packet.ReadFloat();

        Unit unit = GameMaster.instance.GetUnitById(_unitTackling);
        if (unit == null) {
            return;
        }

        unit.TackleIssued(_cooldown);
    }

    public static void UnitStiffed(Packet _packet) {
        int _unitStiffing = _packet.ReadInt();
        int _unitBeingStiffed = _packet.ReadInt();
        float _cooldown = _packet.ReadFloat();

        Unit unit = GameMaster.instance.GetUnitById(_unitStiffing);
        if (unit == null) {
            return;
        }

        unit.StiffIssued(_cooldown);
    }

    public static void BallThrown(Packet _packet) {
        int _unitId = _packet.ReadInt();
        Vector3 _origin = _packet.ReadVector3();
        Vector3 _dest = _packet.ReadVector3();

        Debug.Log($"Distance: {Vector3.Distance(_origin, _dest)}");

        GameMaster.instance.ThrowBall(_unitId, _origin, _dest);
    }

    public static void BallUpdate(Packet _packet) {
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        int _frame = _packet.ReadInt();

        GameMaster.instance.UpdateBall(_position, _rotation, _frame);
    }

    public static void BallCaught(Packet _packet) {
        int _catcherId = _packet.ReadInt();
        int _throwerId = _packet.ReadInt();

        Unit catcher = GameMaster.instance.GetUnitById(_catcherId);

        string catcherName;
        if (ConnectionManager.instance.Slots.TryGetValue(catcher.playerSlotNumber, out catcherName)) {
            PopupText.instance.Show($"{catcherName} caught the ball!");
        } else {
            PopupText.instance.Show($"{catcher.transform.name} caught the ball!");
        }

        GameMaster.instance.BallCaught(catcher);
    }

    public static void HikeCaught(Packet _packet) {
        int _unitId = _packet.ReadInt();

        Unit unit = GameMaster.instance.GetUnitById(_unitId);
        if (unit == null) {
            return;
        }

        GameMaster.instance.BallCaught(unit);
    }

    public static void BallIncomplete(Packet _packet) {
        int _throwerId = _packet.ReadInt();
        Vector3 _landingPos = _packet.ReadVector3();

        PopupText.instance.Show("Incomplete!");
        EffectManager.instance.Show("BallLanded", _landingPos, 2f);
        AudioManager.instance.Play("IncompletePass");

        if (GameMaster.instance.ball != null) {
            GameMaster.instance.ball.Remove();
        }
    }

    public static void BallIntercepted(Packet _packet) {
        int _catcherId = _packet.ReadInt();
        int _throwerId = _packet.ReadInt();

        PopupText.instance.Show("Interception!");
        if (GameMaster.instance.ball != null) {
            GameMaster.instance.ball.Remove();
        }

        Unit catcher = GameMaster.instance.GetUnitById(_catcherId);
        if (catcher == null) {
            return;
        }

        catcher.GiveBall();
    }

    public static void UnitCantThrow(Packet _packet) {
        int _unitId = _packet.ReadInt();

        Unit unit = GameMaster.instance.GetUnitById(_unitId);

        if (unit == null) {
            return;
        }

        unit.canThrow = false;
    }

    public static void UnitScoredTouchdown(Packet _packet) {
        int _unitId = _packet.ReadInt();

        Unit unit = GameMaster.instance.GetUnitById(_unitId);
        if (unit == null) {
            PopupText.instance.Show("Touchdown!");
            AudioManager.instance.Play("Touchdown");
            return;
        }

        GameMaster.instance.Event_Touchdown(unit);
    }

    public static void UnitTackledWithBall(Packet _packet) {
        int _tackledId = _packet.ReadInt();
        int _tacklerId = _packet.ReadInt();

        Unit unit = GameMaster.instance.GetUnitById(_tackledId);
        if (unit == null) {
            return;
        }

        unit.RemoveBall();
        AudioManager.instance.Play("Tackled");
        EffectManager.instance.Show("TackledWithBall", unit.transform.position, 2f);
        Destroy(unit.gameObject);
    }

    public static void UnitWentOutWithBall(Packet _packet) {
        int _unitId = _packet.ReadInt();

        Unit unit = GameMaster.instance.GetUnitById(_unitId);
        if (unit == null) {
            return;
        }

        unit.RemoveBall();
        AudioManager.instance.Play("WentOutOfBounds");
        EffectManager.instance.Show("WentOutWithBall", unit.transform.position, 2f);
        Destroy(unit.gameObject);
    }

    public static void QuarterEnding(Packet _packet) {
        GameState GS = GameState.instance;
        PopupText PT = PopupText.instance;

        int quarter = GS.quarter;

        PopupText.instance.Show($"That's the end of the {quarter}{GameState.numberSuffixes[quarter - 1]} quarter!", 3f);
        
        if (quarter == 2) {
            PT.Show("Halftime! The team that did not win the coinflip at the start of the game will start with the ball!", 5f);
        } else if (quarter == 4) {
            if (GS.redScore == GS.greenScore) {
                PT.Show("The scores are tied! We're going into overtime!", 5f);
            } else {
                GameMaster.instance.Event_GameOver();
            }
        }
    }

    public static void Touchback(Packet _packet) {
        int _tackledId = _packet.ReadInt();

        Unit unit = GameMaster.instance.GetUnitById(_tackledId);
        if (unit == null) {
            return;
        }

        unit.RemoveBall();
        PopupText.instance.Show("Touchback!", 4f);
        AudioManager.instance.Play("Tackled");
        EffectManager.instance.Show("TackledWithBall", unit.transform.position, 2f);
        Destroy(unit.gameObject);
    }

    public static void Safety(Packet _packet) {
        int _tackledId = _packet.ReadInt();

        Unit unit = GameMaster.instance.GetUnitById(_tackledId);
        if (unit == null) {
            return;
        }

        unit.RemoveBall();
        PopupText.instance.Show("Safety!", 5f);
        AudioManager.instance.Play("Tackled");
        EffectManager.instance.Show("TackledWithBall", unit.transform.position, 2f);
        Destroy(unit.gameObject);
    }

    public static void FalseStart(Packet _packet) {

        int _unitId = _packet.ReadInt();

        Unit unit = GameMaster.instance.GetUnitById(_unitId);
        if (unit == null) {
            return;
        }

        if (unit.HasBall()) {
            unit.RemoveBall();
        }

        PopupText.instance.Show("False start! -5 yards");
        AudioManager.instance.Play("Tackled");
        EffectManager.instance.Show("TackledWithBall", unit.transform.position, 2f);
        Destroy(unit.gameObject);
    }
}

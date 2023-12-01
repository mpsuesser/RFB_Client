using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet) {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet) {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    public static void UDPHandshakeRequest() {
        Debug.Log("Starting UDPHandshakeRequest send coroutine.");
        ConnectionManager.instance.StartCoroutine(SendUDPHandshakeRequest());
    }

    private static IEnumerator SendUDPHandshakeRequest() {
        Debug.Log("Sending UDPHandshakeRequest packets.");
        while (ConnectionManager.instance.Connection == ConnectionManager.ConnectionState.ESTABLISHING) {
            using (Packet _packet = new Packet((int)ClientPackets.udpHandshakeRequest)) {
                _packet.Write(Client.instance.myId);
                _packet.Write(ConnectionManager.instance.username);
                _packet.Write(Constants.VERSION);

                SendUDPData(_packet);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public static void RequestToJoinLobby() {
        using (Packet _packet = new Packet((int)ClientPackets.requestToJoinLobby)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(ConnectionManager.instance.username);

            Debug.Log("Sending RequestToJoinLobby");
            SendTCPData(_packet);
        }
    }

    public static void UpdateLobbySlot(int _slot) {
        using (Packet _packet = new Packet((int)ClientPackets.updateLobbySlot)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_slot);

            SendTCPData(_packet);
        }
    }

    public static void BeginGame() {
        using (Packet _packet = new Packet((int)ClientPackets.beginGame)) {
            _packet.Write(Client.instance.myId);

            SendTCPData(_packet);
        }
    }

    public static void MoveToIssued(int _unitId, Vector3 _dest, bool _shiftHeld) {
        using (Packet _packet = new Packet((int)ClientPackets.moveToIssued)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_unitId);
            _packet.Write(_dest);
            _packet.Write(_shiftHeld);

            SendTCPData(_packet);
        }
    }

    public static void UnitRightClicked(int _clickerUnit, int _clickedUnit, bool _shiftHeld) {
        using (Packet _packet = new Packet((int)ClientPackets.unitRightClicked)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_clickerUnit);
            _packet.Write(_clickedUnit);
            _packet.Write(_shiftHeld);

            SendTCPData(_packet);
        }
    }

    public static void BroadcastMessageToTeam(string _msg) {
        using (Packet _packet = new Packet((int)ClientPackets.broadcastMessageToTeam)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_msg);

            SendTCPData(_packet);
        }
    }

    public static void BroadcastMessageToAll(string _msg) {
        using (Packet _packet = new Packet((int)ClientPackets.broadcastMessageToAll)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_msg);

            SendTCPData(_packet);
        }
    }

    public static void UnitCharge(int _unitId) {
        using (Packet _packet = new Packet((int)ClientPackets.unitCharge)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_unitId);

            SendTCPData(_packet);
        }
    }

    public static void UnitJuke(int _unitId) {
        using (Packet _packet = new Packet((int)ClientPackets.unitJuke)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_unitId);

            SendTCPData(_packet);
        }
    }

    public static void UnitTackle(int _unitTackling, int _unitBeingTackled, bool _shiftHeld) {
        using (Packet _packet = new Packet((int)ClientPackets.unitTackle)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_unitTackling);
            _packet.Write(_unitBeingTackled);
            _packet.Write(_shiftHeld);

            SendTCPData(_packet);
        }
    }

    public static void UnitStiff(int _unitStiffing, int _unitBeingStiffed, bool _shiftHeld) {
        using (Packet _packet = new Packet((int)ClientPackets.unitStiff)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_unitStiffing);
            _packet.Write(_unitBeingStiffed);
            _packet.Write(_shiftHeld);

            SendTCPData(_packet);
        }
    }

    public static void UnitThrow(int _unitId, Vector3 _dest) {
        using (Packet _packet = new Packet((int)ClientPackets.unitThrow)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_unitId);
            _packet.Write(_dest);

            SendTCPData(_packet);
        }
    }

    public static void UnitHike(int _unitId) {
        using (Packet _packet = new Packet((int)ClientPackets.unitHike)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_unitId);

            SendTCPData(_packet);
        }
    }

    public static void UnitStop(int _unitId) {
        using (Packet _packet = new Packet((int)ClientPackets.unitStop)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(_unitId);

            SendTCPData(_packet);
        }
    }
    #endregion
}

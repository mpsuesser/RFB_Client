using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager {
    public int id;
    public string username;
    public int slotNumber;

    public PlayerManager(int _id, string _username, int _slotNumber) {
        id = _id;
        username = _username;
        slotNumber = _slotNumber;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaveButton : MonoBehaviour
{
    public void Pushed() {
        ConnectionManager.instance.Disconnect();
    }
}

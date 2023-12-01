using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour {
    public static LobbyManager instance;

    public Font filledSlotFont;

    public LobbyButton[] slots;

    void Awake() {
        #region Singleton
        if (instance != null) {
            Debug.Log("More than one LobbyManager in scene!");
            return;
        }

        instance = this;
        #endregion
    }

    void Start() {
        // load the slots, since ConnectionManager will have this property populated by the time this is loaded, since the scene load is called from there
        Dictionary<int, string> slotsState = ConnectionManager.instance.Slots;
        UpdateSlots(slotsState);
    }

    // should be called at Start() and every time we receive an updateLobby packet
    public void UpdateSlots(Dictionary<int, string> slotsState) {
        foreach (LobbyButton button in slots) {
            if (slotsState.ContainsKey(button.slot)) {
                button.FillSlot(slotsState[button.slot], filledSlotFont);
            } else {
                button.ClearSlot();
            }
        }
    }

    public void LobbyButtonPressed(LobbyButton _button) {
        if (_button.Filled) {
            return;
        }

        ClientSend.UpdateLobbySlot(_button.slot);

        Debug.Log($"Button pressed for slot {_button.slot}!");
    }

    public void GameStartSignaled() {
        SceneManager.LoadScene(3);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyButton : MonoBehaviour
{
    public int slot;

    private Text textBox;
    private Font originalFont;
    private string originalText;

    public bool Filled {
        get {
            return textBox.text != originalText;
        }
    }

    public void Awake() {
        textBox = GetComponentInChildren<Text>();
        originalFont = textBox.font;
        originalText = textBox.text;
    }

    public void Pressed() {
        LobbyManager.instance.LobbyButtonPressed(this);
    }

    public void FillSlot(string _playerName, Font _filledFont) {
        textBox.text = _playerName;
        textBox.font = _filledFont;
    }

    public void ClearSlot() {
        textBox.text = originalText;
        textBox.font = originalFont;
    }
}

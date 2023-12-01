using UnityEngine;
using UnityEngine.UI;

public class ButtonHotkey : MonoBehaviour
{
    public KeyCode key;

    void Update() {
        if (Input.GetKeyDown(key)
            && (ChatHandler.instance == null || !ChatHandler.instance.IsTyping())
            && (PauseMenu.instance == null || !PauseMenu.instance.ui.activeSelf)) {
            GetComponent<Button>().onClick.Invoke();
        }
    }
}

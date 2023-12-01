using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatHandler : MonoBehaviour
{
    public static ChatHandler instance;
    void Awake() {
        if (instance != null) {
            Debug.LogError("More than one GameMaster instance in scene!");
            return;
        }
        instance = this;
    }

    private List<string> chatHistory;

    private enum TypingStatus {
        NOT_TYPING = 1,
        TEAM,
        ALL
    }
    private TypingStatus typing;

    public InputField chatInput;
    public Transform textArea;
    public Text textPrefab;
    public float showTime = 5f;

    public Color deactivatedInput;
    public Color deactivatedTextArea;
    public Color activatedInput;
    public Color activatedTextArea;

    public void NewMessage(string _msg) {
        Text newText = (Text)Instantiate(textPrefab, textArea);
        newText.text = _msg;
        Destroy(newText.gameObject, showTime);
    }

    void Start() {
        typing = TypingStatus.NOT_TYPING;
        CloseChat();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return) && (PauseMenu.instance == null || !PauseMenu.instance.ui.activeSelf)) {
            ToggleChatting();
        }
    }

    private void ToggleChatting() {
        if (typing == TypingStatus.NOT_TYPING) {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                typing = TypingStatus.ALL;
            } else {
                typing = TypingStatus.TEAM;
            }

            OpenChat();
        } else {
            if (chatInput.text != null && chatInput.text.Trim().Length > 0) {
                if (typing == TypingStatus.ALL) {
                    ClientSend.BroadcastMessageToAll(chatInput.text.Trim());
                } else {
                    ClientSend.BroadcastMessageToTeam(chatInput.text.Trim());
                }
            }

            typing = TypingStatus.NOT_TYPING;
            CloseChat();
        }
    }

    private void CloseChat() {
        chatInput.text = "";

        // Changing the colors
        ColorBlock cbInput = chatInput.colors;
        cbInput.selectedColor = deactivatedInput;
        chatInput.colors = cbInput;

        chatInput.gameObject.GetComponent<Image>().color = deactivatedInput;
        textArea.gameObject.GetComponent<Image>().color = deactivatedTextArea;

        chatInput.DeactivateInputField();
    }

    private void OpenChat() {
        textArea.gameObject.GetComponent<Image>().color = activatedTextArea;

        chatInput.ActivateInputField();
        // Changing the colors
        // TODO: fix
        ColorBlock cbInput = chatInput.colors;
        cbInput.selectedColor = activatedInput;
        chatInput.colors = cbInput;
    }

    public bool IsTyping() {
        return typing != TypingStatus.NOT_TYPING;
    }
}

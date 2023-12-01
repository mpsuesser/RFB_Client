using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JukeCooldownTracker : MonoBehaviour {
    public Text textObject;
    private string originalText;
    private Button jukeButton;

    void Awake() {
        jukeButton = GetComponent<Button>();
        originalText = textObject.text;
    }

    // Update is called once per frame
    void Update() {
        Unit selected = UnitSelection.instance.CurrentlySelected;
        if (selected != null && selected.IsJukeOnCooldown()) {
            float cooldown = selected.GetJukeCooldown();
            textObject.text = ((int)cooldown).ToString();
            jukeButton.interactable = false;
        } else {
            textObject.text = originalText;
            jukeButton.interactable = true;
        }
    }
}

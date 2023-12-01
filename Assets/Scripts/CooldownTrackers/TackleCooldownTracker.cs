using System;
using UnityEngine;
using UnityEngine.UI;

public class TackleCooldownTracker : MonoBehaviour {
    public Text textObject;
    private string originalText;
    private Button tackleButton;

    void Awake() {
        tackleButton = GetComponent<Button>();
        originalText = textObject.text;
    }

    // Update is called once per frame
    void Update() {
        Unit selected = UnitSelection.instance.CurrentlySelected;
        if (selected != null && selected.IsTackleOnCooldown()) {
            float cooldown = selected.GetTackleCooldown();
            textObject.text = ((int)cooldown).ToString();
            tackleButton.interactable = false;
        } else {
            textObject.text = originalText;
            tackleButton.interactable = true;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;

public class StiffCooldownTracker : MonoBehaviour {
    public Text textObject;
    private string originalText;
    private Button stiffButton;

    void Awake() {
        stiffButton = GetComponent<Button>();
        originalText = textObject.text;
    }

    // Update is called once per frame
    void Update() {
        Unit selected = UnitSelection.instance.CurrentlySelected;
        if (selected != null && selected.IsStiffOnCooldown()) {
            float cooldown = selected.GetStiffCooldown();
            textObject.text = ((int)cooldown).ToString();
            stiffButton.interactable = false;
        } else {
            textObject.text = originalText;
            stiffButton.interactable = true;
        }
    }
}

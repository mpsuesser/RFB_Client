using System;
using UnityEngine;
using UnityEngine.UI;

public class ChargeCooldownTracker : MonoBehaviour {
    public Text textObject;
    private string originalText;
    private Button chargeButton;

    void Awake() {
        chargeButton = GetComponent<Button>();
        originalText = textObject.text;
    }

    // Update is called once per frame
    void Update() {
        Unit selected = UnitSelection.instance.CurrentlySelected;
        if (selected != null && selected.IsChargeOnCooldown()) {
            float cooldown = selected.GetChargeCooldown();
            textObject.text = ((int)cooldown).ToString();
            chargeButton.interactable = false;
        } else {
            textObject.text = originalText;
            chargeButton.interactable = true;
        }
    }
}

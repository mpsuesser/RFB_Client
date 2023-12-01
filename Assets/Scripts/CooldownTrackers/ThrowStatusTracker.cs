using System;
using UnityEngine;
using UnityEngine.UI;

public class ThrowStatusTracker : MonoBehaviour {
    private Button throwButton;

    void Awake() {
        throwButton = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update() {
        Unit selected = UnitSelection.instance.CurrentlySelected;
        if (selected != null && selected.CanThrow()) {
            throwButton.interactable = true;
        } else {
            throwButton.interactable = false;
        }
    }
}

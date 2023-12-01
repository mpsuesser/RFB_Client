using System;
using UnityEngine;
using UnityEngine.UI;

public class HikeStatusTracker : MonoBehaviour {
    private Button hikeButton;

    void Awake() {
        hikeButton = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update() {
        Unit selected = UnitSelection.instance.CurrentlySelected;
        if (selected != null && selected.CanHike() && !GameState.instance.hiked) {
            hikeButton.interactable = true;
        } else {
            hikeButton.interactable = false;
        }
    }
}

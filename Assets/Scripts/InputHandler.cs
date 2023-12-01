using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    public PauseMenu pauseMenu;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            // If we have a throw queued up, go forward with throwing it
            if (AbilityHandler.instance.IsThrowQueued) {
                AbilityHandler.instance.Throw(Vector3.zero); // TODO: clean this up, messy implementation
            } else if (AbilityHandler.instance.IsTackleQueued) {
                AbilityHandler.instance.Tackle();
            } else if (AbilityHandler.instance.IsStiffQueued) {
                AbilityHandler.instance.Stiff();
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            RightClickHandler.instance.OnRightClick();
        }

        if ((Input.GetKeyDown(KeyCode.F10) || Input.GetKeyDown(KeyCode.P))
            && !ChatHandler.instance.IsTyping()) {
            pauseMenu.Toggle();
        }
    }
}

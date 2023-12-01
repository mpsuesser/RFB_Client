﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightClickHandler : MonoBehaviour {
    public GameObject moveToArrowSprite;

    // Singleton pattern
    public static RightClickHandler instance;
    void Awake() {
        if (instance != null) {
            Debug.LogError("More than one RightClickHandler instance in scene!");
            return;
        }
        instance = this;
    }

    public void OnRightClick() {
        // just clear the queue if we have something queued up
        if (AbilityHandler.instance.IsAnythingQueued()) {
            AbilityHandler.instance.ClearQueues();
            return;
        }

        // we'll handle minimap right clicks in the Minimap class
        if (Minimap.instance.IsClickInMinimap(Input.mousePosition)) {
            return;
        }

        List<Unit> selectedList = UnitSelection.instance.SelectedUnits;
        if (selectedList.Count == 0) {
            return;
        }

        // Cast a ray from the mouse cursor's position
        Ray clickPoint = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hitPoints = Physics.RaycastAll(clickPoint);

        // Check if the ray collided with any objects
        if (hitPoints.Length > 0) {
            // Then do our custom preferential raycast that will get the closest unit and prefer unselected units
            Unit unitHit = UnitSelection.instance.RaycastPreferNotSelected(clickPoint, 300f);

            // if any unit was picked up by that raycast:
            if (unitHit != null) {
                if (selectedList.Count > 1) {
                    // if we have multiple units selected, all selected units but the one clicked will move to that unit
                    foreach (Unit unit in selectedList) {
                        if (unit != unitHit) {
                            ClientSend.UnitRightClicked(unit.unitId, unitHit.unitId, Input.GetKey(KeyCode.LeftShift));
                        }
                    }
                } else {
                    // if we have just one unit selected, make sure it wasn't that unit clicked, then move to it
                    if (unitHit != selectedList[0]) {
                        ClientSend.UnitRightClicked(selectedList[0].unitId, unitHit.unitId, Input.GetKey(KeyCode.LeftShift));
                    }
                }

                return;
            }

            // if no unit was clicked, we loop through and check for a field spot that was hit
            foreach (RaycastHit hitPoint in hitPoints) {
                if (hitPoint.transform.gameObject.tag == "Field") {
                    Vector3 pos = hitPoint.point;

                    foreach (Unit unit in selectedList) {
                        ClientSend.MoveToIssued(unit.unitId, pos, Input.GetKey(KeyCode.LeftShift));
                    }

                    // Create the arrow animation
                    pos.y += 1f;
                    GameObject moveToArrow1 = Instantiate(moveToArrowSprite, pos, Quaternion.identity);
                    Destroy(moveToArrow1, 0.25f);

                    return;
                }
            }
        }
    }
}

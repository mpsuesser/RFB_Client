using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityHandler : MonoBehaviour
{
    // Singleton pattern
    public static AbilityHandler instance;
    void Awake() {
        if (instance != null) {
            Debug.LogError("More than one AbilityHandler instance in scene!");
            return;
        }
        instance = this;
    }

    public LayerMask throwLayerMask;

    private AudioManager AM;

    void Start() {
        AM = AudioManager.instance;
    }

    void Update() {
        if(Input.GetMouseButtonUp(0)) {
            if (abilityJustUsed) {
                Invoke("UnsetAbilityJustUsed", .02f);
            }
        }
    }

    void UnsetAbilityJustUsed() { // for selection reasons, see UnitSelection for logic
        abilityJustUsed = false;
    }

    // ----- Queue status trackers -----
    private bool throwQueued = false;
    public bool IsThrowQueued { get { return throwQueued; } }

    private bool tackleQueued = false;
    public bool IsTackleQueued { get { return tackleQueued; } }

    private bool stiffQueued = false;
    public bool IsStiffQueued { get { return stiffQueued; } }

    public bool IsAnythingQueued() {
        return throwQueued || tackleQueued || stiffQueued;
    }

    private bool abilityJustUsed = false;
    public bool AbilityJustUsed { get { return abilityJustUsed; } }

    // ----- Actions -----
    public void Throw(Vector3 _location) {
        Unit currentlySelected = UnitSelection.instance.CurrentlySelected;
        ClearQueues();

        if (currentlySelected == null) {
            Debug.Log("Currently selected unit was null when attempting to throw.");
            return;
        }

        if (!currentlySelected.CanThrow()) {
            Debug.Log("The selected unit attempted to throw but CanThrow was false.");
            return;
        }

        Vector3 throwLocation;

        if (_location == Vector3.zero) {
            // Cast a ray from the mouse cursor's position
            Ray clickPoint = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitPoint;

            // Check if the ray collided with an object
            if (Physics.Raycast(clickPoint, out hitPoint, Mathf.Infinity, throwLayerMask)) {
                throwLocation = hitPoint.point;
            } else {
                return;
            }
        } else {
            throwLocation = _location; // minimap throw
        }

        throwLocation.y = 0f; // make sure the throw is aimed at the ground
        abilityJustUsed = true;
        ClientSend.UnitThrow(currentlySelected.unitId, throwLocation);
    }

    public void Tackle() {
        Unit currentlySelected = UnitSelection.instance.CurrentlySelected;
        List<Unit> selectedList = UnitSelection.instance.SelectedUnits;

        if (currentlySelected == null) {
            Debug.Log("Currently selected unit was null when attempting to tackle.");
            return;
        }

        if (currentlySelected.IsTackleOnCooldown()) {
            Debug.Log("The selected unit attempted to tackle but it was on cooldown.");
            return;
        }

        // Cast a ray from the mouse cursor's position
        Ray clickPoint = Camera.main.ScreenPointToRay(Input.mousePosition);
        Unit unitHit = UnitSelection.instance.RaycastPreferNotSelected(clickPoint, 300f);

        // Check if the ray collided with a unit
        if (unitHit != null) {
            if (selectedList.Count > 1) {
                // if we have multiple units selected, all selected units but the one clicked will attempt to tackle that unit
                foreach (Unit unit in selectedList) {
                    if (unit != unitHit && !unit.IsTackleOnCooldown()) {
                        ClientSend.UnitTackle(unit.unitId, unitHit.unitId, Input.GetKey(KeyCode.LeftShift));
                    }
                }
            } else {
                // if we have just one unit selected, make sure it wasn't that unit clicked, then tackle it
                if (unitHit != selectedList[0]) {
                    ClientSend.UnitTackle(selectedList[0].unitId, unitHit.unitId, Input.GetKey(KeyCode.LeftShift));
                }
            }

            abilityJustUsed = true;
            ClearQueues();
        }
    }

    public void Stiff() {
        Unit currentlySelected = UnitSelection.instance.CurrentlySelected;
        List<Unit> selectedList = UnitSelection.instance.SelectedUnits;

        if (currentlySelected == null) {
            Debug.Log("Currently selected unit was null when attempting to stiff.");
            return;
        }

        if (currentlySelected.IsStiffOnCooldown()) {
            Debug.Log("The selected unit attempted to stiff but it was on cooldown.");
            return;
        }

        // Cast a ray from the mouse cursor's position
        Ray clickPoint = Camera.main.ScreenPointToRay(Input.mousePosition);
        Unit unitHit = UnitSelection.instance.RaycastPreferNotSelected(clickPoint, 300f);

        // Check if the ray collided with a unit
        if (unitHit != null) {
            if (selectedList.Count > 1) {
                // if we have multiple units selected, all selected units but the one clicked will attempt to stiff that unit
                foreach (Unit unit in selectedList) {
                    if (unit != unitHit && !unit.IsStiffOnCooldown()) {
                        ClientSend.UnitStiff(unit.unitId, unitHit.unitId, Input.GetKey(KeyCode.LeftShift));
                    }
                }
            } else {
                // if we have just one unit selected, make sure it wasn't that unit clicked, then stiff it
                if (unitHit != selectedList[0]) {
                    ClientSend.UnitStiff(selectedList[0].unitId, unitHit.unitId, Input.GetKey(KeyCode.LeftShift));
                }
            }

            abilityJustUsed = true;
            ClearQueues();
        }
    }

    public void Charge() {
        Unit currentlySelected = UnitSelection.instance.CurrentlySelected;
        if (currentlySelected == null) {
            return;
        }

        if (currentlySelected.IsChargeOnCooldown()) {
            AM.Play("CantDoThat");
            return;
        }

        ClientSend.UnitCharge(currentlySelected.unitId);
    }

    public void Juke() {
        Unit currentlySelected = UnitSelection.instance.CurrentlySelected;
        if (currentlySelected == null) {
            return;
        }

        if (currentlySelected.IsJukeOnCooldown()) {
            AM.Play("CantDoThat");
            return;
        }

        ClientSend.UnitJuke(currentlySelected.unitId);
    }

    public void Hike() {
        Unit currentlySelected = UnitSelection.instance.CurrentlySelected;
        if (currentlySelected == null) {
            return;
        }

        ClientSend.UnitHike(currentlySelected.unitId);
    }

    public void Stop() {
        List<Unit> selectedList = UnitSelection.instance.SelectedUnits;

        if (selectedList.Count == 0) {
            return;
        }

        foreach (Unit unit in selectedList) {
            ClientSend.UnitStop(unit.unitId);
        }
    }

    // ----- Queues -----
    public void QueueThrow() {
        Unit selected = UnitSelection.instance.CurrentlySelected;
        if (selected != null && selected.CanThrow()) {
            ClearQueues();
            throwQueued = true;
            CursorController.instance.SetThrowCursor();
            UnitSelection.instance.HideAbilities();
        }
    }

    public void QueueTackle() {
        Unit selected = UnitSelection.instance.CurrentlySelected;
        if (selected == null) {
            return;
        }

        if (selected.IsTackleOnCooldown()) {
            AM.Play("CantDoThat");
            return;
        }

        ClearQueues();
        tackleQueued = true;
        CursorController.instance.SetTackleCursor();
        UnitSelection.instance.HideAbilities();

        Debug.Log("Tackle queued!");
    }


    public void QueueStiff() {
        Unit selected = UnitSelection.instance.CurrentlySelected;
        if (selected == null) {
            return;
        }

        if (selected.IsStiffOnCooldown()) {
            AM.Play("CantDoThat");
            return;
        }

        ClearQueues();
        stiffQueued = true;
        CursorController.instance.SetStiffCursor();
        UnitSelection.instance.HideAbilities();
    }

    // ----- Dequeue -----
    public void ClearQueues() {
        throwQueued = false;
        tackleQueued = false;
        stiffQueued = false;

        CursorController.instance.ResetCursor();
        UnitSelection.instance.ShowAbilities();
    }
}

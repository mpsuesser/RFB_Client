using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lineman : Unit {
    public override void ChildUpdate() {
        if (!GameState.instance.hiked) {
            this.anim.Play("Lineman_BeforeSnap");
        }
    }
}

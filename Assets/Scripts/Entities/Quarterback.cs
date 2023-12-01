using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quarterback : Unit
{
    public override bool CanHike() {
        return !GameState.instance.hiked && (GameState.instance.possession == this.Team);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public GameObject ui;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            OpenScoreboard();
        }

        if (Input.GetKeyUp(KeyCode.Tab)) {
            CloseScoreboard();
        }
    }

    public void OpenScoreboard() {
        ui.SetActive(true);
    }

    public void CloseScoreboard() {
        ui.SetActive(false);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    private GameState GS;

    public Text clock;
    public Text redScore;
    public Text greenScore;
    public Text quarter;
    public Text down;
    public Text toGo;

    void Start()
    {
        GS = GameState.instance;
        if (GS == null) {
            Debug.Log("GameState was null!");
            Destroy(gameObject);
        }

        clock.text = "NN:NN";
        redScore.text = "X";
        greenScore.text = "X";
        quarter.text = "X";
        down.text = "X";
        toGo.text = "XX";

        GS.OnScoreChange += UpdateScore;
        GS.OnQuarterChange += UpdateQuarter;
        GS.OnDownChange += UpdateDown;
        GS.OnToGoChange += UpdateToGo;

        StartCoroutine(UpdateClock());
    }

    public void UpdateScore(int _red, int _green) {
        redScore.text = _red.ToString();
        greenScore.text = _green.ToString();
    }

    public void UpdateQuarter(int _quarter) {
        quarter.text = _quarter.ToString();
    }

    public void UpdateDown(int _down) {
        down.text = _down.ToString();
    }

    public void UpdateToGo(int _toGo) {
        toGo.text = _toGo.ToString();
    }

    private IEnumerator UpdateClock() {
        while(true) {
            int ceil = Mathf.Max(0, (int)Mathf.Ceil(GS.timeLeftInQuarter));
            int minutes = ceil / 60;
            int seconds = ceil - (minutes * 60);

            clock.text = $"{minutes.ToString("D2")}:{seconds.ToString("D2")}";

            yield return new WaitForSeconds(1);
        }
    }
}

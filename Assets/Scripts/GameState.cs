using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour {
    // Singleton pattern
    public static GameState instance;
    void Awake() {
        if (instance != null) {
            Debug.LogError("More than one GameState instance in scene!");
            return;
        }
        instance = this;
    }

    private static GameMaster GM;

    public enum Team {
        Red = 0,
        Green
    }

    public int quarter;
    public float timeLeftInQuarter;
    public bool clockStopped;
    public int redScore = 0;
    public int greenScore = 0;

    public bool hiked;
    public Team possession;
    public int down;
    public static string[] numberSuffixes = new string[4] { "st", "nd", "rd", "th" };
    public float firstDownLocation;
    public float lineOfScrimmageLocation;

    public Text gameClock;
    public Text fieldPosition;
    public Text redScoreDisplay;
    public Text greenScoreDisplay;

    // Observer pattern actions
    public event Action<int> OnDownChange;
    public event Action<int> OnQuarterChange;
    public event Action<int> OnToGoChange;
    public event Action<int,int> OnScoreChange;

    // Start is called before the first frame update
    void Start()
    {
        GM = GameMaster.instance;
    }

    void Update() {
        if (!clockStopped) {
            timeLeftInQuarter -= Time.deltaTime;
        }

        UpdateClockText();
    }

    void UpdateClockText() {
        int ceil = Mathf.Max(0, (int)Mathf.Ceil(timeLeftInQuarter));
        int minutes = ceil / 60;
        int seconds = ceil - (minutes * 60);

        gameClock.text = System.String.Format("{0}:{1:00} Q{2}",
            minutes,
            seconds,
            quarter
        );
    }

    // Update game state
    public void SetDown(int _down) {
        down = _down;

        UpdateFieldPositionText();
        OnDownChange?.Invoke(_down);
    }

    public void SetFieldPosition(float _lineOfScrimmageLocation, float _firstDownLocation) {
        lineOfScrimmageLocation = _lineOfScrimmageLocation;
        firstDownLocation = _firstDownLocation;

        UpdateFieldPositionText();
        OnToGoChange?.Invoke(Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(_lineOfScrimmageLocation - _firstDownLocation))));
    }

    public void SetScore(int redPoints, int greenPoints) {
        redScore = redPoints;
        greenScore = greenPoints;

        UpdateScoreText();
        OnScoreChange?.Invoke(redPoints, greenPoints);
    }

    public void SetTimeLeftInQuarter(float _time) {
        timeLeftInQuarter = _time;
    }

    public void SetClockStopped(bool _clockStopped) {
        clockStopped = _clockStopped;
    }

    public void SetQuarter(int _quarter) {
        quarter = _quarter;

        switch(quarter) {
            case 1:
            case 3:
                PopupText.instance.Show($"The {quarter}{numberSuffixes[quarter-1]} quarter has begun!");
                break;
            case 2:
                break;
            case 4:
                break;
            default:
                Debug.Log("Overtime, not supported in client GameState.SetQuarter");
                break;
        }

        OnQuarterChange?.Invoke(_quarter);
    }

    public void SetPossession(Team _team) {
        possession = _team;
    }

    public void SetHiked(bool _hiked) {
        hiked = _hiked;
    }


    // Reflect updates in UI
    private void UpdateScoreText() {
        redScoreDisplay.text = redScore.ToString();
        greenScoreDisplay.text = greenScore.ToString();
    }

    private void UpdateFieldPositionText() {
        int yardsToGo = (int)Mathf.Round(Mathf.Abs(firstDownLocation - lineOfScrimmageLocation));

        fieldPosition.text = System.String.Format("{0}{1} & {2}",
            down,
            numberSuffixes[down - 1],
            yardsToGo == 0 ? "inches" : yardsToGo.ToString()
        );
    }

    // TO GO
    public void SwapPossession() {
        if (possession == Team.Red) {
            possession = Team.Green;
        } else {
            possession = Team.Red;
        }
    }
}

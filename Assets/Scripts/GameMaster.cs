using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public static GameMaster instance; // singleton

    public Ball ballPrefab;
    public Ball ball; // currently instantiated ball

    public GameObject northEndzoneTrigger;
    public GameObject southEndzoneTrigger;
    public GameObject snapFormationPrefab;
    public GameObject firstDownLinePrefab;
    private GameObject snapFormation;
    private GameObject firstDownLine;

    private bool IsRedPossession { get { return GS.possession == GameState.Team.Red; } }

    private GameState GS;
    private AudioManager AM;
    private PopupText PT;
    private ChatHandler CH;
    private EffectManager EM;

    private Dictionary<int, Unit> unitMap;
    private static bool[] slotTaken = new bool[12]; // default bool value is false
    private static int[] slotPriority = new int[12] { 1, 7, 6, 12, 5, 11, 2, 8, 3, 9, 4, 10 };

    // INT: networking client ID
    // PlayerManager: stores info related to the client
    public static Dictionary<int, PlayerManager> players;

    void Awake() {
        #region Singleton
        if (instance != null) {
            Debug.LogError("More than one GameMaster instance in scene!");
            return;
        }

        instance = this;
        #endregion

        players = new Dictionary<int, PlayerManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        GS = GameState.instance;
        AM = AudioManager.instance;
        PT = PopupText.instance;
        CH = ChatHandler.instance;
        EM = EffectManager.instance;
    }

    public void CreateSnapFormation(float lineOfScrimmageLocation, float firstDownLocation, GameState.Team possession) {
        if (snapFormation != null) Destroy(snapFormation);
        if (firstDownLine != null) Destroy(firstDownLine);

        snapFormation = Instantiate(snapFormationPrefab, new Vector3(0f, 0f, lineOfScrimmageLocation), Quaternion.identity);
        firstDownLine = Instantiate(firstDownLinePrefab, new Vector3(0f, -1f, firstDownLocation), Quaternion.identity);

        string teamWithBall = (possession == GameState.Team.Red) ? "Team1" : "Team2";

        // Populate the unit map with the snap formation we just instantiated
        unitMap = new Dictionary<int, Unit>();
        Unit[] units = snapFormation.GetComponentsInChildren<Unit>();
        if (units == null) {
            Debug.Log("Units were null when trying set the unit map on snap formation.");
        } else {
            for (int i = 0; i < units.Length; i++) {
                unitMap.Add(units[i].unitId, units[i]);

                if (units[i].gameObject.tag != teamWithBall) {
                    units[i].canThrow = false;
                }
            }
        }

        CameraController.instance.FocusOn(new Vector3(0, 0, lineOfScrimmageLocation));
        AbilityHandler.instance.ClearQueues();
        UnitSelection.instance.DeselectAll();
    }

    // ----- Events -----
    public void Event_Touchdown(Unit unit) {
        AM.Play("Touchdown");
        PT.Show("Touchdown!");
        EM.Show("Touchdown", unit.transform.position, 2f);

        unit.RemoveBall();
    }

    public void Event_Interception(Unit catcher, Ball ball) {
        AM.Play("Interception");
        PT.Show("Interception!");

        GS.SwapPossession();
    }

    public void Event_GameOver() {
        if (GS.redScore > GS.greenScore) {
            PT.Show("That's the game! Red team wins!", 60f);
        } else {
            PT.Show("That's the game! Green team wins!", 60f);
        }

        PT.Show("This lobby will remain open for 60 seconds before closing.");
        Invoke("DisconnectClient", 60f);
    }

    private void DisconnectClient() {
        ConnectionManager.instance.Disconnect();
    }

    #region Networking
    public void PlayerConnected(int _id, string _username, int _slotNumber) {
        PlayerManager player = new PlayerManager(_id, _username, _slotNumber);
        players.Add(_id, player);

        if (_id == Client.instance.myId) {
            CH.NewMessage($"[TEST] You connected with ID {_id}, user {_username}, slot# {_slotNumber}...");
        } else {
            CH.NewMessage($"[TEST] {_username} connected w/ ID {_id}, slot# {_slotNumber}.");
        }
    }

    public void PlayerDisconnected(int _id) {
        if (!players.ContainsKey(_id)) {
            return;
        }

        PT.Show($"{players[_id].username} has disconnected.");

        players.Remove(_id);
    }

    public void ThrowBall(int _throwerId, Vector3 _origin, Vector3 _dest) {
        Unit thrower = GetUnitById(_throwerId);
        if (thrower != null) {
            thrower.Threw();
        }

        ball = Instantiate(ballPrefab, _origin, Quaternion.identity);
        ball.SetOrigin(_origin);
        ball.SetDestination(_dest);
        ball.CreateMinimapSprite();

        EM.Show("Throw", _origin, 2f);
        AM.Play("Throw");
    }

    public void HikeBall(int _unitId, Vector3 _origin, Vector3 _dest) {
        Unit unit = GetUnitById(_unitId);
        if (unit == null) {
            return;
        }

        ball = Instantiate(ballPrefab, _origin, Quaternion.identity);
        ball.SetOrigin(_origin);
        ball.SetDestination(_dest);

        AM.Play("Hike");
        PT.Show("Hike!", 2f);
    }

    public void UpdateBall(Vector3 _position, Quaternion _rotation, int _frame) {
        if (ball == null) {
            return;
        }

        ball.UpdatePhysics(_position, _rotation, _frame);
    }

    public void BallCaught(Unit _catcher) {
        if (ball == null) {
            return;
        }

        ball.Caught(_catcher);
    }
    #endregion

    #region Helper Functions
    public Unit GetUnitById(int _unitId) {
        if (unitMap == null) {
            Debug.Log("unitMap null, returning");
            return null;
        }

        Unit unit;
        if (!unitMap.TryGetValue(_unitId, out unit)) {
            Debug.Log("TryGetValue false, returning");
            return null;
        }

        return unit;
    }

    public string GetUsernameByUnitId(int _unitId) {
        Unit unit = GetUnitById(_unitId);
        if (unit == null) {
            return null;
        }

        return ConnectionManager.instance.Slots[unit.playerSlotNumber];
    }

    public Unit GetFirstUnitControlledByPlayerSlot(int _playerSlot) {
        List<int> unitIds = new List<int>(unitMap.Keys);
        unitIds.Sort();

        for (int i = 0; i < unitIds.Count; i++) {
            Unit unit = unitMap[unitIds[i]];
            if (unit.playerSlotNumber == _playerSlot) {
                return unit;
            }
        }

        return null;
    }

    public List<Unit> GetAllLinemenForPlayerSlot(int _playerSlot) {
        List<Unit> linemen = new List<Unit>();
        foreach (Unit unit in unitMap.Values) {
            if (unit.GetComponent<Lineman>() != null && unit.playerSlotNumber == _playerSlot) {
                linemen.Add(unit);
            }
        }

        return linemen;
    }
    #endregion
}

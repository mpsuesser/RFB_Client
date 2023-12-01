using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityMovementAI;

public class Unit : MonoBehaviour {
    public GameState.Team Team => gameObject.tag == "Team1" ? GameState.Team.Red : GameState.Team.Green;

    [Header("Cooldowns")]
    private bool chargeOnCooldown = false;
    private bool jukeOnCooldown = false;
    private bool tackleOnCooldown = false;
    private bool stiffOnCooldown = false;
    private float chargeCooldownRemaining = 0f;
    private float jukeCooldownRemaining = 0f;
    private float tackleCooldownRemaining = 0f;
    private float stiffCooldownRemaining = 0f;

    private SkinnedMeshRenderer modelMesh;
    private int jukedFrames;
    private bool charged = false;
    private bool juked = false;
    private float chargeDurationRemaining = 0f;
    private float jukeDurationRemaining = 0f;

    private bool hasBall = false;
    public bool canThrow = true;

    [Header("Miscellaneous")]
    public int playerSlotNumber;
    public int unitId;
    public Ball ballReference;
    private int originalLayer;

    private Color startColor;
    private Renderer rend;

    // Movement
    private Vector3 destination;
    private Rigidbody rb;
    protected Animation anim;

    private GameObject instantiatedBallInHand;
    public GameObject ballInHandPrefab;
    public Transform leftHandReference;

    private Material material;

    // Start is called before the first frame update
    void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
        rend = gameObject.GetComponent<Renderer>();
        startColor = rend.material.color;
        anim = gameObject.GetComponentInChildren<Animation>();
        modelMesh = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        material = modelMesh.materials[0];
        jukedFrames = 0;
        originalLayer = gameObject.layer;
    }

    // Update is called once per frame
    void Update() {
        // Duration updates
        if (charged) {
            chargeDurationRemaining -= Time.deltaTime;
            if (chargeDurationRemaining <= 0f) {
                charged = false;
                material.DisableKeyword("_EMISSION");
            }
        }

        if (juked) {
            jukeDurationRemaining -= Time.deltaTime;
            if (jukeDurationRemaining <= 0f) {
                juked = false;
                modelMesh.enabled = true;
                gameObject.layer = originalLayer;
            } else if (++jukedFrames % 5 == 0) {
                // every 10 frames, randomly show or don't show the model
                modelMesh.enabled = (Random.value > 0.5f);
            }
        }


        // Cooldown updates
        if (chargeOnCooldown) {
            chargeCooldownRemaining -= Time.deltaTime;

            if (chargeCooldownRemaining <= 0f) {
                chargeOnCooldown = false;
            }
        }

        if (jukeOnCooldown) {
            jukeCooldownRemaining -= Time.deltaTime;

            if (jukeCooldownRemaining <= 0f) {
                jukeOnCooldown = false;
            }
        }

        if (tackleOnCooldown) {
            tackleCooldownRemaining -= Time.deltaTime;

            if (tackleCooldownRemaining <= 0f) {
                tackleOnCooldown = false;
            }
        }

        if (stiffOnCooldown) {
            stiffCooldownRemaining -= Time.deltaTime;

            if (stiffCooldownRemaining <= 0f) {
                stiffOnCooldown = false;
            }
        }
    }

    public virtual void ChildUpdate() {}

    public void UpdatePosition(Vector3 _pos) {
        rb.MovePosition(_pos);

        if (GameState.instance.hiked || this.CanHike()) {
            anim.Play("PlayerRun");
        }
    }

    public void UpdateRotation(Quaternion _rotation) {
        rb.MoveRotation(_rotation);
    }

    public void Threw() {
        RemoveBall();
        anim.Play("Throw");
    }

    // ----- Abilities -----
    public void ChargeIssued(float _cooldown, float _duration) {
        chargeOnCooldown = true;
        charged = true;
        // EffectManager.instance.Show("Charge", transform, _duration);
        material.EnableKeyword("_EMISSION");

        chargeCooldownRemaining = _cooldown;
        chargeDurationRemaining = _duration;
    }

    public void JukeIssued(float _cooldown, float _duration) {
        AudioManager.instance.Play("Juke");
        jukeOnCooldown = true;
        juked = true;
        jukedFrames = 0;

        // if the unit is not controlled by the player, set its layermask to ignore raycast
        string userIssuing = GameMaster.instance.GetUsernameByUnitId(unitId);
        if (userIssuing != ConnectionManager.instance.username) {
            gameObject.layer = 2; // ignore raycast
        }

        jukeCooldownRemaining = _cooldown;
        jukeDurationRemaining = _duration;
    }

    public void TackleIssued(float _cooldown) {
        tackleOnCooldown = true;
        tackleCooldownRemaining = _cooldown;
    }

    public void StiffIssued(float _cooldown) {
        stiffOnCooldown = true;
        stiffCooldownRemaining = _cooldown;
    }

    public void CaughtBall(Ball ball) {
        GiveBall();
    }

    // ----- Cooldowns -----
    public float GetChargeCooldown() {
        return chargeCooldownRemaining;
    }

    public float GetJukeCooldown() {
        return jukeCooldownRemaining;
    }

    public float GetTackleCooldown() {
        return tackleCooldownRemaining;
    }

    public float GetStiffCooldown() {
        return stiffCooldownRemaining;
    }

    public bool IsChargeOnCooldown() {
        return chargeOnCooldown;
    }

    public bool IsJukeOnCooldown() {
        return jukeOnCooldown;
    }

    public bool IsTackleOnCooldown() {
        return tackleOnCooldown;
    }

    public bool IsStiffOnCooldown() {
        return stiffOnCooldown;
    }

    // ---- Ball interactions -----
    public void GiveBall() {
        hasBall = true;

        instantiatedBallInHand = Instantiate(ballInHandPrefab, leftHandReference);
    }

    public void RemoveBall() {
        hasBall = false;

        if (instantiatedBallInHand) {
            Destroy(instantiatedBallInHand);
        }
    }

    public bool HasBall() {
        return hasBall;
    }

    public bool CanThrow() {
        return hasBall && canThrow;
    }

    public virtual bool CanHike() {
        return false;
    }
}

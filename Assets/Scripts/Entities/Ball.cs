using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {
    private Vector3 origin;
    private Vector3 destination;
    private Unit thrower;
    private int lastFrameUpdate;

    private Rigidbody RB;
    public GameObject minimapSpritePrefab;
    private GameObject minimapSprite;

    private void Start() {
        lastFrameUpdate = -1;

        RB = gameObject.GetComponent<Rigidbody>();
    }

    public void UpdatePhysics(Vector3 _position, Quaternion _rotation, int _frame) {
        if (_frame < lastFrameUpdate) {
            return;
        }

        RB.MovePosition(_position);
        RB.MoveRotation(_rotation);

        #region Minimap
        if (minimapSprite != null) {
            Vector3 minimapPosition = _position;
            minimapPosition.y = 2f;

            minimapSprite.transform.position = minimapPosition;
            // minimapSprite.transform.rotation = Quaternion.Euler(0f, _rotation.eulerAngles.y, 0f);
        }
        #endregion

        lastFrameUpdate = _frame;
    }

    public void Caught(Unit catcher) {
        AudioManager.instance.Play("Catch");
        EffectManager.instance.Show("Catch", transform.position, 2f);

        catcher.CaughtBall(this);

        Remove();
    }

    void Landed() {
        EffectManager.instance.Show("BallLanded", transform.position, 2f);
        Remove();
        return;
    }

    public void Remove() {
        if (minimapSprite != null) Destroy(minimapSprite);
        Destroy(throwMarker);
        Destroy(gameObject);
    }

    public void SetOrigin(Vector3 _origin) {
        origin = _origin;
    }

    public void SetDestination(Vector3 _destination) {
        destination = _destination;
        destination.y = -1;
        CreateMarker();
    }

    public GameObject throwMarkerPrefab;
    private GameObject throwMarker;

    void CreateMarker() {
        Vector3 markerLocation = destination;
        markerLocation.y += (float)0.01;
        throwMarker = Instantiate(throwMarkerPrefab, markerLocation, Quaternion.Euler(90,0,0));
    }

    public void SetThrower(Unit _thrower) {
        thrower = _thrower;
    }

    public Unit GetThrower() {
        return thrower;
    }

    // must be called after origin and destination are set
    public void CreateMinimapSprite() {
        Quaternion lookRotation = Quaternion.LookRotation(destination - origin);
        Quaternion spriteRotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
        minimapSprite = Instantiate(minimapSpritePrefab, transform.position, spriteRotation);
    }
}

using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    public static CameraController instance; // singleton pattern
    void Awake() {
        if (instance != null) {
            Debug.LogError("More than one CameraController instance in scene!");
            return;
        }
        instance = this;
    }

    private bool doMovement = true;

    // Zoom
    public float zoomSpeed = 5f;
    public float minY = 10f;
    public float maxY = 80f;

    // Panning
    public float panSpeed = 60f;
    public float panBorderThickness = 10f;
    public float minX = -35f;
    public float maxX = 35f;
    public float minZ = -70f;
    public float maxZ = 60f;

    // Offsets
    private float heightOffset = 22f;

    // Start sequence
    private Vector3 startPosition = new Vector3(0f, 500f, 0f);
    private Vector3 endPosition = new Vector3(0f, 120f, 0f);
    private int gameStartSequenceFrameReference;
    private float gameStartSequenceTimeReference;
    private float gameStartSequenceTime = 3f; // seconds
    private bool startSequence;

    void Start() {
        StartCoroutine(StartGameSequence());
    }

    // Update is called once per frame
    void Update() {
        /* if (Input.GetKeyDown(KeyCode.Escape)) {
            doMovement = !doMovement;
        } */

        if (!doMovement || startSequence) {
            return;
        }


        // Up
        if (Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - panBorderThickness) {
            // Vector3.forward is equivalent to new Vector3 (0f, 0f, 1f)
            transform.Translate(Vector3.forward * panSpeed * Time.deltaTime, Space.World);
        }

        // Down
        if (Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= panBorderThickness) {
            transform.Translate(Vector3.back * panSpeed * Time.deltaTime, Space.World);
        }

        // Left
        if (Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= panBorderThickness) {
            transform.Translate(Vector3.left * panSpeed * Time.deltaTime, Space.World);
        }

        // Right
        if (Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - panBorderThickness) {
            transform.Translate(Vector3.right * panSpeed * Time.deltaTime, Space.World);
        }

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = transform.position;
        pos.y -= scroll * 1000 * zoomSpeed * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    public void ToggleMovement() {
        doMovement = !doMovement;
    }

    private void JumpTo(Vector3 pos) {
        transform.position = pos;
    }

    public void FocusOn(Vector3 _location) {
        startSequence = false;
        StopAllCoroutines();

        Vector3 cameraPosition = _location;
        cameraPosition.y += heightOffset;

        // If it is 90, just leave the Z value at the location since we're looking straight down.
        if (Settings.instance.CameraAngle != 90f) {
            #region Drawing
            /*     
             *                        Camera
             *                         /|
             *                        / | <- angle for calculation = 90 - Settings.instance.CameraAngle
             *                       /  | SOH CAH TOA --- tan(x) = opp/hyp
             *                      /   |
             *                     /    | heightOffset
             *                    /     | (hypotenuse)
             *                   /      |
             *                  /       |
             *                 /        |
             * focusing on -> /_________|
             *                   (opp)
             *       Z distance of camera from focus
             * 
             */
            #endregion
            Debug.Log($"Focusing on: z={_location.z}");
            cameraPosition.z -= heightOffset * Mathf.Tan(Mathf.Deg2Rad * (90f-Settings.instance.CameraAngle));
            Debug.Log("heightOffset:" + heightOffset);
            Debug.Log("tan result: " + Mathf.Tan(Mathf.Deg2Rad * (90f - Settings.instance.CameraAngle)));
            Debug.Log($"Setting camera position: {cameraPosition.z}");
        }

        transform.rotation = Quaternion.Euler(Settings.instance.CameraAngle, 0f, 0f);

        JumpTo(cameraPosition);
    }

    IEnumerator StartGameSequence() {
        startSequence = true;

        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        yield return new WaitForSeconds(1);
        StartCoroutine(StartMovingCamera());
    }

    IEnumerator StartMovingCamera() {
        gameStartSequenceFrameReference = Time.frameCount;
        gameStartSequenceTimeReference = Time.time;

        while (transform.position.y > endPosition.y + .1f) {
            float timePassed = Time.time - gameStartSequenceTimeReference;
            float multiplier = 1; /* Mathf.Max(1, timePassed); */
            int _yRotation = ((Time.frameCount - gameStartSequenceFrameReference) % 360);
            float timeRatio = timePassed / gameStartSequenceTime;

            if (_yRotation == 1 && timeRatio > .66f) {
                yield break;
            }

            transform.rotation = Quaternion.Euler(
                90f,
                _yRotation * multiplier,
                0f
            );

            transform.position = Vector3.Lerp(startPosition, endPosition, timeRatio);

            yield return null;
        }
    }
}

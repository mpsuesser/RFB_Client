using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Minimap : MonoBehaviour, IPointerDownHandler {
    public static Minimap instance;

    public Camera MM;
    public CameraController mainCameraController;

    private RectTransform screenRect;

    private void Awake() {
        #region Singleton
        if (instance != null) {
            Debug.Log("More than two Minimaps in scene!");
            return;
        }

        instance = this;
        #endregion

        screenRect = gameObject.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData _eventData) {
        Debug.Log("OnPointerClick called!");

        Vector3 worldPressPoint = GetWorldPressPoint(_eventData.position);
        Debug.DrawLine(worldPressPoint, MM.transform.position, Color.blue, 4f);

        if (_eventData.button == PointerEventData.InputButton.Right) {
            Debug.Log("Right click");
            List<Unit> selectedList = UnitSelection.instance.SelectedUnits;
            if (selectedList.Count == 0) {
                return;
            }

            foreach (Unit unit in selectedList) {
                ClientSend.MoveToIssued(unit.unitId, worldPressPoint, Input.GetKey(KeyCode.LeftShift));
            }
        } else if (_eventData.button == PointerEventData.InputButton.Left) {
            if (AbilityHandler.instance.IsThrowQueued) {
                AbilityHandler.instance.Throw(worldPressPoint);
            } else {
                mainCameraController.FocusOn(worldPressPoint);
            }
        }


        // WORKS!
        // TODO: determine if throw, moveto, or change camera pos, then use worldPressPoint as reference
        // confirm accuracy, the scaledPressPoint only seemed to be outputting 1 sig fig on debug
    }

    // Gets world press point from 2D click point on minimap image
    private Vector3 GetWorldPressPoint(Vector2 _pressPoint) {
        Vector3[] imageWorldCorners = new Vector3[4];
        screenRect.GetWorldCorners(imageWorldCorners);
        /* [0]: bottom left     (2.7, 2.7, 0.0)
         * [1]: top left        (2.7, 109.3, 0.0)
         * [2]: top right       (109.3, 109.3, 0.0)
         * [3]: bottom right    (109.3, 2.7, 0.0)
         */

        Vector3[] cameraViewportCorners = new Vector3[4];
        cameraViewportCorners[0] = MM.ViewportToWorldPoint(new Vector3(0, 0, 100));
        cameraViewportCorners[1] = MM.ViewportToWorldPoint(new Vector3(0, 1, 100));
        cameraViewportCorners[2] = MM.ViewportToWorldPoint(new Vector3(1, 1, 100));
        cameraViewportCorners[3] = MM.ViewportToWorldPoint(new Vector3(1, 0, 100));
        /* [0]: bottom left     (0, 0, -)
         * [1]: top left        (0, 1, -)
         * [2]: top right       (1, 1, -)
         * [3]: bottom right    (1, 0, -)
         */

        Debug.Log("Camera viewpoint corners");
        for (int i = 0; i < 4; i++) {
            Debug.Log("Corner " + i + ": " + cameraViewportCorners[i]);
        }

        Debug.DrawLine(MM.transform.position, cameraViewportCorners[0], Color.red, 4.0f);
        Debug.DrawLine(MM.transform.position, cameraViewportCorners[1], Color.red, 4.0f);
        Debug.DrawLine(MM.transform.position, cameraViewportCorners[2], Color.red, 4.0f);
        Debug.DrawLine(MM.transform.position, cameraViewportCorners[3], Color.red, 4.0f);

        // Subtract the offset in world position of the image
        _pressPoint.x -= imageWorldCorners[0].x;
        _pressPoint.y -= imageWorldCorners[0].y;

        // For reference, let's also subtract the same offset from the top right coords, which will act as our max
        Vector2 maxImageLocation = new Vector2(
            imageWorldCorners[2].x - imageWorldCorners[0].x,
            imageWorldCorners[2].y - imageWorldCorners[0].y
        );

        // Get the proportion x and y of clicked, so min 0 and max 1 for both values
        Vector2 scaledPressPoint = new Vector2(
            _pressPoint.x / maxImageLocation.x,
            _pressPoint.y / maxImageLocation.y
        );

        Debug.Log("Scaled press point: " + scaledPressPoint);

        Vector2 viewportSize = new Vector2(
            cameraViewportCorners[2].x - cameraViewportCorners[0].x,
            cameraViewportCorners[2].z - cameraViewportCorners[0].z
        );

        Vector3 worldPressPoint = new Vector3(
            (scaledPressPoint.x * viewportSize.x) + cameraViewportCorners[0].x,
            0,
            (scaledPressPoint.y * viewportSize.y) + cameraViewportCorners[0].z
        );

        return worldPressPoint;
    }

    public bool IsClickInMinimap(Vector2 _clickPos) {
        Vector3[] imageWorldCorners = new Vector3[4];
        screenRect.GetWorldCorners(imageWorldCorners);
        /* [0]: bottom left     (2.7, 2.7, 0.0)
         * [1]: top left        (2.7, 109.3, 0.0)
         * [2]: top right       (109.3, 109.3, 0.0)
         * [3]: bottom right    (109.3, 2.7, 0.0)
         */

        return (
            _clickPos.x > imageWorldCorners[0].x &&
            _clickPos.x < imageWorldCorners[2].x &&
            _clickPos.y > imageWorldCorners[0].y &&
            _clickPos.y < imageWorldCorners[2].y
        );
    }
}

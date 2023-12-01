using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour {
    public static Settings instance;

    public float Volume { get; set; }
    public float CameraAngle { get; set; }

    void Awake() {
        #region Singleton
        if (instance != null) {
            Debug.Log("More than one Settings instance in scene!");

            return;
        }

        instance = this;
        #endregion
    }

    void Start() {
        Volume = .75f;
        CameraAngle = 65f;

        PreloadManager.instance.Ready(this);
    }
}

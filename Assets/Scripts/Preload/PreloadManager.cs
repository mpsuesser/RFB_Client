using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloadManager : MonoBehaviour
{
    public static PreloadManager instance;

    private bool audioManagerReady;
    private bool connectionManagerReady;
    private bool clientManagerReady;
    private bool cursorControllerReady;
    private bool threadManagerReady;
    private bool settingsReady;

    private bool EverythingLoaded {
        get {
            return audioManagerReady
                && connectionManagerReady
                && clientManagerReady
                && cursorControllerReady
                && threadManagerReady
                && settingsReady;
        }
    }

    private void Awake() {
        instance = this;

        audioManagerReady = false;
        connectionManagerReady = false;
        clientManagerReady = false;
        cursorControllerReady = false;
        threadManagerReady = false;
        settingsReady = false;
    }

    public void Ready(AudioManager _am) {
        audioManagerReady = true;
        CheckLoadStatus();
    }

    public void Ready(ConnectionManager _cm) {
        connectionManagerReady = true;
        CheckLoadStatus();
    }

    public void Ready(Client _cm) {
        clientManagerReady = true;
        CheckLoadStatus();
    }

    public void Ready(CursorController _cc) {
        cursorControllerReady = true;
        CheckLoadStatus();
    }

    public void Ready(ThreadManager _tm) {
        threadManagerReady = true;
        CheckLoadStatus();
    }

    public void Ready(Settings _s) {
        settingsReady = true;
        CheckLoadStatus();
    }

    public void CheckLoadStatus() {
        if (EverythingLoaded) {
            SceneManager.LoadScene(1);
        }
    }
}

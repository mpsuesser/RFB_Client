using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour {
    public static CursorController instance; // singleton pattern
    void Awake() {
        #region Singleton
        if (instance != null) {
            Debug.LogError("More than one CursorController instance in scene!");
            return;
        }
        instance = this;
        #endregion

        DontDestroyOnLoad(gameObject);
    }

    public Texture2D defaultCursorSprite;
    public Texture2D throwCursorSprite;
    public Texture2D tackleCursorSprite;
    public Texture2D stiffCursorSprite;

    void Start() {
        ResetCursor();

        PreloadManager.instance.Ready(this);
    }

    public void ResetCursor() {
        SetCursor(defaultCursorSprite);
    }

    public void SetThrowCursor() {
        SetCursor(throwCursorSprite);
    }

    public void SetTackleCursor() {
        SetCursor(tackleCursorSprite);
    }

    public void SetStiffCursor() {
        SetCursor(stiffCursorSprite);
    }

    private void SetCursor(Texture2D sprite) {
        Cursor.SetCursor(
            sprite,
            new Vector2(sprite.width/2, sprite.height/2),
            CursorMode.Auto
        );
    }
}

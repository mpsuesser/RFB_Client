using UnityEngine;
using UnityEngine.UI;

public class PopupText : MonoBehaviour
{
    public static PopupText instance;

    public Transform textArea;
    public Text textPrefab;

    private void Awake() {
        #region Singleton
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
        }
        #endregion
    }

    private void Start() {
        Show("The game is about to begin...", 4.5f);
    }

    public void Show(string msg, float time = 3f) {
        Text newText = (Text)Instantiate(textPrefab, textArea);
        newText.text = msg;

        Destroy(newText.gameObject, time);
    }
}

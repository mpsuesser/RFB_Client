using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;

    public GameObject ui;
    public CameraController mainCamera;

    public SettingsMenu settingsMenuPrefab;
    private SettingsMenu settingsMenu;
    public GameObject overlayCanvas;
    public GameObject parent;

    #region Singleton
    private void Awake() {
        instance = this;
    }
    #endregion

    public void Toggle() {
        mainCamera.ToggleMovement();

        if (parent.activeSelf) { // pause menu is currently open
            if (settingsMenu != null) {
                settingsMenu.BackButtonHit(); // let's close the settings menu properly
            }
        }

        parent.SetActive(!parent.activeSelf);
    }

    public void Quit() {
        ConnectionManager.instance.Disconnect();
        return;
    }

    public void Settings() {
        settingsMenu = Instantiate(settingsMenuPrefab, overlayCanvas.transform);
        settingsMenu.Close += SettingsClosed;

        gameObject.SetActive(false);
    }

    public void SettingsClosed() {
        settingsMenu = null;

        gameObject.SetActive(true);
    }

    public void VoteToForfeit() {
        return;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tabs : MonoBehaviour
{
    public GameObject generalMenuItems;
    public GameObject gameplayMenuItems;

    // Start is called before the first frame update
    void Start()
    {
        GeneralClicked();
    }

    public void GeneralClicked() {
        gameplayMenuItems.SetActive(false);
        generalMenuItems.SetActive(true);
    }

    public void GameplayClicked() {
        generalMenuItems.SetActive(false);
        gameplayMenuItems.SetActive(true);
    }
}

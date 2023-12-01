using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EffectManager : MonoBehaviour {
    public string[] names;
    public GameObject[] effectPrefabs;

    // Will be constructed below based on the two above, since dictionaries are not serializable
    private Dictionary<string, GameObject> effects;

    public static EffectManager instance; // singleton

    // Start is called before the first frame update
    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        if (names.Length != effectPrefabs.Length) {
            Debug.LogWarning("Effect instances do not match!");

            return;
        }

        // construct the dictionary
        effects = new Dictionary<string, GameObject>();
        for (int i = 0; i < names.Length; i++) {
            effects.Add(names[i], effectPrefabs[i]);
        }
    }

    public void Show(string _name, Vector3 _location, float _duration) {
        try {
            // Get the effect from the dictionary based on the given _name
            foreach (KeyValuePair<string, GameObject> kvp in effects) {
                if (kvp.Key == _name && kvp.Value != null) {
                    // Create the effect at _location
                    GameObject effectInstance = (GameObject)Instantiate(kvp.Value, _location, Quaternion.identity);

                    // End the effect in _duration time
                    Destroy(effectInstance, _duration);

                    return;
                }
            }
        } catch (Exception) {
            Debug.LogWarning($"Could not find effect {_name}");
        }
    }

    public void Show(string _name, Transform _owner, float _duration) {
        try {
            // Get the effect from the dictionary based on the given _name
            foreach (KeyValuePair<string, GameObject> kvp in effects) {
                if (kvp.Key == _name && kvp.Value != null) {
                    // Create the effect at _location
                    GameObject effectInstance = (GameObject)Instantiate(kvp.Value, _owner.position, kvp.Value.transform.rotation, _owner);

                    // End the effect in _duration time
                    Destroy(effectInstance, _duration);

                    return;
                }
            }
        } catch (Exception) {
            Debug.LogWarning($"Could not find effect {_name}");
        }
    }
}

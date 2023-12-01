using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour {
    public AudioMixer audioMixer;
    public Sound[] sounds;

    public static AudioManager instance; // singleton

    // Start is called before the first frame update
    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = s.group;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    void Start() {
        PreloadManager.instance.Ready(this);

        // Play("Intro");
    }

    public void Play(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning($"Could not find sound {name}");
            return;
        }

        // Use our volume setting as a multiplier on the sound, so we can adjust the volume of sounds individually in the sounds array relative to each other
        audioMixer.SetFloat("volume", Mathf.Log10(Settings.instance.Volume * s.volume) * 20);
        s.source.Play();
    }
}

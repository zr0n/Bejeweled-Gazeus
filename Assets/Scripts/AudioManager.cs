using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BejeweledGazeus
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField]
        AudioClip audioClearFruit;
        [SerializeField]
        AudioSource audioSource;
        [SerializeField]
        float volume = 1f;

        public void PlayAudioClearFruit()
        {
            audioSource.PlayOneShot(audioClearFruit, volume);
        }
    }

}

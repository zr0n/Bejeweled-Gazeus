using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BejeweledGazeus
{
    public class Fader : MonoBehaviour
    {
        public Image imageFader;
        public Color fadeColor = Color.black;
        
        [SerializeField]
        float fadeDuration = 1f;

        Color _originalColor;
        
        // Start is called before the first frame update
        void Start()
        {
            _originalColor = imageFader.color;
        }
        
        public void FadeIn() => StartCoroutine(FadeTo(1f, fadeDuration));

        public void FadeOut() => StartCoroutine(FadeTo(0f, fadeDuration));

        IEnumerator FadeTo(float newAlpha, float duration)
        {
            float oldAlpha = 1f - newAlpha;
            float time = 0f;

            while((time/duration) <= 1f)
            {
                Color oldColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, oldAlpha);
                Color newColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, newAlpha);
                imageFader.color = Color.Lerp(oldColor, newColor, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
        }
    }
}

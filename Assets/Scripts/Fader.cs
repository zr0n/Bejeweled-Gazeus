/*
MIT License

Copyright (c) 2021 Luiz Fernando Alves dos Santos

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BejeweledGazeus
{
    public class Fader : MonoBehaviour
    {
        //The image we will show smoothly, having the fade effect
        public Image imageFader;
        //Fade Color
        public Color fadeColor = Color.black;
        
        [SerializeField]
        //How much time the animation will take
        float fadeDuration = 1f;

        Color _originalColor;
        
        // Start is called before the first frame update
        void Start()
        {
            _originalColor = imageFader.color;
        }
        
        public void FadeIn() => StartCoroutine(FadeTo(1f, fadeDuration));

        public void FadeOut() => StartCoroutine(FadeTo(0f, fadeDuration));

        //The animation coroutine. It goes from the inverse of newAlpha (one minus) to the newAlpha itself
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
            imageFader.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, newAlpha);

        }
    }
}

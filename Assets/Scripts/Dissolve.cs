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
using UnityEngine.Events;

namespace BejeweledGazeus
{
    public class Dissolve : MonoBehaviour
    {
        //The renderer where we will update the materials parameters
        public Renderer renderer;
        //How much seconds the animation will take
        public float animationTime = 1f;
        //The parameter key in shader
        public const string dissolveAmountReference = "_DissolveAmount";
        //Called when the dissolve animation ends
        public UnityEvent onDisappear;
        //Called when the dissolve animation ends
        public UnityEvent onAppear;

        float _currentDissolve = 1f;
        float _initialDissolve;
        // Start is called before the first frame update
        void Start()
        {
            if (!renderer)
                renderer = GetComponent<Renderer>();

            _initialDissolve = renderer.material.GetFloat(dissolveAmountReference);
            _currentDissolve = _initialDissolve;

        }

        public void Disappear(System.Action callback = null)
        {
            StartCoroutine(AnimateRenderer(1f, callback));
        }
        public void Appear(System.Action callback = null)
        {
            StartCoroutine(AnimateRenderer(0f, callback));
        }

        //Used to recycle the fruit, so we reset the dissolve status
        public void Reset()
        {
            _currentDissolve = _initialDissolve;
            foreach (var material in renderer.materials)
            {
                material.SetFloat(dissolveAmountReference, _currentDissolve);
            }
        }

        //The animation itself. It interpolates from _currentDissolve to newDissolve in animationTime duration
        private IEnumerator AnimateRenderer(float newDissolve = 0f, System.Action callback = null)
        {
            float timeCount = 0f;
            float oldOpacity = _currentDissolve;

            while (true)
            {
                timeCount += Time.deltaTime;
                float alpha = timeCount / animationTime;
                _currentDissolve = Mathf.Lerp(oldOpacity, newDissolve, alpha);

                foreach (var material in renderer.materials)
                {
                    material.SetFloat(dissolveAmountReference, _currentDissolve);
                }

                alpha = Mathf.Clamp(alpha, 0f, 1f);

                if (alpha == 1f)
                {
                    if (newDissolve == 0f)
                    {
                        onAppear?.Invoke();
                    }
                    else
                    {
                        onDisappear?.Invoke();
                    }
                    break;
                }

                yield return null;
            }
            callback?.Invoke();
        }
    }
}

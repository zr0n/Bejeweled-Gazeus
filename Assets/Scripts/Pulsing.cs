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

namespace BejeweledGazeus
{
    //This class is responsible for the pulsing animatino effect (scaling fruit up and down smoothly)
    public class Pulsing : MonoBehaviour
    {
        [SerializeField]
        //Min scale the fruit can reach when scaling down
        float minScale = .4f;
        [SerializeField]
        //Max scale the fruit can reach when scaling up
        float maxScale = 1.6f;
        [SerializeField]
        //Animation step duration
        float transitionDuration = .8f;
        [SerializeField]
        //Which transform we want to animate (usually we only animate the model to avoid scaling the collider)
        Transform transformToAnimate;

        //In which phase the animation is (up, down)
        AnimatePhase _phase;
        float _initialScale;

        // Start is called before the first frame update
        void Start()
        {
            _initialScale = transformToAnimate.localScale.x;
        }

        public void StartAnimation()
        {
            ToggleAnimation();
        }

        public void StopAnimation()
        {
            StopAllCoroutines();
            _phase = AnimatePhase.AnimatingDown;
            StartCoroutine(AnimateScale(_initialScale));
        }

        //Used to recycle fruit when it goes to the object pool
        public void ResetPulsing()
        {
            StopAllCoroutines();
            transformToAnimate.localScale = Vector3.one * _initialScale;
        }

        //Revert animation direction. If it up it becomes down and if it is down it becomes up
        void ToggleAnimation()
        {
            StopAllCoroutines();
            _phase = _phase == AnimatePhase.AnimatingDown ? AnimatePhase.AnimatingUp : AnimatePhase.AnimatingDown;
            float newScale = _phase == AnimatePhase.AnimatingDown ? minScale : maxScale;
            StartCoroutine(AnimateScale(newScale, ToggleAnimation));
        }

        //Smoothly animate the scale using linear interpolation and calling the callback when its finished
        IEnumerator AnimateScale(float newScale, System.Action callback = null)
        {
            float oldScale = transformToAnimate.localScale.x;

            for(float time = 0f; time <= transitionDuration; time += Time.deltaTime)
            {
                Vector3 desiredScale = Vector3.one * Mathf.Lerp(oldScale, newScale, time / transitionDuration);
                transformToAnimate.localScale = desiredScale;
                yield return null;
            }

            callback?.Invoke();
        }

        enum AnimatePhase
        {
            AnimatingDown = 0,
            AnimatingUp = 1
        }
    }

}

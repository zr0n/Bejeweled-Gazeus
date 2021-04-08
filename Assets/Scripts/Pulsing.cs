using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BejeweledGazeus
{
    public class Pulsing : MonoBehaviour
    {
        [SerializeField]
        float minScale = .4f;
        [SerializeField]
        float maxScale = 1.6f;
        [SerializeField]
        float transitionDuration = .8f;
        [SerializeField]
        Transform transformToAnimate;


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
        public void ResetPulsing()
        {
            StopAllCoroutines();
            transformToAnimate.localScale = Vector3.one * _initialScale;
        }

        void ToggleAnimation()
        {
            StopAllCoroutines();
            _phase = _phase == AnimatePhase.AnimatingDown ? AnimatePhase.AnimatingUp : AnimatePhase.AnimatingDown;
            float newScale = _phase == AnimatePhase.AnimatingDown ? minScale : maxScale;
            StartCoroutine(AnimateScale(newScale, ToggleAnimation));
        }

        IEnumerator AnimateScale(float newScale, System.Action callback = null)
        {
            float oldScale = transformToAnimate.localScale.x;

            for(float time = 0f; time <= transitionDuration; time += Time.deltaTime)
            {
                Vector3 desiredScale = Vector3.one * Mathf.Lerp(oldScale, newScale, time / transitionDuration);
                transformToAnimate.localScale = desiredScale;
                yield return null;
            }

            if (callback != null)
                callback();

        }

        enum AnimatePhase
        {
            AnimatingDown = 0,
            AnimatingUp = 1
        }
    }

}

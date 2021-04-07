using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BejeweledGazeus
{
    public class TimeManager : MonoBehaviour
    {

        [SerializeField]
        Slider timeSlider;
        [SerializeField]
        float initialTime = 10f;
        [SerializeField]
        float progressSpeed = 15f;
        [SerializeField]
        float animationDuration = 2f;

        float _currentTime;
        float _sliderWidth;
        float _sliderHeight;
        bool _animating;
        RectTransform _rectTransform;

        void Start()
        {
            _rectTransform = (timeSlider.transform as RectTransform);
            _currentTime = initialTime;
            _sliderWidth = _rectTransform.rect.width;
            _sliderHeight = _rectTransform.rect.height;
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
        }

        void Update()
        {
            if (!GameController.instance.gameStarted)
                return;

            if (_animating)
                return;

            DecreaseTimer();
            UpdateProgressBar();
            CheckGameOver();
        }

        public void AnimateSliderIn()
        {
            _animating = true;

            StartCoroutine(AnimateSlider(OnFinishAnimation));
        }

        public void AnimateSliderOut()
        {
            _animating = true;
            StartCoroutine(AnimateSlider(null, true));
        }

        public void IncreaseTimer(float value)
        {
            _currentTime += value;
            _currentTime = Mathf.Min(initialTime, _currentTime);
        }

        IEnumerator AnimateSlider(System.Action callback, bool reverse = false)
        {
            float timer = 0f;

            while(timer <= animationDuration)
            {
                timer += Time.deltaTime;
                float from = reverse ? _sliderWidth : 0f;
                float to = reverse ? 0 : _sliderWidth;
                float newWidth = Mathf.Lerp(from, to, timer / animationDuration);
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);

                from = reverse ? _sliderHeight : 0f;
                to = reverse ? 0 : _sliderHeight;
                float newHeight = Mathf.Lerp(from, to, timer / animationDuration);

                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);


                yield return null;
            }

            callback?.Invoke();
        }
        
        void OnFinishAnimation()
        {
            _animating = false;
            GameController.instance.StartGame();
        }
        void CheckGameOver()
        {
            if (_currentTime <= 0f)
                GameController.instance.GameOver();
        }

        void DecreaseTimer()
        {
            _currentTime -= Time.deltaTime;
            _currentTime = Mathf.Max(0f, _currentTime);
        }

        void UpdateProgressBar()
        {
            timeSlider.value = Mathf.Lerp(timeSlider.value, _currentTime / initialTime, progressSpeed * Time.deltaTime);
        }
    }

}

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
    public class TimeManager : MonoBehaviour
    {

        [SerializeField]
        //Slider we used as progress bar to show the time remaining
        Slider timeSlider;
        [SerializeField]
        //Maximum time player can have
        float initialTime = 10f;
        [SerializeField]
        //Speed that we used to interpolate the progress bar value
        float progressSpeed = 15f;
        [SerializeField]
        //The progress bar fade in animation duration
        float animationDuration = 2f;

        float _currentTime;
        float _sliderWidth;
        float _sliderHeight;
        bool _animating;
        RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = (timeSlider.transform as RectTransform);
        }
        void Start()
        {
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

        //Shows Slider with procedural animation
        public void AnimateSliderIn()
        {
            _animating = true;

            StartCoroutine(AnimateSlider(OnFinishAnimation));
        }

        //Hide Slider with procedural animation
        public void AnimateSliderOut()
        {
            _animating = true;
            StartCoroutine(AnimateSlider(null, true));
        }

        //Increase time remaining clamping it to don't pass the initial time given to the player
        public void IncreaseTimer(float value)
        {
            _currentTime += value;
            _currentTime = Mathf.Min(initialTime, _currentTime);
        }

        //Shows or Hide the slider, then calls its callback parameter
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
        
        //When the slider is being shown at the screen, then allow player input
        void OnFinishAnimation()
        {
            _animating = false;
            GameController.instance.StartGame();
        }

        //Check if time <= 0 then call game over
        void CheckGameOver()
        {
            if (_currentTime <= 0f)
                GameController.instance.GameOver();
        }

        //Decrease time by delta of frame
        void DecreaseTimer()
        {
            _currentTime -= Time.deltaTime;
            _currentTime = Mathf.Max(0f, _currentTime);
        }

        //Interpolate the progress bar value, giving an idea of animation
        void UpdateProgressBar()
        {
            timeSlider.value = Mathf.Lerp(timeSlider.value, _currentTime / initialTime, progressSpeed * Time.deltaTime);
        }
    }

}

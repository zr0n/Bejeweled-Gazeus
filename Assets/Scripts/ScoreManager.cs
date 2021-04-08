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
    public class ScoreManager : MonoBehaviour
    {
        public int score
        {
            get => _desiredScore;
            set
            {
                _desiredScore = value;
            }
        }

        [SerializeField]
        //The text where we will show the score
        Text textScore;
        [SerializeField]
        //The speed of the score will change (we interpolate it instead of set a raw value giving the text animation effect)
        float textSpeed = 5f;

        //The current score number
        int _currentScore;
        //The actual score number, _currentScore increases slowly until reach this value
        int _desiredScore;

        #region Singleton
        public static ScoreManager instance => _instance;
        static ScoreManager _instance;
        private void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }
        #endregion Singleton

        private void Start()
        {
            textScore.text = "";
        }

        //Interpolate _desiredScore and update the text
        private void Update()
        {
            if (_currentScore >= _desiredScore)
                return;
            //A formula to speed up when the difference is bigger
            _currentScore += Mathf.RoundToInt(Mathf.Max(300f,(_desiredScore - _currentScore)) * textSpeed * Time.deltaTime);
            _currentScore = Mathf.Min(_desiredScore, _currentScore);
            UpdateText(_currentScore);
        }

        void UpdateText(int score)
        {
            textScore.text = score.ToString("000000");
        }
    }
}

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
        Text textScore;
        [SerializeField]
        float textSpeed = 5f;


        int _currentScore;
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

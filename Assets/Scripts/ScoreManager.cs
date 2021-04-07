using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BejeweledGazeus
{
    public class ScoreManager : MonoBehaviour
    {
        public int score;

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
    }
}

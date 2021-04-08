using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BejeweledGazeus
{
    public class Countdown : MonoBehaviour
    {
        public GameObject canvasCountdown;
        
        [SerializeField]
        Text textCountdown;
        [SerializeField]
        int countSeconds = 7;

        void Start()
        {
            canvasCountdown.SetActive(false);
            UpdateText();
        }

        void UpdateText()
        {
            textCountdown.text = countSeconds.ToString();
        }
        public IEnumerator StartCounting(System.Action callback)
        {
            while (countSeconds > 0)
            {
                countSeconds--;
                UpdateText();
                yield return new WaitForSeconds(1f);
            }
            callback();
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BejeweledGazeus
{
    public class FruitInteraction : MonoBehaviour
    {
        public UnityEvent pointerDown; 
        public UnityEvent pointerUp;
        public void BecomeFocused()
        {
            pointerDown?.Invoke();
        }

        public void LoseFocus()
        {
            pointerUp?.Invoke();
        }
    }
}


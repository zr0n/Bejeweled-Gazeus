using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BejeweledGazeus
{
    public class AnimationChecker : MonoBehaviour
    {
        static bool _playedAnimationBefore;
        // Start is called before the first frame update
        void Start()
        {
            if (_playedAnimationBefore)
            {
                GetComponent<Animator>().enabled = false;
                Fader fader = GetComponent<Fader>();
                fader.imageFader.color = new Color(fader.fadeColor.r, fader.fadeColor.g, fader.fadeColor.b, 0f);
                return;
            }

            _playedAnimationBefore = true;
        }

        public void OnFinishAnimation()
        {
            GameController.instance.timeManager.AnimateSliderIn();
        }
    }
}

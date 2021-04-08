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
    public class AnimationChecker : MonoBehaviour
    {
        //Was this animation player before? It is static so it persists on scene loading
        static bool _playedAnimationBefore;
        // Start is called before the first frame update
        void Start()
        {
            //If played animation before, then go straight to game
            if (_playedAnimationBefore)
            {
                GetComponent<Animator>().enabled = false;
                Fader fader = GetComponent<Fader>();
                fader.imageFader.color = new Color(fader.fadeColor.r, fader.fadeColor.g, fader.fadeColor.b, 0f);
                GameController.instance.timeManager.AnimateSliderIn();
                return;
            }

            _playedAnimationBefore = true;
        }

        //this is called by an animation event, in CameraIntro.anim at the last frame of the animation
        public void OnFinishAnimation()
        {
            GameController.instance.timeManager.AnimateSliderIn();
        }
    }
}

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
    //This class rotates the fruit seemlesly if _rotating == true and back to its original rotation if _rotating == false
    public class Rotator : MonoBehaviour
    {
        [SerializeField]
        Vector3 rotationDirection = new Vector3(0f, 1f, 0f);
        [SerializeField]
        float backToOriginalRotationSpeed = 15f;

        [SerializeField]
        float rotationSpeed = 220f;

        bool _rotating;
        Quaternion _originalRotation;
        // Start is called before the first frame update
        void Start()
        {
            _originalRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            if (_rotating)
                transform.Rotate(rotationDirection * rotationSpeed * Time.deltaTime);
            else
                transform.rotation = Quaternion.Lerp(transform.rotation, _originalRotation, backToOriginalRotationSpeed * Time.deltaTime);
        }

        public void ResetRotation()
        {
            _rotating = false;
            transform.rotation = _originalRotation;
        }

        public void StartRotation()
        {
            _rotating = true;
        }

        public void StopRotation()
        {
            _rotating = false;
        }
    }

}

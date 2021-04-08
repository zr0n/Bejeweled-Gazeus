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
    //Responsible for check user interactions with the fruits
    public class InteractionManager : MonoBehaviour
    {

        //If it's true the user can't interact with fruits because the grid is busy
        bool _interactionBlocked => !GameController.instance.gameStarted ||
                                    GameController.instance.movingFruits.Count > 0;
        //The fruit we are focusing now
        FruitInteraction _focusedFruit;
                                        

        // Update is called once per frame
        void Update()
        {
            if (_interactionBlocked)
                return;

            if (JustPressedMouseOrTouch())
            {
                CheckForFruitInMousePosition();
            }
            else if (!IsPressingMouseOrTouch() && _focusedFruit)
            {
                _focusedFruit.LoseFocus();
                _focusedFruit = null;
            }
                

        }
        
        //check if a player just pressed the mouse button or touch in this frame
        bool JustPressedMouseOrTouch()
        {
            if (Input.GetMouseButtonDown(0))
                return true;
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                return true;
            else
                return false;
        }

        //Check if a player is still pressing the touch or mouse button
        bool IsPressingMouseOrTouch()
        {
            if (Input.GetMouseButton(0))
                return true;
            if (Input.touchCount > 0)
                return true;
            else
                return false;
        }

        //Do a raycast to find fruits in the mouse position
        void CheckForFruitInMousePosition()
        {
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3 cameraPos = Camera.main.transform.position;
            Vector3 direction = (worldPos - cameraPos).normalized;

            Ray ray = new Ray(cameraPos, direction);

            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                if (hit.collider)
                {
                    FruitInteraction fruit = hit.collider.GetComponentInParent<FruitInteraction>();

                    if (fruit)
                    {
                        _focusedFruit = fruit;
                        fruit.BecomeFocused();
                    }
                }
            }
        }

    }
}

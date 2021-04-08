using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BejeweledGazeus
{
    public class InteractionManager : MonoBehaviour
    {
        public static FruitInteraction focusedFruit => _focusedFruit;

        static FruitInteraction _focusedFruit;
        bool _interactionBlocked => !GameController.instance.gameStarted ||
                                    GameController.instance.movingFruits.Count > 0;
                                        

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
        
        //check if a player pressed the mouse button or touch in this frame
        bool JustPressedMouseOrTouch()
        {
            if (Input.GetMouseButtonDown(0))
                return true;
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                return true;
            else
                return false;
        }

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

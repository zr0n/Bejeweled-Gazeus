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
    public class Fruit : MonoBehaviour
    {
        [Tooltip("The speed that the fruit will move following the mouse")]
        public float moveSpeed = 5f;

        [Tooltip("Minimum magnitude to move the piece")]
        public float minMagnitude = 32f;
       
        [Tooltip("The Pulsing animation component")]
        public Pulsing pulsingAnimation;
        public Slot slot;

        [HideInInspector]
        public FruitInteraction fruitInteraction;
        [HideInInspector]
        //Holds the new position this fruit wants to move
        public Vector2 newGridPosition;
        [HideInInspector]
        //If true the fruit is falling, therefore it's invalid
        public bool falling;
        [HideInInspector]
        //If true this fruit is in the recycle zone and should not be considered for the game logic yet
        public bool isInPool;


        [SerializeField]
        Rigidbody rigidBody;
        [SerializeField]
        //How many time will we wait before starting the Dissolve Animation and recycle fruit?
        float intervalBeforeDestroy = 3f;
        [SerializeField]
        //The maximum magnitude of how many force we can randomly apply to the fruit in its three axis
        Vector3 forceVariation = new Vector3(2f, 2f, 2f);
        [SerializeField]
        Rotator rotator;
        [SerializeField]
        Dissolve dissolve;

        //The position of mouse when player starts touching the screen
        Vector3 _mouseStart;
        //The position (world space) of where the fruit is moving to
        Vector3 _movingTo;
        //If true the player is dragging the fruit to somewhere
        bool _movingByMouseOrTouch;
        //If true we should update the position in direction of _movingTo
        bool _shouldMove;

        Rotator _rotator;
        Pulsing _pulsing;
        Slot.Type _originalType;

        //If true the fruit has finished its movement in this frame
        bool _justFinishedMovement
        {
            get
            {
                return _shouldMove && Vector3.Distance(transform.localPosition, _movingTo) < .01f;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            rigidBody.isKinematic = true;
            _rotator = GetComponent<Rotator>();
            _pulsing = GetComponent<Pulsing>();
            SetupFruitInteraction();
        }

        // Update is called once per frame
        void Update()
        {
            CheckMouseMovement();
            CheckAutoMovement();
        }

        //Set the slot its fruit belongs to
        public void SetSlot(Slot slot)
        {
            slot.fruit = this;
            this.slot = slot;
            _originalType = slot.type;
        }

        //Smooth move to position (linear interpolation)
        public void SmoothMoveTo(Vector2 position)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, position, Time.deltaTime * moveSpeed);
        }

        //When the fruit starts following the mouse movement
        public void StartFruitMovement()
        {
            _movingByMouseOrTouch = true;
            _mouseStart = Input.mousePosition;
        }

        //Move the fruit to where it belongs in grid
        public void GoToGridPosition()
        {
            GameController.instance.movingFruits.Add(this);
            _movingTo = GameController.GridToWorldPosition(slot.position);
            _shouldMove = true;
        }

        //Move to an empty slot, clonning the old slot type
        public void MoveToEmptyGridPosition(Vector2 position)
        {
            Slot newSlot = new Slot(position, slot.type, this);
            slot = newSlot;
        }

        //Used for object pooling. Reset the fruit properties, so we can use it again as a new one
        public void ResetFruit()
        {
            falling = false;
            _movingByMouseOrTouch = false;
            _shouldMove = false;
            rigidBody.isKinematic = true;
            dissolve.Reset();
            _rotator.ResetRotation();
            _pulsing.ResetPulsing();
        }

        //Enable physical engine gravity and apply a random force to the fruit
        public void StartFalling()
        {
            falling = true;
            rigidBody.isKinematic = false;
            rotator.enabled = false;
            ApplyRandomForceAndTorqueImpulse();
            StartCoroutine(WaitAndRecycle());
        }

        //Apply random force and torque
        void ApplyRandomForceAndTorqueImpulse()
        {
            float x = Random.Range(-Mathf.Abs(forceVariation.x), Mathf.Abs(forceVariation.x));
            float y = Random.Range(-Mathf.Abs(forceVariation.y), Mathf.Abs(forceVariation.y));
            float z = Random.Range(-Mathf.Abs(forceVariation.z), Mathf.Abs(forceVariation.z));

            Vector3 force = new Vector3(x, y, z);

            rigidBody.AddForce(force, ForceMode.Impulse);
            rigidBody.AddTorque(force, ForceMode.Impulse);
        }

        //Wait for intervalBeforeDestroy, start the dissolve animation and when it's finished (the fruit isn't visible anymore) recycle the fruit
        IEnumerator WaitAndRecycle()
        {
            yield return new WaitForSeconds(intervalBeforeDestroy);

            dissolve.Disappear(() =>
            {
                GameController.instance.pool.Recycle(_originalType, this);
            });
        }
        //Check if fruit should be going to some place automatically
        void CheckAutoMovement()
        {
            if (_shouldMove)
            {
                SmoothMoveTo(_movingTo);

                if (_justFinishedMovement)
                {
                    _shouldMove = false;
                    GameController.instance.movingFruits.Remove(this);

                    if(GameController.instance.swap.Length > 0 && GameController.instance.swap[0] == this)
                        GameController.instance.shouldCheckBoardOnNextFrame = true;

                    GameController.instance.spawnPositions[(int)slot.position.x] =
                        Mathf.Clamp(GameController.instance.spawnPositions[(int)slot.position.x] - 1, 0, GameController.instance.width);
                }
            }
        }

        //When player click or touch this fruit
        void OnBecomeFocused()
        {
            if (GameController.instance.fruitClicked)
            {
                GameController.instance.fruitClicked.rotator.StopRotation();
                GameController.instance.StopPulsingNeighbours(GameController.instance.fruitClicked);
            }

            //avoid interaction while moving fruit
            if (falling || GameController.instance.movingFruits.Count > 0) return;
;
            StartFruitMovement();
        }

        //when player stop clicking or touching this fruit
        void OnLoseFocus()
        {
            //avoid interaction while moving fruit
            if (falling || GameController.instance.movingFruits.Count > 0) return;

            _movingByMouseOrTouch = false;

            if(newGridPosition.Equals(slot.position))
                OnClick();
            else
                CheckSwap();
        }

        //When the user clicks the fruit (and not keep pressing to drag)
        void OnClick()
        {
            if (!GameController.instance.fruitClicked)
            {
                GameController.instance.fruitClicked = this;
                GameController.instance.StartPulsingNeighbours(this);
                rotator.StartRotation();
                GoToGridPosition();
            }

            if (GameController.instance.fruitClicked == this)
                return;
            
            GameController.instance.fruitClicked.rotator.StopRotation();
            GameController.instance.StopPulsingNeighbours(GameController.instance.fruitClicked);

            if (GameController.instance.IsNeighbour(this, GameController.instance.fruitClicked))
                GameController.instance.SwapFruits(this, GameController.instance.fruitClicked);

            

            GameController.instance.fruitClicked = null;
        }

        //Get fruit interaction component and add event listeners
        void SetupFruitInteraction()
        {
            if (!fruitInteraction)
                fruitInteraction = GetComponent<FruitInteraction>();

            if (!fruitInteraction)
            {
                Debug.LogError("No Fruit interaction detected.");
            }
            else
            {
                fruitInteraction.pointerDown.AddListener(OnBecomeFocused);
                fruitInteraction.pointerUp.AddListener(OnLoseFocus);
            }
        }

        //Check if the fruit should move, then add its offset
        void CheckMouseMovement()
        {
            if (_movingByMouseOrTouch)
            {
                Vector2 direction = Input.mousePosition - _mouseStart;
                Vector2 offset = new Vector2();
                if (direction.magnitude > minMagnitude)
                {
                    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                        offset = new Vector2(direction.normalized.x > 0f ? 1f : -1f, 0f);
                    else
                        offset = new Vector2(0f, direction.normalized.y > 0f ? -1f : 1f);
                }

                newGridPosition = slot.position + offset;

                Vector2 position;

                if (newGridPosition.Equals(slot.position))
                    position = GameController.GridToWorldPosition(slot.position);
                else
                    position = GameController.GridToWorldPosition(slot.position) + (new Vector3(offset.x * .5f, -offset.y * .5f));

                SmoothMoveTo(position);
            }
        }

        //Check if can swap
        void CheckSwap()
        {
            if (GameController.instance.fruitClicked)
                GameController.instance.fruitClicked = null;
            
            if (newGridPosition.Equals(slot.position)) return;

            Slot otherSlot = GameController.instance.GetSlot(newGridPosition);
            if (otherSlot.type != Slot.Type.Blank)
            {
                GameController.instance.SwapFruits(this, otherSlot.fruit);
            }
        }
    }

}

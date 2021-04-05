using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BejeweledGazeus
{
    public class Fruit : MonoBehaviour
    {
        [Tooltip("The speed that the fruit will move following the mouse")]
        public float moveSpeed = 15f;

        [Tooltip("Minimum magnitude to move the piece")]
        public float minMagnitude = 32f;

        [HideInInspector]
        public FruitInteraction fruitInteraction;

        [HideInInspector]
        public Vector2 newGridPosition;

        //[HideInInspector]
        public Slot slot;

        bool _movingByMouseOrTouch;
        Vector3 _mouseStart;
        Vector3 _movingTo;
        bool _shouldMove;

        // Start is called before the first frame update
        void Start()
        {
            SetupFruitInteraction();
            
        }

        // Update is called once per frame
        void Update()
        {
            CheckMouseMovement();
            CheckAutoMovement();
        }

        public void SetSlot(Slot slot)
        {
            slot.fruit = this;
            this.slot = slot;
        }

        //Check if fruit should be going to some place automatically
        void CheckAutoMovement()
        {
            if (_shouldMove)
            {
                SmoothMoveTo(_movingTo);

                if (Vector3.Distance(transform.localPosition, _movingTo) < .01f)
                {
                    _shouldMove = false;

                    if (GameController.instance.swap.Length > 0 && GameController.instance.swap[0] == this)
                        GameController.instance.CheckConnectedNeighbours();
                }
            }
        }

        //When player click or touch this fruit
        void OnBecomeFocused()
        {
            StartFruitMovement();
        }

        //when player stop clicking or touching this fruit
        void OnLoseFocus()
        {
            _movingByMouseOrTouch = false;
            CheckSwap();
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
            _movingTo = GameController.GridToWorldPosition(slot.position);
            _shouldMove = true;
        }

        public void MoveToEmptyGridPosition(Vector2 position)
        {
            Slot newSlot = new Slot(position, slot.type, this);
            slot = newSlot;
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
            if (newGridPosition.Equals(slot.position)) return;

            Slot otherSlot = GameController.instance.GetSlot(newGridPosition);
            if (otherSlot.type != Slot.Type.Blank)
            {
                GameController.instance.SwapFruits(this, otherSlot.fruit);
            }
        }
    }

}

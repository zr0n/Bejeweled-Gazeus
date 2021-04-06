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

        [HideInInspector]
        public FruitInteraction fruitInteraction;
        [HideInInspector]
        public Vector2 newGridPosition;
        [HideInInspector]
        public Slot slot;

        [SerializeField]
        Rigidbody rigidBody;
        [SerializeField]
        float intervalBeforeDestroy = 3f;
        [SerializeField]
        Vector3 forceVariation = new Vector3(2f, 2f, 2f);
        [SerializeField]
        Rotator rotator;

        Vector3 _mouseStart;
        Vector3 _movingTo;
        bool _movingByMouseOrTouch;
        bool _falling;
        bool _shouldMove;
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

        public void MoveToEmptyGridPosition(Vector2 position)
        {
            Slot newSlot = new Slot(position, slot.type, this);
            slot = newSlot;
        }

        public void StartFalling()
        {
            rigidBody.isKinematic = false;
            rotator.enabled = false;
            ApplyRandomForceAndTorqueImpulse();
            StartCoroutine(WaitAndDestroy());
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

        IEnumerator WaitAndDestroy()
        {
            yield return new WaitForSeconds(intervalBeforeDestroy);
            Destroy(gameObject);
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
            //avoid interaction while moving fruit
            if (_falling || GameController.instance.movingFruits.Count > 0) return;
;
            StartFruitMovement();
        }

        //when player stop clicking or touching this fruit
        void OnLoseFocus()
        {
            //avoid interaction while moving fruit
            if (_falling || GameController.instance.movingFruits.Count > 0) return;

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
                return;
            }
            
            GameController.instance.StopPulsingNeighbours(GameController.instance.fruitClicked);
            GameController.instance.fruitClicked.rotator.StopRotation();

            if(GameController.instance.fruitClicked != this && GameController.instance.IsNeighbour(this, GameController.instance.fruitClicked))
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
            if (newGridPosition.Equals(slot.position)) return;

            Slot otherSlot = GameController.instance.GetSlot(newGridPosition);
            if (otherSlot.type != Slot.Type.Blank)
            {
                GameController.instance.SwapFruits(this, otherSlot.fruit);
            }
        }
    }

}

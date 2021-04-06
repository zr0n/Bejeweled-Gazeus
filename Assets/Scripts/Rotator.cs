using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BejeweledGazeus
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField]
        Vector3 rotationDirection = new Vector3(0f, 1f, 0f);
        [SerializeField]
        float rotationSpeed = 15f;

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
            {
                Debug.Log("Rotating " + (rotationDirection * rotationSpeed * Time.deltaTime));
                transform.Rotate(rotationDirection * rotationSpeed * Time.deltaTime);
            }
            else
                transform.rotation = Quaternion.Lerp(transform.rotation, _originalRotation, rotationSpeed * Time.deltaTime);
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

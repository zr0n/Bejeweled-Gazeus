using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BejeweledGazeus
{
    public class ObjectPool : MonoBehaviour
    {

        Dictionary<Slot.Type, List<Fruit>> _pool = new Dictionary<Slot.Type, List<Fruit>>();
        
        public void Recycle(Slot.Type type, Fruit fruit)
        {
            fruit.ResetFruit();
            if (_pool[type] == null)
                _pool[type] = new List<Fruit>();
            _pool[type].Add(fruit);
            fruit.gameObject.SetActive(false);
        }

        public Fruit GetFromPoolOrSpawn(Slot.Type type, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion())
        {
            Fruit fruit;
            
            if (!_pool.ContainsKey(type) || _pool[type] == null || _pool[type].Count == 0)
            {
                _pool[type] = new List<Fruit>();
                //spawn new object
                GameObject template = GameController.instance.fruitsTemplates[(int)(type - 1)];
                fruit = Instantiate(template, position, rotation).GetComponent<Fruit>();

                //Debug.Log("Spawning fruit " + fruit.name);
            }
            else
            {
                //get from pool
                fruit = _pool[type][0];
                _pool[type].RemoveAt(0);

                fruit.transform.position = position;
                fruit.transform.rotation = rotation;
                fruit.gameObject.SetActive(true);

                //Debug.Log("Recycling fruit " + fruit.name);
            }

            return fruit;

        }
    }

}

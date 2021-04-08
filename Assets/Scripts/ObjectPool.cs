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
    //This class allow us to save the fruits we don't want anymore and when we need new ones we use them instead of instantiate new objects
    public class ObjectPool : MonoBehaviour
    {
        //The pool hashmap. It's separated by the fruit type
        Dictionary<Slot.Type, List<Fruit>> _pool = new Dictionary<Slot.Type, List<Fruit>>();
        
        //Reset the fruit data and add it to the pool
        public void Recycle(Slot.Type type, Fruit fruit)
        {
            fruit.ResetFruit();
            fruit.isInPool = true;
            if (_pool[type] == null)
                _pool[type] = new List<Fruit>();
            _pool[type].Add(fruit);
            fruit.gameObject.SetActive(false);
        }

        //Search in the pool for a fruit of the given type. If it doesn't find any, then instantiate a new one
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
                fruit.isInPool = false;

                fruit.transform.position = position;
                fruit.transform.rotation = rotation;
                fruit.gameObject.SetActive(true);

                //Debug.Log("Recycling fruit " + fruit.name);
            }

            return fruit;

        }
    }

}

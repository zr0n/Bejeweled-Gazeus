using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BejeweledGazeus
{
    [System.Serializable]
    public class Slot
    {
        public Type type;
        public Vector2 position;

        public Slot(Vector2 position, Type type = Type.Blank)
        {
            this.position = position;
            this.type = type;
        }


        //Inner Classes
        [System.Serializable]
        //This class allows to edit a multidimensional array in editor without using any editor scripts
        public struct Group
        {
            public Slot[] slots;

            

            public Group(int width)
            {
                slots = new Slot[width];
                for(int i = 0; i < width; i++)
                    slots[i] = new Slot(Vector2.zero);
            }

            public void AddList(List<Slot> list)
            {
                List<Slot> newSlots = new List<Slot>();

                if (slots == null)
                    slots = new Slot[0];

                foreach (var slot in slots) //keep the old
                    newSlots.Add(slot);

                foreach (var slot in list) //add the new
                    newSlots.Add(slot);

                slots = newSlots.ToArray();
            }
        }
        [System.Serializable]
        public enum Type
        {
            Blank = 0,
            Apple = 1,
            Banana = 2,
            Cherries = 3,
            Orange = 4,
            Pear = 5
        }
    }

   

    
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BejeweledGazeus
{
    public class GameController : MonoBehaviour
    {
        public GameObject[] fruitsTemplates;
        [SerializeField]
        int width = 8;
        [SerializeField]
        int height = 8;
        [SerializeField]
        GameObject gridParent;

        public Slot.Group[] grid;

        [HideInInspector] public Fruit[] swap;

        static Vector2[] _directions =
        {
            Vector2.up,
            Vector2.left,
            Vector2.down,
            Vector2.right
        };


        #region Singleton
        //Singleton Setup (variables)
        public static GameController instance
        {
            get
            {
                return _instance;
            }
        }
        private static GameController _instance;
        //end Singleton (variables)

        //Singleton (exclusivity check)
        void Awake()
        {
            if (_instance)
                Destroy(gameObject);
            else
                _instance = this;
        }
        #endregion Singleton
        //end Singleton (exclusivity check)

        void Start()
        {
            swap = new Fruit[0];

            SetupGrid();
            CheckConnectedNeighbours();
            SpawnFruits();
        }

        //Cast a position in grid (Vector2) to a position in the world (Vector3)
        public static Vector3 GridToWorldPosition(Vector2 gridPosition)
        {
            return new Vector3(-2f + gridPosition.x, 7f - gridPosition.y);
        }

        //Swap a slot for another one and move the fruits to its new position
        public void SwapFruits(Fruit a, Fruit b)
        {
            var slotB = new Slot(b.slot.position, b.slot.type);
            slotB.fruit = b;
            var slotA = new Slot(a.slot.position, a.slot.type);
            slotA.fruit = a;
            
            b.slot = slotA;
            a.slot = slotB;

            grid[(int)a.slot.position.x].slots[(int)a.slot.position.y] = slotA;
            grid[(int)b.slot.position.x].slots[(int)b.slot.position.y] = slotB;

            a.GoToGridPosition();
            b.GoToGridPosition();

            swap = new Fruit[] { a, b };
        }

        //Get a slot in given position
        public Slot GetSlot(Vector2 position)
        {
            if (position.x < 0f || grid.Length <= position.x)
                return new Slot(position);

            var line = grid[(int)position.x];
            var slot = line.slots.Length > position.y && position.y > -1f ? line.slots[(int)position.y] : new Slot(position);
            
            return slot;
        }

        //Search for matching neighbours and delete them
        public void CheckConnectedNeighbours()
        {
            List<Slot> delete;
            bool matchedAtLeastThree = false;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Vector2 position = new Vector2(i, j);
                    Slot.Type fruitType = GetType(position);

                    if (fruitType == Slot.Type.Blank) continue;

                    delete = new List<Slot>();

                    while (GetConnectedNeighbours(position).slots.Length > 0)
                    {
                        Slot slot = GetSlot(position);

                        if (!ContainsSlot(delete, slot))
                        {
                            delete.Add(slot);
                            matchedAtLeastThree = true;
                        }

                        SetTypeAt(position, GetNewFruitType(delete));
                    }
                }
            }

            if (!matchedAtLeastThree && swap.Length > 0)
            {
                SwapFruits(swap[0], swap[1]);
                swap = new Fruit[0];
            }
        }

        //Initialize the grid with random fruits
        void SetupGrid()
        {
            //Create Lines
            grid = new Slot.Group[height];

            //Create Columns
            for (int i = 0; i < grid.Length; i++)
            {
                grid[i] = new Slot.Group(width);
                for(int j = 0; j < width; j++)
                {
                    int randomIndex = Random.Range(0, fruitsTemplates.Length);
                    //We need to add +1 because 0 = blank
                    grid[i].slots[j].type = (Slot.Type) (randomIndex + 1);
                    grid[i].slots[j].position = new Vector2(i, j);
                }
            }
        }

        void SpawnFruits()
        {
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    Slot.Type type = grid[i].slots[j].type;
                    if (type == Slot.Type.Blank) continue;

                    GameObject template = fruitsTemplates[(int) (type - 1)];

                    GameObject fruit = Instantiate(template, gridParent.transform);
                    fruit.GetComponent<Fruit>().SetSlot(grid[i].slots[j]);
                    fruit.transform.localPosition = new Vector3(-2f + i, 7f - j);
                }
            }
        }

        

        //Get Neighbours with the same fruit type (recursive)
        Slot.Group GetConnectedNeighbours(Vector2 position, bool starter = true)
        {
            Slot.Group neighbours = new Slot.Group();
            Slot.Type type = GetType(position);


            //Check if node is in middle of at least two other nodes of the same type
            List<List<Slot>> checkMiddle = new List<List<Slot>>
            {
                new List<Slot> { GetSlot(position + Vector2.up), GetSlot(position + Vector2.down) },
                new List<Slot> { GetSlot(position + Vector2.left), GetSlot(position + Vector2.right) }
            };

            for (int i = 0; i < 2; i++)
            {
                List<Slot> slots = checkMiddle[i];
                if (slots[0].type == slots[1].type && slots[0].type == type)
                {
                    neighbours.AddList(slots);
                }
            }

            //Check if there are at least 3 connected nodes, starting from position and going straight to _directions[i]
            foreach (var direction in _directions)
            {
                int matches = 1;
                var line = new List<Slot>() {};
                
                for(int i = 1; i < 3; i++)
                {
                    Vector2 neighbour = position + (direction * i);
                    if(type != Slot.Type.Blank && GetType(neighbour) == type)
                    {
                        matches++;
                        line.Add(GetSlot(neighbour));
                    }
                }
                if(matches >= 3) //if matched 3 or more...
                    neighbours.AddList(line); //...add the matched nodes to the list
            }

            

            //Check for crossing match
            for(int i = 0; i < _directions.Length; i++)
            {
                int neighbourCross = i + 1;

                List<Slot> matchingNeighbours = new List<Slot>();

                if(i == _directions.Length - 1)
                {
                    neighbourCross = 0;
                }

                List<Slot> neighboursDiagonal = new List<Slot>{ GetSlot(position + _directions[i]), GetSlot(position + _directions[neighbourCross]), GetSlot(position + (_directions[i] + _directions[neighbourCross])), };
                foreach(var n in neighboursDiagonal)
                {
                    if(n.type == type)
                    {
                        matchingNeighbours.Add(n);
                    }
                }

                if (matchingNeighbours.Count >= 3)
                    neighbours.AddList(matchingNeighbours);

            }

            //Recursivity
            if (starter && neighbours.slots != null)
            {
                foreach (var neighbour in neighbours.slots)
                    neighbours.AddList(new List<Slot>(GetConnectedNeighbours(neighbour.position, false).slots));
            }

            //if matched at least 3 colors then add the selected fruit to the list
            if (neighbours.slots != null && neighbours.slots.Length > 0)
            {

                neighbours.AddList(new List<Slot> { GetSlot(position) });
                RemoveDuplicates(ref neighbours);
            }
            else
                neighbours.slots = new Slot[0];

            
            return neighbours;
        }

        //Get the fruit type on specific point of the grid
        Slot.Type GetType(Vector2 position)
        {
            return GetSlot(position).type;
        }

        

        //Filter a slot group removing the duplicates
        void RemoveDuplicates(ref Slot.Group group)
        {
            List<Slot> listSlots = new List<Slot>(group.slots);
            List<Slot> filtered = new List<Slot>();

            for(int i = 0; i < listSlots.Count; i++)
            {
                for (int j = 0; j < listSlots.Count; j++)
                {
                    var slot = listSlots[j];

                    if(slot != listSlots[i] || i == j)
                    {
                        filtered.Add(listSlots[j]);
                    }
                }
            }

            group.slots = filtered.ToArray();
        }

        //Return true if a slot is found. It searches by the slot position and not the object reference (which can be different)
        bool ContainsSlot(List<Slot> list, Slot slot)
        {
            foreach(var s in list)
            {
                if((s.position - slot.position).magnitude < .1f) //are the same spot?
                {
                    return true;
                }
            }

            return false;
        }


        Slot.Type GetNewFruitType(List<Slot> delete)
        {
            List<Slot.Type> filtered = new List<Slot.Type>();
            var types = (Slot.Type[]) System.Enum.GetValues(typeof(Slot.Type));
            //Loop through all values
            foreach (var type in types)
            {
                if (type == Slot.Type.Blank) continue;
                filtered.Add(type);
            }

            foreach (var slot in delete)
                filtered.Remove(slot.type);

            return filtered[Random.Range(0, filtered.Count)];
        }

        void SetTypeAt(Vector2 position, Slot.Type fruitType)
        {
            grid[(int)position.x].slots[(int)position.y].type = fruitType;
        }
    }
}

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
using UnityEngine.SceneManagement;

namespace BejeweledGazeus
{
    public class GameController : MonoBehaviour
    {
        //The prefabs of the fruits. The index is important because it matches the type enum
        public GameObject[] fruitsTemplates;
        //Grid Width
        public int width = 8;
        //Grid Height
        public int height = 8;
        //Each item of this array is an array of slots
        public Slot.Group[] grid;
        public TimeManager timeManager;
        public ObjectPool pool;
        
        [SerializeField]
        //The parent the fruits spawned should have.
        GameObject gridParent;
        [SerializeField]
        //Interval between finish fruit movement and check the grid for connected fruits
        float intervalBeforeCheckGrid = .5f;
        [SerializeField]
        //How much score player earn by fruit cleared
        int scoreByFruit;
        [SerializeField]
        //How much time player earn by fruit cleared
        float timeToIncrementByFruit = .5f;
        [SerializeField]
        AudioManager audioManager;
        [SerializeField]
        Countdown countdown;

        [HideInInspector]
        //Used to memorize the last fruits swapped, so we can back them to original position if it's an invalid movement
        public Fruit[] swap;
        [HideInInspector]
        //Used to calculate the position of the new fruits spawned, it's based on the position of fruits cleared
        public int[] spawnPositions;
        [HideInInspector]
        //If true verify the board on next frame
        public bool shouldCheckBoardOnNextFrame;
        [HideInInspector]
        //List of fruits being moved at that moment, used to block player input while something is going on
        public List<Fruit> movingFruits = new List<Fruit>();
        [HideInInspector]
        //The fruit user clicked (and not dragged away)
        public Fruit fruitClicked;
        [HideInInspector]
        //If true the game is going on, else it is stopped. Used to block player input before and after the game
        public bool gameStarted;

        //Constant variable holding the offset of the directions. It's useful to use in loops, so we don't need to rewrite code
        readonly Vector2[] _neighboursOffsets =
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

            spawnPositions = new int[width];
        }

        void Update()
        {
            ClearMovingFruits();
            if (movingFruits.Count == 0 && shouldCheckBoardOnNextFrame)
            {
                shouldCheckBoardOnNextFrame = false;
                StartCoroutine(WaitAndCheckBoard());
            }
        }

        public void StartGame()
        {
            gameStarted = true;
        }

        //Cast a position in grid (Vector2) to a position in the world (Vector3)
        public static Vector3 GridToWorldPosition(Vector2 gridPosition)
        {
            return new Vector3(-2f + gridPosition.x, 7f - gridPosition.y);
        }

        //Swap a slot for another one and move the fruits to its new position
        public void SwapFruits(Fruit a, Fruit b)
        {
            var slotB = new Slot(b.slot.position, b.slot.type, b);
            var slotA = new Slot(a.slot.position, a.slot.type, a);
            

            a.slot = new Slot(slotB.position, slotA.type, slotA.fruit);
            b.slot = new Slot(slotA.position, slotB.type, slotB.fruit);

            grid[(int)slotB.position.x].slots[(int)slotB.position.y] = a.slot;
            grid[(int)slotA.position.x].slots[(int)slotA.position.y] = b.slot;

            a.GoToGridPosition();
            b.GoToGridPosition();

            swap = new Fruit[] { a, b };
        }

        //Start pulsing procedural animation (scaling up and down) on every neighbour of fruit (except diagonal ones)
        public void StartPulsingNeighbours(Fruit fruit)
        {
            List<Fruit> neighbours = GetNeighbours(fruit);
            foreach(var neighbour in neighbours)
            {
                neighbour.pulsingAnimation.StartAnimation();
            }
        }

        //Stop pulsing procedural animation (scaling up and down) on every neighbour of fruit (except diagonal ones)
        public void StopPulsingNeighbours(Fruit fruit)
        {
            List<Fruit> neighbours = GetNeighbours(fruit);
            foreach (var neighbour in neighbours)
            {
                neighbour.pulsingAnimation.StopAnimation();
            }
        }

        //Get a slot in given position
        public Slot GetSlot(Vector2 position)
        {
            if (position.x < 0f || grid.Length <= position.x)
            {
                Slot newSlot = new Slot(position);
                newSlot.type = Slot.Type.Unassigned;
                return newSlot;
            }

            var line = grid[(int)position.x];
            Slot slot;
            if(position.y < height && position.y > -1f)
            {
                slot = line.slots[(int)position.y];
            }
            else
            {
                slot = new Slot(position);
                slot.type = Slot.Type.Unassigned;
            }
            
            return slot;
        }

        //Search for matching neighbours and delete them
        public void CheckConnectedNeighbours()
        {
            List<Slot> connected;

            bool matchedAtLeastThree = false;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Vector2 position = new Vector2(i, j);
                    Slot.Type fruitType = GetType(position);

                    if (fruitType == Slot.Type.Blank || fruitType == Slot.Type.Unassigned)
                        continue;

                    connected = new List<Slot>();
                    while (GetConnectedNeighbours(position).slots.Length > 0)
                    {
                        var connectedSlots = GetConnectedNeighbours(position).slots;
                        foreach(var s in connectedSlots)
                        {
                            connected.Add(s);
                        }
                        matchedAtLeastThree = true;

                        SetTypeAt(position, GetNewFruitType(connected));
                    }


                    foreach (var slot in connected)
                    {
                        if (slot.fruit && !slot.fruit.falling)
                        {
                            ScoreManager.instance.score += scoreByFruit;
                            timeManager.IncreaseTimer(timeToIncrementByFruit);

                            slot.fruit.StartFalling();
                            slot.type = Slot.Type.Blank;
                            slot.fruit = null;
                            grid[(int)slot.position.x].slots[(int)slot.position.y] = slot;
                        }
                    }
                }
            }

            //didn't make score, then back to fruits original position
            if (!matchedAtLeastThree && swap.Length > 0)
            {
                SwapFruits(swap[0], swap[1]);
            }
            else if (matchedAtLeastThree)
            {
                if(gameStarted) //avoid doing this on initial frame
                    audioManager.PlayAudioClearFruit();

                PushFruitsDown();
                shouldCheckBoardOnNextFrame = true;
            }
            swap = new Fruit[0];
        }

        //Check if a fruit is neighbour of another
        public bool IsNeighbour(Fruit fruitA, Fruit fruitB)
        {
            List<Fruit> neighbours = GetNeighbours(fruitA);

            return !!neighbours.Find((neighbour) => neighbour == fruitB);
        }

        //When game is over: Block Input, show countdown canvas, hide time progress bar
        public void GameOver()
        {
            gameStarted = false;
            FindObjectOfType<Fader>().imageFader.transform.parent.gameObject.SetActive(false);
            countdown.canvasCountdown.SetActive(true);
            StartCoroutine(countdown.StartCounting(RestartGame));
            timeManager.AnimateSliderOut();
        }

        //Reload this scene
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        //Get Fruits neighbours (except diagonal ones)
        List<Fruit> GetNeighbours(Fruit fruit)
        {
            List<Fruit> neighbours = new List<Fruit>();

            foreach(var direction in _neighboursOffsets)
            {
                Vector2 position = fruit.slot.position + direction;
                Fruit neighbourFruit = GetSlot(position).fruit;
                if (neighbourFruit)
                {
                    neighbours.Add(neighbourFruit);
                }
            }

            return neighbours;
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
                    //We need to add +1 because 0 == blank
                    grid[i].slots[j].type = (Slot.Type) (randomIndex + 1);
                    grid[i].slots[j].position = new Vector2(i, j);
                }
            }
        }

        //Spawn fruit at the first frame of game
        void SpawnFruits()
        {
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    Slot.Type type = grid[i].slots[j].type;
                    if (type == Slot.Type.Blank) continue;

                    Fruit fruit = pool.GetFromPoolOrSpawn(type);
                    fruit.transform.parent = gridParent.transform;

                    fruit.SetSlot(grid[i].slots[j]);
                    fruit.transform.localPosition = new Vector3(-2f + i, 7f - j);
                }
            }
        }

        //Check if node is in middle of at least two other nodes of the same type and get neighbours connected
        List<Slot> GetConnectedNeighboursMiddle(Vector2 position, Slot.Type type)
        {
            List<Slot> matchingNeighbours = new List<Slot>();
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
                    foreach (var slot in slots)
                        matchingNeighbours.Add(slot);
                }
            }

            return matchingNeighbours;
        }

        //Check for crossing match
        List<Slot> GetConnectedNeighboursCross(Vector2 position, Slot.Type type)
        {
            for (int i = 0; i < _neighboursOffsets.Length; i++)
            {
                int neighbourCross = i + 1;

                List<Slot> matchingNeighbours = new List<Slot>();

                if (i == _neighboursOffsets.Length - 1)
                {
                    neighbourCross = 0;
                }

                List<Slot> neighboursDiagonal = new List<Slot> { GetSlot(position + _neighboursOffsets[i]), GetSlot(position + _neighboursOffsets[neighbourCross]), GetSlot(position + (_neighboursOffsets[i] + _neighboursOffsets[neighbourCross])), };
                foreach (var n in neighboursDiagonal)
                {
                    if (n.type == type)
                    {
                        matchingNeighbours.Add(n);
                    }
                }

                if (matchingNeighbours.Count >= 3)
                    return matchingNeighbours;
            }

            return new List<Slot>();
        }

        //Check if there are at least 3 connected nodes, starting from position and going straight to _directions[i]
        List<Slot> GetConnectedNeighboursStraight(Vector2 position, Slot.Type type)
        {
            foreach (var direction in _neighboursOffsets)
            {
                int matches = 1;
                var line = new List<Slot>() { };

                for (int i = 1; i < 3; i++)
                {
                    Vector2 neighbour = position + (direction * i);
                    if (type != Slot.Type.Blank && GetType(neighbour) == type)
                    {
                        matches++;
                        line.Add(GetSlot(neighbour));
                    }
                }
                if (matches >= 3) //if matched 3 or more...
                    return line; //...add the matched nodes to the list
            }

            return new List<Slot>();
        }
        

        //Get Neighbours with the same fruit type (recursive)
        Slot.Group GetConnectedNeighbours(Vector2 position, bool starter = true)
        {
            Slot.Group neighbours = new Slot.Group();
            Slot.Type type = GetType(position);


            //Get neighbours in all possible combinations
            neighbours.AddList(GetConnectedNeighboursMiddle(position, type));
            neighbours.AddList(GetConnectedNeighboursStraight(position, type));
            neighbours.AddList(GetConnectedNeighboursCross(position, type));

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

        //Wait for the interval and recheck the board 
        IEnumerator WaitAndCheckBoard()
        {
            yield return new WaitForSeconds(intervalBeforeCheckGrid);
            CheckConnectedNeighbours();
        }

        //Remove invalid fruits from the movingFruits array
        void ClearMovingFruits()
        {
            while (IsThereAnInvalidMovingFruit())
            {
                for (int i = 0; i < movingFruits.Count; i++)
                {
                    if (!movingFruits[i] || movingFruits[i].isInPool)
                        movingFruits.RemoveAt(i);
                }

            }
        }

        //Return true if there is an invalid fruit among the movingFruits
        bool IsThereAnInvalidMovingFruit()
        {
            foreach (Fruit fruit in movingFruits)
                if (!fruit || fruit.isInPool)
                    return true;

            return false;
        }

        //Push fruits to the neighbour down slot if it's empty (gravity effect)
        void PushFruitsDown()
        {
            for(int i = 0; i < width; i++)
            {
                for(int j = (height - 1); j > -1; j--)
                {
                    Vector2 position = new Vector2(i, j);
                    Slot slot = GetSlot(position);

                    //If it is not an empty space: do nothing
                    if (slot.type != Slot.Type.Blank)
                        continue;

                    //else move the pieces down;
                    for(int k = (j - 1); k > -2; k--)
                    {

                        Vector2 nextPosition = new Vector2(i, k);
                        Slot nextSlot = GetSlot(nextPosition);

                        if (nextSlot.type == Slot.Type.Blank)
                            continue;

                        if (nextSlot.type != Slot.Type.Unassigned)
                        {
                            slot.fruit = nextSlot.fruit;
                            slot.type = nextSlot.type;
                            slot.fruit.SetSlot(slot);

                            grid[(int)slot.position.x].slots[(int)slot.position.y] = slot;

                            nextSlot.fruit.GoToGridPosition();

                            nextSlot.fruit = null;
                            nextSlot.type = Slot.Type.Blank;

                            grid[(int)nextSlot.position.x].slots[(int)nextSlot.position.y] = nextSlot;
                        }
                        else
                        {
                            //instantiate new fruits
                            int randomIndex = Random.Range(0, fruitsTemplates.Length);
                            Vector2 spawnPosition = new Vector2(i, -1 - spawnPositions[i]);


                            slot.type = (Slot.Type) (randomIndex + 1);
                            Fruit newFruit = pool.GetFromPoolOrSpawn(slot.type);
                            newFruit.transform.parent = gridParent.transform;
                            newFruit.SetSlot(slot);
                            newFruit.transform.localPosition = GridToWorldPosition(spawnPosition);
                            grid[(int)slot.position.x].slots[(int)slot.position.y] = slot;

                            newFruit.GoToGridPosition();

                            spawnPositions[i]++;
                        }
                        break;
                    }
                }
            }
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

        //Get a new fruit type, ignoring the ones in the list parameter
        Slot.Type GetNewFruitType(List<Slot> delete)
        {
            List<Slot.Type> filtered = new List<Slot.Type>();
            var types = (Slot.Type[]) System.Enum.GetValues(typeof(Slot.Type));
            //Loop through all values
            foreach (var type in types)
            {
                if (type == Slot.Type.Blank || type == Slot.Type.Unassigned) continue;
                filtered.Add(type);
            }

            foreach (var slot in delete)
                filtered.Remove(slot.type);

            return filtered[Random.Range(0, filtered.Count)];
        }

        //Set a slot type at given position
        void SetTypeAt(Vector2 position, Slot.Type fruitType)
        {
            grid[(int)position.x].slots[(int)position.y].type = fruitType;
        }
    }
}

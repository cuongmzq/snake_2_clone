using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Re doing this after watching this series: https://www.youtube.com/watch?v=eZIym7p4LRc
//while coding for createMap(),
//I don't want to have the coordiate like him.
//So I did a little bit customize and reconfig it by my own way
//Also a little bit different handling player node and object:
//combined player head and tail in one list: PlayerNodeList
//Which is tracking both player head and tail gameObject and their Node;

namespace MySnake
{
    public class GameManager : MonoBehaviour
    {
        public int maxWidth;
        public int maxHeight;

        public Color color1;
        public Color color2;
        public Color playerColor;
        public Color fruitColor;

        private Node[,] grid;
        private List<Node> availableNode = new List<Node>();

        private GameObject playerHead;
        private List<NodeObject> playerNodeList = new List<NodeObject>();

        private GameObject fruit;
        private Node fruitNode;

        private const float playerHeadScale = 1.25f;
        private const float playerTailScale = 0.85f;

        public float timer = 0;
        public float playerMoveRate;

        public bool up, down, left, right;

        Direction currentDirection;

        private enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        #region Init
        private void Start()
        {
            createMap();
            centerCamera();

            createPlayerHead();
            createFruit();
        }

        private void createMap()
        {
            Texture2D texture = new Texture2D(maxWidth, maxHeight)
            {
                filterMode = FilterMode.Point
            };

            grid = new Node[maxWidth, maxHeight];

            for (int w = maxWidth - 1; w >= 0; --w)
            {
                for (int h = maxHeight - 1; h >= 0; --h)
                {
                    if ((w + h) % 2 == 0)
                    {
                        texture.SetPixel(w, h, color1);
                    }
                    else
                    {
                        texture.SetPixel(w, h, color2);
                    }

                    Node n = new Node(w, h);

                    grid[w, h] = n;
                    availableNode.Add(n);
                }
            }

            texture.Apply();

            GameObject newMap = new GameObject("Map");

            SpriteRenderer newMapSpriteRenderer = newMap.AddComponent<SpriteRenderer>();
            Rect rect = new Rect(0, 0, maxWidth, maxHeight);
            newMapSpriteRenderer.sprite = Sprite.Create(texture, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
        }

        private GameObject createObject(Color color, string name = "object", int order = 1)
        {
            GameObject newObject = new GameObject(name);

            Texture2D texture = new Texture2D(1, 1)
            {
                filterMode = FilterMode.Point
            };

            texture.SetPixel(0, 0, color);
            texture.Apply();

            SpriteRenderer spriteRenderer = newObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = order;
            Rect rect = new Rect(0, 0, 1, 1);
            spriteRenderer.sprite = Sprite.Create(texture, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            return newObject;
        }

        private void createPlayer()
        {
            playerHead = createObject(playerColor, "Player", 2);
            playerHead.transform.localScale = Vector3.one * playerHeadScale;
        }

        private void createPlayerHead()
        {
            createPlayer();

            Node n = getNode(maxWidth / 2, maxHeight / 2);
            placeObjectOffset(playerHead, n.worldPosition);
            addPlayerNodeObject(playerHead, n);

            availableNode.Remove(n);
        }

        private void createPlayerTail(int x, int y)
        {
            GameObject playerTail = createObject(playerColor, "PlayerTail", 2);
            playerTail.transform.localScale = Vector3.one * playerTailScale;

            Node n = getNode(x, y);
            placeObjectOffset(playerTail, n.worldPosition);
            addPlayerNodeObject(playerTail, n);
        }

        private void addPlayerNodeObject(GameObject obj, Node n)
        {
            NodeObject playerNodeObject = new NodeObject();

            playerNodeObject.node = n;
            playerNodeObject.obj = obj;
            playerNodeList.Add(playerNodeObject);
        }

        private void createFruit()
        {
            fruit = createObject(fruitColor, "Fruit", 1);
            fruitNode = randomPosition();
            fruit.transform.position = fruitNode.worldPosition;

            availableNode.Remove(fruitNode);
        }

        #endregion

        #region Update
        private void getInput()
        {
            up = Input.GetButtonDown("Up");
            down = Input.GetButtonDown("Down");
            left = Input.GetButtonDown("Left");
            right = Input.GetButtonDown("Right");
        }

        private void setPlayerDirection()
        {
            if (up)
            {
                currentDirection = Direction.Up;
            }
            else if (down)
            {
                currentDirection = Direction.Down;
            }
            else if (left)
            {
                currentDirection = Direction.Left;

            }
            else if (right)
            {
                currentDirection = Direction.Right;
            }
        }

        private void movePlayer()
        {
            int x = 0;
            int y = 0;

            switch (currentDirection)
            {
                case Direction.Up:
                    y = 1;
                    break;
                case Direction.Down:
                    y = -1;
                    break;
                case Direction.Left:
                    x = -1;
                    break;
                case Direction.Right:
                    x = 1;
                    break;
            }

            if (playerNodeList.Count == 0)
            {
                return;
            }

            Node targetPosition = getNode(playerNodeList[0].node.x + x, playerNodeList[0].node.y + y);

            if (targetPosition == null)
            {
                //return;
            }
            else
            {
                /* If new position is a snake tail then stop; Put it here before moving any of the
                 * player part */
                for (int playerIndex = playerNodeList.Count - 1; playerIndex > 2; --playerIndex)
                {
                    if (playerNodeList[playerIndex].node == targetPosition)
                        return;
                }

                availableNode.Remove(targetPosition);
                availableNode.Add(playerNodeList[playerNodeList.Count - 1].node);

                //Error: Can't delete when add the first tail !
                if (targetPosition == fruitNode)
                {
                    eatFruit();
                    createPlayerTail(playerNodeList[playerNodeList.Count - 1].node.x, playerNodeList[playerNodeList.Count - 1].node.y);
                }

                /*last tail position is the previous tail position.
                 *Because the head has different scale so differen Position.
                 *If just let the tail position equal to the head position then it's have no different.*/

                for (int playerIndex = playerNodeList.Count - 1; playerIndex > 0; --playerIndex)
                {

                    if (playerIndex == 1)
                    {
                        placeObjectOffset(playerNodeList[playerIndex].obj, playerNodeList[0].node.worldPosition);
                        playerNodeList[playerIndex].node = playerNodeList[0].node;
                        break;
                    }

                    playerNodeList[playerIndex].obj.transform.position = playerNodeList[playerIndex - 1].obj.transform.position;
                    playerNodeList[playerIndex].node = playerNodeList[playerIndex - 1].node;
                }

                placeObjectOffset(playerNodeList[0].obj, targetPosition.worldPosition);
                playerNodeList[0].node = targetPosition;

                Debug.Log(availableNode.Count);

            }
        }

        private void eatFruit()
        {
            fruitNode = randomPosition();
            fruit.transform.position = fruitNode.worldPosition;
        }

        private void Update()
        {
            getInput();
            setPlayerDirection();

            timer += Time.deltaTime;

            if (timer > playerMoveRate)
            {
                movePlayer();
                timer = 0.0f;
            }
        }

        #endregion

        #region Utilities
        /*The Idea is because each tile has Vector2.zero anchor so
        it will be grown from the bottom left.
        So I added 1 - localScale then If they are smaller
        the opration help move upper right a little bit. Otherwise
        minus it to the bottom left again.*/
        private void placeObjectOffset(GameObject obj, Vector3 pos)
        {
            Vector3 newPosition = pos;

            newPosition.x += (1 - obj.transform.localScale.x) * 0.5f;
            newPosition.y += (1 - obj.transform.localScale.y) * 0.5f;

            obj.transform.position = newPosition;
        }

        private void centerCamera()
        {
            Camera cam = Camera.main;
            Vector3 position = Vector3.zero;

            position.x = maxWidth / 2.0f;
            position.y = maxHeight / 2.0f;
            position.z = -10;

            cam.transform.position = position;
        }

        private Node getNode(int x, int y)
        {
            if (x < 0 || x > maxWidth - 1 || y < 0 || y > maxHeight - 1)
                return null;
            return grid[x, y];
        }

        private Node randomPosition()
        {
            if (availableNode.Count <= 0)
            {
                return null;
            }

            int nodeIndex = Random.Range(0, availableNode.Count);

            return getNode(availableNode[nodeIndex].x, availableNode[nodeIndex].y);
        }

        #endregion
    }
}

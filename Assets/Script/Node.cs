using System;
using UnityEngine;
namespace MySnake
{
    public class Node
    {
        public int x;
        public int y;
        public Vector3Int worldPosition;


        public Node()
        {
            x = 0;
            y = 0;
            worldPosition = Vector3Int.zero;
        }

        public Node(int _x, int _y)
        {
            x = _x;
            y = _y;
            worldPosition = new Vector3Int(_x, _y, 0);
        }
    }
}
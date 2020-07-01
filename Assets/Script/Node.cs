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

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
            worldPosition = new Vector3Int(x, y, 0);
        }
    }
}
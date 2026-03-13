using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// -1 for no child

struct Node {
    int myID;
    int firstChild;
    List<int> boidIDs;
    public Node(int myID, int firstChild) {
        this.myID = myID;
        this.firstChild = firstChild;
        boidIDs = new List<int>();
    }

    public List<int> BoidIDs {get => boidIDs; set => boidIDs = value; }
    public int FirstChild {get => firstChild; set => firstChild = value; }


}

// This current QuadTree is implemented using a tpyical tree structure.
// For more performance, should be swapped to flat arrays
public class QuadTreeSearch : IBoidSearch {
    Boid[] boids;
    Vector3[] boidPositions;
    int leafCapacity;
    float simBoundRadius;
    List<Node> Nodes;
    Node root;

    public QuadTreeSearch(int numBoids, int leafCapacity, float simBoundRadius) {
        boids = new Boid[numBoids];
        boidPositions = new Vector3[numBoids];
        this.leafCapacity = leafCapacity;
        this.simBoundRadius = simBoundRadius;
        Node node = new Node(0,-1);
        Nodes.Add(node);
        root = node;
    }

    public void AddBoid(int index, Vector3 position, Boid boid) {
        boidPositions[index] = position;
        boids[index] = boid;

        (Node parentNode, bool success) = findLeaf(position);
        if (!success) {
            Debug.LogWarning("No Cell Found for boid add");
            return;
        }
        if(parentNode.BoidIDs.Count + 1 <= leafCapacity) {
            parentNode.BoidIDs.Add(index);
        }
        else {
            // we need to split here

        }
    }


    (Node, bool) findLeaf(Vector3 position) {
        Vector2 startingBoxDims = new Vector2(simBoundRadius,simBoundRadius);
        return findLeafRecur(position, 1, startingBoxDims, Vector2.zero, root);
    }

    (Node,bool) findLeafRecur(Vector3 pos, int depth, Vector2 boxDims, Vector2 offset, Node currNode) {
        if (isPointInSquare(pos, boxDims, offset)){
            if(currNode.BoidIDs.Count <= leafCapacity) {
                return (currNode, true);
            }
            float width = boxDims.x / depth;
            float height = boxDims.y / depth;

            Vector2 newBoxDims = new Vector2(width,height);

            Vector2 tl = offset;
            Vector2 tr = new Vector2(offset.x + width, offset.y);
            Vector2 bl = new Vector2(offset.x, offset.y + height);
            Vector2 br = new Vector2(offset.x + width, offset.y + height);

            int firstChild = currNode.FirstChild;
            Node tlNode = Nodes[firstChild];
            Node trNode = Nodes[firstChild + 1];
            Node blNode = Nodes[firstChild + 2];
            Node brNode = Nodes[firstChild + 3];

            (Node tlNodeFound, bool tlRes)  = findLeafRecur(pos,depth + 1,newBoxDims,tl,tlNode);
            (Node trNodeFound, bool trRes)  = findLeafRecur(pos,depth + 1,newBoxDims,tr,trNode);
            (Node blNodeFound, bool blRes)  = findLeafRecur(pos,depth + 1,newBoxDims,bl,blNode);
            (Node brNodeFound, bool brRes)  = findLeafRecur(pos,depth + 1,newBoxDims,br,brNode);

            if(tlRes) return (tlNodeFound, tlRes);
            if(trRes) return (trNodeFound, trRes);
            if(blRes) return (blNodeFound, blRes);
            if(brRes) return (brNodeFound, brRes);

        }
        // shouldnt reach here
        return (new Node(), false);
    }



    bool isPointInSquare(Vector3 pos, Vector2 boxDims, Vector2 offset) {
        Vector2 pos2D = new Vector2(pos.x,pos.z);

        bool xGood = pos2D.x >= offset.x && pos2D.x < offset.x + boxDims.x;
        bool zGood = pos2D.y >= offset.y && pos2D.y < offset.y + boxDims.y;

        return xGood && zGood;
    }


    public void RemoveBoid(int index) {
        
    }

    public void UpdatePosition(int index, Vector3 position) {
        
    }

    public (int numNeighbors, int numChecks, Boid[] neighbors) FindNeighbors(int index, float radius) {
        return (0,0,null);
    }
}
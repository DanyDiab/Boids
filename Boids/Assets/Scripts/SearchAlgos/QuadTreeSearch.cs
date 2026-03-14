using System;
using System.Collections.Generic;
using UnityEngine;
// -1 for no child



// This current QuadTree is implemented using a tpyical tree structure.
// For more performance, should be swapped to flat arrays
namespace QuadTree {
    
public class QuadTreeSearch : IBoidSearch {
    Boid[] boids;
    Vector3[] boidPositions;
    int leafCapacity;
    float simBoundRadius;
    List<Node> Nodes;
    SimulationParameters simParams;
    int numBoids;

    public QuadTreeSearch(int numBoids, int leafCapacity, float simBoundRadius, SimulationParameters simParams) {
        boids = new Boid[numBoids];
        boidPositions = new Vector3[numBoids];
        Nodes = new List<Node>();
        this.leafCapacity = leafCapacity;
        this.simBoundRadius = simBoundRadius;
        this.simParams = simParams;
        Node node = new Node(-1);
        Nodes.Add(node);
        numBoids = simParams.NumBoids;
    }

    public void AddBoid(int index, Vector3 position, Boid boid) {
        boidPositions[index] = position;
        boids[index] = boid;
        addBoidToTree(position, index);

    }


    void addBoidToTree(Vector3 position, int index) {
        Vector2 startingBoxDims = new Vector2(simBoundRadius,simBoundRadius);
        findLeafRecur(position, 1, startingBoxDims, new Vector2(-simBoundRadius / 2,-simBoundRadius / 2), 0, index);
    }
    
    void findLeafRecur(Vector3 pos, int depth, Vector2 boxDims, Vector2 offset, int currNodeIndex, int index) {
        if (!isPointInSquare(pos, boxDims, offset)) {
            return;
        }
        float width = boxDims.x / 2;
        float height = boxDims.y / 2;
        Vector2 newBoxDims = new Vector2(width,height);

        Vector2 tl = offset;
        Vector2 tr = new Vector2(offset.x + width, offset.y);
        Vector2 bl = new Vector2(offset.x, offset.y + height);
        Vector2 br = new Vector2(offset.x + width, offset.y + height);

        // leaf node
        Node currNode = Nodes[currNodeIndex];
        if(currNode.FirstChild == -1) {
            currNode.BoidIDs.Add(index);
            if(currNode.BoidIDs.Count <= leafCapacity) {
                Nodes[currNodeIndex] = currNode;
                return;
            }
            // split
            
            Node newtlNode = new Node(-1);
            Node newtrNode = new Node(-1);
            Node newblNode = new Node(-1);
            Node newbrNode = new Node(-1);

            currNode.FirstChild = Nodes.Count;

            Nodes.Add(newtlNode);
            Nodes.Add(newtrNode);
            Nodes.Add(newblNode);
            Nodes.Add(newbrNode);

            // redistribute boids
            foreach(int boidID in currNode.BoidIDs) {
                Vector3 boidPos = boidPositions[boidID];
                if (isPointInSquare(boidPos, newBoxDims, tl)) {
                    newtlNode.BoidIDs.Add(boidID);
                }
                if (isPointInSquare(boidPos, newBoxDims, tr)) {
                    newtrNode.BoidIDs.Add(boidID);
                }
                if (isPointInSquare(boidPos, newBoxDims, bl)) {
                    newblNode.BoidIDs.Add(boidID);
                }
                if (isPointInSquare(boidPos, newBoxDims, br)) {
                    newbrNode.BoidIDs.Add(boidID);
                }
            }
            currNode.BoidIDs.Clear();
            Nodes[currNodeIndex] = currNode;
            return;
        }
        
        // branch recursive step
        int firstChild = currNode.FirstChild;

        findLeafRecur(pos,depth + 1,newBoxDims,tl,firstChild,index);
        findLeafRecur(pos,depth + 1,newBoxDims,tr,firstChild + 1,index);
        findLeafRecur(pos,depth + 1,newBoxDims,bl,firstChild + 2,index);
        findLeafRecur(pos,depth + 1,newBoxDims,br,firstChild + 3,index);
    }

    bool isPointInSquare(Vector3 pos, Vector2 boxDims, Vector2 offset) {
        Vector2 pos2D = new Vector2(pos.x,pos.z);

        bool xGood = pos2D.x >= offset.x && pos2D.x < offset.x + boxDims.x;
        bool zGood = pos2D.y >= offset.y && pos2D.y < offset.y + boxDims.y;

        return xGood && zGood;
    }

    void buildQuadTree() {
        Nodes.Clear();
        for(int i = 0; i < numBoids; i++) {
            Vector3 pos = boidPositions[i];
            addBoidToTree(pos,i);
        }
    }

    public void RemoveBoid(int index) {
        boids[index] = null;
    }

    public void UpdatePosition(int index, Vector3 position) {
        boidPositions[index] = position;
    }

    public (int numNeighbors, int numChecks, Boid[] neighbors) FindNeighbors(int index, float radius) {
        if(index == 0) {
            buildQuadTree();
        }
        return (0,0,null);
    }
}

}
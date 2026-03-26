using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
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
        List<Node> nodes;
        SimulationParameters simParams;
        int numBoids;
        Boid[] currNeighbors;
        int quadrentChecks;
        Node[] nodePool;
        int poolCount;
        List<int> foundBoids;

        public QuadTreeSearch(int numBoids, int leafCapacity, float simBoundRadius, SimulationParameters simParams) {
            boids = new Boid[numBoids];
            boidPositions = new Vector3[numBoids];
            nodes = new List<Node>();
            this.leafCapacity = leafCapacity;
            this.simBoundRadius = simBoundRadius;
            int maxNumNodes = Mathf.Max(4 * numBoids / leafCapacity * 2, 16);
            nodePool = new Node[maxNumNodes];
            poolCount = 0;
            this.simParams = simParams;
            Node node = new Node(-1);
            nodes.Add(node);
            this.numBoids = simParams.NumBoids;
            currNeighbors = new Boid[numBoids - 1];
            quadrentChecks = 0;
            foundBoids = new List<int>();
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
            Node currNode = nodes[currNodeIndex];
            if(currNode.FirstChild == -1) {
                currNode.BoidIDs.Add(index);
                if(currNode.BoidIDs.Count <= leafCapacity) {
                    nodes[currNodeIndex] = currNode;
                    return;
                }
                // split
                

                Node newtlNode, newtrNode, newblNode, newbrNode;

                newtlNode = grabFromPool(-1);
                newtrNode = grabFromPool(-1);
                newblNode = grabFromPool(-1);
                newbrNode = grabFromPool(-1);

                currNode.FirstChild = nodes.Count;

                nodes.Add(newtlNode);
                nodes.Add(newtrNode);
                nodes.Add(newblNode);
                nodes.Add(newbrNode);

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
                nodes[currNodeIndex] = currNode;
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
            addAllNodesToPool();
            nodes.Clear();
            Node node = grabFromPool(-1);
            nodes.Add(node);
            for(int i = 0; i < numBoids; i++) {
                Vector3 pos = boidPositions[i];
                addBoidToTree(pos,i);
            }
            simParams.Nodes = nodes;
        }

        public void RemoveBoid(int index) {
            boids[index] = null;
        }

        public void UpdatePosition(int index, Vector3 position) {
            boidPositions[index] = position;
        }

        bool IsBoidTouchingQuadrent(Vector3 position, float radius, Vector2 boxDims, Vector2 min) {
            quadrentChecks++;
            Vector2 pos2 = new Vector2(position.x,position.z);
            Vector2 max = min + boxDims;

            // find closest point on quadrent to boid
            float cx = Mathf.Clamp(pos2.x,min.x,max.x);
            float cy = Mathf.Clamp(pos2.y,min.y,max.y);

            Vector2 clamped = new Vector2(cx,cy);
            float distance = (pos2 - clamped).sqrMagnitude;
            if(distance < radius * radius) {
                return true;
            }
            return false;
        }

        void FindTouchingLeafs(Vector3 position, float radius, int nodeIndex, Vector2 boxDims, Vector2 offset, List<int> result) {
            if (!IsBoidTouchingQuadrent(position, radius, boxDims, offset)) {
                return;
            }
            Node currNode = nodes[nodeIndex];
            int firstChild = currNode.FirstChild;
            // branch
            Vector2 newDims = new Vector2(boxDims.x / 2, boxDims.y / 2);
            if(firstChild != -1) {
                Vector2 tl = offset;
                Vector2 tr = new Vector2(offset.x + newDims.x, offset.y);
                Vector2 bl = new Vector2(offset.x, offset.y + newDims.y);
                Vector2 br = new Vector2(offset.x + newDims.x, offset.y + newDims.y);

                if (IsBoidTouchingQuadrent(position, radius, newDims, tl)) {
                    FindTouchingLeafs(position,radius,firstChild,newDims,tl, result);
                }
                if (IsBoidTouchingQuadrent(position, radius, newDims, tr)) {
                    FindTouchingLeafs(position,radius,firstChild + 1,newDims,tr,result);
                }
                if (IsBoidTouchingQuadrent(position, radius, newDims, bl)) {
                    FindTouchingLeafs(position,radius,firstChild + 2,newDims,bl,result);
                }
                if (IsBoidTouchingQuadrent(position, radius, newDims, br)) {
                    FindTouchingLeafs(position,radius,firstChild + 3,newDims, br,result);
                }
                return;
            }
            // leaf
            else {
                result.AddRange(currNode.BoidIDs);
            }
        }

        public (int numNeighbors, int numChecks, Boid[] neighbors) FindNeighbors(int index, float radius) {
            if(index == 0) {
                buildQuadTree();
            }
            Vector3 position = boidPositions[index];
            Vector2 boxDims = new Vector2(simBoundRadius,simBoundRadius);
            
            foundBoids.Clear();
            FindTouchingLeafs(position,radius,0,boxDims,new Vector2(-simBoundRadius / 2,-simBoundRadius / 2),foundBoids);

            int numNeighbors = 0;
            int numChecks = 0;
            int count = foundBoids.Count;
            for(int i = 0; i < count; i++) {
                int currBoidID = foundBoids[i];
                if(currBoidID == index) continue;
                numChecks++;
                Boid currBoid = boids[currBoidID];
                Vector3 currBoidPos = boidPositions[currBoidID];

                float dist = (currBoidPos - position).sqrMagnitude;
                if(dist < radius * radius) {
                    currNeighbors[numNeighbors] = currBoid;
                    numNeighbors++;
                }
            }
            return (numNeighbors,numChecks,currNeighbors);
        }


    // grabs a new node if available, otherwise allocates
        Node grabFromPool(int firstChild) {
            if(poolCount == 0) {
                return new Node(-1);
            }
            poolCount--;
            nodePool[poolCount].FirstChild = firstChild;
            return nodePool[poolCount];
        }

        void addAllNodesToPool() {
            for(int i = 0; i < nodes.Count; i++) {
                nodes[i].BoidIDs.Clear();
                // resize if needed
                if(poolCount >= nodePool.Length) {
                    Array.Resize(ref nodePool, nodePool.Length * 2 + 10);
                }
                nodePool[poolCount++] = nodes[i];
            }
        }



    }
}
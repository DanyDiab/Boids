using System.Collections.Generic;

namespace QuadTree {
    public struct Node {
        int firstChild;
        List<int> boidIDs;
        public Node(int firstChild) {
            this.firstChild = firstChild;
            boidIDs = new List<int>();
        }

        public List<int> BoidIDs {get => boidIDs; set => boidIDs = value; }
        public int FirstChild {get => firstChild; set => firstChild = value; }
    } 
}


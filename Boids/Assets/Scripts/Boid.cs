using UnityEngine;

public class Boid : MonoBehaviour{
    [SerializeField] float speed;
    [SerializeField] float radius;
    IBoidSearch search;

    int myIndex;
    GameObject boid; 


    public void init(int index, GameObject boid, IBoidSearch search) {
        boid.SetActive(true);
        myIndex = index;
        this.boid = boid;
        this.search = search;
        search.AddBoid(index,boid.transform.position,this);
    }

    public void disable() {
        boid.SetActive(false);
    }
    void Start(){
         
    }

    
    void Update(){
        move();
    }


    void move() {
        Boid[] neighbors;
        int numNeighbors;
        (numNeighbors, neighbors) = search.FindNeighbors(myIndex,radius);
        

        transform.Translate(Vector3.back * speed * Time.deltaTime);
        search.UpdatePosition(myIndex,transform.position);
    }
}

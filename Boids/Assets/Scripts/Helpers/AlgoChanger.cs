using UnityEngine;
using UnityEngine.InputSystem;

public class AlgoChanger : MonoBehaviour{
    
    [SerializeField] SimulationParameters simParams;
    

    void Update() {
        if(Keyboard.current == null) return;

        if (Keyboard.current.bKey.isPressed) {
            simParams.CurrSearchAlgo = SearchAlgos.BF;
        }
        else if (Keyboard.current.uKey.isPressed) {
            simParams.CurrSearchAlgo = SearchAlgos.UNIFORMGRID;
        }
        else if (Keyboard.current.qKey.isPressed) {
            simParams.CurrSearchAlgo = SearchAlgos.QUADTREE;
        }
    }
}

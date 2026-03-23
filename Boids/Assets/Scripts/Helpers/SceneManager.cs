using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class SlideManager : MonoBehaviour{
    static SlideManager instance;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else {
            Destroy(gameObject);
        }
    }

    void Update() {
        if(Keyboard.current == null) return;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame) {
            goToNextSlide();
        }
        else if(Keyboard.current.leftArrowKey.wasPressedThisFrame) {
            goToPreviousSlide();
        } 
    }

    void goToNextSlide() {
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        if(nextScene > SceneManager.sceneCountInBuildSettings - 1) return;
        SceneManager.LoadScene(nextScene);
    }

    void goToPreviousSlide() {
        int prevScene = SceneManager.GetActiveScene().buildIndex - 1;
        if(prevScene < 0) return;
        SceneManager.LoadScene(prevScene);

    }

}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class SlideManager : MonoBehaviour{
    static SlideManager instance;

    [SerializeField]
    List<string> scenes = new List<string>();

    int currSceneIndex = 0;
    int sceneCount;
    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else {
            Destroy(gameObject);
        }
    }
    void Start() {
        string currentScene = SceneManager.GetActiveScene().name;
        currSceneIndex = scenes.IndexOf(currentScene);
        Debug.Log(currSceneIndex);
        sceneCount = scenes.Count; 
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
        if(currSceneIndex >= sceneCount - 1) return;
        currSceneIndex++;
        SceneManager.LoadScene(scenes[currSceneIndex]);
    }

    void goToPreviousSlide() {
        if(currSceneIndex <= 0) return;
        currSceneIndex--;
        SceneManager.LoadScene(scenes[currSceneIndex]);
    }

}

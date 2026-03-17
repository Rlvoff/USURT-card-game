using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelC : MonoBehaviour
{
    private Animator anim;
    public int levelToLoad;

    public Vector3 position;
    public VectorValue playerStorage;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    public void FadeToLavel()
    {
        anim.SetTrigger("fade");
    }
    public void OnFadeComplete()
    {
        playerStorage.initialValue = position;
        SceneManager.LoadScene(levelToLoad);
    }
    
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if(Instance == null)
            Instance = this;    
    }

    void Start()
    {
        
    }

    

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }
}
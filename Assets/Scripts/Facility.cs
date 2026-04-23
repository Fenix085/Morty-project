using UnityEngine;
using UnityEngine.SceneManagement;

public class Facility : MonoBehaviour
{
    private Canvas canvas;
    public string puzzleScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
        canvas.gameObject.SetActive(false);
    }

    public void ChangeScene()
    {
        
        SceneManager.LoadScene(puzzleScene);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Vector3.Distance(PlayerController_RB.Instance.transform.position, transform.position) < 6)
        {
            canvas.gameObject.SetActive(true);
        }
        else
        {
            canvas.gameObject.SetActive(false);
        }
    }
}

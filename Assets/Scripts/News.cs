using UnityEngine;

public class News : MonoBehaviour
{  
    private Canvas canvas; 
    void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
    }
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

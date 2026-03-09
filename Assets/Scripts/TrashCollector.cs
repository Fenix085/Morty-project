using UnityEngine;

public class TrashCollector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что объект с тегом Trash
        if (other.CompareTag("Trash"))
        {
            Debug.Log("Trash collected, good job!");
            Destroy(other.gameObject); // Удаляем объект
        }
    }
}
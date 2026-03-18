using UnityEngine;

public class TrashCollector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other == null || !other.CompareTag("Trash"))
            return;

        try
        {
            TrashCounterUI.GetOrCreate()?.RegisterCollectedTrash(other.gameObject);
        }
        catch
        {
        }

        Destroy(other.gameObject);
    }
}

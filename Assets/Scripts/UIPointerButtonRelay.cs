using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIPointerButtonRelay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private UnityEvent onPointerDown;
    [SerializeField] private UnityEvent onPointerUp;

    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp?.Invoke();
    }
}

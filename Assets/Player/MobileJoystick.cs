using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    [SerializeField] private float movementRange = 80f;

    private RectTransform backgroundRect;

    public static MobileJoystick Instance { get; private set; }

    public Vector2 Value { get; private set; }

    private void Awake()
    {
        backgroundRect = background != null ? background : (RectTransform)transform;

        if (handle == null && backgroundRect.childCount > 0)
            handle = backgroundRect.GetChild(0) as RectTransform;

        ResetHandle();
    }

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateStick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateStick(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetHandle();
    }

    private void UpdateStick(PointerEventData eventData)
    {
        if (backgroundRect == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                backgroundRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        Value = Vector2.ClampMagnitude(localPoint / movementRange, 1f);

        if (handle != null)
            handle.anchoredPosition = Value * movementRange;
    }

    private void ResetHandle()
    {
        Value = Vector2.zero;

        if (handle != null)
            handle.anchoredPosition = Vector2.zero;
    }
}

using UnityEngine;
using DG.Tweening;

public class Crusher : MonoBehaviour
{
    [Header("Tween Settings")]
    [SerializeField] private float upDistance = 1f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float delay = 0f;
    [SerializeField] private Ease ease = Ease.InSine;
    [SerializeField] private bool playOnStart = true;

    private Tween upDownTween;
    private float startY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startY = transform.position.y;

        if (playOnStart)
            StartTween();
    }

    // Create and start the up/down tween
    public void StartTween()
    {
        KillTween();

        Vector3 target = new Vector3(transform.position.x, startY + upDistance, transform.position.z);
        upDownTween = transform.DOMove(target, duration).SetEase(ease).SetDelay(delay).SetLoops(-1, LoopType.Yoyo);
    }

    // Stop and cleanup the tween
    public void KillTween()
    {
        if (upDownTween != null)
        {
            upDownTween.Kill();
            upDownTween = null;
        }
    }

    void OnDisable()
    {
        KillTween();
    }

    void OnDestroy()
    {
        KillTween();
    }
}

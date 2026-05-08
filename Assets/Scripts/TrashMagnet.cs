using UnityEngine;
using System.Collections;

public class TrashMagnet : MonoBehaviour
{
    public float attractDistance = 3f;
    public float moveSpeed = 5f;

    private Transform player;
    private bool isAttracting = false;
    private Collider col;

    public AudioClip attractSound;
    private AudioSource activeSource;
    private bool isFadingOut = false;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        col = GetComponent<Collider>();
    }


    void Update()
    {
        if (!col.isTrigger) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < attractDistance && !isAttracting)
        {
            isAttracting = true;
            
            if (SoundEffectsManager.Instance != null && SoundEffectsManager.Instance.vacuumSound != null)
            {
                activeSource = SoundEffectsManager.Instance.PlayLoopingSound(
                    SoundEffectsManager.Instance.vacuumSound,
                    SoundEffectsManager.Instance.volume
                );
            }
        }

        if (isAttracting)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }

        
        if (distance < 0.2f && !isFadingOut)
        {
            StartCoroutine(FadeAndDestroy());
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        isFadingOut = true;
        float startVol = activeSource != null ? activeSource.volume : 0;
        float duration = 0.2f;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (activeSource != null)
            {
                activeSource.volume = Mathf.Lerp(startVol, 0, timer / duration);
            }
            yield return null;
        }

        if (activeSource != null)
        {
            Destroy(activeSource.gameObject);
        }

        Destroy(gameObject);
    }
}
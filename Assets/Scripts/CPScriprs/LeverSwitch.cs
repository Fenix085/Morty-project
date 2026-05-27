using System.Collections;
using UnityEngine;

public class LeverSwitch : MonoBehaviour
{
    [SerializeField, Tooltip("Lever that appears after interaction")]
    private GameObject newLever;

    [SerializeField, Tooltip("Maximum interaction distance")]
    private float interactRange = 2.5f;

    [SerializeField, Tooltip("Object to enable on activation")]
    private GameObject glowObject;

    [SerializeField, Tooltip("Activation sound")]
    private AudioClip activationSound;

    private Transform _player;
    private Collider _playerCollider;
    private Collider _leverCollider;
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        if (ControllerForLevel.Instance != null)
        {
            _player = ControllerForLevel.Instance.transform;
            _playerCollider = ControllerForLevel.Instance.GetComponent<Collider>();
        }
        else
            Debug.LogError("LeverSwitch: ControllerForLevel instance not found!");

        _leverCollider = GetComponent<Collider>();

        if (newLever != null)
            newLever.SetActive(false);

        if (glowObject != null)
            glowObject.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (_player == null)
        {
            Debug.LogError("LeverSwitch: Player not found!");
            return;
        }
    }

    private void Activate()
    {
        if (newLever != null)
        {
            newLever.transform.position = transform.position;
            newLever.transform.rotation = transform.rotation;
            newLever.SetActive(true);
        }

        if (glowObject != null)
            glowObject.SetActive(true);

        // играем звук через отдельный объект который не выключается
        if (activationSound != null)
            AudioSource.PlayClipAtPoint(activationSound, transform.position);

        gameObject.SetActive(false);
    }

    public void fiftyfivetosixtyone()
    {
        Vector3 leverPos = _leverCollider != null ? _leverCollider.bounds.center : transform.position;
        Vector3 playerPos = _playerCollider != null ? _playerCollider.bounds.center : _player.position;
        float dist = Vector3.Distance(leverPos, playerPos);

        if (dist > interactRange) return;

        Activate();
    }


}
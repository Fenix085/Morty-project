using UnityEngine;

public class LeverSwitch : MonoBehaviour
{
    [SerializeField, Tooltip("Lever that appears after interaction")]
    private GameObject newLever;

    [SerializeField, Tooltip("Maximum interaction distance")]
    private float interactRange = 2.5f;

    [SerializeField, Tooltip("Object to enable on activation")]
    private GameObject glowObject;

    private Transform _player;
    private Collider _playerCollider;

    private Collider _leverCollider;

    private void Start()
    {
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
        if (!Input.GetKeyDown(KeyCode.F)) return;

        if (_player == null)
        {
            Debug.LogError("LeverSwitch: Player not found! Make sure Player tag is set.");
            return;
        }

        Vector3 leverPos = _leverCollider != null ? _leverCollider.bounds.center : transform.position;
        Vector3 playerPos = _playerCollider != null ? _playerCollider.bounds.center : _player.position;
        float dist = Vector3.Distance(leverPos, playerPos);
        Debug.Log($"LeverSwitch: distance to player = {dist}, interactRange = {interactRange}");

        if (dist > interactRange)
        {
            Debug.Log("LeverSwitch: too far away");
            return;
        }

        Activate();
    }

    private void Activate()
    {
        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = transform.rotation;

        // show new lever and light
        if (newLever != null)
        {
            newLever.transform.position = spawnPos;
            newLever.transform.rotation = spawnRot;
            newLever.SetActive(true);
        }

        if (glowObject != null)
            glowObject.SetActive(true);

        // disable old lever
        gameObject.SetActive(false);
    }
}
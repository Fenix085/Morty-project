using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // inspector variables
    [SerializeField, Tooltip("Player transform for camera to follow")]
    private Transform playerTransform;
    [SerializeField, Tooltip("Camera offset from player (x not used)")]
    private Vector3 offsetPosition = new Vector3(0, 5, 5);
    [SerializeField]
    private bool lookAt = true;

    // privates
    private Transform _mainCam = null;

    // Use this for initialization
    private void Start()
    {
        if(playerTransform == null)
        {
            Debug.LogError("CameraMovement is missing playerTransform");
        }
        else
        {
            _mainCam = Camera.main.transform;
        }
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        UpdateCamera();
    }

    /// <summary>
    /// Update camera position and rotation
    /// </summary>
    private void UpdateCamera()
    {
        if (playerTransform == null)
        {
            return;
        }
        // camera rig position
        transform.position = playerTransform.position + -(playerTransform.forward * offsetPosition.z) + (playerTransform.up * offsetPosition.y);
        // point camera at player
        if (lookAt)
        {
            // point camera at player using players up direction
            _mainCam.LookAt(playerTransform, playerTransform.up);
        }
        else
        {
            _mainCam.LookAt(playerTransform);
        }
    }
}

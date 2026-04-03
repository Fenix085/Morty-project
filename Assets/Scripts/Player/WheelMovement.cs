using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelMovement : MonoBehaviour {

    // inspector variables
    [SerializeField, Tooltip("Wheel roller gameobjects to rotate")]
    private List<Transform> rollers = new List<Transform>();
    [SerializeField, Tooltip("Adjust rotation speed")]
    private float speedAdjustment = 100;

    // privates
    private Rigidbody _playerRB = null;
    private float _playerSpeed = 0f;
    private Vector3 _prevPosition = Vector3.zero;

    // Use this for initialization
    private void Start()
    {
        // get rigidbody
        if (gameObject.GetComponent<Rigidbody>())
        {
            _playerRB = gameObject.GetComponent<Rigidbody>();
            // set previous position
            _prevPosition = _playerRB.position;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("No rigidbody found on gameObject... WheelMovement script");
#endif
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // estimate player movement speed
        _playerSpeed = (_playerRB.position - _prevPosition).magnitude * 1 / Time.deltaTime;
        _prevPosition = _playerRB.position;
        // rotate rollers
        if(speedAdjustment > 0)
        {
            Rotate(_playerSpeed * speedAdjustment);
        }
    }

    /// <summary>
    /// Rotate wheel rollers
    /// </summary>
    /// <param name="speed"></param>
    private void Rotate(float speed)
    {
        int count = rollers.Count;
        for (int i = 0; i < count; i++)
        {
            rollers[i].Rotate((-Vector3.up * speed) * Time.deltaTime);
        }
    }
}

/*********************************************************************************************************
 * Class controls players flash light;
 * - works by checking if a raycast has to pass through the players body to reach the scenes light source 
 * *******************************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDetection : MonoBehaviour {

    // inspector variables
    [SerializeField, Tooltip("Main light in the scene")]
    private Transform lightSource;
    [SerializeField, Tooltip("Players torch light")]
    private GameObject spotLight;
    [SerializeField, Tooltip("Players torch light avatar, emissive object to turn on/off *Optional*")]
    private GameObject spotLightAvatar;
    [SerializeField, Tooltip("Radius used to project raycasts from.")]
    private float objRadius = 10;
    
    // privates
    private Vector3 _direction;
    private float _distance = 10;
    private bool _lightOn = false;
    private bool _lightPrevState = true; // set to true by default so that light state is checked on start
    private int _rayCount = 4;
    private List<GameObject> _rayPoints = new List<GameObject>();
    private Material _lightAvatarMat = null;
    private Color _lightAvatarEmissive;

    // Use this for initialization
    private void Start()
    {
        // setup up raycast points
        CreateRayPoints();
        // set light avatar material if one
        if(spotLightAvatar != null)
        {
            _lightAvatarMat = spotLightAvatar.GetComponent<Renderer>().material;
            _lightAvatarEmissive = _lightAvatarMat.GetColor("_EmissionColor");
            
            SwitchLightAvatar(_lightOn);
        }
    }

    // FixedUpdate is called every fixed framerate frame
    private void FixedUpdate()
    {
        if (lightSource != null)
        {
            //get direction lightsource is facing
            _direction = -lightSource.forward;
            // loop through raycast points
            for (int i = 0; i < _rayCount; i++)
            {
#if UNITY_EDITOR
                Debug.DrawRay(_rayPoints[i].transform.position, _direction * _distance, Color.magenta);
#endif
                // check if raycast intersects with player
                RaycastHit hitInfo;
                if (Physics.Raycast(_rayPoints[i].transform.position, _direction, out hitInfo, _distance))
                {
                    if (hitInfo.transform.tag == "Player")
                    {
                        _lightOn = true;
                        break; // don't bother checking the rest
                    }
                }
                else if (spotLight.activeSelf)
                {
                    _lightOn = false;
                }
            }
            // check if light state has changed
            if (_lightOn != _lightPrevState)
            {
                // set light to result
                spotLight.SetActive(_lightOn);
                // set light avatar
                SwitchLightAvatar(_lightOn);
                // update prev light state
                _lightPrevState = _lightOn;
            }

        }
    }
    /// <summary>
    /// Create objects to raycst from
    /// </summary>
    private void CreateRayPoints()
    {
        float x;
        float z;
        float angle = 25f;

        for (int i = 0; i < _rayCount; i++)
        {
            // set x/y positions for raycast point
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * objRadius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * objRadius;
            // add rayPoint gameObject to list
            _rayPoints.Add( new GameObject("RayCast_" + i));
            // set parent transform so that point moves with player
            _rayPoints[i].transform.SetParent(transform);
            // set rayPoint position
            _rayPoints[i].transform.localPosition = new Vector3(x, 0, z);
            // increment angle for next rayPoint
            angle += (360f / _rayCount);
        }
    }

    /// <summary>
    /// Switch Spot Light Avatar emission on and off
    /// </summary>
    /// <param name="turnOn"></param>
    private void SwitchLightAvatar(bool turnOn)
    {
        // turn on emissive color
        if(turnOn)
        {
            _lightAvatarMat.SetColor("_EmissionColor", _lightAvatarEmissive);
        }
        else
        {
            _lightAvatarMat.SetColor("_EmissionColor", Color.black);
        }
    }
}

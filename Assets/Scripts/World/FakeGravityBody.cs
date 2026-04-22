/*********************************************************************************************************
 * The FakeGravityBody class should be place on any moveable object you want drawn to your world
 * *******************************************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FakeGravityBody : MonoBehaviour {

    // inspector variables
    [SerializeField, Tooltip("Attractor object to be drawn to, if left blank first available world will be used")]
    private FakeGravity attractor;
    [SerializeField, Tooltip("Set object solid once settled")]
    private bool setSolid = false;
    
    // privates
    private Transform _objTransform;
    private Rigidbody _objRigidbody;
    
    // properties
    public FakeGravity Attractor { get { return attractor; } set { attractor = value; } }
    
    // Use this for initialization
	private void Start () {
        // set rigidbody
        _objRigidbody = GetComponent<Rigidbody>();
        _objRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        _objRigidbody.useGravity = false;
        _objTransform = transform;
        // get attractor if not provided
        if (attractor == null)
        {
            attractor = ResolveAttractor();
            if (attractor == null)
            {
                Debug.LogWarning("FakeGravityBody: No object tagged 'World' was found. Assign an attractor in the inspector.", gameObject);
            }
        }
	}

    /// <summary>
    /// Find a valid world attractor in scene and recover from missing setup.
    /// </summary>
    private FakeGravity ResolveAttractor()
    {
        FakeGravity[] gravitySources = FindObjectsByType<FakeGravity>(FindObjectsSortMode.None);
        if (gravitySources.Length > 0)
        {
            return gravitySources[0];
        }

        GameObject worldObject = null;
        try
        {
            worldObject = GameObject.FindGameObjectWithTag("World");
        }
        catch (UnityException)
        {
            // Ignore missing tag configuration and keep searching by name.
        }

        if (worldObject == null)
        {
            Transform[] sceneObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int count = sceneObjects.Length;
            for (int i = 0; i < count; i++)
            {
                if (sceneObjects[i].name.ToLowerInvariant() == "world")
                {
                    worldObject = sceneObjects[i].gameObject;
                    break;
                }
            }
        }

        if (worldObject == null)
        {
            return null;
        }

        FakeGravity worldGravity = worldObject.GetComponent<FakeGravity>();
        if (worldGravity == null)
        {
            // Blend model imports often drop gameplay scripts.
            worldGravity = worldObject.AddComponent<FakeGravity>();
            Debug.LogWarning("FakeGravityBody: Added missing FakeGravity component to world object at runtime.", worldObject);
        }

        return worldGravity;
    }
	
	// Update is called once per frame
	private void Update () {
        // return if kinematic
        if (_objRigidbody.isKinematic)
        {
            return;
        }
        // check if object sleeping yet
        if (setSolid)
        {
            ObjectResting();
        }
        // apply gravity to object
        if (attractor != null)
        {
            attractor.Attract(_objTransform);
        }
	}

    /// <summary>
    /// Check if rigidbody is sleeping
    /// </summary>
    private void ObjectResting()
    {
        if(gameObject.GetComponent<Rigidbody>().IsSleeping())
        {
            _objRigidbody.isKinematic = true;
        }
    }
}

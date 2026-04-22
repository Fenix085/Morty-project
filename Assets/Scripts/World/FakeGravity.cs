/*********************************************************************************************************
 * The FakeGravity class should be place on your world object that other objects are to be drawn to.
 * *******************************************************************************************************/ 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeGravity : MonoBehaviour {

    // inspector variables
    [SerializeField, Tooltip("Amount of gravity to be applied to objects")]
    private float gravity = -10;
    [SerializeField, Tooltip("Planet Radius")]
    private float size = 10;

    // privates
    private string _worldObjectTag = "World";
    private float _objRotSpeed = 50;
    private float _gravityBoost = 0;

    // properties
    public float WorldSize { get { return size; } }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        gameObject.tag = _worldObjectTag;
        EnsureCollisionSurface();
    }

    /// <summary>
    /// Ensure this world has at least one non-trigger collider so objects can land.
    /// </summary>
    private void EnsureCollisionSurface()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        if (colliders.Length > 0)
        {
            return;
        }

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
        int count = meshFilters.Length;
        int added = 0;
        for (int i = 0; i < count; i++)
        {
            MeshFilter meshFilter = meshFilters[i];
            if (meshFilter.sharedMesh == null)
            {
                continue;
            }

            if (meshFilter.GetComponent<Collider>() != null)
            {
                continue;
            }

            MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            added++;
        }

        if (added > 0)
        {
            Debug.LogWarning("FakeGravity: Added mesh colliders to world model at runtime because no colliders were found.", gameObject);
            return;
        }

        SphereCollider fallbackCollider = gameObject.AddComponent<SphereCollider>();
        fallbackCollider.radius = Mathf.Max(0.5f, size);
        Debug.LogWarning("FakeGravity: Added fallback sphere collider at runtime because no mesh colliders could be created.", gameObject);
    }

    /// <summary>
    /// Generates false gravity for world objects 
    /// </summary>
    /// <param name="objBody"></param>
    public void Attract(Transform objBody)
    {
        // set planet gravity direction for the object body
        Vector3 gravityDir = (objBody.position - transform.position).normalized;
        Vector3 bodyUp = objBody.up;
        // apply gravity to objects rigidbody
        objBody.GetComponent<Rigidbody>().AddForce(gravityDir * (gravity + _gravityBoost));
        // update the objects rotation in relation to the planet
        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityDir) * objBody.rotation;
        // smooth rotation
        objBody.rotation = Quaternion.Slerp(objBody.rotation, targetRotation, _objRotSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Function to increase world gravity during transfer
    /// </summary>
    /// <param name="increaseGravity"></param>
    public void IncreaseGravity(bool increaseGravity, float amount)
    {
        if(increaseGravity)
        {
            _gravityBoost = gravity * amount;
        }
        else
        {
            _gravityBoost = 0;
        }
    }
}

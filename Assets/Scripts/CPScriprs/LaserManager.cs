using System.Collections.Generic;
using UnityEngine;

public class LaserManager : MonoBehaviour
{
    public static LaserManager Instance;

    private Dictionary<string, Laser> lasers = new Dictionary<string, Laser>();

    void Awake()
    {
        Instance = this;

        Laser[] allLasers = FindObjectsOfType<Laser>();

        foreach (var l in allLasers)
        {
            if (!lasers.ContainsKey(l.laserID))
                lasers.Add(l.laserID, l);
        }
    }

    public void SetLaser(string id, bool state)
    {
        if (lasers.ContainsKey(id))
        {
            lasers[id].SetState(state);
        }
    }
}
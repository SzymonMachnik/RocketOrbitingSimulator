using UnityEngine;
using System.Collections.Generic;

public class StaticObjectsOrbiting : MonoBehaviour
{
    private const float G = 100f;
    public GameObject earth;
    public GameObject satelite;

    
    LineRenderer lr;
    private List<Vector3> trajectoryPoints = new List<Vector3>();

    void Start() {
        lr = satelite.GetComponent<LineRenderer>();
        if (lr == null)
        {
            lr = satelite.AddComponent<LineRenderer>();
        }

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.positionCount = 0;

        InitialVelocity();
    }

    void FixedUpdate()
    {
        Gravity();
        UpdateTrajectory();
    }

    void Gravity()
    {
        float m1 = satelite.GetComponent<Rigidbody>().mass;
        float m2 = earth.GetComponent<Rigidbody>().mass;
        float r = Vector3.Distance(satelite.transform.position, earth.transform.position);

        satelite.GetComponent<Rigidbody>().AddForce((earth.transform.position - satelite.transform.position).normalized *
            (G * (m1 * m2) / (r * r)));
    }

    void InitialVelocity()
    {
        satelite.transform.LookAt(earth.transform);

        satelite.GetComponent<Rigidbody>().linearVelocity = satelite.transform.right * 150;
    }

    void UpdateTrajectory()
    {
        trajectoryPoints.Add(satelite.transform.position);

        if (trajectoryPoints.Count > 5000)
        {
            trajectoryPoints.RemoveAt(0);
        }

        lr.positionCount = trajectoryPoints.Count;
        lr.SetPositions(trajectoryPoints.ToArray());
    }
}
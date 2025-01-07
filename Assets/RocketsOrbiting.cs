using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class RocketsOrbiting : MonoBehaviour
{
    private const float G = 100f;
    public GameObject earth;
    public GameObject rocket;
    public int startVelocity = 0;
    private Rigidbody rocketRigidbody;

    private TMP_Text uiTextRocketOrbitHeight;
    private TMP_Text uiTextRocketVelocity;
    private TMP_Text uiTextVelocityChangeSliderValue;
    private Slider uiSliderVelocityChange;

    private Slider uiSliderInitialVelocity;

    LineRenderer lr;
    private List<Vector3> trajectoryPoints = new List<Vector3>();

    private bool isRocketLaunched = false;
    public bool boostAfterLaunch = false;
    private bool isRocketBoostedAfterStart = false;

    bool rocketFlying = false;

    void Start() 
    {
        rocketRigidbody = rocket.GetComponent<Rigidbody>();

        lr = rocket.GetComponent<LineRenderer>();
        if (lr == null) {
            lr = rocket.AddComponent<LineRenderer>();
        }

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.positionCount = 0;

        GameObject gameObject = GameObject.FindGameObjectWithTag("UiTextRocketVelocity");
        if (gameObject != null) {
            uiTextRocketVelocity = gameObject.GetComponent<TMP_Text>();
        } else {
            Debug.Log("UiTextRocketVelocity not found.");
        }

        gameObject = GameObject.FindGameObjectWithTag("UiTextRocketOrbitHeight");
        if (gameObject != null) {
            uiTextRocketOrbitHeight = gameObject.GetComponent<TMP_Text>();
        } else {
            Debug.Log("UiTextRocketOrbitHeight not found.");
        }

        gameObject = GameObject.FindGameObjectWithTag("UiSliderVelocityChange");
        if (gameObject != null) {
            uiSliderVelocityChange = gameObject.GetComponent<Slider>();
        } else {
            Debug.Log("UiSliderVelocityChange not found.");
        }

        gameObject = GameObject.FindGameObjectWithTag("UiTextVelocityChangeSliderValue");
        if (gameObject != null) {
            uiTextVelocityChangeSliderValue = gameObject.GetComponent<TMP_Text>();
        } else {
            Debug.Log("UiTextVelocityChangeSliderValue not found.");
        }


        gameObject = GameObject.FindGameObjectWithTag("UiSliderInitialVelocity");
        if (gameObject != null) {
            uiSliderInitialVelocity = gameObject.GetComponent<Slider>();
        } else {
            Debug.Log("UiSliderInitialVelocity not found.");
        }

    }

    void FixedUpdate() 
    {
        if (rocketFlying) {
            Gravity();
            UpdateTrajectory();
            Rotate();
            if (boostAfterLaunch && isRocketBoostedAfterStart == false) {
                BoostRocketAfterStart();
            }
        }
    }

    void Update() 
    {
        UpdateText();
        UpdateSlider();
    }

    void Gravity()
    {
        float m1 = rocketRigidbody.mass;
        float m2 = earth.GetComponent<Rigidbody>().mass;
        float r = Vector3.Distance(rocket.transform.position, earth.transform.position);

        rocketRigidbody.AddForce((earth.transform.position - rocket.transform.position).normalized *
            (G * (m1 * m2) / (r * r)));
    }

    public void OnButtonClickLaunchRocket() 
    {
        if (isRocketLaunched == false) {
            rocket.transform.LookAt(earth.transform);

            if (startVelocity == 0) {
                startVelocity = (int)uiSliderInitialVelocity.value;
            }
            rocketRigidbody.linearVelocity = rocket.transform.right * startVelocity;

            rocketFlying = true;
        }
        // CalculateOrbitalHeight();
    }

    void UpdateTrajectory() 
    {
        trajectoryPoints.Add(rocket.transform.position);

        if (trajectoryPoints.Count > 5000) {
            trajectoryPoints.RemoveAt(0);
        }

        lr.positionCount = trajectoryPoints.Count;
        lr.SetPositions(trajectoryPoints.ToArray());
    }


    void UpdateText()
    {
        if (uiTextRocketVelocity != null) {
            uiTextRocketVelocity.text = $"VELOCITY: {rocketRigidbody.linearVelocity.magnitude:F2}\n";
        }

        if (uiTextRocketOrbitHeight != null) {
            float distance = Vector3.Distance(rocket.transform.position, earth.transform.position) - 348 - 50;
            distance = Mathf.Max(distance, 0);
            uiTextRocketOrbitHeight.text = $"DISTANCE TO EARTH: {distance:F2}\n";
        }
    }

    void UpdateSlider() {
        uiTextVelocityChangeSliderValue.text = (uiSliderVelocityChange.value).ToString();
    }

    public void OnButtonClickBoostRocket() {
        rocketRigidbody.linearVelocity += rocketRigidbody.linearVelocity.normalized * uiSliderVelocityChange.value;
    }

    void Rotate() {
        float distance = Vector3.Distance(rocket.transform.position, earth.transform.position);
        float xCord = rocket.transform.position.x;
        float yAngle = Mathf.Acos(xCord / distance) * Mathf.Rad2Deg * -1;
        if (rocket.transform.position.z < 0) {
            yAngle *= -1;
        }
        rocket.transform.rotation = Quaternion.Euler(0, yAngle, 0);
    }

    void CalculateOrbitalHeight()
    {
        float earthMass = earth.GetComponent<Rigidbody>().mass;
        int earthRadius = 348;
        float energyStart = (0.5f * Mathf.Pow(startVelocity, 2)) - (G * earthMass / earthRadius);
        float orbitalRadius = (float)(G * earthMass / (-2 * energyStart));
        float orbitalHeight = orbitalRadius - earthRadius;

        Debug.Log($"Docelowa wysokość orbity: {orbitalHeight:F2}");
    }

    void BoostRocketAfterStart() {
        if (rocket.transform.position.z < 0) {
            float rCurrent = Vector3.Distance(rocket.transform.position, earth.transform.position);
            float earthMass = earth.GetComponent<Rigidbody>().mass;
            float vOrbit = Mathf.Sqrt(G * earthMass / rCurrent);
            float vCurrent = rocketRigidbody.linearVelocity.magnitude;
            float deltaV = vOrbit - vCurrent;

            rocketRigidbody.linearVelocity += rocketRigidbody.linearVelocity.normalized * deltaV;

            Debug.Log($"Boosted! vOrbit: {vOrbit:F2}, vCurrent: {vCurrent:F2}, deltaV: {deltaV:F2}");

            isRocketBoostedAfterStart = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == earth && rocketFlying) 
        {
            rocketRigidbody.linearVelocity = Vector3.zero;
            rocketRigidbody.angularVelocity = Vector3.zero;

            rocketRigidbody.isKinematic = true;

            rocketFlying = false;
            Debug.Log("Rocket hit the ground!");
        }
    }
}
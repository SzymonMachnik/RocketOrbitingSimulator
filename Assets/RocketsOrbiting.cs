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

        GameObject textObject = GameObject.FindGameObjectWithTag("UiTextRocketVelocity");
        if (textObject != null) {
            uiTextRocketVelocity = textObject.GetComponent<TMP_Text>();
        }

        textObject = GameObject.FindGameObjectWithTag("UiTextRocketOrbitHeight");
        if (textObject != null) {
            uiTextRocketOrbitHeight = textObject.GetComponent<TMP_Text>();
        }

        GameObject sliderObject = GameObject.FindGameObjectWithTag("UiSliderVelocityChange");
        uiSliderVelocityChange = sliderObject.GetComponent<Slider>();

        textObject = GameObject.FindGameObjectWithTag("UiTextVelocityChangeSliderValue");
        uiTextVelocityChangeSliderValue = textObject.GetComponent<TMP_Text>();

        sliderObject = GameObject.FindGameObjectWithTag("UiSliderInitialVelocity");
        uiSliderInitialVelocity = sliderObject.GetComponent<Slider>();
        
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
                Debug.Log(startVelocity);
            }
            Debug.Log(startVelocity);
            rocketRigidbody.linearVelocity = rocket.transform.right * startVelocity;

            rocketFlying = true;
        }

        // CalculateOrbitalHeight();
    }

    void UpdateTrajectory() 
    {
        trajectoryPoints.Add(rocket.transform.position);

        if (trajectoryPoints.Count > 5000)
        {
            trajectoryPoints.RemoveAt(0);
        }

        lr.positionCount = trajectoryPoints.Count;
        lr.SetPositions(trajectoryPoints.ToArray());
    }

    void HandleInput() 
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            rocketRigidbody.linearVelocity += rocketRigidbody.linearVelocity.normalized * 10f;
        }
    }

    void UpdateText()
    {
        if (uiTextRocketVelocity != null)
        {
            uiTextRocketVelocity.text = $"VELOCITY: {rocketRigidbody.linearVelocity.magnitude:F2}\n";
        }

        if (uiTextRocketOrbitHeight != null)
        {
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
        int EarthRadius = 348;
        // Energia początkowa rakiety
        float energyStart = (0.5f * Mathf.Pow(startVelocity, 2)) - (G * earthMass / EarthRadius);

        // Promień docelowej orbity
        float orbitalRadius = (float)(G * earthMass / (-2 * energyStart));

        // Wysokość nad powierzchnią Ziemi
        float orbitalHeight = orbitalRadius - EarthRadius;

        // Wyświetlenie wyniku
        Debug.Log($"Docelowa wysokość orbity: {orbitalHeight:F2}");
    }

void BoostRocketAfterStart() {
    if (rocket.transform.position.z < 0) {
        // Aktualny promień orbity
        float rCurrent = Vector3.Distance(rocket.transform.position, earth.transform.position);

        // Masa Ziemi
        float earthMass = earth.GetComponent<Rigidbody>().mass;

        // Oblicz docelową prędkość orbitalną
        float vOrbit = Mathf.Sqrt(G * earthMass / rCurrent);

        // Aktualna prędkość rakiety
        float vCurrent = rocketRigidbody.linearVelocity.magnitude;

        // Oblicz różnicę prędkości (deltaV)
        float deltaV = vOrbit - vCurrent;

        // Zastosuj deltaV jako boost w kierunku aktualnego wektora prędkości
        rocketRigidbody.linearVelocity += rocketRigidbody.linearVelocity.normalized * deltaV;

        Debug.Log($"Boosted! vOrbit: {vOrbit:F2}, vCurrent: {vCurrent:F2}, deltaV: {deltaV:F2}");

        // Ustaw flagę
        isRocketBoostedAfterStart = true;
    }
}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == earth && rocketFlying) 
        {
            rocketRigidbody.linearVelocity = Vector3.zero;  // Wyzerowanie prędkości
            rocketRigidbody.angularVelocity = Vector3.zero; // Wyzerowanie momentu pędu

            rocketRigidbody.isKinematic = true;

            rocketFlying = false;
            Debug.Log("Rocket hit the ground!");
        }
    }
}
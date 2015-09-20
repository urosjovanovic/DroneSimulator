using UnityEngine;
using System.Collections;
using System.Timers;

public sealed class DroneLightsController : MonoBehaviour
{
    private Timer blinkIntervalTimer;
    private Timer blinkDurationTimer;
    private bool gpsLightOn;
    private Renderer gpsLightRenderer;

    public Transform GPSLightModel;
    public Light GPSLight;
    [Range(0, 5)]
    public float BlinkInterval = 1;
    [Range(0, 5)]
    public float BlinkDuration = 0.1f;

    // Use this for initialization
    private void Start()
    {
        this.gpsLightRenderer = this.GPSLightModel.gameObject.GetComponent<Renderer>();
        this.blinkIntervalTimer = new Timer(this.BlinkInterval * 1000);
        this.blinkDurationTimer = new Timer(this.BlinkDuration * 1000);
        this.blinkIntervalTimer.Elapsed += blinkIntervalTimer_Elapsed;
        this.blinkDurationTimer.Elapsed += blinkDurationTimer_Elapsed;
        this.blinkIntervalTimer.Start();
    }

    // Update is called once per frame
    private void Update()
    {
        this.gpsLightRenderer.material.shader = this.gpsLightOn ? Shader.Find("Unlit/Color") : Shader.Find("Standard");
        this.gpsLightRenderer.material.color = this.gpsLightOn ? Color.green : Color.white;
        if (this.GPSLight != null)
            this.GPSLight.enabled = this.gpsLightOn;
    }

    private void blinkIntervalTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        this.blinkIntervalTimer.Stop();
        this.gpsLightOn = true;
        this.blinkDurationTimer.Start();
    }

    private void blinkDurationTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        this.blinkDurationTimer.Stop();
        this.gpsLightOn = false;
        this.blinkIntervalTimer.Start();
    }
}

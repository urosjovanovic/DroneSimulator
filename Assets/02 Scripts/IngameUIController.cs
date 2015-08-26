using UnityEngine;
using UnityEngine.UI;

public class IngameUIController : MonoBehaviour
{
    private const float gaugeArrowMaxRotation = 176f;
    private const float compasStartAngle = 270f;
    private float gaugeArrowAngle;
    private float droneMaxSpeed;
    private float pitchBackgroundHorizontalPosition;
    private Canvas canvas;

    // Inspector variables
    public Transform DroneModel;
    public Image GaugeArrow;
    public Text SpeedText;
    public Text SpeedMeasureText;
    public Text HeightText;
    public Image PitchArrow;
    public Image PitchBackground;
    public Image Compass;
    public bool UseMetersPerSecond;

    #region Properties

    private bool HasDroneModel
    {
        get { return this.DroneModel != null; }
    }

    #endregion

    // Use this for initialization
    private void Start()
    {
        this.gaugeArrowAngle = this.GaugeArrow.transform.rotation.eulerAngles.z;
        this.pitchBackgroundHorizontalPosition = this.PitchBackground.transform.position.y;
        this.droneMaxSpeed = this.HasDroneModel ? this.DroneModel.GetComponent<DroneController>().MaxSpeed : 1.0f;
        this.SpeedMeasureText.text = this.UseMetersPerSecond ? "m/s" : "km/h";
        this.canvas = this.GetComponent<Canvas>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetButtonDown("ShowUIToggle"))
            this.canvas.enabled = !this.canvas.enabled;

        if (this.canvas.enabled)
        {
            this.GaugeArrow.transform.localEulerAngles = new Vector3(this.GaugeArrow.transform.localEulerAngles.x, this.GaugeArrow.transform.localEulerAngles.y, this.gaugeArrowAngle - (this.HasDroneModel ? this.DroneModel.GetComponent<Rigidbody>().velocity.magnitude : 0) * gaugeArrowMaxRotation / this.droneMaxSpeed);
            this.SpeedText.text = string.Format(this.UseMetersPerSecond ? "{0:0.0}" : "{0:0}", this.HasDroneModel ? this.DroneModel.GetComponent<Rigidbody>().velocity.magnitude * (this.UseMetersPerSecond ? 1.0f : 3.6f) : 0);

            int pitchSign = this.DroneModel.eulerAngles.x > 180 ? -1 : 1;
            float pitchAmount = (pitchSign > 0 ? this.DroneModel.eulerAngles.x : 360 - this.DroneModel.eulerAngles.x) / 90 * 83;
            Vector3 pos = this.PitchBackground.GetComponent<RectTransform>().position;
            this.PitchBackground.GetComponent<RectTransform>().position = new Vector3(pos.x, this.pitchBackgroundHorizontalPosition + (pitchSign * pitchAmount), pos.z);
            this.PitchArrow.transform.localEulerAngles = new Vector3(this.PitchArrow.transform.localEulerAngles.x, this.PitchArrow.transform.localEulerAngles.y, this.HasDroneModel ? this.DroneModel.rotation.eulerAngles.z : 0);

            this.HeightText.text = string.Format("{0:0.0} m", this.HasDroneModel ? this.DroneModel.position.y - 0.1 : 0);

            this.Compass.transform.localEulerAngles = new Vector3(this.Compass.transform.localEulerAngles.x, this.Compass.transform.localEulerAngles.y, this.HasDroneModel ? this.DroneModel.rotation.eulerAngles.y - compasStartAngle : 0);
        }
    }
}
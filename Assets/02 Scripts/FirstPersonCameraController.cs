using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour
{
    // Inspector variables
    public Transform DroneModel;
    public float MaxDistanceFromDrone;
    public float MinDistanceFromDrone;
    public float CameraHeight;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (this.DroneModel != null)
        {
            this.transform.LookAt(this.DroneModel);

            float distance = Vector3.Distance(this.DroneModel.position, this.transform.position);
            if (distance > this.MaxDistanceFromDrone)
            {
                this.transform.position = Vector3.Slerp(this.transform.position, new Vector3(
                    this.DroneModel.position.x - this.transform.forward.x * this.MaxDistanceFromDrone,
                    this.CameraHeight,
                    this.DroneModel.position.z - this.transform.forward.z * this.MaxDistanceFromDrone
                    ),
                    Time.deltaTime);
            }
            else if (distance < this.MinDistanceFromDrone)
            {
                this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(
                    this.transform.position.x - this.transform.forward.x * this.MinDistanceFromDrone,
                    this.CameraHeight,
                    this.transform.position.z - this.transform.forward.z * this.MinDistanceFromDrone
                    ),
                    Time.deltaTime);
            }
            else
                this.transform.position = new Vector3(this.transform.position.x, this.CameraHeight, this.transform.position.z);
        }
    }
}
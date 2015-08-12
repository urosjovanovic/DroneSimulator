using UnityEngine;
using System.Collections;
using System.Linq;

public sealed class CameraSwitcher : MonoBehaviour 
{
    private int activeCameraIndex;
    public Camera[] Cameras;
	public Transform Target;
	public int rtsCameraIndex = 2;

	// Use this for initialization
	void Start () 
    {
        for (int i = 0; i < this.Cameras.Length; i++)
            this.Cameras[i].enabled = i == 0;
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if(Input.GetButtonDown("ChangeCamera"))
        {
			this.Cameras[activeCameraIndex].enabled = false;
			this.activeCameraIndex = (this.activeCameraIndex + 1) % this.Cameras.Length;

			// Skip the rtsCamera
			if(this.activeCameraIndex == rtsCameraIndex) {
				this.activeCameraIndex = (this.activeCameraIndex + 1) % this.Cameras.Length;
			}

			this.Cameras[activeCameraIndex].enabled = true;
        }
	}

	public void SwitchRtsCamera() {		
		this.Cameras [activeCameraIndex].enabled = false;

		var drone = this.Target.gameObject;

		DroneController dc = drone.GetComponent<DroneController>();

		if (this.activeCameraIndex != this.rtsCameraIndex) {
			this.activeCameraIndex = this.rtsCameraIndex;
			var rtsCamera = this.Cameras [this.activeCameraIndex];
			rtsCamera.transform.position = this.Target.transform.position + new Vector3 (2, 2, 2);
			rtsCamera.transform.LookAt (this.Target.transform.position);
			dc.RtsMode = true;
			dc.planeY = this.Target.gameObject.transform.position.y;
			dc.droneMovementPlane = new Plane(Vector3.up, new Vector3(0, dc.planeY , 0));
			drone.GetComponent<LineRenderer>().enabled = true;
			dc.ClearAutoMovement();
			rtsCamera.GetComponent<RTSCameraController>().enabled = true;

			rtsCamera.enabled = true;
		} else {
			drone.GetComponent<LineRenderer>().enabled = false;

			this.activeCameraIndex = 0;
			dc.RtsMode = false;
			this.Cameras[this.activeCameraIndex].enabled = true;
		}
	}
}

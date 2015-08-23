using UnityEngine;
using System.Collections;

public sealed class CameraController : MonoBehaviour
{
    private int activeCameraIndex = 0;
    private bool showPreviewCam;
    private int previewCamPadding;
    private Camera[] cameras;

    // Inspector variables
    [Range(0.1f, 1.0f)]
    public float PreviewCameraWidth = 0.16f;
    public Camera ThirdPersonCamera;
    public Camera DroneCamera;
	public Camera RTSCamera;

    private Camera ActiveCam
    {
        get { return this.cameras[this.activeCameraIndex]; }
    }

    private Camera PreviewCam
    {
        get;
        set;
    }

    // Use this for initialization
    private void Start()
    {
        this.cameras = new Camera[] { this.ThirdPersonCamera, this.DroneCamera };
        for (int i = 0; i < this.cameras.Length; i++)
        {
            this.cameras[i].depth = this.cameras.Length - 1 - i;
            this.cameras[i].rect = new Rect(0, 0, 1, 1);
        }
        this.previewCamPadding = 12;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetButtonDown("ChangeCamera"))
        {
            this.activeCameraIndex = (this.activeCameraIndex + 1) % this.cameras.Length;
            this.ShowActiveCam();
            if (this.showPreviewCam)
                this.ShowPreviewCam();
        }
        if (Input.GetButtonDown("PreviewCamToggle"))
        {
            this.showPreviewCam = !this.showPreviewCam;
            if (this.showPreviewCam)
                this.ShowPreviewCam();
            else
                this.ShowActiveCam();
        }
		if (Input.GetButtonDown ("RTSCamToggle")) {
			this.SwitchRtsCamera();
		}
    }

    private void OnGUI()
    {
        if (showPreviewCam)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.black);
            texture.Apply();
            GUI.skin.box.normal.background = texture;
            Rect rect = this.PreviewCam.pixelRect;
            GUI.Box(new Rect(rect.xMin - 1, Screen.height - rect.yMax - 6, 1, rect.height + 11), GUIContent.none);
            GUI.Box(new Rect(rect.xMin - 1 + rect.width, Screen.height - rect.yMax - 6, 1, rect.height + 11), GUIContent.none);
            GUI.Box(new Rect(rect.xMin - 1, Screen.height - rect.yMax - 1, rect.width, 1), GUIContent.none);
            GUI.Box(new Rect(rect.xMin - 1, Screen.height - rect.yMax - 1 + rect.height, rect.width, 1), GUIContent.none);
        }
    }

    private void ShowActiveCam()
    {
        foreach (var camera in this.cameras)
            camera.depth = -1;
        this.ActiveCam.depth = 0;
        this.ActiveCam.rect = new Rect(0, 0, 1, 1);
    }

    private void ShowPreviewCam()
    {
        float paddingRight = this.previewCamPadding * 100.0f / this.ActiveCam.pixelWidth / 100;
        float paddingBottom = this.previewCamPadding * 100.0f / this.ActiveCam.pixelHeight / 100;
        Rect previewCamRect = new Rect(1 - this.PreviewCameraWidth - paddingRight, paddingBottom, this.PreviewCameraWidth, this.PreviewCameraWidth * this.ActiveCam.aspect * 3 / 4);
        if (this.ActiveCam == this.DroneCamera)
        {
            this.ThirdPersonCamera.depth = 1;
            this.ThirdPersonCamera.rect = previewCamRect;
            this.PreviewCam = this.ThirdPersonCamera;
        }
        else
        {
            this.DroneCamera.depth = 1;
            this.DroneCamera.rect = previewCamRect;
            this.PreviewCam = this.DroneCamera;
        }
    }

	public void SwitchRtsCamera() {		
		this.cameras [activeCameraIndex].enabled = false;
		
		var drone = GameObject.Find ("DJIPhantom");
		DroneController dc = drone.GetComponent<DroneController>();
		
		if (this.RTSCamera.enabled == false) {
			this.RTSCamera.transform.position = drone.transform.position + new Vector3 (2, 2, 2);
			this.RTSCamera.transform.LookAt (drone.transform.position);
			this.RTSCamera.GetComponent<RTSCameraController>().enabled = true;
			dc.RtsMode = true;
			dc.planeY = drone.gameObject.transform.position.y;
			dc.droneMovementPlane = new Plane(Vector3.up, new Vector3(0, dc.planeY , 0));
			drone.GetComponent<LineRenderer>().enabled = true;
			dc.ClearAutoMovement();
			this.RTSCamera.enabled = true;
		} else {
			drone.GetComponent<LineRenderer>().enabled = false;
			this.RTSCamera.GetComponent<RTSCameraController>().enabled = false;
			this.RTSCamera.enabled = false;
			this.activeCameraIndex = 0;
			dc.RtsMode = false;
			this.cameras[this.activeCameraIndex].enabled = true;
		}
	}
}

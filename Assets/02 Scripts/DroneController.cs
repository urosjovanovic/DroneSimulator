using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class DroneController : MonoBehaviour 
{
    private Rotor[] rotors;
    private Transform droneModel;
    private Rigidbody droneRigidbody;
    private float verticalSpeed;
    private Vector3 droneCameraViewDirection;
    private float droneModelAngle;

    #region Inspector Accessible Params

    //Looking from the top of the drone
    public Transform FrontLeftRotor;
    public Transform FrontRightRotor;
    public Transform BackLeftRotor;
    public Transform BackRightRotor;

    //Optional params
    public Camera DroneCamera;
    public Transform CameraRollSystem;
    public Transform CameraPitchSystem;

    [Range(1, 10)]
    public float MaxSpeed = 6;
    [Range(0, 2)]
    public float VerticalStabilizationFactor = 1.0f;
    [Range(0, 2)]
    public float VerticalAccelerationFactor = 1.2f;
    [Range(0, 2)]
    public float HorizontalXStabilizationFactor = 1.0f;
    [Range(0, 2)]
    public float HorizontalYStabilizationFactor = 1.0f;
    [Range(0, 2)]
    public float HorizontalXAccFactor = 1.0f;
    [Range(0, 2)]
    public float HorizontalYAccFactor = 1.0f;

    #endregion

    #region Properties

    private bool HasCamera
    {
        get { return this.DroneCamera != null; }
    }

    private bool CameraRollSupported
    {
        get { return this.CameraRollSystem != null; }
    }

    private bool CameraPitchSupported
    {
        get { return this.CameraPitchSystem != null; }
    } 

    #endregion

	#region RTSParams

	public bool RtsMode = false;

	private float verticalAxis = 0;
	private float horizontalAxis = 0.0f;
	private float tiltAxis = 0;
	private float sidewayTiltAxis = 0;
	
	public Plane droneMovementPlane;
	public float planeY;
	private GameObject RTSCamera;

	public float scrollFactor = 2f;

	private List<Vector3> destinationPoints;
	private bool moving = false;
	private bool inCoroutine = false;
	#endregion

	// Use this for initialization
	void Start () 
    {
        try
        {
            if (this.FrontLeftRotor == null || this.FrontRightRotor == null || this.BackLeftRotor == null || this.BackRightRotor == null)
                throw new ArgumentNullException("One or more rotor objects are missing");

            this.droneModel = this.gameObject.transform;
            this.droneRigidbody = this.droneModel.GetComponent<Rigidbody>();

            if (this.droneRigidbody == null)
                throw new ArgumentNullException("No rigidbody found.");

            this.rotors = new Rotor[4] { new Rotor(this.FrontLeftRotor), new Rotor(this.FrontRightRotor), new Rotor(this.BackLeftRotor), new Rotor(this.BackRightRotor) };

            if (this.HasCamera)
            {
                this.droneCameraViewDirection = this.DroneCamera.transform.forward;
                this.droneModelAngle = this.droneModel.eulerAngles.y;
            }

			this.RTSCamera = GameObject.Find("RTSCamera");
			this.destinationPoints = new List<Vector3>();
        }
        catch(Exception ex)
        {
            Debug.LogError("DroneController Initialization failed: " + ex.Message);
            this.enabled = false;
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        foreach (var rotor in this.rotors)
            rotor.Transform.RotateAround(rotor.Transform.GetObjectCenter(), this.droneModel.up, 3600*Time.deltaTime);

        float cameraAxis = Input.GetAxis("CameraAxis");
        float cameraAngle = Vector3.Angle(Vector3.up, this.droneCameraViewDirection);
        this.droneCameraViewDirection = Quaternion.Euler(0, this.droneModel.eulerAngles.y - this.droneModelAngle, 0) * this.droneCameraViewDirection;
        if(cameraAngle >= 90 && cameraAxis >= 0 || cameraAngle < 179 && cameraAxis <= 0)
            this.droneCameraViewDirection = Quaternion.AngleAxis(cameraAxis, -this.DroneCamera.transform.right) * this.droneCameraViewDirection;
        this.DroneCamera.transform.forward = this.droneCameraViewDirection;
        this.droneModelAngle = this.droneModel.eulerAngles.y;
	}

    // Run all physics based stuff here
    void FixedUpdate ()
    {
		if (!RtsMode) {
			verticalAxis = Input.GetAxis ("Vertical");
			horizontalAxis = Input.GetAxis ("Horizontal");
			tiltAxis = Input.GetAxis ("Vertical2");
			sidewayTiltAxis = Input.GetAxis ("Horizontal2");
		} else {
			if(!inCoroutine) {
				tiltAxis = 0.0f;
			}

			DrawLine();
			if(destinationPoints.Count > 0 && !moving) {
				StartCoroutine("MoveTo", 0);
			}
		}

        foreach (var rotor in this.rotors)
            rotor.Force = 2.4525f;

        this.Elevate(verticalAxis * this.VerticalAccelerationFactor);
        this.Tilt(tiltAxis);
        this.SidewayTilt(sidewayTiltAxis);

        foreach (var rotor in this.rotors)
            this.droneRigidbody.AddForceAtPosition(Vector3.up * rotor.Force, rotor.Transform.position, ForceMode.Force);

        Vector3 relativeVelocity = this.droneRigidbody.GetLocalVelocity();


        if (tiltAxis >= 0 && relativeVelocity.z < 0 || tiltAxis <= 0 && relativeVelocity.z > 0) {
			tiltAxis -= relativeVelocity.z * this.HorizontalXStabilizationFactor;
		} 
		Debug.Log ("Tilt axis: " + tiltAxis);
        this.droneRigidbody.AddForce(Vector3.Cross(this.droneModel.right, Vector3.up).normalized * tiltAxis * this.MaxSpeed * this.HorizontalXAccFactor);

        if (sidewayTiltAxis >= 0 && relativeVelocity.x < 0 || sidewayTiltAxis <= 0 && relativeVelocity.x > 0 && !RtsMode)
            sidewayTiltAxis -= relativeVelocity.x * this.HorizontalYStabilizationFactor;
        this.droneRigidbody.AddForce(Vector3.Cross(Vector3.up, this.droneModel.forward).normalized * sidewayTiltAxis * this.MaxSpeed * this.HorizontalYAccFactor);
		
        if (horizontalAxis >= 0 && this.droneRigidbody.angularVelocity.y < 0 || horizontalAxis <= 0 && this.droneRigidbody.angularVelocity.y > 0 && !RtsMode) {
			horizontalAxis -= this.droneRigidbody.angularVelocity.y;
		}
        this.droneRigidbody.AddTorque(Vector3.up * horizontalAxis * 0.5f);

        if (this.droneRigidbody.velocity.magnitude > this.MaxSpeed)
            this.droneRigidbody.velocity = this.droneRigidbody.velocity.normalized * this.MaxSpeed;

        //Debug.Log(string.Format("X: {1:0.0} Y: {0:0.0} Z: {2:0.0}", this.droneRigidbody.velocity.y, relativeVelocity.x, relativeVelocity.z));
    }

    private void Elevate(float amount)
    {
        foreach (var rotor in this.rotors)
        {
            rotor.Force += amount;
            if (amount >= 0 && this.droneRigidbody.velocity.y < 0 || amount <= 0 && this.droneRigidbody.velocity.y > 0)
                rotor.Force -= this.droneRigidbody.velocity.y * this.VerticalStabilizationFactor;
        }
    }

    private void Tilt(float amount)
    {
        float force = amount * rotors[0].Force / 2;
        this.rotors[0].Force -= force;
        this.rotors[1].Force -= force;
        this.rotors[2].Force += force;
        this.rotors[3].Force += force;
    }

    private void SidewayTilt(float amount)
    {
        float force = amount * rotors[0].Force / 2;
        this.rotors[1].Force -= force;
        this.rotors[3].Force -= force;
        this.rotors[0].Force += force;
        this.rotors[2].Force += force;
    }

	private void DrawLine() {
		var lineRenderer = gameObject.GetComponent<LineRenderer> ();
		lineRenderer.SetWidth (0.03f, 0.01f);
		lineRenderer.SetPosition (0, gameObject.transform.position);

		float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

		if (Input.GetKey (KeyCode.LeftControl)) {
			planeY += scrollWheel * scrollFactor;
			droneMovementPlane = new Plane(Vector3.up, new Vector3(0, planeY, 0));
		}

		var ray = RTSCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		float rayDistance = 0f;

		if (droneMovementPlane.Raycast (ray, out rayDistance)) {
			var point = ray.GetPoint(rayDistance);
			lineRenderer.SetPosition (1, point);
			if(Input.GetMouseButtonDown(1)) {
				destinationPoints.Add(point);
			}
		} 
	}

	private IEnumerator MoveTo(int destinationPointIndex){
		moving = true;
		Debug.Log ("Move to: " + destinationPoints[destinationPointIndex]);

		yield return StartCoroutine("RotateToPoint", destinationPoints [destinationPointIndex]);
		yield return new WaitForSeconds (1);
		yield return StartCoroutine ("MoveForward", Vector3.Distance(gameObject.transform.position, destinationPoints[destinationPointIndex]));

		destinationPoints.RemoveAt (destinationPointIndex);
		moving = false;
		yield return null;
	}

	 private IEnumerator RotateToPoint(Vector3 point) {
		Debug.Log ("Rotate to point.");

		var dronePos = new Vector3 (gameObject.transform.position.x, 0, gameObject.transform.position.z);
		var pointPos = new Vector3 (point.x, 0, point.z);
		var rotateDir = pointPos - dronePos;
		var angleBetween = Vector3.Angle (gameObject.transform.forward, rotateDir);

		while (angleBetween > 3) {
			angleBetween = Vector3.Angle (gameObject.transform.forward, rotateDir);
			horizontalAxis = 0.5f;
			yield return null;
		}

		horizontalAxis = 0.0f;
		Debug.Log ("Rotated");
		yield return null;
	}

	private IEnumerator MoveForward(float distance) {
		inCoroutine = true;
		Debug.Log ("Move forward.");
		var target = gameObject.transform.position + gameObject.transform.forward * distance;
		Debug.Log ("Position: " + gameObject.transform.position + ", target: " + target);

		while (Vector3.Distance(gameObject.transform.position, target) > 0.1) {
			tiltAxis = 0.2f;
			yield return null;
			tiltAxis = 0.0f;
		}
	
		inCoroutine = false;
		Debug.Log ("Moved forward for: " + distance);

		yield return null;
	}

	public void ClearAxes() {
		this.verticalAxis = 0.0f;
		this.horizontalAxis = 0.0f;
		this.tiltAxis = 0.0f;
		this.sidewayTiltAxis = 0.0f;
	}
}

public sealed class Rotor
{
    private readonly Transform transform;
    private float force;

    public Rotor(Transform transform)
    {
        this.transform = transform;
        this.force = 0;
    }

    public float Force
    {
        get { return this.force; }
        set { this.force = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    public Transform Transform
    {
        get { return this.transform; }
    }
}

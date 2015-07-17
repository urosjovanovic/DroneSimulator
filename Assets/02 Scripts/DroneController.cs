using UnityEngine;
using System;
using System.Collections;

public sealed class DroneController : MonoBehaviour 
{
    private Rotor[] rotors;
    private Vector3 cameraViewDirection;
    private Transform droneModel;
    private Rigidbody droneRigidbody;
    private float verticalSpeed;

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

    public float VerticalStabilizationFactor = 0.35f;
    [Range(0, 2)]
    public float VerticalAccelerationFactor = 1.2f;
    [Range(1, 10)]
    public float MaxVerticalSpeed = 6;

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
                this.cameraViewDirection = this.DroneCamera.transform.forward;
            else
                this.cameraViewDirection = this.CameraPitchSystem.forward;
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

        //TODO: Simulating camera gyro
        //if (this.CameraRollSupported)
        //    this.CameraRollSystem.up = Vector3.up;
        //if (this.CameraPitchSupported)
        //    this.CameraPitchSystem.forward = this.cameraViewDirection;
	}

    // Run all physics based stuff here
    void FixedUpdate ()
    {
        foreach (var rotor in this.rotors)
            rotor.Force = 2.4525f;

        float verticalAxis = Input.GetAxis("Vertical");
        float tiltAxis = Input.GetAxis("RightAnalogVertical");
        float sidewayTiltAxis = Input.GetAxis("RightAnalogHorizontal");

        if (verticalAxis == 0 && tiltAxis == 0 && sidewayTiltAxis == 0)
        {
            if (this.droneRigidbody.velocity.magnitude> 0)
                this.droneRigidbody.drag += this.VerticalStabilizationFactor;
        }
        else
            this.droneRigidbody.drag = 0;

        this.Elevate(verticalAxis * this.VerticalAccelerationFactor);
        this.Tilt(tiltAxis);
        this.SidewayTilt(sidewayTiltAxis);

        foreach (var rotor in this.rotors)
            this.droneRigidbody.AddForceAtPosition(Vector3.up * rotor.Force, rotor.Transform.position, ForceMode.Force);

        if (this.droneRigidbody.velocity.magnitude > this.MaxVerticalSpeed)
            this.droneRigidbody.velocity = this.droneRigidbody.velocity.normalized * this.MaxVerticalSpeed;

        Debug.Log(this.droneRigidbody.velocity);
    }

    private void Elevate(float amount)
    {
        foreach (var rotor in this.rotors)
            rotor.Force += amount;
    }

    private void Tilt(float amount)
    {
        float force = amount * 1.0f;
        this.rotors[0].Force -= force;
        this.rotors[1].Force -= force;
        this.rotors[2].Force += force;
        this.rotors[3].Force += force;

        this.droneRigidbody.AddForce(Vector3.Cross(this.droneModel.right, Vector3.up).normalized * amount);
    }

    private void SidewayTilt(float amount)
    {
        float force = amount*1.0f;
        this.rotors[1].Force -= force;
        this.rotors[3].Force -= force;
        this.rotors[0].Force += force;
        this.rotors[2].Force += force;

        this.droneRigidbody.AddForce(Vector3.Cross(Vector3.up, this.droneModel.forward).normalized * amount);
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
        get { return force; }
        set { force = value; }
    }

    public Transform Transform
    {
        get { return transform; }
    }
}

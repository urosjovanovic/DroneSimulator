using UnityEngine;
using System;
using System.Collections;

public class DroneController : MonoBehaviour 
{
    private Transform[] rotors;
    private Vector3 cameraViewDirection;

    //Represents the whole model (root object)
    public Transform DroneModel;

    //Looking from the top of the drone
    public Transform FrontLeftRotor;
    public Transform FrontRightRotor;
    public Transform BackLeftRotor;
    public Transform BackRightRotor;

    //Optional params
    public Camera DroneCamera;
    public Transform CameraRollSystem;
    public Transform CameraPitchSystem;

	// Use this for initialization
	void Start () 
    {
        try
        {
            if (this.DroneModel == null)
                throw new ArgumentNullException("No drone model attached");

            if (this.FrontLeftRotor == null || this.FrontRightRotor == null || this.BackLeftRotor == null || this.BackRightRotor == null)
                throw new ArgumentNullException("One or more rotor objects are missing");
        }
        catch
        {
            this.enabled = false;
            throw;
        }

        this.rotors = new Transform[4]{ this.FrontLeftRotor, this.FrontRightRotor, this.BackLeftRotor, this.BackRightRotor };
        if (this.HasCamera)
            this.cameraViewDirection = this.DroneCamera.transform.forward;
        else
            this.cameraViewDirection = this.CameraPitchSystem.forward;
	}
	
	// Update is called once per frame
	void Update () 
    {
        foreach (var rotor in this.rotors)
            rotor.RotateAround(rotor.GetObjectCenter(), this.DroneModel.up, 3600*Time.deltaTime);

        //Simulating camera gyro
        if (this.CameraRollSupported)
            this.CameraRollSystem.up = Vector3.up;
        if (this.CameraPitchSupported)
            this.CameraPitchSystem.forward = this.cameraViewDirection;
	}

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
}

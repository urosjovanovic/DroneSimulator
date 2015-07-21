using UnityEngine;
using System.Collections;

public sealed class CameraSwitcher : MonoBehaviour 
{
    private int activeCameraIndex;
    public Camera[] Cameras;

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
            this.Cameras[activeCameraIndex].enabled = true;
        }
	}
}

using UnityEngine;
using System.Collections;

public class RTSCameraController : MonoBehaviour {
	public float scrollSpeed = 1;
	public float rotateSpeed = 50;

	float mouseX = 0;
	float mouseY = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		mouseX = Input.mousePosition.x;
		mouseY = Input.mousePosition.y;

		var rotateAround = Vector3.zero;

		if (Input.GetMouseButtonDown (1)) {
			rotateAround = Input.mousePosition + new Vector3(0, 0, 2);
		}

		var rightClick = Input.GetMouseButton (1);

		if (!rightClick) {
			if (mouseX < 0) {
				gameObject.transform.Translate (Vector3.right * -scrollSpeed * Time.deltaTime);
			} else if (mouseX > Screen.width) {
				gameObject.transform.Translate (Vector3.right * scrollSpeed * Time.deltaTime);
			}

			if (mouseY < 0) { 
				var translateAlong = gameObject.transform.forward + gameObject.transform.up;
				translateAlong.y = 0;
				gameObject.transform.position -= translateAlong * scrollSpeed * Time.deltaTime; 
			} 
		
			if (mouseY > Screen.height) { 
				var translateAlong = gameObject.transform.forward + gameObject.transform.up;
				translateAlong.y = 0;
				gameObject.transform.position += translateAlong * scrollSpeed * Time.deltaTime;  
			} 
		} else {
			var cursorCoords = gameObject.GetComponent<Camera>().ScreenToWorldPoint(rotateAround);

			if (mouseX < 0) {
				gameObject.transform.RotateAround(cursorCoords, Vector3.up, rotateSpeed * Time.deltaTime);
			} 
			else if(mouseX > Screen.width){
				gameObject.transform.RotateAround(cursorCoords, Vector3.up, -rotateSpeed * Time.deltaTime);
			}

			//gameObject.transform.RotateAround(cursorCoords, Vector3.up, 
		}
	}
}

using UnityEngine;
using System.Collections;

public class TopDownCameraController : MonoBehaviour {
	public float scrollSpeed = 5;
	float mouseX = 0;
	float mouseY = 0;
	float zoomSpeed = 50;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		mouseX = Input.mousePosition.x;
		mouseY = Input.mousePosition.y;

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

		float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
		if (scrollWheel > 0 && gameObject.transform.position.y > 1) {
			gameObject.transform.Translate (Vector3.forward * scrollWheel * zoomSpeed * Time.deltaTime);
		} else if (scrollWheel < 0 && gameObject.transform.position.y < 20) {
			gameObject.transform.Translate (Vector3.forward * scrollWheel * zoomSpeed * Time.deltaTime);
		}

	}
}

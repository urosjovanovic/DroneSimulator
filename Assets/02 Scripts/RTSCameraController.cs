﻿using UnityEngine;
using System.Collections;

public class RTSCameraController : MonoBehaviour {
	public float scrollSpeed = 1;
	public float LrRotateSpeed = 50;
	public float UdRotateSpeed = 30;
	public float zoomSpeed = 20;

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

		var middleClick = Input.GetMouseButton (2);

		if (!middleClick) {
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
				gameObject.transform.RotateAround(cursorCoords, Vector3.up, LrRotateSpeed * Time.deltaTime);
			} 
			else if(mouseX > Screen.width){
				gameObject.transform.RotateAround(cursorCoords, Vector3.up, -LrRotateSpeed * Time.deltaTime);
			}

			if (mouseY < 0) { 
				gameObject.transform.Rotate(Vector3.right, UdRotateSpeed * Time.deltaTime);
			} 
			
			if (mouseY > Screen.height) { 
				gameObject.transform.Rotate(Vector3.right, -UdRotateSpeed * Time.deltaTime);
			} 

			//gameObject.transform.RotateAround(cursorCoords, Vector3.up, 
		}

		float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
		if (scrollWheel > 0 && gameObject.transform.position.y > 1 && !Input.GetKey (KeyCode.LeftControl)) {
			gameObject.transform.Translate (Vector3.forward * scrollWheel * zoomSpeed * Time.deltaTime);
		} else if (scrollWheel < 0 && gameObject.transform.position.y < 20 && !Input.GetKey (KeyCode.LeftControl)) {
			gameObject.transform.Translate (Vector3.forward * scrollWheel * zoomSpeed * Time.deltaTime);
		}
	}
}

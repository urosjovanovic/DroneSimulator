using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TopDownCameraController : MonoBehaviour {
	public float scrollSpeed = 5;
	public DroneController dc;
	public RectTransform coordinatePanel;

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

		if (dc.RtsMode && dc.TopDownMode) {
			DrawTopDown();
		}
	}

	public void DrawTopDown() {
		var ray = gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		float rayDistance = 0f;
		
		if (dc.droneMovementPlane.Raycast(ray, out rayDistance))
		{
			var point = ray.GetPoint(rayDistance);
			if (Input.GetMouseButtonDown(1) && !coordinatePanel.gameObject.activeSelf)
			{
				/*var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				sphere.GetComponent<SphereCollider>().isTrigger = true;
				sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
				sphere.transform.position = point;
				sphere.AddComponent<TriggerDestroy>();
				destinationPoints.Add(point);*/
				DrawPointInput(point);
			}
		}
		
	}
	
	public void DrawPointInput(Vector3 point) {
		coordinatePanel.gameObject.SetActive (true);
		var xCoord = coordinatePanel.FindChild ("XCoordinate");
		var zCoord = coordinatePanel.FindChild ("ZCoordinate");

		if (xCoord != null && zCoord != null) {
			xCoord.gameObject.GetComponent<InputField>().text = point.x + "";
			zCoord.gameObject.GetComponent<InputField>().text = point.z + "";
		}
	}

	public void ConfirmButtonClick() {
		var xCoord = coordinatePanel.FindChild ("XCoordinate");
		var yCoord = coordinatePanel.FindChild ("YCoordinate");
		var zCoord = coordinatePanel.FindChild ("ZCoordinate");

		if(xCoord && yCoord && zCoord) {
			float xPos, yPos, zPos;
			bool xCheck = float.TryParse(xCoord.gameObject.GetComponent<InputField>().text, out xPos);
			bool yCheck = float.TryParse(yCoord.gameObject.GetComponent<InputField>().text, out yPos);
			bool zCheck = float.TryParse(zCoord.gameObject.GetComponent<InputField>().text, out zPos);

			if(xCheck && yCheck && zCheck) {
				var destinationPoint = new Vector3(xPos, yPos, zPos);
				dc.DestinationPoints.Add(destinationPoint);
				coordinatePanel.gameObject.SetActive(false);
			}
		}
	}

}

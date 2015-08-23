using UnityEngine;
using System.Collections;

public class TriggerDestroy : MonoBehaviour {

	void OnTriggerEnter(Collider other){
		Destroy (gameObject);
	}
}

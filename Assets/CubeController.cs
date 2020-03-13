using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
	public Camera Camera;

	// Start is called before the first frame update
	void Start() {
	}

	// Update is called once per frame
	void Update() {
		Vector2 mousePos = Input.mousePosition;
		Vector2 windowSize = GameObject.Find("Main Camera").GetComponent<TransparentWindow>().windowSize;
		float angle = Mathf.Atan2(mousePos.y - windowSize.x / 2, windowSize.y / 2 - mousePos.x);
		transform.rotation = Quaternion.EulerRotation(0, 0, angle);
		if (mousePos.x > 0 && mousePos.y < windowSize.y) {
			#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
			GameObject.Find("Main Camera").GetComponent<TransparentWindow>().ActivateWindow();
			#endif // !UNITY_EDITOR && UNITY_STANDALONE_WIN

			GetComponent<MeshRenderer>().material.color = Color.blue;
		}
		else {
			GetComponent<MeshRenderer>().material.color = Color.green;
		}
	}
}

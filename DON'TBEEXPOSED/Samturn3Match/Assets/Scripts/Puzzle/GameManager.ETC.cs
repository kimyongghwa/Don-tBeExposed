using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public partial class GameManager : MonoBehaviour {

	private bool IsPointerOverUIObject()
	{
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}

	public IEnumerator UpDownPanel(GameObject panel){
		while (true){
			panel.GetComponent<RectTransform> ().localPosition = new Vector3 (panel.GetComponent<RectTransform> ().localPosition.x, - 10f * (Mathf.PingPong (2f*Time.time, 2.0f) - 1.0f), panel.GetComponent<RectTransform> ().localPosition.z);
			yield return null;
		}
	}


}

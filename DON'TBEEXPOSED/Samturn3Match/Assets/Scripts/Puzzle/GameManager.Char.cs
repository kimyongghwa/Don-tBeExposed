using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public partial class GameManager: MonoBehaviour {
	public Image face2d;

	int id    = 0;
	float charx = 0;
	float chary = 0;
	float charz = 0;
	float enemyx = 0;
	DataCharacter masterChar;
	DataCharacter clientChar;

	void AwakeCharSetup () {

		charx = 1;
		chary = 7.4f;
		charz = 0;

		int generalnumber = (int)TOY.YUBI;
		masterChar = CCM.Instance.CharacterSetting (id, generalnumber, charx, chary, charz, (int)COUNTRYTYPE.CHOK ,-1);    // Create Character
		Vector3 dir = new Vector3 (charx+0.2f, chary, charz-1);                                          // character direction
		RotateToVector (masterChar.playerChar.transform, dir);                                        // rotate character
		masterChar.playerChar.gameObject.transform.localScale += new Vector3(0.8f, 0.8f, 0.8f);

		int enemynumber = (int)TOY.JOJO;
		enemyx = 5.1f;
		clientChar = CCM.Instance.CharacterSetting (1, enemynumber, enemyx, chary, charz, (int)COUNTRYTYPE.WI ,-1);    // Create Character
		Vector3 dir2 = new Vector3 (enemyx -0.3f, chary, charz-1);                                          // character direction
		RotateToVector (clientChar.playerChar.transform, dir2);                                        // rotate character
		clientChar.playerChar.gameObject.transform.localScale += new Vector3(0.8f, 0.8f, 0.8f);

		//face2d.sprite = DataManager.Instance.dataGeneral[generalnumber].SsChar;                             // 2d face

	}

	public void RotateToVector (Transform self, Vector3 target)
	{
		Vector3 dir = target - self.position;
		Vector3 dirXZ = new Vector3(dir.x, 0f, dir.z);

		if (dirXZ == Vector3.zero)
			return;

		self.rotation = Quaternion.LookRotation(dirXZ);
	}	
}

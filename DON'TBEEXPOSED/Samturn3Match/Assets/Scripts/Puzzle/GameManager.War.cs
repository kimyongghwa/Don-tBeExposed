using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public partial class GameManager : MonoBehaviour {

	public GameObject panelWin;
	public GameObject panelLose;

	public Text TxtHpMaster;
	public Text TxtHpClient;

	public Image hpGaugeMaster;
	public Image hpGaugeClient;

	public int iMyHpBase    = 100;
	public int iEnemyHpBase = 100;

	public int iMyHp    = 100;
	public int iEnemyHp = 100;

	void MyAttack(int damage){

		masterChar.playerChar.GetComponent<PlayerFSM> ().SetState (CHARCTERSTATE.Attack);   // 캐릭터 공격 애니
		StartCoroutine (RotateChar (masterChar.playerChar,true));
		StartCoroutine (HitChar (clientChar.playerChar,false,true,damage));

	}

	IEnumerator RotateChar(GameObject CharObj,bool bmaster){
		yield return new WaitForSeconds (0.8f);
		if (bmaster) {
			CharObj.transform.Find ("yubi").transform.rotation = Quaternion.Euler (0, 165f, 0);
			CharObj.transform.Find ("yubi").transform.position = new Vector3 (charx, chary, charz);
		} else {
			CharObj.transform.Find ("yubi").transform.rotation = Quaternion.Euler (0, 195f, 0);
			CharObj.transform.Find ("yubi").transform.position = new Vector3 (enemyx, chary, charz);
		}
	}
	IEnumerator HitChar (GameObject CharObj,bool bmaster,bool bsend, int damage){
		yield return new WaitForSeconds (0.2f);
		CharObj.GetComponent<PlayerFSM> ().SetState (CHARCTERSTATE.Hit);
		if (bsend) {
			SendHpDamage (damage);
		}
		yield return new WaitForSeconds (0.5f);
		if (bmaster) {
			CharObj.transform.Find ("yubi").transform.position = new Vector3 (charx, chary, charz);
		} else {
			CharObj.transform.Find ("yubi").transform.position = new Vector3 (enemyx, chary, charz);
		}
	}

	void SendHpDamage(int damage){
		
		iEnemyHp -= damage;
		hpGaugeClient.fillAmount = (float)iEnemyHp / iEnemyHpBase;

		if (iEnemyHp <= 0) {
			iEnemyHp = 0;
			panelWin.SetActive (true);
			StartCoroutine (UpDownPanel (panelWin));
			masterChar.playerChar.GetComponent<PlayerFSM> ().SetState (CHARCTERSTATE.WinPose1);
			clientChar.playerChar.GetComponent<PlayerFSM> ().SetState (CHARCTERSTATE.Dead);
			StartCoroutine (ReGame());
		}

		TxtHpClient.text = iEnemyHp.ToString ();
	}

	public IEnumerator ReGame(){
		yield return new WaitForSeconds (2f);
		iEnemyHp = iEnemyHpBase;
		TxtHpClient.text = iEnemyHpBase.ToString ();
		hpGaugeClient.fillAmount = 1f;
		masterChar.playerChar.GetComponent<PlayerFSM> ().SetState (CHARCTERSTATE.Idle);
		clientChar.playerChar.GetComponent<PlayerFSM> ().SetState (CHARCTERSTATE.Idle);
		panelWin.SetActive (false);
	}
}

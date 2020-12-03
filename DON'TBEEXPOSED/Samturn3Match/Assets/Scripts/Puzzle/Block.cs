using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	GameManager gameMgr;

	public bool bMoveDown = false;
	public bool bDead = false;
	public int iBloackType = -1;
	public int iX, iY;

	float step = 0;
	float fMoveDownSpeed = 10f;

	public Vector3 startPos;
	public Vector3 target;

	void Awake () {
		gameMgr = GameObject.Find ("GameManager").GetComponent<GameManager> ();
	}
	
	void Update () {

		if (bMoveDown && transform.position != target) {  // 도착 지점에 도달할 때까지 이동 
			step += fMoveDownSpeed * Time.deltaTime;
			transform.position = Vector3.MoveTowards (startPos, target, step);   // 선택한 블록
			if(transform.position == target){                                  // 이동 완료
				gameMgr.Board [(int)target.x] [(int)target.y] = iBloackType;
				iX = (int)target.x;
				iY = (int)target.y;
				step = 0;
				bMoveDown = false;
			}
		}

	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

enum BLOCK{
	BLANK  = -1,
}

public partial class GameManager : MonoBehaviour {

	public int iBoardX = 7;            // 가로 개수
	public int iBoardY = 7;            // 세로 개수
	public int[][] Board;            

	int iImgType ;

	public Sprite[] imgBlock;         // 블록 종류 
	public GameObject gBlockPrefab;   
	public GameObject selectBlock;   
	public GameObject targetBlock;
	public GameObject[] uiMe;

	Vector3 mouseStartPoint;          // 마우스 드래그 시작 위치  
	Vector3 mouseEndPoint;            // 마우스 드래그 마지막 위치
	Vector3 _mouseOffset;             // 마우스 이동 거리 

	Ray clickRay;
	RaycastHit clickHit;

	public bool bBlockMove       = false;
	public bool bBlockMoveDelete = false;
	public bool bBlockComeback   = false;
	public bool bDrag = true;
	public bool bReCheck = false;

	public Vector3 startPos1;         // 선택한 블록 시작 위치
	public Vector3 endPos1;           // 선택한 블록 끝 위치
	public Vector3 startPos2;         // 선택한 블록 끝 위치   :  이동할 위치 블록의 시작 위치
	public Vector3 endPos2;           // 선택한 블록 시작 위치 :  이동할 위치 블록의 끝 위치 

	public float fMoveDist  = 30f;    // 블록이 이동 가능한 드래그 최소 거리
	public float fMoveSpeed = 10f;    // 블록 이동 속도
	float blockMoveStep;

	public Text txtDebug;

	void Awake () {

		AwakeCharSetup ();                          // GameManager.Char   캐릭터 셋팅
		//AwakePhotonConnect ();                      // PhotonNetwork 접속

		Board = new int[iBoardX][];
		for (int i = 0; i < iBoardX; i++) {
			Board[i] = new int[iBoardY];
		}

		iImgType = imgBlock.Length;
		CreateBlock();                    // 처음 블록 생성 

		uiMe [0].SetActive (true);

		TxtHpMaster.text = iMyHpBase.ToString ();
		TxtHpClient.text = iEnemyHp.ToString ();
	}
	
	void Update () {
		if (Input.GetMouseButtonDown (0)) {

			mouseStartPoint = Input.mousePosition;   // Mouse Start Position

			clickRay = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (clickRay, out clickHit, Mathf.Infinity)) {     // 
				if (bDrag && clickHit.collider.tag == "BLOCK") {    // 블록 클릭 
					
					selectBlock = clickHit.collider.gameObject;     // 이동할 블록 선택
					startPos1 = selectBlock.transform.position;     // 이동할 블록 시작 위치 

				} 
			}
		}
		if (Input.GetMouseButton (0)) {                            // 드래그 중 
			mouseEndPoint = Input.mousePosition;     // Mouse End position

			_mouseOffset = mouseStartPoint - mouseEndPoint;   // 마우스 이동 거리 

			if (selectBlock && bDrag ) {
				if (_mouseOffset.x > fMoveDist) {           // Drag Left
					if (selectBlock.transform.position.x > 0) {            // 화면 밖 벗어나지 않기
						MoveDirection (-1, 0);
					}
				}
				if (_mouseOffset.x < -fMoveDist) {          // Drag Right
					if (selectBlock.transform.position.x < iBoardX-1) {    // 화면 밖 벗어나지 않기   
						MoveDirection (1, 0);
					}
				}
				if (_mouseOffset.y > fMoveDist) {           // Drag Down
					if (selectBlock.transform.position.y > 0) {            // 화면 밖 벗어나지 않기   
						MoveDirection (0, -1);
					}
				}
				if (_mouseOffset.y < -fMoveDist) {          // Drag Up
					if (selectBlock.transform.position.y < iBoardY-1) {     // 화면 밖 벗어나지 않기
						MoveDirection (0, 1);
					}
				}
			}
		}

		if (bBlockMove) {       // 선택한 블록 이동 시작
			
			if (targetBlock.transform.position != endPos2) {  // 도착 지점에 도달할 때까지 이동 
			
				blockMoveStep += fMoveSpeed * Time.deltaTime;
				selectBlock.transform.position = Vector3.MoveTowards (startPos1, endPos1, blockMoveStep);   // 선택한 블록
				targetBlock.transform.position = Vector3.MoveTowards (startPos2, endPos2, blockMoveStep);   // 기존 위치 블록
			
			} else {                                          // 이동 완료
				
				bBlockMove = false;             // 이동 완료    
				blockMoveStep = 0;

				SwitchBoard ();                 // 블록 교환  

				StartCoroutine(CheckMatch ());  // 매치 시작

			}

		}
		if (bBlockComeback && targetBlock) {       // 블록 원위치
			
			if (targetBlock.transform.position != endPos2) {  // 도착 지점에 도달할 때까지 이동 
			
				blockMoveStep += fMoveSpeed * Time.deltaTime;
				selectBlock.transform.position = Vector3.MoveTowards (startPos1, endPos1, blockMoveStep);   // 선택한 블록
				targetBlock.transform.position = Vector3.MoveTowards (startPos2, endPos2, blockMoveStep);   // 기존 위치 블록
			
			} else {                                          // 블록 원위치 완료 
				
				bBlockComeback = false;                 
				blockMoveStep = 0;

				SwitchBoard ();                 // 블록 교환  

				selectBlock = null;
				targetBlock = null;

				bDrag = true;                   // 드래그 가능

			}

		}

		if (bBlockMoveDelete) {               // 블럭 삭제 후 블럭이 이동 중인지 체크  
			bool bBlockMoveEnd = true;
			GameObject[] blocks = GameObject.FindGameObjectsWithTag ("BLOCK");
			foreach (GameObject block in blocks) {
				if (block.GetComponent<Block> ().bMoveDown == true) {  //  블럭이 움직이는 중
					bBlockMoveEnd = false;              // 이동이 안 끝났음 
				}
			}

			if (bBlockMoveDelete && bBlockMoveEnd) {    // 모든 블럭이 멈추면 다시 체크 
				
				bBlockMoveDelete = false;
				BlockDown ();
			}
		}

		//UpdatePhoton ();

	}

	void CreateBlock(){                                   // 처음 블록 생성                 
		for (int j = 0; j < iBoardY; j++) // 세로 개수
		{
			for (int i = 0; i < iBoardX; i++) // 가로 개수
			{
				GameObject tmp = Instantiate(gBlockPrefab, new Vector3 (i, j, 0), Quaternion.identity) as GameObject;
				tmp.transform.SetParent (this.transform);
				int itype = (int)Random.Range (0, iImgType);
				tmp.GetComponent<Block> ().iX = i;
				tmp.GetComponent<Block> ().iY = j;
				tmp.GetComponent<Block> ().iBloackType    = itype;
				tmp.GetComponent<SpriteRenderer>().sprite = imgBlock[itype];
				Board[i][j]                               = itype;
			}
		}
		StartCoroutine(CheckMatch ()); // 매치 시작
	}

	void MoveDirection(int dirx, int diry){                    // 블록 이동 방향
		DataManager.Instance.SoundPlay(DataManager.Instance.sound_swordswing1, 1f);

		bDrag = false;

		endPos1 = new Vector3 (startPos1.x + dirx, startPos1.y + diry, startPos1.z);

		GameObject[] blocks = GameObject.FindGameObjectsWithTag ("BLOCK");
		foreach (GameObject block in blocks) {
			if (block.GetComponent<Block> ().iX == endPos1.x && block.GetComponent<Block> ().iY == endPos1.y) {    // 이동할 블럭 찾기 
				targetBlock = block;
			}
		}

		startPos2 = endPos1;      
		endPos2   = startPos1;     // 블록이 이동할 위치 넣어주기 

		bBlockMove = true;         // 움직이기 시작
	}

	void SwitchBoard(){
		selectBlock.GetComponent<Block> ().iX = (int)endPos1.x;
		selectBlock.GetComponent<Block> ().iY = (int)endPos1.y;

		targetBlock.GetComponent<Block> ().iX = (int)endPos2.x;
		targetBlock.GetComponent<Block> ().iY = (int)endPos2.y;

		int tmptype = Board[(int)startPos1.x] [(int)startPos1.y];
		Board [(int)startPos1.x] [(int)startPos1.y] = Board [(int)endPos1.x] [(int)endPos1.y];
		Board [(int)endPos1.x] [(int)endPos1.y] = tmptype;
	}

	IEnumerator CheckMatch(){

		GameObject[] blocks = GameObject.FindGameObjectsWithTag ("BLOCK");

		for (int j = 0; j < iBoardY; j++) { 
			for (int i = 0; i < iBoardX -2; i++) {              // 가로로 매치 체크
				if (Board [i] [j] == Board [i + 1] [j]) {
					if (Board [i] [j]== Board [i + 2] [j]) {
						foreach (GameObject block in blocks) {
							if (block.GetComponent<Block> ().iX == i && block.GetComponent<Block> ().iY == j) {
								block.GetComponent<Block> ().bDead = true;
							}
							if (block.GetComponent<Block> ().iX == i+1 && block.GetComponent<Block> ().iY == j) {
								block.GetComponent<Block> ().bDead = true;
							}
							if (block.GetComponent<Block> ().iX == i+2 && block.GetComponent<Block> ().iY == j) {
								block.GetComponent<Block> ().bDead = true;
							}
						}
					}
				}
			}
		}
		for (int i = 0; i < iBoardX  ; i++) {
			for (int j = 0; j < iBoardY -2; j++) {            // 세로로 매치 체크
				if (Board [i] [j] == Board [i] [j + 1]) {
					if (Board [i] [j] == Board [i] [j + 2]) {
						foreach (GameObject block in blocks) {
							if (block.GetComponent<Block> ().iX == i && block.GetComponent<Block> ().iY == j) {
								block.GetComponent<Block> ().bDead = true;
							}
							if (block.GetComponent<Block> ().iX == i && block.GetComponent<Block> ().iY == j+1) {
								block.GetComponent<Block> ().bDead = true;
							}
							if (block.GetComponent<Block> ().iX == i && block.GetComponent<Block> ().iY == j+2) {
								block.GetComponent<Block> ().bDead = true;
							}
						}
					}
				}
			}
		}

		if (NoMatch ()) {                    // 매치된 게 없으면 
			if (bReCheck) {               // 매치된 블럭이 재설정 된 후 매치된 게 없어서 다시 시작   
				bReCheck = false; 
				bDrag = true;
			}else{                        // 드래그 해서 매치된 게 없어서 원위치        
				Vector3 tmpstart = startPos1;
				Vector3 tmpend   = endPos1;

				startPos1  = startPos2;      
				endPos1    = endPos2;   
				startPos2  = tmpstart;      
				endPos2    = tmpend;   

				bBlockComeback = true;     // 블록 원위치
			}
		} else {                                 // 매치된 게 있으면 블럭 삭제 
			ShowOnMatchEffect (); 
			yield return new WaitForSeconds (0.2f);
			selectBlock = null;
			targetBlock = null;

			bReCheck = false;
			DeleteBlock ();   // 매치된 블록 지우기 
		}

	}
	bool NoMatch(){        // 매치된 블록이 없으면 원위치
		
		GameObject[] blocks = GameObject.FindGameObjectsWithTag ("BLOCK");
		foreach (GameObject block in blocks) {
			if (block.GetComponent<Block> ().bDead) {
				return false;    // 매치된 블록이 있음
			}
		}
		return true;         // 매치된 블록이 없어서 원위치
	}
	void ShowOnMatchEffect(){
		DataManager.Instance.SoundPlay(DataManager.Instance.sound_cut01, 1f);
		GameObject[] blocks = GameObject.FindGameObjectsWithTag ("BLOCK");
		foreach (GameObject block in blocks) {
			if (block.GetComponent<Block> ().bDead) {
				block.transform.Find ("eff01").gameObject.SetActive (true);
			}
		}
	}
	void DeleteBlock(){
		int damage = 0;
		bool bmatch = false;
		GameObject[] blocks = GameObject.FindGameObjectsWithTag ("BLOCK");
		foreach (GameObject block in blocks) {
			if (block.GetComponent<Block> ().bDead) {
				damage += 1;
				Board [block.GetComponent<Block> ().iX] [block.GetComponent<Block> ().iY] = (int)BLOCK.BLANK;
				Destroy(block);
				bmatch = true;
			}
		}
		if (bmatch) {      // 매치된 게 있으면 블럭 내려옴

			MyAttack (damage);   // 나의 공격

			BlockDown ();
		} else {           // 매치된 게 없으면 다시 드래그 시작 
			bDrag = true;
		}
	}

	void BlockDown(){
		bool bmovedown = false;
		for (int i = 0; i < iBoardX  ; i++) {
			bool bcheckdead = false;
			for (int j = iBoardY-1; j > -1 ; j--) {          
				if (!bcheckdead && Board [i] [j] == (int)BLOCK.BLANK) {
					bcheckdead = true;
					bmovedown = true;

					GameObject tmp = Instantiate(gBlockPrefab, new Vector3 (i, iBoardY, 0), Quaternion.identity) as GameObject;  // 새로운 블럭 생성
					tmp.transform.SetParent (this.transform);
					int itype = (int)Random.Range (0, iImgType);
					tmp.GetComponent<Block> ().iX             = i;
					tmp.GetComponent<Block> ().iY             = iBoardY;
					tmp.GetComponent<Block> ().iBloackType    = itype;
					tmp.GetComponent<SpriteRenderer>().sprite = imgBlock[itype];

					GameObject[] blocks = GameObject.FindGameObjectsWithTag ("BLOCK");
					for (int k = j + 1; k < iBoardY+1; k++) {     // 새로 생성된 블럭도 포함해서 내려감 
						foreach (GameObject block in blocks) {
							if (block.GetComponent<Block> ().iX == i && block.GetComponent<Block> ().iY == k) {
								block.GetComponent<Block> ().startPos  = new Vector3 (i, k, 0);
								block.GetComponent<Block> ().target    = new Vector3 (i, k - 1, 0);
								block.GetComponent<Block> ().bMoveDown = true;
								break;
							}
						}
					}
				}
			}
		}
		if (bmovedown) {                 // 빈 칸이 남아 있으면 다시 블럭 내려옴
			bBlockMoveDelete = true;
		} else {                         // 빈 칸이 남아 있지 않으면 다시 매치 시작
			bReCheck = true;
			StartCoroutine(CheckMatch ()); // 매치 시작
		}
	}

	void DeleteAllBlock(){   // 재시작시 기존 블록 모두 지움
		
		GameObject[] blocks = GameObject.FindGameObjectsWithTag ("BLOCK");
		foreach (GameObject block in blocks) {
			Destroy(block);
		}

	}
}

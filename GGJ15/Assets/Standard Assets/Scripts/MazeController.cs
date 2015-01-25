using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimeController{
	private float startTime;
	private float restSeconds;
	public int RemainingSeconds = 1; //only one timer. 
	private float displaySeconds;
	private float displayMinutes;
	public int countDownSeconds = 120;
	private float timeLeft;
	public string timeText;


	public void Start(){
		startTime = Time.time;
	}

	public string GetTime(){
		return timeText;
	}

	public void Update(){

		if(RemainingSeconds <= 0 ){
			return;
		}
		float TimeLeft = Time.time - startTime;
		restSeconds = countDownSeconds - TimeLeft;
		RemainingSeconds = Mathf.CeilToInt (restSeconds);

		displaySeconds = RemainingSeconds % 60;
		displayMinutes = (RemainingSeconds / 60) % 60;
		timeText = displayMinutes.ToString () + ":";
		if (displaySeconds > 9) {
			timeText += displaySeconds.ToString();
		}
		else{
			timeText += "0" + displaySeconds.ToString();
		}

		//return timeText;
	}
}
public enum EPlayer{
	PLAYER_0,
	PLAYER_1,
}

public struct Exit{
	public Vector3 Position;
	public EPlayer PlayerIndex;
	public bool HasDoor;
}


public class MazeController : MonoBehaviour {

	public static int TOTAL_WIDTH = 50;
	public static int TOTAL_HEIGHT = 50;
	public static Exit[] MazeExits;
	public static int MAX_EXITS = 5;

	public GameObject wall;
	public GameObject camera;
	public GameObject testCube;
	public static int IntSizeBits = 0 ;
	public ArrayList WallArray;
	private ArrayList SwitchArray;
    public static int TotalExits = 0 ;
	public bool Initialized = false;
	public GameObject Player1;
	public GameObject Player2;
	public GameObject SwitchObj;
	public GameObject Door;
	public GameObject TimeText;
	public GameObject WinLoseText;
	public GameObject SoundExit;
	public GameObject WallHit;
	public GameObject SoundSwitch;
//	public GUIText TimeText;
	//public GameObject camera;



	public Color Player1DoorColor;// = Color.cyan, 
	public Color Player2DoorColor;// = Color.red;
	public Color InsideWallColor;// = Color.white;
	public Color Player1Color;// = Color.blue;
	public Color Player2Color;// = Color.red;

	private int MAX_ITERATIONS = 200;
	TimeController Timer;
	// starts from left bottom corder, goes right, and goes up a row in array order. 
	//Least significant bit has information about the left bottom cell. 
	//Most significant bit has information (0/1) about the top right cell. 
	public static int[] MazeBitMap; //Dangerous. exposing bitmap to outside maze directly.

	private enum Direction
	{	
		HORIZONTAL,
		VERTICAL,
	}


	void SetBorder()
	{
		GameObject newWall;

		int BitMapCellCount = 0;
		int ExitIndex = 0;

		for(int k = 0; k < TOTAL_HEIGHT; k++){
			for (int i = 0; i < TOTAL_WIDTH; i++) {

				if(i == TOTAL_WIDTH - 1 || k == TOTAL_HEIGHT - 1 || i == 0 || k == 0){
					newWall = (GameObject)Instantiate(wall, new Vector3(i, 0, k), Quaternion.identity);
					WallArray.Add(newWall);
					newWall.renderer.material.color = Color.gray;

					bool Corner = (i == 0 && k == 0 ) || ( i == 0 && k == TOTAL_HEIGHT - 1) || (i == TOTAL_WIDTH - 1 && k == 0 ) || ( i == TOTAL_WIDTH - 1 && k == TOTAL_HEIGHT - 1 );
					bool bExitAllowed =  false;
					if(i == 0  || i == TOTAL_WIDTH - 1){
						bExitAllowed = (k > 4 && k < TOTAL_HEIGHT - 4) ? true : false;
					}
					else if(k == 0 || k == TOTAL_HEIGHT - 1){
						bExitAllowed = (i > 4 && i < TOTAL_HEIGHT)? true : false;
					}
					newWall.renderer.enabled = true;
					//create exit
					if(Random.Range(0,45) == 0 && ExitIndex < MAX_EXITS && !Corner && bExitAllowed)
					{
						//exit
						if(ExitIndex % 2 == 0){
							//player 1
							newWall.renderer.material.color = Player1DoorColor;
							//CreateNewDoor(i,k,Player1DoorColor);
							MazeExits[ExitIndex].PlayerIndex = EPlayer.PLAYER_0;						
						}
						else{
							newWall.renderer.material.color = Player2DoorColor;
							//CreateNewDoor(i,k,Player2DoorColor);
							MazeExits[ExitIndex].PlayerIndex = EPlayer.PLAYER_1;
						}
						MazeExits[ExitIndex].Position.x = i;
						MazeExits[ExitIndex].Position.z = k;
						MazeExits[ExitIndex].HasDoor = false;

						ExitIndex = ExitIndex + 1;

						//newWall.renderer.material.color.a = 0.5;
					}
					else{
						SetBitMap(BitMapCellCount);
					}


				 }//if border
					
			  	 BitMapCellCount++;
			}//for loop : inner.
		}

		//Verify if each player has atleast one exit.
		bool P1 = false, P2 = false;
		for(int i = 0 ; i < ExitIndex ;i++){
			if(MazeExits[i].PlayerIndex == EPlayer.PLAYER_0){ P1 = true; }
			else if (MazeExits[i].PlayerIndex == EPlayer.PLAYER_1) { P2 = true; }
		}
		if (!P1) {
						//hack : pick a left border wall instead of random location. 
			int z = TOTAL_HEIGHT / 2;
			int x = 0 ;
			newWall = (GameObject)Instantiate (wall, new Vector3 (x, 0, z), Quaternion.identity); //Warning memory leak. todo : find the existing wall in that location. 
			newWall.renderer.enabled = true;
			WallArray.Add (newWall);

						//looks like there was only one exit and it belongs to p2. because the p1 and p2 exits alternate.
						newWall.renderer.material.color = Player1DoorColor;
						MazeExits [ExitIndex].Position = new Vector3 (0, 0, z);
						MazeExits [ExitIndex].PlayerIndex = EPlayer.PLAYER_0;	
			MazeExits[ExitIndex].HasDoor = false;
			//CreateNewDoor(x,z,Player1DoorColor);
			//newWall.renderer.enabled = false;
			//add to bit mask

			SetBitMap(x, z);
			ExitIndex++;

			} 
		   else if (!P2) {
			//hack : pick a left border wall instead of random location. 
			int z = TOTAL_HEIGHT / 2;
			int x = 0 ;
			newWall = (GameObject)Instantiate (wall, new Vector3 (x, 0, z), Quaternion.identity); //Warning memory leak. todo : find the existing wall in that location. 
			WallArray.Add (newWall);
			newWall.renderer.enabled = true;

			
			//looks like there was only one exit and it belongs to p1. because the p1 and p2 exits alternate during generation
			newWall.renderer.material.color = Player2DoorColor;
			MazeExits [ExitIndex].Position = new Vector3 (0, 0, z);
			MazeExits [ExitIndex].PlayerIndex = EPlayer.PLAYER_1;	
			MazeExits[ExitIndex].HasDoor = false;
			//CreateNewDoor(x,z,Player1DoorColor);
			//newWall.renderer.enabled = false;
			
			//add to bit mask
			SetBitMap(x, z);
			ExitIndex++;
			}
				
		TotalExits = ExitIndex;
	}//func

	
	// Use this for initialization
	void Awake() {
		IntSizeBits = sizeof(int) * 8;
		MazeBitMap = new int[( TOTAL_WIDTH * TOTAL_HEIGHT / IntSizeBits ) + 1 ];
		MazeExits = new Exit[MAX_EXITS];
		WallArray = new ArrayList();
		SwitchArray = new ArrayList ();
		GUIText t = WinLoseText.GetComponent<GUIText> ();
		t.text = "";
		SetBorder ();	
		Timer = new TimeController ();
		Timer.Start ();
		Timer.countDownSeconds = 30;

		RecursiveDivideAlgorithm (0, 0, TOTAL_WIDTH, TOTAL_HEIGHT, Direction.HORIZONTAL, 0);
		camera.transform.position = new Vector3 (TOTAL_WIDTH / 2, 50, TOTAL_HEIGHT / 2);

		//Create Door
		CreateDoor ();
		CreateSwitch ();
		Initialized = true;

	}
	
	// Update is called once per frame
	void Update () {
		Timer.Update ();
		//Text t = CountdownTimerText.GetComponent<Text> ();
//		t.text = Timer.timeText;
	//	t.text = Timer.GetTime ();
		//Debug.Log (t.text);
		GUIText t = TimeText.GetComponent<GUIText> ();
		t.text = Timer.GetTime ();

		if (Timer.RemainingSeconds <= 0) {
			PlayersLost();
		}


	}

	void TestBitMap(){
		int index = 0;
		for(int k = 0; k < TOTAL_HEIGHT; k++){
			for (int i = 0; i < TOTAL_WIDTH; i++) {
				
				if(BitMapHasEntry(i,k)){
					GameObject testcb = (GameObject)Instantiate(testCube, new Vector3(i, 5, k), Quaternion.identity);
					testcb.renderer.material.color = Color.blue;
				}
				index++;
			}
		}
	}
	void RecursiveDivideAlgorithm(int x, int y, int width, int height, Direction d, int IterationCount)
	{
		
		Direction Dir = GetDirection(width, height);
		if(++IterationCount > MAX_ITERATIONS){ Debug.Log("Stack OverFlow Warning"); }
		if( width <= 1 || height <= 1) { return; }
		if(d == Direction.HORIZONTAL){ RecursiveHorizontalDivide(x, y, width, height, Dir, IterationCount); }
		else if( d == Direction.VERTICAL){ RecursiveVerticalDivide(x, y, width, height, Dir, IterationCount); }
	}
	
	Direction GetDirection(int w, int h)
	{
		if (w == h) {
			int r = Random.Range(0,2);
			if(r > 0){ return Direction.HORIZONTAL; }
			return Direction.VERTICAL;
		}
		
		return (w < h ? Direction.HORIZONTAL : Direction.VERTICAL);
	}
	
	void RecursiveHorizontalDivide(int x, int y, int w, int h, Direction d, int IterationCount){
		int y1 = Random.Range (y + 1, y + h - 1);
		int Hole = Random.Range (x + 1, x + w - 1);
		int Hole2 = Random.Range (x + 1, x + w - 1);
		GameObject newWall;

		for (int xStep = x+1; xStep < (x+w-1) && xStep != Hole && xStep != Hole2; xStep++) {
			if(WallExistsInArray(xStep, y1)){continue;}
			newWall = (GameObject)Instantiate(wall, new Vector3(xStep, 0, y1), Quaternion.identity);
			WallArray.Add (newWall);
			SetBitMap(xStep, y1);
			newWall.renderer.enabled = true;
			newWall.renderer.material.color = InsideWallColor;
		}
		
		RecursiveDivideAlgorithm (x, y, w, y1 - y, d, IterationCount);
		RecursiveDivideAlgorithm (x, y1, w, y + h - y1, d, IterationCount);
	}
	
	void RecursiveVerticalDivide(int x, int y, int w, int h, Direction d, int IterationCount){
		int x1 = Random.Range (x + 1, x + w - 1);
		int Hole = Random.Range (y + 1, y + h - 1);
		int Hole2 = Random.Range (y + 1, y + h - 1);

		GameObject newWall;
		for (int yStep = y+1; yStep < (y+h-1) && yStep != Hole && yStep != Hole2; yStep++) {
			//newWall = (GameObject)Instantiate(wall, new Vector3(yStep, 0, x1), Quaternion.identity);
			if(WallExistsInArray(x1, yStep)){continue;}
			newWall = (GameObject)Instantiate(wall, new Vector3(x1, 0, yStep), Quaternion.identity);
			WallArray.Add (newWall);
			SetBitMap(x1, yStep);
			newWall.renderer.enabled = true;
			newWall.renderer.material.color = InsideWallColor;
		}
		RecursiveDivideAlgorithm (x, y, x1 - x, h, d, IterationCount);
		RecursiveDivideAlgorithm (x1, y, x + w - x1, h, d, IterationCount);
	}
	
	public bool HasExited(Vector3 InPos, EPlayer p){
		for(int i = 0 ; i < MAX_EXITS ; i++){
			if(InPos.x == MazeExits[i].Position.x && InPos.z == MazeExits[i].Position.z && MazeExits[i].PlayerIndex == p){
				SoundExit.audio.Play();
				return true;
			}
		}
		return false;
	}

	private void CreateDoor()
	{
		//Alternate between each player's exit and create doors. Make sure there are no other exits nearby.

		bool P1Door = false, P2Door = false;
		for(int i = 0 ; i < TotalExits; i++){
			if(MazeExits[i].PlayerIndex == EPlayer.PLAYER_0 ){
				if(!P1Door){

					CreateDoorInternal(i);
					P1Door = true;

				}
				else{
					P1Door = false;
				}
			}//if
			else if(MazeExits[i].PlayerIndex == EPlayer.PLAYER_1){
				if(!P2Door){

					CreateDoorInternal(i);
					P2Door = true;

				}
				else{
					P2Door = false;
				}
			}

		}
	}//function

	private void CreateDoorInternal(int MazeExitIndex){
		Vector3 pos = MazeExits[MazeExitIndex].Position;
		EPlayer PlayerIndex = MazeExits [MazeExitIndex].PlayerIndex;

		Color c = (PlayerIndex == EPlayer.PLAYER_0) ? Player1DoorColor : Player2DoorColor;

		if (pos.x == 0) {
			//left border, somewhere. 
			for(float x1 = pos.x + 1 ; x1 <= pos.x + 2; x1++){

				CreateNewWall(x1, pos.z - 1, c);
				CreateNewWall(x1, pos.z, c);
				CreateNewWall(x1, pos.z + 1, c);

				if(x1 == pos.x + 1){
					ClearWall(x1, pos.z);
				}//if


			}//for
			MazeExits[MazeExitIndex].HasDoor = true;
		}// if
		else if(pos.x == TOTAL_WIDTH -1){
			//right border. 
			for(float x1 = pos.x - 1; x1 >= pos.x - 2; x1--){

				CreateNewWall(x1, pos.z - 1, c);
				CreateNewWall(x1, pos.z, c);
				CreateNewWall(x1, pos.z + 1,c);
				
				if(x1 == pos.x - 1){
					ClearWall(x1, pos.z);
				}//if
			}//for
			MazeExits[MazeExitIndex].HasDoor = true;
		}//ELSE
		else if(pos.z == 0){
			//bottom border
			for(float y1 = pos.z + 1 ; y1 <= pos.z + 2 ;y1++){

				CreateNewWall(pos.x - 1 , y1,c);
				CreateNewWall(pos.x, y1, c);
				CreateNewWall(pos.x + 1, y1, c);

				if(y1 == pos.z + 1){
					ClearWall(pos.x, y1);
				}
			}//for
			MazeExits[MazeExitIndex].HasDoor = true;
		}//bottom border
		else if(pos.z == TOTAL_HEIGHT - 1){
			for(float y1 = pos.z - 1 ; y1 >= pos.z - 2 ;y1--){

				CreateNewWall(pos.x - 1 , y1, c);
				CreateNewWall(pos.x, y1, c );
				CreateNewWall(pos.x + 1, y1, c);
				
				if(y1 == pos.z - 1){
					ClearWall(pos.x, y1);
				}
			}
			MazeExits[MazeExitIndex].HasDoor = true;
		}

	}//func

	private GameObject CreateNewWall(float x, float z, Color c){
		ClearWall (x, z);
		GameObject newWall = (GameObject)Instantiate (wall, new Vector3 (x, 0, z), Quaternion.identity); 
		newWall.renderer.enabled = true;
		WallArray.Add (newWall);
		newWall.renderer.material.color = c;

		//add to bit mask
		SetBitMap (x, z);
		return newWall;
	}//func

	private void ClearWall(float x, float z){

		//get the wall at location, and hide. 
		foreach( GameObject wall in WallArray){
			if((int)wall.transform.position.x == (int)x && (int)wall.transform.position.z == (int)z){
				wall.renderer.enabled = false;	
				ClearBitMap(x, z);
			}
		}
	}//func

	private bool WallExistsInArray(float x, float z){
		foreach( GameObject wall in WallArray){
			if((int)wall.transform.position.x == (int)x && (int)wall.transform.position.z == (int)z){
				return true;
			}
		}
		return false;
	}//function

	private void SetBitMap(int index){
		MazeBitMap[index / IntSizeBits] |= (0x1 << (index % IntSizeBits));
	}

	private void SetBitMap(float x, float z){
		int index = (int)(z * TOTAL_WIDTH + x);
		SetBitMap (index);
	}

	public bool BitMapHasEntry(float x, float z){
		int index = (int)(z * TOTAL_WIDTH + x);
		return ((MazeBitMap[index / IntSizeBits] & ( 1 << (index % IntSizeBits))) != 0 );
	}

	private void ClearBitMap(float x, float z){
		int index = (int)(z * TOTAL_WIDTH + x);
		MazeBitMap[index / IntSizeBits] &= ~(1 << (index % IntSizeBits));
	}
	private void CreateSwitch()
	{
		int x = Random.Range (20, TOTAL_WIDTH -20);
		int y = Random.Range (20, TOTAL_HEIGHT - 20);

		ClearWall (x - 1, y - 1);
		ClearWall (x - 1, y);
		ClearWall (x - 1, y + 1);
		ClearWall (x, y - 1);
		ClearWall (x, y);
		ClearWall (x, y + 1);
		ClearWall (x + 1, y - 1);
		ClearWall (x, y);
		ClearWall (x, y + 1);

		GameObject w = CreateNewSwitch (x, y, Player1Color);

		//CreateNewWall (x, y, Color.blue); //p1

		SwitchArray.Add (w);

		int x1 = x;
		int y1 = y;
		do {
		 x1 = Random.Range (15, TOTAL_WIDTH - 15);
		 y1 = Random.Range (15, TOTAL_HEIGHT - 15);
				} while(x1 == x && y1 ==y);
		x = x1;
		y = y1;

		ClearWall (x - 1, y - 1);
		ClearWall (x - 1, y);
		ClearWall (x - 1, y + 1);
		ClearWall (x, y - 1);
		ClearWall (x, y);
		ClearWall (x, y + 1);
		ClearWall (x + 1, y - 1);
		ClearWall (x, y);
		ClearWall (x, y + 1);
		
		GameObject w1 = CreateNewSwitch (x, y, Player2Color);
		//CreateNewWall (x, y, Color.red); //p1
		SwitchArray.Add (w1);

	}

	public bool HandleSwitch(Vector3 Inpos, EPlayer p){

		foreach( GameObject wall in SwitchArray){
			if((int)wall.transform.position.x == (int)Inpos.x && (int)wall.transform.position.z == (int)Inpos.z){
			
				//clear door. 
				if(wall.renderer.material.color == Player1Color && p == EPlayer.PLAYER_1){
					//player 1 switch. 
					for(int i = 0 ; i < TotalExits ;i++){
						Exit e = MazeExits[i];
						if(e.PlayerIndex == EPlayer.PLAYER_0 && e.HasDoor){
							ClearDoor(e);
						}// player 1

					}// for

					ClearWall(Inpos.x, Inpos.z);
					CreateNewWall(Inpos.x, Inpos.z, InsideWallColor);

				}

				else if(wall.renderer.material.color == Player2Color && p == EPlayer.PLAYER_0){
					//player 1 switch. 
					for(int i = 0 ; i < TotalExits ;i++){
						Exit e = MazeExits[i];
						if(e.PlayerIndex == EPlayer.PLAYER_1 && e.HasDoor){
							ClearDoor(e);
						}// player 1
					}// for

					ClearWall(Inpos.x, Inpos.z);
					CreateNewWall(Inpos.x, Inpos.z, InsideWallColor);
					
				}

				SoundSwitch.audio.Play();
				return true;
			}
		}
		return false;
	}

private void ClearDoor(Exit e){
	//clear all doors of player 0
	Vector3 pos = e.Position;

	if (pos.x == 0) {
		//left border, somewhere. 
		for(float x1 = pos.x + 1 ; x1 <= pos.x + 2; x1++){
			
			ClearWall(x1, pos.z - 1);
			ClearWall(x1, pos.z);
			ClearWall(x1, pos.z + 1);
			
		}//for
		e.HasDoor = false;
	}// if
	else if(pos.x == TOTAL_WIDTH -1){
		//right border. 
		for(float x1 = pos.x - 1; x1 >= pos.x - 2; x1--){
			
			ClearWall(x1, pos.z - 1);
			ClearWall(x1, pos.z);
			ClearWall(x1, pos.z + 1);
			
		}//for
		e.HasDoor = false;
	}//ELSE
	else if(pos.z == 0){
		//bottom border
		for(float y1 = pos.z + 1 ; y1 <= pos.z + 2 ;y1++){
			
			ClearWall(pos.x - 1 , y1);
			ClearWall(pos.x, y1);
			ClearWall(pos.x + 1, y1);
			
		}//for
		e.HasDoor = false;
	}//bottom border
	else if(pos.z == TOTAL_HEIGHT - 1){
		for(float y1 = pos.z - 1 ; y1 >= pos.z - 2 ;y1--){
			
			ClearWall(pos.x - 1 , y1);
			ClearWall(pos.x, y1);
			ClearWall(pos.x + 1, y1);
			
		}
		e.HasDoor = false;
	}

	}//func
	public void Playerswon(){


	 GUIText t = WinLoseText.GetComponent<GUIText> ();
		if(t.text == ""){
			t.text = "Won!!";
			Timer.RemainingSeconds = 0;
		//	AudioSource aa = WinLoseText.GetComponent<AudioSource>();
			//audio.PlayOneShot(aa.clip);
			//aa.Play();
			WinLoseText.audio.Play();
		}

	}

	public void PlayersLost(){
		GUIText t = WinLoseText.GetComponent<GUIText> ();
		if(t.text ==""){
			t.text = "Boo!!!";
			Timer.RemainingSeconds = 0;
			WinLoseText.audio.Play();         
		}
		
	}
	
	public bool IsGameOver(){
				return ( Initialized && Timer.RemainingSeconds <= 0 );
	}

	public GameObject CreateNewSwitch(float x, float z, Color c){

	    ClearWall (x, z);
		GameObject obj = (GameObject)Instantiate (SwitchObj, new Vector3 (x, 0.0f, z), Quaternion.identity); 
		obj.renderer.sortingOrder = 1;
		obj.renderer.enabled = true;
		obj.transform.Rotate (new Vector3 (90, 0, 0));
		obj.transform.localScale = new Vector3 (1f, 0.5f, 0.5f);
		WallArray.Add (obj);
		obj.renderer.material.color = c;
		//obj.transform.position.y = 2;
		
		//add to bit mask
		SetBitMap (x, z);
		return obj;

	}


	private GameObject CreateNewDoor(float x, float z, Color c){
		//	return;
		ClearWall (x, z);
		GameObject obj = (GameObject)Instantiate (Door, new Vector3 (x, 1.0f, z), Quaternion.identity); 
		//obj.renderer.sortingOrder = 1;
		obj.renderer.enabled = true;
		obj.transform.Rotate (new Vector3 (90, 0, 0));
		obj.transform.localScale = new Vector3 (2f, 2f, 2f);
		WallArray.Add (obj);
		//obj.renderer.material.color = c;
		//obj.transform.position.y = 2;
		
		//add to bit mask
		SetBitMap (x, z);
		return obj;
		
	}


}
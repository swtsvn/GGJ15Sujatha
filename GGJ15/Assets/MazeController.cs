using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimeController{
	private float startTime;
	private float restSeconds;
	public static int RemainingSeconds = 1; //only one timer. 
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
	public Vector2 Position;
	public EPlayer PlayerIndex;
}

public class MazeController : MonoBehaviour {

	public static int TOTAL_WIDTH = 50;
	public static int TOTAL_HEIGHT = 50;
	public static Exit[] MazeExits;
	//public static Vector2 Player1Exit = 0 ;
	//public static int Player2Exit = 0 ;
	public static int MAX_EXITS = 5;

	public GameObject wall;
	public GameObject camera;
	public GameObject CountdownTimerText;
	public GameObject testCube;
	public static int IntSizeBits = 0 ;

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

					newWall.renderer.material.color = Color.cyan;

					bool Corner = (i == 0 && k == 0 ) || ( i == 0 && k == TOTAL_HEIGHT - 1) || (i == TOTAL_WIDTH - 1 && k == 0 ) || ( i == TOTAL_WIDTH - 1 && k == TOTAL_HEIGHT - 1 );
					if(Random.Range(0,45) == 0 && ExitIndex < MAX_EXITS && !Corner)
					{
						//exit
						if(ExitIndex % 2 == 0){
							//player 1
							newWall.renderer.material.color = Color.blue;
							MazeExits[ExitIndex].Position = new Vector3(i, 0, k);
							MazeExits[ExitIndex].PlayerIndex = EPlayer.PLAYER_0;						
						}
						else{
							newWall.renderer.material.color = Color.red;
							MazeExits[ExitIndex].Position = new Vector3(i, 0, k);
							MazeExits[ExitIndex].PlayerIndex = EPlayer.PLAYER_1;
						}
						ExitIndex++;
					}
					else{
						MazeBitMap[BitMapCellCount / IntSizeBits] |= (0x1 << (BitMapCellCount % IntSizeBits));
					}
					newWall.renderer.enabled = true;

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
						newWall = (GameObject)Instantiate (wall, new Vector3 (0, 0, z), Quaternion.identity); //Warning memory leak. todo : find the existing wall in that location. 
			

						//looks like there was only one exit and it belongs to p2. because the p1 and p2 exits alternate.
						newWall.renderer.material.color = Color.blue;
						MazeExits [ExitIndex].Position = new Vector3 (0, 0, z);
						MazeExits [ExitIndex].PlayerIndex = EPlayer.PLAYER_0;		
				} else if (!P2) {
						//hack : pick a left border wall instead of random location. 
						int z = TOTAL_HEIGHT / 2;
						newWall = (GameObject)Instantiate (wall, new Vector3 (0, 0, z), Quaternion.identity); //Warning memory leak. todo : find the existing wall in that location. 
			
			
						//looks like there was only one exit and it belongs to p1. because the p1 and p2 exits alternate during generation
						newWall.renderer.material.color = Color.red;
						MazeExits [ExitIndex].Position = new Vector3 (0, 0, z);
						MazeExits [ExitIndex].PlayerIndex = EPlayer.PLAYER_1;		
				}
				

	}//func

	
	// Use this for initialization
	void Awake() {
		IntSizeBits = sizeof(int) * sizeof(byte);
		MazeBitMap = new int[( TOTAL_WIDTH * TOTAL_HEIGHT / IntSizeBits ) + 1 ];
		MazeExits = new Exit[MAX_EXITS];

		SetBorder ();	
		Timer = new TimeController ();
		Timer.Start ();
		Timer.countDownSeconds = 15;

		RecursiveDivideAlgorithm (0, 0, TOTAL_WIDTH, TOTAL_HEIGHT, Direction.HORIZONTAL, 0);
		camera.transform.position = new Vector3 (TOTAL_WIDTH / 2, 50, TOTAL_HEIGHT / 2);

		//Create Door
		CreateDoor ();

	}
	
	// Update is called once per frame
	void Update () {
		Timer.Update ();
		Text t = CountdownTimerText.GetComponent<Text> ();
		t.text = Timer.timeText;
		//Debug.Log (Timer.GetTime());
		//t.text = "sfa";
		t.text = Timer.GetTime ();
		//Debug.Log (MazeBitMap.ToString ());
		//TestBitMap ();
	}

	void TestBitMap(){
		int index = 0;
		for(int k = 0; k < TOTAL_HEIGHT; k++){
			for (int i = 0; i < TOTAL_WIDTH; i++) {
				
				if((MazeBitMap[index / IntSizeBits] & ( 1 << (index % IntSizeBits))) != 0 ){
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
			//newWall = (GameObject)Instantiate(wall, new Vector3(y1, 0, xStep), Quaternion.identity);
			newWall = (GameObject)Instantiate(wall, new Vector3(xStep, 0, y1), Quaternion.identity);
			int index = y1 * TOTAL_WIDTH + xStep;
			MazeBitMap[index / IntSizeBits] |= (1 << (index % IntSizeBits));
			newWall.renderer.enabled = true;
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
			newWall = (GameObject)Instantiate(wall, new Vector3(x1, 0, yStep), Quaternion.identity);
			int index = yStep * TOTAL_WIDTH + x1;
			MazeBitMap[index / IntSizeBits] |= (1 << (index % IntSizeBits));
			newWall.renderer.enabled = true;
		}
		RecursiveDivideAlgorithm (x, y, x1 - x, h, d, IterationCount);
		RecursiveDivideAlgorithm (x1, y, x + w - x1, h, d, IterationCount);
	}

	public static bool IsWall(int index)
	{
		return ((MazeBitMap [index / IntSizeBits] & (1 << (index % IntSizeBits))) != 0);
	}

	public static bool HasExited(Vector3 InPos){
		for(int i = 0 ; i < MAX_EXITS ; i++){
			if(InPos.x == MazeExits[i].Position.x && InPos.y == MazeExits[i].Position.y){
				return true;
			}
		}
		return false;
	}

	private void CreateDoor()
	{
		//Alternate between each player's exit and create doors. Make sure there are no other exits nearby.

		bool P1Door = false, P2Door = false;
		for(int i = 0 ; i < MAX_EXITS; i++){
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
		ClearSorroundingWalls(MazeExitIndex);
	}//func

	private void ClearSorroundingWalls(int MazeExitIndex){
		Vector2 pos = MazeExits[MazeExitIndex].Position;
		EPlayer PlayerIndex = MazeExits [MazeExitIndex].PlayerIndex;

		if (pos.x == 0) {
			//left border, somewhere. 
			for(float x1 = pos.x + 1 ; x1 <= pos.x + 2; x1++){
				//clear the sorrounding blocks to allow door creation . 
				//ClearWall(x1, pos.y-1);
				//ClearWall(x1, pos.y+1);

				CreateNewWall(x1, pos.y - 1);
				CreateNewWall(x1, pos.y);
				CreateNewWall(x1, pos.y + 1);

				if(x1 == pos.x + 1){
					ClearWall(x1, pos.y);
				}//if
			}//for
		}// if
		else if(pos.x == TOTAL_WIDTH -1){
			//right border. 
			for(float x1 = pos.x - 1; x1 <= pos.x - 2; x1++){
				CreateNewWall(x1, pos.y - 1);
				CreateNewWall(x1, pos.y);
				CreateNewWall(x1, pos.y + 1);
				
				if(x1 == pos.x - 1){
					ClearWall(x1, pos.y);
				}//if
			}//for
		}//ELSE
		else if(pos.y == 0){
			//bottom border
			for(float y1 = pos.y + 1 ; y1 <= pos.y + 2 ;y1++){
				CreateNewWall(pos.x - 1 , y1);
				CreateNewWall(pos.x, y1);
				CreateNewWall(pos.x + 1, y1);

				if(y1 == pos.y + 1){
					ClearWall(pos.x, y1);
				}
			}//for
		}//bottom border
		else if(pos.y == TOTAL_HEIGHT - 1){
			for(float y1 = pos.y - 1 ; y1 <= pos.y - 2 ;y1++){
				CreateNewWall(pos.x - 1 , y1);
				CreateNewWall(pos.x, y1);
				CreateNewWall(pos.x + 1, y1);
				
				if(y1 == pos.y - 1){
					ClearWall(pos.x, y1);
				}
			}
		}

	}//func

	private void CreateNewWall(float x, float y){
	}//func
	private void ClearWall(float x, float y){
	}//func
}

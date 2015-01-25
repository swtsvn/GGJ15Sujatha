using UnityEngine;
using System.Collections;


public class player2Controller : MonoBehaviour {

	public bool Won = false;

	private GameObject maze;
	private MazeController mazeScript;

	private GameObject otherPlayer;
	private player1Controller otherPlayerScript;

	// Use this for initialization
	void Start () {

		PlayerCommon.Start ();

		maze = GameObject.Find ("Maze");
		mazeScript = maze.GetComponent<MazeController> ();

		otherPlayer = GameObject.Find ("player1");
		otherPlayerScript = otherPlayer.GetComponent<player1Controller> ();
		

		int x = Random.Range (10,15);
		int z = Random.Range (10,15);
		//int index = z * MazeController.TOTAL_WIDTH + x;
		while( mazeScript.BitMapHasEntry(x,z)){
			x = Random.Range (10,15);
			z = Random.Range (10,15);
			//index = z * MazeController.TOTAL_WIDTH + x;
		}
		transform.position = new Vector3 (x, 0, z);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void FixedUpdate(){
		if(Won){return;}
		if(mazeScript.IsGameOver()){
			//play sound and animation for first time. 
			return;
		}
		KeyCode left = KeyCode.LeftArrow;
		KeyCode right = KeyCode.RightArrow;
		KeyCode up = KeyCode.UpArrow;
		KeyCode down = KeyCode.DownArrow;
		

		float xDir = 0;
		float yDir = 0;
		if (Input.GetKeyDown (left)) {
			xDir = -1;
		} 
		else if (Input.GetKeyDown (right)) {
			xDir = 1;
		}
		
		if(Input.GetKeyDown(down)){
			yDir = -1;
		}
		else if(Input.GetKeyDown(up)){
			yDir  = 1;
		}
		
		Vector3 NewPos = transform.position + new Vector3 (xDir, 0, yDir);
		if((xDir != 0 || yDir != 0) && !IsCollidingWithWall(NewPos)){
			//check player player collision. 
			if(NewPos.x == otherPlayer.transform.position.x && NewPos.z == otherPlayer.transform.position.z){
				xDir = xDir * -1 ;
				yDir = yDir * -1 ;
				Vector3 CurPos = transform.position;
				CurPos.x = CurPos.x + xDir;
				CurPos.z = CurPos.z + yDir;
				NewPos = transform.position;
				if(!IsCollidingWithWall(CurPos)){
					NewPos = CurPos;
				}
			}
			transform.position = NewPos;
			if(mazeScript.HasExited(NewPos, EPlayer.PLAYER_1)){
				Won = true;
				renderer.material.color = Color.cyan;
				renderer.enabled = false;
				if(otherPlayerScript.Won){
					//game won
					mazeScript.Playerswon();
				}
				//play sound 
			}
			//play sound 
		}
	}

	private bool IsCollidingWithWall(Vector3 NewPos){
		if(mazeScript.BitMapHasEntry(NewPos.x, NewPos.z)){
			if(mazeScript.HandleSwitch(NewPos, EPlayer.PLAYER_1)){
				//play switch sound
			}
			else{
				//play block sound
			}
			return true;
		}
		return false;
	}
}

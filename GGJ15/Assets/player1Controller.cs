using UnityEngine;
using System.Collections;

public class player1Controller : MonoBehaviour {

	public bool Won = false;
	private GameObject maze;
	private MazeController mazeScript;
	private GameObject otherPlayer;
	private player2Controller otherPlayerScript;

	// Use this for initialization
	void Start () {

		PlayerCommon.Start ();

		maze = GameObject.Find ("Maze");
		mazeScript = maze.GetComponent<MazeController> ();

		otherPlayer = GameObject.Find ("player2");
		otherPlayerScript = otherPlayer.GetComponent<player2Controller> ();

		int x = Random.Range (20, MazeController.TOTAL_WIDTH - 20);
		int z = Random.Range (20, MazeController.TOTAL_HEIGHT - 20);
		//int index = z * MazeController.TOTAL_WIDTH + x;
		while( mazeScript.BitMapHasEntry(x, z)){
			x = Random.Range (20, MazeController.TOTAL_WIDTH - 20);
			z = Random.Range (20, MazeController.TOTAL_HEIGHT - 20);
		}
		
		transform.position = new Vector3 (x, 0, z);
	}
	
	// Update is called once per frame before rendering. 
	void Update () {
	
	}

	//before physics. 
	void FixedUpdate(){
		if(Won){return;}
		if(TimeController.RemainingSeconds <= 0){
			//play sound and animation for first time. 
			return;
		}

		KeyCode left = KeyCode.LeftArrow;
		KeyCode right = KeyCode.RightArrow;
		KeyCode up = KeyCode.UpArrow;
		KeyCode down = KeyCode.DownArrow;

		left = KeyCode.A;
		right = KeyCode.D;
		up = KeyCode.W;
		down = KeyCode.X;

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

		if(mazeScript.BitMapHasEntry(NewPos.x, NewPos.z)){
			 if(mazeScript.HandleSwitch(NewPos, EPlayer.PLAYER_0)){
				//play switch sound
				}
			else{
				//play block sound
			}
		}
		else{
			transform.position = NewPos;
			if(mazeScript.HasExited(NewPos)){
				Won = true;
				renderer.material.color = Color.cyan;
				if(otherPlayerScript.Won){
					//game won
					mazeScript.Playerswon();
				}
				//play sound 
				//play animation
			}
			//play sound 
		}
	}
}

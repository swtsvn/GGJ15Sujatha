using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class player1Controller : MonoBehaviour {

	public bool Won = false;
	private GameObject maze;
	private MazeController mazeScript;
	private GameObject otherPlayer;
	private player2Controller otherPlayerScript;

//	public AudioSource AudioHitWall;
//	public AudioClip AudioClipHitWall;

	// Use this for initialization
	void Start () {

		PlayerCommon.Start ();

		maze = GameObject.Find ("Maze");
		mazeScript = maze.GetComponent<MazeController> ();

		otherPlayer = GameObject.Find ("player2");
		otherPlayerScript = otherPlayer.GetComponent<player2Controller> ();

		int x = Random.Range (10, 15);
		int z = Random.Range (10, 15);
		//int index = z * MazeController.TOTAL_WIDTH + x;
		while( mazeScript.BitMapHasEntry(x, z)){
			x = Random.Range (10,15);
			z = Random.Range (10,15);
		}
		
		transform.position = new Vector3 (x, 0, z);
	}
	
	// Update is called once per frame before rendering. 
	void Update () {
	
	}

	//before physics. 
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

		left = KeyCode.A;
		right = KeyCode.D;
		up = KeyCode.W;
		down = KeyCode.X;

		float xDir = 0;
		float yDir = 0;
		float value = 1.0f;
		if (Input.GetKeyDown (left)) {
			xDir = -value;
		} 
		else if (Input.GetKeyDown (right)) {
			xDir = value;
		}

		if(Input.GetKeyDown(down)){
			yDir = -value;
		}
		else if(Input.GetKeyDown(up)){
			yDir  = value;
		}


		Vector3 NewPos = transform.position + new Vector3 (xDir, 0, yDir);


		if((xDir != 0 || yDir != 0) &&  !IsCollidingWithWall(NewPos)){
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
			
			if(mazeScript.HasExited(NewPos, EPlayer.PLAYER_0)){
				Won = true;
				//renderer.material.color = Color.cyan;
				renderer.enabled = false;
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

	private bool IsCollidingWithWall(Vector3 NewPos){
		if(mazeScript.BitMapHasEntry(NewPos.x, NewPos.z)){
			if(mazeScript.HandleSwitch(NewPos, EPlayer.PLAYER_0)){
				//play switch sound
			}
			else{
				mazeScript.WallHit.audio.Play();
			}
			return true;
		}
		return false;
	}


}

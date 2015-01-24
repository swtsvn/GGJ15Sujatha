using UnityEngine;
using System.Collections;


public class player2Controller : MonoBehaviour {

	public static bool Won = false;

	// Use this for initialization
	void Start () {
		int x = Random.Range (1, MazeController.TOTAL_WIDTH - 2);
		int z = Random.Range (1, MazeController.TOTAL_HEIGHT - 2);
		int index = z * MazeController.TOTAL_WIDTH + x;
		while( MazeController.IsWall(index)){
			x = Random.Range (1, MazeController.TOTAL_WIDTH - 2);
			z = Random.Range (1, MazeController.TOTAL_HEIGHT - 2);
			index = z * MazeController.TOTAL_WIDTH + x;
		}
		
		transform.position = new Vector3 (x, 0, z);
		PlayerCommon.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
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
		
		//float x = Input.GetAxis ("Horizontal");
		//	float z = Input.GetAxis ("Vertical");
		//float speed = 3.0f;
		Vector3 NewPos = transform.position + new Vector3 (xDir, 0, yDir);
		int BitMapIndex = (int)NewPos.z * MazeController.TOTAL_WIDTH + (int)NewPos.x;
		if(MazeController.IsWall(BitMapIndex)){
			//play sound . cannot move.
		}
		else{
			transform.position = NewPos;
			if(MazeController.HasExited(NewPos)){
				Won = true;
				renderer.material.color = Color.cyan;
				//play sound 
			}
			//play sound 
		}
	}
}

using System;
using System.Collections;
using UnityEngine;

class ModScript : UniFileScriptBehaviour
{
    public void OnBind()
    {
	
    }
	
	//Called when the script has been loaded and before the song starts.
	public void OnSongStarting()
	{
	
	}
	
	//Called when the countdown has ended.
	public void OnSongStarted()
	{
	
	}
	
	//Called when the song is finished.
	public void OnSongDone()
	{
	
	}
	
	//Called when a beat occurs.
	public void OnBeat(int currentBeat)
	{
		print("Current Beat from Modscript is " + currentBeat);
		
		if(currentBeat %% 2 == 0){
			//Move the notes camera to the left instantly.
			Vector3 newPosition = new Vector3(-0.5f,2,-10);
		} else {
			
			Vector3 newPosition = new Vector3(0.5f,2,-10);
		}
		
		Song.instance.mainCamera.transform.position = newPosition;
		Song.instance.mainCamera.transform.LeanMoveX(0,.45f);
	}
	
	//Called when the song gets paused
	public void OnPause()
	{
		
	}
	
	//Called when the song gets unpaused
	public void OnUnpause()
	{
	
	}
	
	//Called when the player dies.
	public void OnDeath()
	{
	
	}
}
using UnityEngine;
using System.Collections;
using GoMap;

using GoShared;
using System;
using UnityEngine.Events;


public class MoveAvatar : MonoBehaviour {

	public GOMap goMap;
	public GameObject avatarFigure;

	public AvatarAnimationState animationState = AvatarAnimationState.Idle;
	[HideInInspector] public float dist;
	public enum AvatarAnimationState{
		Idle, 
		Walk,
		Run
	};
	public GOAvatarAnimationStateEvent OnAnimationStateChanged;

	// Use this for initialization
	void Start () {

		goMap.locationManager.onOriginSet.AddListener((Coordinates) => {OnOriginSet(Coordinates);});
		goMap.locationManager.onLocationChanged.AddListener((Coordinates) => {OnLocationChanged(Coordinates);});
		if (goMap.useElevation)
			goMap.OnTileLoad.AddListener((GOTile) => {OnTileLoad(GOTile);});
	}

	#region GoMap events

	public void OnTileLoad (GOTile tile) {

		Vector3 currentLocation = goMap.locationManager.currentLocation.convertCoordinateToVector ();

		if (tile.goTile.vectorIsInTile(currentLocation)) {

			if (goMap.useElevation)
				currentLocation = GOMap.AltitudeToPoint (currentLocation);
			
			transform.position = currentLocation;
		} 
	}

	#endregion

	#region Location manager events

	void OnOriginSet (Coordinates currentLocation) {

		//Position
		Vector3 currentPosition = currentLocation.convertCoordinateToVector (0);
		if(goMap.useElevation)
			currentPosition = GOMap.AltitudeToPoint (currentPosition);

		transform.position = currentPosition;

	}

	void OnLocationChanged (Coordinates currentLocation) {

		Vector3 lastPosition = transform.position;

		//Position
		Vector3 currentPosition = currentLocation.convertCoordinateToVector (0);

		if(goMap.useElevation)
			currentPosition = GOMap.AltitudeToPoint (currentPosition);

		if (lastPosition == Vector3.zero) {
			lastPosition = currentPosition;
		}
			
		moveAvatar (lastPosition,currentPosition);

	}

	#endregion

	#region Move Avatar

	void moveAvatar (Vector3 lastPosition, Vector3 currentPosition) {




		StartCoroutine (move (lastPosition,currentPosition,0.5f));
	}

	private IEnumerator move(Vector3 lastPosition, Vector3 currentPosition, float time) {

		float elapsedTime = 0;
		Vector3 targetDir = currentPosition-lastPosition;
		Quaternion finalRotation = Quaternion.LookRotation (targetDir);

		while (elapsedTime < time)
		{
			transform.position = Vector3.Lerp(lastPosition, currentPosition, (elapsedTime / time));
			avatarFigure.transform.rotation = Quaternion.Lerp(avatarFigure.transform.rotation, finalRotation,(elapsedTime / time));

			elapsedTime += Time.deltaTime;

			dist = Vector3.Distance (lastPosition, currentPosition);

			AvatarAnimationState state = AvatarAnimationState.Idle; 

			if (dist > 4)
				state = AvatarAnimationState.Run;
			else state = AvatarAnimationState.Walk;

			if (state != animationState) {

				animationState = state;
				OnAnimationStateChanged.Invoke(animationState);
			}

			yield return new WaitForEndOfFrame();
		}

		animationState = AvatarAnimationState.Idle;
		OnAnimationStateChanged.Invoke(animationState);

	}
		
	void rotateAvatar(Vector3 lastPosition) {

		Vector3 targetDir = transform.position-lastPosition;

		if (targetDir != Vector3.zero) {
			avatarFigure.transform.rotation = Quaternion.Slerp(
				avatarFigure.transform.rotation,
				Quaternion.LookRotation(targetDir),
				Time.deltaTime * 10.0f
			);
		}
	}

	#endregion
}

[Serializable]
public class GOAvatarAnimationStateEvent : UnityEvent <MoveAvatar.AvatarAnimationState> {


}

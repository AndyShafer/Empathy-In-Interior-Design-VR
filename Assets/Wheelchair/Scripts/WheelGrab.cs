using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelGrab : MonoBehaviour {

	public WheelchairController wheelchair;
	public bool rightSide;
	public float grabBegin = 0.55f;
	public float grabEnd = 0.35f;
	private bool grabbed = false;

	// Update is called once per frame
	void Update () {
		if(rightSide && grabbed){
			// Player released right wheel
			if(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) <= grabEnd){
				print("Right side released");
				grabbed = false;
				wheelchair.rightGrabbed = false;
			}
		}
		else if(!rightSide && grabbed){
			// Player released left wheel
			if(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) <= grabEnd){
				print("Left side released");
				grabbed = false;
				wheelchair.leftGrabbed = false;
			}
		}
	}

	void OnTriggerStay(Collider other) {
		if(rightSide && !grabbed){
			// Player grabbed right wheel
			if(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) >= grabBegin){
				print("Right side grabbed");
				grabbed = true;
				wheelchair.rightGrabbed = true;
			}
		}
		else if(!rightSide && !grabbed){
			// Player grabbed left wheel
			if(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) >= grabBegin){
				print("Left side grabbed");
				grabbed = true;
				wheelchair.leftGrabbed = true;
			}
		}
		//print("Grab zone " + rightSide);
	}
}

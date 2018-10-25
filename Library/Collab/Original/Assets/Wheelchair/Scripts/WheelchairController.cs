using UnityEngine;
using System.Collections;

// Materials with reduced friction are applied to the wheels' colliders to simulate rolling
public class WheelchairController : MonoBehaviour {

	public float speed = 1.2f;
	public float turnSpeed = .1f;
	public float maxVelocityChange = .1f;
	public GameObject leftWheel;
	public GameObject rightWheel;
	public Transform rightPoint;
	public Transform leftPoint;
	public OVRInput.Controller leftController;
	public OVRInput.Controller rightController;
	public bool rightGrabbed = false;
	public bool leftGrabbed = false;

	private float wheelRadius = 0.4f;
	private Rigidbody rb;
	private AudioSource audioSource;
	private bool audioPlaying = false;

	public Vector3 rightWheelCenter;
	public Vector3 rightWheelBottom;
	public Vector3 leftWheelCenter;
	public Vector3 leftWheelBottom;
	public Vector3 frontRightWheelBottom;
	public Vector3 frontLeftWheelBottom;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
		audioSource = GetComponent<AudioSource>();
	}

	// Update is called once per frame
	void Update () {
		CheckBrakes();
		Move();
		RotateWheels();
		AlignToSlope ();
	}

	// Slow down if player grabs wheels
	private void CheckBrakes(){
		if((rightGrabbed && Mathf.Abs(OVRInput.GetLocalControllerVelocity(rightController).z) < .1) || (leftGrabbed && Mathf.Abs(OVRInput.GetLocalControllerVelocity(leftController).z) < .1)){
			rb.drag = 2;
		}
		else{
			rb.drag = 0;
		}
	}

	// Turn the wheelchair
	private void Turn(float horizontalInput){
		// Vector3 rotation = new Vector3(0, horizontalInput * turnSpeed, 0);
		// rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
		rb.AddTorque(transform.up * horizontalInput * turnSpeed, ForceMode.VelocityChange);
	}

	// Move the wheelchair forwards and backwards
	private void Move(){
		float rightVelocity;
		float leftVelocity;

		// Apply input to both wheels
		if(rightGrabbed && leftGrabbed){
			// Calculate how fast right hand is moving
			rightVelocity = OVRInput.GetLocalControllerVelocity(rightController).z;
			// Calculate how fast left hand is moving
			leftVelocity = OVRInput.GetLocalControllerVelocity(leftController).z;

			// If both hands are moving in the same direction
			if((rightVelocity >= .1 && leftVelocity >= .1) || (rightVelocity <= -.1 && leftVelocity <= -.1)){
				// Forward momentum will equal the average velocity of both hands
				Vector3 targetVelocity = new Vector3(0, 0, (rightVelocity + leftVelocity) / 2);
				targetVelocity = transform.TransformDirection(targetVelocity);
				targetVelocity *= speed;

				// Apply a force that attempts to reach our target velocity
				Vector3 velocity = rb.velocity;

				// Don't try and slow down using input in the same direction
				if(velocity.magnitude <= targetVelocity.magnitude){
					Vector3 velocityChange = (targetVelocity - velocity);
					velocityChange.y = 0;
					velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
					velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
					rb.AddForce(velocityChange, ForceMode.VelocityChange);
				}
			}

			// One hand stopped or moving backwards (Turn)
			else if((rightVelocity < .1 && leftVelocity >= .1) || (rightVelocity >= .1 && leftVelocity < .1)){
				// Turn based on the magnitude of the difference in direction
				Turn(leftVelocity - rightVelocity);
			}
		}

		// Only applying input to right wheel
		else if(rightGrabbed && !leftGrabbed){
			// Calculate how fast we should be moving
			rightVelocity = OVRInput.GetLocalControllerVelocity(rightController).z;

			// Translate 1/3 of the velocity into forward motion
			Vector3 targetVelocity = new Vector3(0, 0, rightVelocity * 1/3);
			targetVelocity = transform.TransformDirection(targetVelocity);
			targetVelocity *= speed;

			// Apply a force that attempts to reach our target velocity
			Vector3 velocity = rb.velocity;

			// Don't try and slow down using input in the same direction
			if(velocity.magnitude <= targetVelocity.magnitude){
				Vector3 velocityChange = (targetVelocity - velocity);
				velocityChange.y = 0;
				velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
				velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
				rb.AddForce(velocityChange, ForceMode.VelocityChange);
			}

			// Translate 2/3 of the velocity into turning motion
			Turn(- (rightVelocity * 2/3) );
		}

		// Only applying input to left wheel
		else if(!rightGrabbed && leftGrabbed){
			// Calculate how fast we should be moving
			leftVelocity = OVRInput.GetLocalControllerVelocity(leftController).z;

			// Translate 1/3 of the velocity into forward motion
			Vector3 targetVelocity = new Vector3(0, 0, leftVelocity * 1/3);
			targetVelocity = transform.TransformDirection(targetVelocity);
			targetVelocity *= speed;

			// Apply a force that attempts to reach our target velocity
			Vector3 velocity = rb.velocity;

			// Don't try and slow down using input in the same direction
			if(velocity.magnitude <= targetVelocity.magnitude){
				Vector3 velocityChange = (targetVelocity - velocity);
				velocityChange.y = 0;
				velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
				velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
				rb.AddForce(velocityChange, ForceMode.VelocityChange);
			}

			// Translate 2/3 of the velocity into turning motion
			Turn(leftVelocity * 2/3);
		}
	}

	// Animate the wheels to look like they're turning
	private void RotateWheels(){
		float rightVelocity = rb.GetPointVelocity(rightPoint.position).magnitude;
		float leftVelocity = rb.GetPointVelocity(leftPoint.position).magnitude;

		// Play audio based on wheels rolling
		if(rightVelocity >= .1 || leftVelocity >= .1){
			if(!audioPlaying){
				audioPlaying = true;
				audioSource.Play();
			}
		}
		else{
			audioPlaying = false;
			audioSource.Stop();
		}

		// Rotate both wheels based on each sides speed
		RotateOneWheel(rightPoint);
		RotateOneWheel(leftPoint);
	}

	// Rotate one wheel based on it's velocity
	private void RotateOneWheel(Transform point){
		print(point.position);
		float velocity = rb.GetPointVelocity(point.position).magnitude;

		Vector3 localVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(point.position));

		// Determine direction to "spin" the wheel; 1 is forward, -1 is backward
		float dir;
		if(localVelocity.z < 0){
			// forwards
			dir = 1;
		}
		else {
			// backwards
			dir = -1;
		}

		// Rotate based on speed, wheel radius, and direction
		float degRot = ( (velocity * Time.deltaTime) / wheelRadius) * Mathf.Rad2Deg * dir;
		if(point == leftPoint){
			leftWheel.transform.Rotate(degRot, 0, 0);
		}
		else if(point == rightPoint){
			rightWheel.transform.Rotate(degRot, 0, 0);
		}
	}

	private void AlignToSlope() {
		float wheelRadius = (leftWheelBottom - leftWheelCenter).magnitude;
		float distanceBetweenWheels = (leftWheelCenter - rightWheelCenter).magnitude;
		RaycastHit leftWheelRaycast;
		RaycastHit rightWheelRaycast;
		if (Physics.Raycast (leftWheelCenter, -Vector3.up, out leftWheelRaycast, 100f)) {
			Debug.Log ("raycast hit");
			transform.RotateAround (rightWheelBottom, Vector3.forward, Mathf.Atan ((wheelRadius - leftWheelRaycast.distance) / distanceBetweenWheels));
		} else {
			Debug.Log ("no raycast hit");
		}
		if (Physics.Raycast (rightWheelCenter, -Vector3.up, out rightWheelRaycast)) {
			transform.RotateAround (leftWheelBottom, Vector3.forward, Mathf.Atan ((wheelRadius - rightWheelRaycast.distance) / distanceBetweenWheels) + 10);
		}
	}
}


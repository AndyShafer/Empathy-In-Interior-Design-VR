using UnityEngine;
using System.Collections;

// Materials with reduced friction are applied to the wheels' colliders to simulate rolling
public class WheelchairController : MonoBehaviour {

	public float speed = 0.8f;
	public float turnSpeed = 0.4f;
	public float maxVelocityChange = .1f;
	public GameObject leftWheel;
	public GameObject rightWheel;
	public Transform rightPoint;
	public Transform leftPoint;
	public Transform centerPoint;
	public OVRInput.Controller leftController;
	public OVRInput.Controller rightController;
	public bool rightGrabbed = false;
	public bool leftGrabbed = false;

	private float wheelRadius = 0.4f;
	private Rigidbody rb;
	private AudioSource audioSource;
	private bool audioPlaying = false;

    public bool align = true;
    public Vector3 rightWheelCenter;
	public Vector3 rightWheelBottom;
	public Vector3 leftWheelCenter;
	public Vector3 leftWheelBottom;
	public Vector3 frontRightWheelBottom;
	public Vector3 frontLeftWheelBottom;

    private const float RAYCAST_HEIGHT = .1f;
    private const int LAYER_MASK = ~((1 << 10) | (1 << 9)); // Ignore the wheelchair when raycasting.

    // Use this for initialization
    void Start () {
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
		audioSource = GetComponent<AudioSource>();
		rb.centerOfMass = centerPoint.localPosition;
	}

	// Update is called once per frame
	void Update () {
		CheckBrakes();
		// Turn(Input.GetAxis("Horizontal"));
		Move();
		RotateWheels();
        if(align)
		    AlignToSlope();
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
		Vector3 rotation = new Vector3(0, horizontalInput * turnSpeed, 0);
		rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));

		// rb.AddTorque(transform.up * horizontalInput * turnSpeed, ForceMode.VelocityChange);

		// Vector3 vel = new Vector3(0, horizontalInput * turnSpeed, 0);
		// print(vel);
		// rb.angularVelocity = vel;
		// print(rb.angularVelocity);
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
				// Set center of mass based on if one side is not moving
				if(Mathf.Abs(rightVelocity) < .1){
					rb.centerOfMass = rightPoint.localPosition;
				}
				else if(Mathf.Abs(leftVelocity) < .1){
					rb.centerOfMass = leftPoint.localPosition;
				}
				else{
					rb.centerOfMass = centerPoint.localPosition;
				}

				// Turn based on the magnitude of the difference in direction
				Turn(leftVelocity - rightVelocity);
			}
		}

		// Only applying input to right wheel
		else if(rightGrabbed && !leftGrabbed){
			// Calculate how fast we should be moving
			rightVelocity = OVRInput.GetLocalControllerVelocity(rightController).z;

			// Translate 1/3 of the velocity into forward motion
			Vector3 targetVelocity = new Vector3(0, 0, rightVelocity * 2/3);
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

			// Translate 2/3 of the velocity into turning motion and shift centerOfMass
			rb.centerOfMass = leftPoint.localPosition;
			Turn(- (rightVelocity * 2/3) );
		}

		// Only applying input to left wheel
		else if(!rightGrabbed && leftGrabbed){
			// Calculate how fast we should be moving
			leftVelocity = OVRInput.GetLocalControllerVelocity(leftController).z;

			// Translate 1/3 of the velocity into forward motion
			Vector3 targetVelocity = new Vector3(0, 0, leftVelocity * 2/3);
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

			// Translate 2/3 of the velocity into turning motion and shift centerOfMass
			rb.centerOfMass = rightPoint.localPosition;
			Turn(leftVelocity * 2/3);
		}
	}

	// Animate the wheels to look like they're turning
	private void RotateWheels(){
		float rightVelocity = rb.GetPointVelocity(rightPoint.localPosition).magnitude;
		float leftVelocity = rb.GetPointVelocity(leftPoint.localPosition).magnitude;

		// Play audio based on wheels rolling
		if(rightVelocity >= .1 || leftVelocity >= .1){
			if(!audioPlaying){
				audioPlaying = true;
				audioSource.Play();
			}
		}
		else{
			audioPlaying = false;
			audioSource.Pause();
		}

		// Rotate both wheels based on each sides speed
		RotateOneWheel(rightPoint);
		RotateOneWheel(leftPoint);
	}

	// Rotate one wheel based on it's velocity
	private void RotateOneWheel(Transform point){
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
        // Note: raycasts will detect any colliders that are not in the WheelGrab or Wheelchair layer, even if it is part of the WheelchairCharacter.
        float wheelRadius = (leftWheelBottom - leftWheelCenter).magnitude;
        float distanceBetweenWheels = (leftWheelCenter - rightWheelCenter).magnitude;
        float distanceToFrontWheels = Vector3.Dot((frontLeftWheelBottom - leftWheelBottom), transform.TransformDirection(Vector3.forward));
        RaycastHit leftWheelRaycast;
        RaycastHit rightWheelRaycast;
        RaycastHit frontRaycast;
        RaycastHit backRaycast;
        // Raycast left and right wheel and rotate to align to the surface.
        if (Physics.Raycast(transform.TransformPoint(leftWheelCenter), leftWheelBottom - leftWheelCenter, out leftWheelRaycast, 2f, LAYER_MASK))
        {
            transform.RotateAround(transform.TransformPoint(rightWheelBottom), transform.TransformDirection(Vector3.forward), -Mathf.Atan((wheelRadius - leftWheelRaycast.distance) / distanceBetweenWheels));
        }
        if (Physics.Raycast(transform.TransformPoint(rightWheelCenter), rightWheelBottom - rightWheelCenter, out rightWheelRaycast, 2f, LAYER_MASK))
        {
            transform.RotateAround(transform.TransformPoint(leftWheelBottom), transform.TransformDirection(Vector3.forward), Mathf.Atan((wheelRadius - rightWheelRaycast.distance) / distanceBetweenWheels));
        }

        // Raycast front and back and rotate to align to the surface.
        if (Physics.Raycast((transform.TransformPoint(frontLeftWheelBottom) + transform.TransformPoint(frontRightWheelBottom)) / 2 + transform.TransformDirection(Vector3.up) * RAYCAST_HEIGHT, transform.TransformDirection(Vector3.down), out frontRaycast, RAYCAST_HEIGHT + 2f, LAYER_MASK))
        {
            transform.RotateAround(transform.TransformPoint(leftWheelBottom), transform.TransformPoint(rightWheelBottom) - transform.TransformPoint(leftWheelBottom), -Mathf.Atan((RAYCAST_HEIGHT - frontRaycast.distance) / distanceToFrontWheels));
        }
        if (Physics.Raycast((transform.TransformPoint(leftWheelCenter) + transform.TransformPoint(rightWheelCenter)) / 2, transform.TransformDirection(Vector3.down), out backRaycast, 2f, LAYER_MASK))
        {
            transform.RotateAround(transform.TransformPoint(frontLeftWheelBottom), transform.TransformPoint(frontRightWheelBottom) - transform.TransformPoint(frontLeftWheelBottom), Mathf.Atan((wheelRadius - backRaycast.distance) / distanceToFrontWheels));
        }
    }
}


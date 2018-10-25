using UnityEngine;
using System.Collections;

// Materials with reduced friction are applied to the wheels' colliders to simulate rolling
public class WheelchairController_NoVR : MonoBehaviour
{

    public float speed = 1f;
    public float turnSpeed = 1f;
    public float maxVelocityChange = 10.0f;
    public GameObject leftWheel;
    public GameObject rightWheel;
    public OVRInput.Controller leftController;
    public OVRInput.Controller rightController;
    public bool rightGrabbed = false;
    public bool leftGrabbed = false;

    private float wheelRadius = 0.4f;
    private CharacterController cc;
    private Rigidbody rb;

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
    void Start()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO move input and turning to directions touch controllers are moved in while gripping

        CheckBrakes();
        Turn();
        Move();
        RotateWheels();
        if(align)
            AlignToSlope();
    }

    // TODO If player is squeezing grips on wheels, increase drag to slow down
    private void CheckBrakes()
    {
        if (Input.GetKey("space"))
        {
            rb.drag = 2;
        }
        else
        {
            rb.drag = 0;
        }
    }

    // Turn the wheelchair
    private void Turn()
    {
        transform.Rotate(0, Input.GetAxis("Horizontal") * turnSpeed, 0);
    }

    // Move the wheelchair forwards and backwards
    private void Move()
    {
        // ** Only run this section if the user is providing input (so that he keeps rolling otherwise)
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(0, 0, Input.GetAxis("Vertical"));
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= speed;

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rb.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.y = 0;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }

    // Animate the wheels to look like they're turning
    private void RotateWheels()
    {

        float velocity = rb.velocity.magnitude;

        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

        // Determine direction to "spin" the wheel; 1 is forward, -1 is backward
        float dir;
        if (localVelocity.z < 0)
        {
            // forwards
            dir = 1;
        }
        else
        {
            // backwards
            dir = -1;
        }

        // Rotate based on speed, wheel radius, and direction
        float degRot = ((velocity * Time.deltaTime) / wheelRadius) * Mathf.Rad2Deg * dir;
        leftWheel.transform.Rotate(degRot, 0, 0);
        rightWheel.transform.Rotate(degRot, 0, 0);
    }
    private void AlignToSlope()
    {
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

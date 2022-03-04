using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum UserSubActions
{
    No,
    ChangingOrientation
}

[RequireComponent(typeof(LightEntityPhysics))]
public class PlayerController : MonoBehaviour
{
    public XYZNavController normalHelper;
    public GameObject testSphere;

    public Camera camera;
    public Transform head;
    public Transform body;
    public Transform surfaceHelper;

    public float changeOrientationSpeed = 1f;

    public float radius = 0.5f;
    public float height = 2;
    public int mouseSensitivity = 10;
    public float jumpHeight = 5.0f;

    public Vector3 gravityVector = new Vector3(0, -1, 0);
    public bool applyGravityByTransformRotation = true;


    [Header("Physics")]
    public LayerMask discludePlayer;
    private LightEntityPhysics physics;


    private Vector3 localVelocity;
    private Vector3 gravityVelocity;
    private Vector3 playerVelocity;
    private Vector3 surfaceNormal;

    private bool groundedPlayer;
    private float playerSpeed = 10.0f;
    private float gravityValue = 9.81f;
    private float gravityValueFrameScale = 0.01f;
    private float yaw = 0f;
    private float pitch = 0.0f;

    private bool isNormalSelectionMode = false;

    private Quaternion targetHeadLocalRotation;
    private Quaternion targetModelRotation;

    private UserSubActions userSubActions = UserSubActions.No;

    private Vector3 lastSelectedNormal;
    private Vector3 lastSelectedPoint;

    private CapsuleCollider capsuleCol;


    void Start()
    {
        body = transform;
        physics = GetComponent<LightEntityPhysics>();
        capsuleCol = GetComponent<CapsuleCollider>();

        if (applyGravityByTransformRotation) gravityVector = -transform.up;
    }

    void Update()
    {
        if(userSubActions == UserSubActions.No)
        {
            yaw = mouseSensitivity * Input.GetAxis("Mouse X");
            pitch = -mouseSensitivity * Input.GetAxis("Mouse Y");

            transform.Rotate(0, yaw * Time.deltaTime, 0);
            head.Rotate(pitch * Time.deltaTime, 0, 0);
        }

        groundedPlayer = isGrounded();

        Vector3 move = Vector3.zero;

        if (userSubActions == UserSubActions.No)
        {
            move = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")) * playerSpeed;

            move = correctVelocityBySlope(move);
        }

        gravityVelocity += gravityVector * gravityValue * Time.deltaTime;
        Vector3 localGravityVelocity = transform.InverseTransformVector(gravityVelocity);

        if (groundedPlayer && localGravityVelocity.y < 0)
        {
            localGravityVelocity.y = 0f;
            gravityVelocity = transform.TransformVector(localGravityVelocity);
        }

        if (userSubActions == UserSubActions.No)
        {

            if (Input.GetButtonDown("Jump") && groundedPlayer)
            {
                gravityVelocity += gravityVector * -Mathf.Sqrt(jumpHeight * 3.0f * gravityValue);
            }

        }

        //Debug.Log(gravityVelocity);

        playerVelocity = move + gravityVelocity;

        //playerVelocity = correctVelocityBySlope(playerVelocity);

        playerVelocity = playerVelocity * Time.deltaTime;

        Move(playerVelocity);


        if (Input.GetKeyDown(KeyCode.Q))
        {
            isNormalSelectionMode = true;
            normalHelper.show();
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            isNormalSelectionMode = false;
            ActionsByNormalSelection();
            normalHelper.hide();

            lastSelectedNormal = Vector3.zero;
        }


        NormalSelection();
        SubActionsDeals();
    }

    void Move(Vector3 moveVector)
    {
        transform.position += moveVector;
        CollisionCheck();

    }

    private void CollisionCheck()
    {
        Collider[] overlaps = new Collider[4];
        var capsulePoints = Utils.getCapsuleColliderPoints(capsuleCol);

        int num = Physics.OverlapCapsuleNonAlloc(transform.TransformPoint(capsulePoints.point0), transform.TransformPoint(capsulePoints.point1), capsuleCol.radius, overlaps, discludePlayer, QueryTriggerInteraction.UseGlobal);

        for (int i = 0; i < num; i++)
        {

            Transform t = overlaps[i].transform;
            Vector3 dir;
            float dist;

            if (Physics.ComputePenetration(capsuleCol, transform.position, transform.rotation, overlaps[i], t.position, t.rotation, out dir, out dist))
            {
                Vector3 penetrationVector = dir * dist;
                Vector3 velocityProjected = Vector3.Project(gravityVelocity, -dir);
                transform.position = transform.position + penetrationVector;
                //vel -= velocityProjected;
            }

        }
    }

    void SubActionsDeals()
    {
        if(userSubActions == UserSubActions.ChangingOrientation)
        {

            transform.rotation = Quaternion.Slerp(body.localRotation, targetModelRotation, Time.deltaTime * changeOrientationSpeed);
            head.localRotation = Quaternion.Slerp(head.localRotation, targetHeadLocalRotation, Time.deltaTime * changeOrientationSpeed);
            gravityVelocity = Vector3.zero;

            if (
                Quaternion.Angle(transform.rotation, targetModelRotation) < 1 &&
                Quaternion.Angle(head.localRotation, targetHeadLocalRotation) < 1)
            {
                transform.rotation = targetModelRotation;
                head.localRotation = targetHeadLocalRotation;
                userSubActions = UserSubActions.No;
            }
        }
    }

    private void ActionsByNormalSelection()
    {
        SetNewGravityVector(lastSelectedNormal*-1);
    }

    private void NormalSelection()
    {

        if (isNormalSelectionMode)
        {
            RaycastHit hit;
            Ray ray = new Ray(camera.transform.position + camera.transform.forward * 2, camera.transform.forward);

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 incomingVec = hit.point - camera.transform.position;

                lastSelectedNormal = hit.normal;
                lastSelectedPoint = hit.point;

                //testSphere.transform.position = lastSelectedPoint;

                normalHelper.setTarget(lastSelectedPoint, lastSelectedNormal);

                Debug.DrawLine(camera.transform.position, lastSelectedPoint, Color.red);
                Debug.DrawRay(lastSelectedPoint, lastSelectedNormal, Color.green);
            }
        }
    }


    private void SetNewGravityVector(Vector3 newGravityVector)
    {
        Quaternion startModelRoation = transform.rotation;
        Vector3 newSurfaceNormal = -newGravityVector;

        transform.rotation = Quaternion.FromToRotation(Vector3.up, newSurfaceNormal);

        Vector3 lastPointLocalDirection = transform.InverseTransformPoint(lastSelectedPoint);

        float angle = Mathf.Atan2(lastPointLocalDirection.z, lastPointLocalDirection.x) * Mathf.Rad2Deg - 90;
        Quaternion modelRotation = transform.rotation * Quaternion.Euler(0f, -angle, 0f);

        transform.rotation = modelRotation;

        Vector3 headLocalPosition = transform.InverseTransformPoint(lastSelectedPoint);
        float angleCam = Mathf.Atan2(headLocalPosition.y, headLocalPosition.z) * Mathf.Rad2Deg - 90;


        Quaternion headRotation = Quaternion.Euler(- angleCam - 90, 0f, 0f);

        transform.rotation = startModelRoation;

        targetHeadLocalRotation = headRotation;
        targetModelRotation = modelRotation;


        gravityVelocity = Vector3.zero;

        gravityVector = newGravityVector;

        userSubActions = UserSubActions.ChangingOrientation;

    }
    void OnDrawGizmosSelected()
    {
        var capsulePoints = Utils.getCapsuleColliderPoints(capsuleCol);
        Ray ray = new Ray(transform.TransformPoint(capsulePoints.point0), -transform.up);
        RaycastHit tempHit = new RaycastHit();

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(capsulePoints.point0), capsuleCol.radius + 0.0f);
    }

    private Vector3 correctVelocityBySlope(Vector3 moveValue)
    {
        Vector3 vVelocity = new Vector3(moveValue.x, moveValue.y, moveValue.z);
        Vector3 newSurfaceNormal = -gravityVector;


        if (surfaceNormal != Vector3.zero && newSurfaceNormal != surfaceNormal)
        {
            Quaternion surfaceAngle = Quaternion.FromToRotation(transform.up, surfaceNormal);
            Vector3 slopeMovement = surfaceAngle * new Vector3(vVelocity.x, vVelocity.y, vVelocity.z);

            return slopeMovement;
        }

        return moveValue;
    }

    private bool isGrounded()  
    {
        var capsulePoints = Utils.getCapsuleColliderPoints(capsuleCol);
        RaycastHit hit;

        if (Physics.SphereCast(transform.TransformPoint(capsulePoints.point0), capsuleCol.radius - 0.01f, -transform.up, out hit, 0.09f))
        {
            surfaceNormal = hit.normal;
            surfaceHelper.rotation = Quaternion.LookRotation(surfaceNormal, transform.forward);

            if (-gravityVector == surfaceNormal)
            {
                Vector3 center = hit.point + capsuleCol.radius * hit.normal;
                transform.position = center + transform.up * 0.53f;
            }

            return true;
        }
        else
        {
            surfaceNormal = Vector3.zero;
            return false;
        }
    }

    public static float AngleOffAroundAxis(Vector3 v, Vector3 forward, Vector3 axis)
    {
        Vector3 right = Vector3.Cross(axis, forward).normalized;
        forward = Vector3.Cross(right, axis).normalized;
        return Mathf.Atan2(Vector3.Dot(v, right), Vector3.Dot(v, forward)) * (180 / Mathf.PI);
    }
}

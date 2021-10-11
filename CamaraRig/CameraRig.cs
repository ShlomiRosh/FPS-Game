using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraRig : MonoBehaviour
{
    // This class holder the cam settings
    #region ClassCameraSettings
    [System.Serializable]
    public class CameraSettings
    {
        [Header("-Positioning-")]
        public Vector3 camPositionOffsetLeft;
        public Vector3 camPositionOffsetRight;

        [Header("-Camera Options-")]
        public Camera UICamera;
        public float mouseXSensitivity = 1.0f;
        public float mouseYSensitivity = 1.0f;
        public float minAngle = -30.0f;
        public float maxAngle = 70.0f;
        public float rotationSpeed = 5.0f;
        public float maxCheckDist = 0.1f;

        [Header("-Zoom-")]
        public float fieldOfView = 70.0f;
        public float zoomFieldOfView = 30.0f;
        public float zoomSpeed = 3.0f;

        [Header("-Visual Options-")]
        public float hideMeshWhenDistance = 0.5f;
    }
    #endregion ClassCameraSettings
    // This class holder the input settings [input from user]
    #region ClassInputSettings
    [System.Serializable]
    public class InputSettings
    {
        public string verticalAxis = "Mouse X"; // "Vertical"
        public string horizontalAxis = "Mouse Y"; // "Horizontal"
        public string aimButton = "Fire2"; // The key Left alt
        public string switchShoulderButton = "Fire4"; // The v key
    }
    #endregion ClassInputSettings
    // This class holder the movments settings [how fast we move the camara]
    #region ClassMovementSettings
    [System.Serializable]
    public class MovementSettings
    {
        public float movementLerpSpeed = 5.0f;
    }
    #endregion ClassMovementSettings

    #region ClassCameraRig Variables // This Class
    public enum Shoulder
    {
        Right, Left
    }
    public Shoulder shoulder;
    public Transform target;
    public bool autoTargetPlayer;
    public LayerMask wallLayers;
    [SerializeField] public CameraSettings cameraSettings;
    [SerializeField] public InputSettings input;
    [SerializeField] public MovementSettings movement;
    float newX = 0.0f; // Privet var
    float newY = 0.0f; // Privet var
    #endregion ClassCamaraRig Variables

    #region ClassCameraRig Properties // This Class
    public Camera mainCamera { get; protected set; }
    public Transform pivot { get; set; }
    #endregion ClassCamaraRig Properties

    #region ClassCameraRig Functions // This Class
    void Start() // Start is called before the first frame update
    {
        mainCamera = Camera.main;
        pivot = transform.GetChild(0);
    }

    void Update() // Update is called once per frame
    {
        if (target)
        {
            if (Application.isPlaying)
            {
                RotateCamera(); // Update new rotation
                CheckWall(); // Chack for walls behind the camara
                CheckMeshRenderer(); // Hide the meshes whan we to close to the target
                Zoom(Input.GetButton(input.aimButton)); // Zoom camara in and out
                if (Input.GetButtonDown(input.switchShoulderButton)) // The v key
                {
                    SwitchShoulders();
                }
            }
        }
    }

    void LateUpdate() // Hadle camara to Follow the player
    {
        if (!target)
        {
            TargetPlayer();
        }
        else
        {
            Vector3 targetPostion = target.position;
            Quaternion targetRotation = target.rotation;
            FollowTarget(targetPostion, targetRotation);
        }
    }

    void TargetPlayer() // Finds the plater gameObject and sets it as target
    {
        if (autoTargetPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                Transform playerT = player.transform;
                target = playerT;
            }
        }
    }

    void FollowTarget(Vector3 targetPosition, Quaternion targetRotation) // Following the target with Time.deltaTime smoothly
    {
        if (!Application.isPlaying)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
        else
        {
            Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, 
                Time.deltaTime * movement.movementLerpSpeed);
            transform.position = newPos;
        }
    }

    void RotateCamera() //Rotates the camera with input
    {
        if (!pivot)
            return;
        newX += cameraSettings.mouseXSensitivity * Input.GetAxis(input.verticalAxis); // That will by our new vertical view
        newY += cameraSettings.mouseYSensitivity * Input.GetAxis(input.horizontalAxis); // That will by our new horizontal view
        Vector3 eulerAngleAxis = new Vector3();
        eulerAngleAxis.x = newY; // Invert the x value for unity
        eulerAngleAxis.y = newX; // Invert the x value for unity
        newX = Mathf.Repeat(newX, 360); // Whan we hit 360 in x angel we Back to 0 
        newY = Mathf.Clamp(newY, cameraSettings.minAngle, cameraSettings.maxAngle); //  Whan we hit MAX/MIN in y angel we Clamp the camara
        Quaternion newRotation = Quaternion.Slerp(pivot.localRotation,
            Quaternion.Euler(eulerAngleAxis), Time.deltaTime * cameraSettings.rotationSpeed); // Set new rotations
        pivot.localRotation = newRotation;
    }

    void CheckWall() // Checks the wall and moves the camera up if we hit
    {
        if (!pivot || !mainCamera)
            return;
        RaycastHit hit;
        Transform mainCamT = mainCamera.transform;
        Vector3 mainCamPos = mainCamT.position;
        Vector3 pivotPos = pivot.position;
        Vector3 start = pivotPos;
        Vector3 dir = mainCamPos - pivotPos;
        float dist = Mathf.Abs(shoulder == Shoulder.Left ? cameraSettings.camPositionOffsetLeft.z
            : cameraSettings.camPositionOffsetRight.z);
        if (Physics.SphereCast(start, cameraSettings.maxCheckDist, dir, out hit, dist, wallLayers))
        {
            MoveCamUp(hit, pivotPos, dir, mainCamT);
        }
        else
        {
            switch (shoulder)
            {
                case Shoulder.Left:
                    PostionCamera(cameraSettings.camPositionOffsetLeft);
                    break;
                case Shoulder.Right:
                    PostionCamera(cameraSettings.camPositionOffsetRight);
                    break;
            }
        }
    }

    void MoveCamUp(RaycastHit hit, Vector3 pivotPos, Vector3 dir, Transform cameraT) // This moves the camera forward when we hit a wall
    {
        float hitDist = hit.distance;
        Vector3 sphereCastCenter = pivotPos + (dir.normalized * hitDist);
        cameraT.position = sphereCastCenter;
    }

    void PostionCamera(Vector3 cameraPos) // Postions the cameras localPosition to a given location
    {
        if (!mainCamera)
            return;
        Transform mainCamT = mainCamera.transform;
        Vector3 mainCamPos = mainCamT.localPosition;
        Vector3 newPos = Vector3.Lerp(mainCamPos, cameraPos, Time.deltaTime * movement.movementLerpSpeed);
        mainCamT.localPosition = newPos;
    }
 
    void CheckMeshRenderer() // Hides the mesh targets mesh renderers when too close
    {
        if (!mainCamera || !target)
            return;
        SkinnedMeshRenderer[] meshes = target.GetComponentsInChildren<SkinnedMeshRenderer>();
        Transform mainCamT = mainCamera.transform;
        Vector3 mainCamPos = mainCamT.position;
        Vector3 targetPos = target.position;
        float dist = Vector3.Distance(mainCamPos, (targetPos + target.up));
        if (meshes.Length > 0)
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                if (dist <= cameraSettings.hideMeshWhenDistance)
                {
                    meshes[i].enabled = false;
                }
                else
                {
                    meshes[i].enabled = true;
                }
            }
        }
    }

    void Zoom(bool isZooming) // Zooms the camera in and out
    {
        if (!mainCamera)
            return;
        if (isZooming)
        {
            float newFieldOfView = Mathf.Lerp(mainCamera.fieldOfView,
                cameraSettings.zoomFieldOfView, Time.deltaTime * cameraSettings.zoomSpeed);
            mainCamera.fieldOfView = newFieldOfView;
            if (cameraSettings.UICamera != null)
            {
                cameraSettings.UICamera.fieldOfView = newFieldOfView;
            }
        }
        else
        {
            float originalFieldOfView = Mathf.Lerp(mainCamera.fieldOfView,
                cameraSettings.fieldOfView, Time.deltaTime * cameraSettings.zoomSpeed);
            mainCamera.fieldOfView = originalFieldOfView;
            if (cameraSettings.UICamera != null)
            {
                cameraSettings.UICamera.fieldOfView = originalFieldOfView;
            }
        }
    }

    public void SwitchShoulders() // Switches the cameras shoulder view
    {
        switch (shoulder)
        {
            case Shoulder.Left:
                shoulder = Shoulder.Right;
                break;
            case Shoulder.Right:
                shoulder = Shoulder.Left;
                break;
        }
    }
    #endregion ClassCamaraRig Functions
}

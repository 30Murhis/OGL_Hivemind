using System.Collections;
using UnityEngine;

/// <summary>
/// Camera controller for 2D looping view camera rig.
/// <para>Controls two cameras to create an illusion of looping and infinite 2D scene.</para>
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("References")]
    [Tooltip("Background object, from which the distance between the two cameras can be retrieved.")]
    public GameObject background;
    [Tooltip("The object the camera is following.")]
    public Transform target;

    [Tooltip("The main camera object.")]
    public Transform mainCamera;
    [Tooltip("The support camera object.")]
    public Transform supportCamera;

    [Header("Camera Offsets")]
    [Tooltip("Constant x-offset. NOTE: not implemented completely.")]
    public float offsetX = 0;
    [Tooltip("Constant y-offset. Calculated to be half of the target's height.")]
    public float offsetY = 0;
    [Tooltip("Constant z-offset. Set to -10, because that's the default camera z-offset in 2D view.")]
    public float offsetZ = -10;
    [Tooltip("Running x-offset.")]
    public float offsetXForRun = 5f;

    [Header("Camera Zoom")]
    [Tooltip("Default zoom level for normal situations.")]
    public float normalZoomLevel = 9.5f;
    [Tooltip("Run zoom level, towards which the camera goes when running.")]
    public float runZoomLevel = 14f;
    [Tooltip("Determines how much zoom level changes every 10ms.")]
    public float zoomRate = 0.3f;
    
    [Header("Other")]
    [Tooltip("Camera's movement speed, which determines how fast it reaches CameraTargetPosition")]
    [Range(0f, 100f)]
    public float normalCameraMoveSpeed = 25.0f;
    [Tooltip("Maximum camera speed multiplier, which activates when distance to target is greater than run offset. Used to move camera faster the further the target is.")]
    [Range(1f, 10f)]
    public float maxCameraMoveSpeedMultiplier = 5.0f;
    [Tooltip("The actual position the camera is moving towards.")]
    public Vector3 cameraTargetPosition;

    float offsetXRunCurrent = 0;
    float backgroundWidth;
    float cameraMoveSpeedMultiplier = 1f;
    CameraState currentCameraState;
    RunCameraZoomState runCameraZoomState;
    IEnumerator zoomCoroutine;

    public enum CameraState
    {
        Idle,
        Walk,
        Run,
        MissingTarget
    }

    enum RunCameraZoomState
    {
        Inactive,
        ZoomingIn,
        ZoomingOut
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // If main camera not set, gets it
        if (!mainCamera) mainCamera = Camera.main.transform;

        // If support camera not set, tries to find it from children
        if (!supportCamera) // supportCamera = transform.GetChild(1);
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<Camera>() && child != Camera.main.transform)
                {
                    supportCamera = child;
                    break;
                }
            }
        }

        // If background object not set, finds one
        if (background == null) background = FindObjectOfType<BackgroundGenerator>().gameObject;

        // Gets map's width, which is used to set support camera position
        SetSupportCameraDistance();

        // Set the main camera to center and the support camera to the side
        mainCamera.localPosition = new Vector3(0, mainCamera.localPosition.y, offsetZ);
        supportCamera.localPosition = new Vector3(backgroundWidth, supportCamera.localPosition.y, offsetZ);

        // If target is set, sets target to parent object
        if (target)
            transform.SetParent(target);

        // Set zoom levels to normal zoom levels
        mainCamera.GetComponent<Camera>().orthographicSize = normalZoomLevel;
        supportCamera.GetComponent<Camera>().orthographicSize = normalZoomLevel;

        // Subscribe to character manager's character events
        CharacterManager.OnCharacterChange += CharacterManager_OnCharacterChange;
        CharacterManager.OnCharacterDeath += CharacterManager_OnCharacterDeath;

        // Set default camera state and zoom state
        currentCameraState = CameraState.Idle;
        runCameraZoomState = RunCameraZoomState.Inactive;
    }

    /// <summary>
    /// Update camera positions in late update just to make sure the target object's positions are updated first.
    /// </summary>
    void LateUpdate()
    {
        if (background == null)
        {
            background = FindObjectOfType<BackgroundGenerator>().gameObject;
            SetSupportCameraDistance();
        }

        CheckForTarget();

        MoveCamera();

        // Sets the support camera's x-position to the opposite side of the map.
        if (Mathf.Sign(transform.position.x) == Mathf.Sign(supportCamera.localPosition.x))
        {
            supportCamera.localPosition = new Vector3(supportCamera.localPosition.x * -1, supportCamera.localPosition.y, supportCamera.localPosition.z);
        }
    }

    /// <summary>
    /// Moves camera towards camera target position based on current CameraState.
    /// </summary>
    void MoveCamera()
    {
        switch (currentCameraState)
        {
            // Idle and Walk behaviour are the same, at least for now.
            case CameraState.Idle:
            case CameraState.Walk:
                cameraTargetPosition = target.position;
                break;
            case CameraState.Run:
                cameraTargetPosition = new Vector3(target.position.x, target.position.y, target.position.z);
                cameraTargetPosition.x += offsetXRunCurrent;
                break;
            case CameraState.MissingTarget:
                ChangeZoomLevelWithSteps(0.5f * Time.deltaTime, false, 20f);
                return;
        }

        // Set/add constant offsets
        cameraTargetPosition.x += offsetX;
        cameraTargetPosition.y += offsetY;
        cameraTargetPosition.z = offsetZ; // z offset is not added, instead it is always kept at certain point

        // Calculate x axis distance to target
        float distance = Mathf.Abs(cameraTargetPosition.x - transform.position.x);

        // Set camera speed multiplier to a value based on distance, if the distance is outside the run offset range
        //cameraMoveSpeedMultiplier = distance > offsetXForRun ? Mathf.Clamp(distance, 1f, maxCameraMoveSpeedMultiplier) : 1f;

        // If distance is outside the run offset range, sets multiplier to distance clamped between 1 and given maximum multiplier
        if (distance > offsetXForRun * 2)
            cameraMoveSpeedMultiplier = Mathf.Clamp(distance, 1f, maxCameraMoveSpeedMultiplier);
        else
            cameraMoveSpeedMultiplier = 1f;

        // Move the camera rig towards the camera target position.
        // Because the main camera's local position is at (0,0,0),
        // this keeps the main camera at the target position.
        transform.position = Vector3.MoveTowards(transform.position, cameraTargetPosition, normalCameraMoveSpeed * Time.deltaTime * cameraMoveSpeedMultiplier);
    }

    /// <summary>
    /// If character death event occurs, stops being that character's child.
    /// </summary>
    /// <param name="entityData"></param>
    void CharacterManager_OnCharacterDeath(EntityData entityData)
    {
        if (target.GetComponent<Entity>().entityData == entityData)
        {
            target = null;
            transform.parent = null;
        }
    }

    /// <summary>
    /// When character change event occurs, changes camera target.
    /// </summary>
    void CharacterManager_OnCharacterChange()
    {
        ChangeTarget(CharacterManager.GetCurrentCharacterObject());
    }

    /// <summary>
    /// Checks if target is set and accessible. If not, tries to get a new target.
    /// <para>If target cannot be set or found, sets camera state to MissingTarget.</para>
    /// </summary>
    void CheckForTarget()
    {
        if (!target || !target.gameObject.activeInHierarchy)
        {
            GameObject newTarget = CharacterManager.GetCurrentCharacterObject();
            
            if (newTarget)
            {
                ChangeTarget(newTarget);
            }
        }
        
        if (!target)
        {
            currentCameraState = CameraState.MissingTarget;
        }
    }

    /// <summary>
    /// Sets width, which is used to set support camera's distance from the main camera.
    /// </summary>
    void SetSupportCameraDistance()
    {
        // If width is obtainable from BackgroundGenerator, gets it
        BackgroundGenerator bgGen = FindObjectOfType<BackgroundGenerator>();
        if (bgGen && bgGen.GetBackgroundWidth() > 0)
        {
            backgroundWidth = bgGen.GetBackgroundWidth();
            return;
        }
        
        // If width is still not set, calculates it from backgrounds child's
        for (int i = 0; i < background.transform.childCount; i++)
        {
            if (background.transform.GetChild(i).name.Contains("Background"))
                backgroundWidth += background.transform.GetChild(i).GetComponent<SpriteRenderer>().bounds.size.x;
        }
    }

    /// <summary>
    /// Sets camera's target position to new position.
    /// </summary>
    /// <param name="position">New camera target position.</param>
    void SetCameraTargetPosition(Vector3 position)
    {
        cameraTargetPosition = position;
    }

    /// <summary>
    /// Set current camera state to a new one.
    /// </summary>
    /// <param name="newState">New state for the camera.</param>
    public void SetCameraState(CameraState newState)
    {
        currentCameraState = newState;
    }

    /// <summary>
    /// Changes target object of the camera to follow.
    /// </summary>
    /// <param name="target">Target object.</param>
    public void ChangeTarget(GameObject target)
    {
        if (!target)
        {
            transform.SetParent(null);
            SetCameraState(CameraState.MissingTarget);
            return;
        }

        this.target = target.transform;
        transform.SetParent(this.target);
        offsetY = CharacterManager.GetCurrentCharacterHeight() / 2;
    }

    /// <summary>
    /// Changes x-axis' run offset for the camera towards wanted direction.
    /// </summary>
    /// <param name="direction">Chosen direction. -1 => left, 1 => right in x-axis.</param>
    public void SetRunXOffset(int direction)
    {
        offsetXRunCurrent = Mathf.Sign(direction) * offsetXForRun;
    }

    /// <summary>
    /// Activates/deactivates run camera with its zoom levels and offsets.
    /// </summary>
    /// <param name="activate">Activate run camera.</param>
    /// <param name="direction">Movement direction. Required if value = true.</param>
    public void ActivateRunCamera(bool activate, int direction = 0)
    {
        if (activate && direction == 0)
        {
            //Debug.LogWarning("Run camera activation failed; direction set to 0, when it needs to be something else.", gameObject);
            return;
        }

        if (activate)
        {
            if (runCameraZoomState != RunCameraZoomState.ZoomingOut)
            {
                EndPreviousZoomCoroutine();
                zoomCoroutine = ChangeZoomLevel(runZoomLevel);
                StartCoroutine(zoomCoroutine);
            }

            SetRunXOffset(direction);
            currentCameraState = CameraState.Run;
        }
        else
        {
            if (runCameraZoomState != RunCameraZoomState.ZoomingIn)
            {
                EndPreviousZoomCoroutine();
                zoomCoroutine = ChangeZoomLevel(normalZoomLevel);
                StartCoroutine(zoomCoroutine);
            }

            currentCameraState = CameraState.Walk;
        }
    }

    /// <summary>
    /// If a zoom coroutine is running, stops it and sets run camera zoom state to inactive.
    /// </summary>
    void EndPreviousZoomCoroutine()
    {
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            runCameraZoomState = RunCameraZoomState.Inactive;
        }
    }

    /// <summary>
    /// Changes camera zoom level over time to target value.
    /// <para>Zoom level is set by changing the size of orthographic camera.</para>
    /// </summary>
    /// <param name="targetValue">New target value of the zoom level.</param>
    /// <returns></returns>
    IEnumerator ChangeZoomLevel(float targetValue)
    {
        float beginningValue = mainCamera.GetComponent<Camera>().orthographicSize;

        if (beginningValue > targetValue)
        {
            currentCameraState = CameraState.Run;
            runCameraZoomState = RunCameraZoomState.ZoomingIn;
            while (mainCamera.GetComponent<Camera>().orthographicSize > targetValue)
            {
                mainCamera.GetComponent<Camera>().orthographicSize -= zoomRate;
                supportCamera.GetComponent<Camera>().orthographicSize -= zoomRate;
                yield return new WaitForSeconds(0.01f);
            }
        }
        else if (beginningValue < targetValue)
        {
            currentCameraState = CameraState.Walk;
            runCameraZoomState = RunCameraZoomState.ZoomingOut;
            while (mainCamera.GetComponent<Camera>().orthographicSize < targetValue)
            {
                mainCamera.GetComponent<Camera>().orthographicSize += zoomRate;
                supportCamera.GetComponent<Camera>().orthographicSize += zoomRate;
                yield return new WaitForSeconds(0.001f);
            }
        }

        mainCamera.GetComponent<Camera>().orthographicSize = targetValue;
        supportCamera.GetComponent<Camera>().orthographicSize = targetValue;
        runCameraZoomState = RunCameraZoomState.Inactive;
    }

    /// <summary>
    /// Increases/decreases camera's zoom levels towards a given direction.
    /// </summary>
    /// <param name="direction">Direction to change to. Positive number zooms in, negative zooms out.</param>
    public void ChangeZoomLevelToDirection(float direction)
    {
        float newZoomLevel = 0.5f * Mathf.Sign(direction);
        mainCamera.GetComponent<Camera>().orthographicSize += newZoomLevel;
        supportCamera.GetComponent<Camera>().orthographicSize += newZoomLevel;
        normalZoomLevel += newZoomLevel;
    }

    /// <summary>
    /// Sets camera's zoom level to a given value.
    /// <para>Clamps the value between 0 and 30.</para>
    /// </summary>
    /// <param name="value">Value of the new zoom level.</param>
    public void ChangeZoomLevelToValue(float value)
    {
        float newZoomLevel = Mathf.Clamp(value, 0f, 30f);
        mainCamera.GetComponent<Camera>().orthographicSize = newZoomLevel;
        supportCamera.GetComponent<Camera>().orthographicSize = newZoomLevel;
    }

    /// <summary>
    /// Changes camera's zoom level based on given steps.
    /// <para>Can be set to keep the minimum zoom level of 0.</para>
    /// <para>Default step direction is positive, which means zooming out.</para>
    /// <para>Zooming in can be achieved by setting the step to a negative value.</para>
    /// </summary>
    /// <param name="step">Change per call.</param>
    /// <param name="keepOverZero">Do not allow zoom level to go negative.</param>
    /// <param name="maxValue">Max value of zoom level. Note that if keepOverZero = false and step is negative, it will not go below -maxValue.</param>
    public void ChangeZoomLevelWithSteps(float step, bool keepOverZero = false, float maxValue = 0)
    {
        Camera main = mainCamera.GetComponent<Camera>();
        Camera support = supportCamera.GetComponent<Camera>();

        if (keepOverZero)
        {
            if (main.orthographicSize <= 0)
            {
                return;
            }
        }

        if (maxValue > 0)
        {
            if (main.orthographicSize >= maxValue || main.orthographicSize <= -maxValue)
            {
                return;
            }
        }

        main.orthographicSize += step;
        support.orthographicSize += step;
    }
}

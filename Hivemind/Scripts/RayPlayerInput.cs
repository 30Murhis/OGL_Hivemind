using UnityEngine;
using FMOD.Studio;

/// <summary>
/// Handles most of player's inputs.
/// </summary>
public class RayPlayerInput : MonoBehaviour {

    [Tooltip("Are player inputs registered.")]
    public bool enablePlayerInput = true;
    [Tooltip("The projectile prefab that is shot when left mouse is clicked.")]
    public GameObject projectilePrefab;
    [Tooltip("The object the spore shot is instantiated on.")]
    public GameObject sporeShotSource;

    [FMODUnity.EventRef]
    [Tooltip("The FMOD sound used when spore shot is shot.")]
    public string sporeShotSound = "event:/SFX/sporeshoot";
    
    GameObject shot;
    RayMovement rayMovement;
    float facingDirection = 1;
    
    void Start () {
	    if (!sporeShotSource)
            sporeShotSource = transform.FindChild("SporeShotSource").gameObject;

        rayMovement = GetComponent<RayMovement>();
    }

    void Update()
    {
        // Try interaction via InteractionPerimeter
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractionPerimeter.Instance.InteractWithCurrentTarget();
        }

        // If player input is disabled, stops here, allowing only inputs above this to be registered
        if (!enablePlayerInput) return;

        // Change interaction target
        if (Input.GetKeyDown(KeyCode.Q))
        {
            InteractionPerimeter.Instance.TryGetNextInteractionTarget();
        }

        // Shooting
        if (Input.GetButtonDown("Fire1"))
        {
            if (projectilePrefab)
            {
                Shoot();
            }
            else
            {
                Debug.LogWarning("Could not shoot; unassigned spore shot prefab.", this);
            }
        }

        // Horizontal & vertical movement (even though vertical is pointless in this game)
        rayMovement.CharacterInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Set facing direction based on movement direction
        if (rayMovement.CharacterInput != Vector2.zero)
            facingDirection = Mathf.Sign(rayMovement.CharacterInput.x);

        // Jumping
        rayMovement.Jump = Input.GetKeyDown(KeyCode.Space); // GetKey() enables bunny hopping

        // Running
        rayMovement.Run = Input.GetKey(KeyCode.LeftShift);

        // Run camera activation based on speed, run state and direction
        CameraController.Instance.ActivateRunCamera((rayMovement.velocity.x != 0 && rayMovement.Run), (int)rayMovement.CharacterInput.x);

        // Character changing
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CharacterManager.ChangeCurrentCharacter();
        }
    }

    /// <summary>
    /// Attempts to shoot. Only happens when there is no previous shots in the level.
    /// </summary>
    void Shoot()
    {
        if (shot == null)
        {
            FMODUnity.RuntimeManager.PlayOneShot(sporeShotSound, gameObject.transform.position);
            shot = (GameObject)Instantiate(projectilePrefab, sporeShotSource.transform.position, Quaternion.Euler(new Vector3(0, facingDirection > 0 ? 0 : 180, 0)));
        }
    }
}

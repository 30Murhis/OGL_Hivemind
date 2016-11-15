using UnityEngine;

/// <summary>
/// NPC script with simple AI behaviour and infecting method.
/// </summary>
public class RayNPC : MonoBehaviour {

    public bool enableSimpleAI = true;
    public float moveSpeed = 0.5f;

    public bool enableStateLogging = false;

	// initialize the AI eyesight
	public float sightRange = 5f;
	public Transform SightStart, SightEnd;


    enum State
    {
        Infected,
        Idle,
        Wandering,
        Seeking,
        Chasing
    }

    public RandomComment comment;
    RayMovement rayMovement;
    State currentState = State.Idle;
    
    float moveDirection = 1;

    private void Awake()
    {
        rayMovement = GetComponent<RayMovement>();

        //InvokeRepeating("StateRandomization", 1, 1);
    }

    void Start()
    {
        if (comment == null) comment = GetComponentInChildren<RandomComment>(true);
        comment.transform.parent.gameObject.SetActive(false);
    }

    void Update()
    {
        /* Reminder about random chances.
        float randValue = Random.value;
        if (randValue < .45f) // 45% of the time
        {
            // Do Normal Attack 1
        }
        else if (randValue < .9f) // 45% of the time
        {
            // Do Normal Attack 2
        }
        else // 10% of the time
        {
            // Do Special Attack
        }
        */

        if (enableSimpleAI)
        {
            // If FPS=30, the chance of changing state is 6% per second.
            if (Random.value < .002f)
                SwitchState();

            if (Random.value < .002f)
            {
                if (comment == null) comment = GetComponentInChildren<RandomComment>(true);
                comment.NewRandomComment();
            }
            
            switch (currentState)
            {
                case State.Idle:
                    // Does nothing.
                    break;

                case State.Wandering:
                    rayMovement.CharacterInput = new Vector2(moveSpeed * moveDirection, 0);
                    break;

                case State.Seeking:
                    // Seeking for player.
                    break;

                case State.Chasing:
                    rayMovement.CharacterInput = new Vector2(moveSpeed * moveDirection, 0);
                    rayMovement.Run = true;
                    break;
            }
        }

		Sight();
    }

    void StateRandomization()
    {
        // The chance of changing state is 10% on every call.
        if (Random.value < .10f)
            SwitchState();
    }

    /// <summary>
    /// Randomizes the new state and possible walking direction.
    /// </summary>
    void SwitchState()
    {
        // Simple for now
        if (currentState == State.Idle)
        {
            currentState = State.Wandering;

            if (Random.value < 0.5f) moveDirection = 1;
            else moveDirection = -1;
        }
        else {
            currentState = State.Idle;
            rayMovement.CharacterInput = Vector2.zero;
        }

        /* Random chance for all
        float randValue = Random.value;
        if (randValue < .75f) // 75% chance of idle
        {
            currentState = State.Idle;
        }
        else // 25% chance of walking
        {
            currentState = State.Wandering;
            if (Random.value < 0.5f) walkDirection = 1;
            else walkDirection = -1;
        }
        */

        // Logging code.
        if (!enableStateLogging) return;
        string npcAction = gameObject.name + " is now " + currentState.ToString();
        if (currentState == State.Wandering)
        {
            npcAction += " to ";
            if (moveDirection < 0) npcAction += "left";
            else npcAction += "right";
        }
        Debug.Log(npcAction);
    }

    /// <summary>
    /// Infects the NPC, who then becomes part of the hivemind.
    /// </summary>
    public void Infect()
    {
        enableSimpleAI = false;
        tag = "Player";
        name = "Infected " + gameObject.name;
        rayMovement.CharacterInput = Vector2.zero;
        FindObjectOfType<AdvancedHivemind>().AddCharacter(this.gameObject);
    }

    /// <summary>
    /// Activates/Deactivates AI behaviour.
    /// </summary>
    /// <param name="active">Set active.</param>
    public void SetAIBehaviourActive(bool active)
    {
        enableSimpleAI = active;
        if (!active)
        {
            currentState = State.Idle;
            rayMovement.CharacterInput = Vector2.zero;
        }
    }

    /// <summary>
    /// Old way to turn the NPC to face a target. Not used in any current codes.
    /// </summary>
    /// <param name="target">Target to face.</param>
    /// <param name="flipped">If turning a ghost on the other side of the map.</param>
    public void TurnTowards(Transform target, bool flipped = false)
    {
        Vector3 ls = transform.localScale;
        float side = transform.position.x - target.position.x;

        if (!flipped)
        {
            if (Mathf.Sign(ls.x) == Mathf.Sign(side))
                transform.localScale = new Vector3(ls.x * -1, ls.y, ls.z);
        }
        else
        {
            if (Mathf.Sign(ls.x) == Mathf.Sign(target.position.x))
                transform.localScale = new Vector3(ls.x * -1, ls.y, ls.z);
        }
    }

	void Sight(){



		SightEnd.position = new Vector2(SightStart.position.x + sightRange * moveDirection, SightStart.position.y);

		Debug.DrawLine (SightStart.position, SightEnd.position , Color.blue);
		if (Physics2D.Linecast (SightStart.position, SightEnd.position,1 << LayerMask.NameToLayer("Player"))) {
			print ("detected !");
		}

	}
}


using UnityEngine;

/// <summary>
/// Infecting spore cloud.
/// </summary>
public class SporeCloud : MonoBehaviour {

    public float reachRadius = 0.5f;

    ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void FixedUpdate()
    {
        // Infects NPC's withing certain radius. Not needed if OnTriggerEnter2D is used.
        //Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, reachRadius);
        //for (int i = 0; i < colliders.Length; i++)
        //{
        //    Transform colTransform = colliders[i].transform;
        //    if (colTransform.tag == "NPC" && !colTransform.name.Contains("Ghost") && colTransform.GetComponent<RayNPC>())
        //    {
        //        colTransform.GetComponentInParent<RayNPC>().Infect();
        //    }
        //}

        // Destroys the cloud when last particle dies.
        if (ps)
        {
            if (!ps.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }

    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.DrawWireSphere(transform.position, reachRadius);
    //}

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "NPC" && !col.name.Contains("Ghost") && col.GetComponent<RayNPC>())
        {
            col.GetComponentInParent<RayNPC>().Infect();
        }
    }
}

using UnityEngine;

/// <summary>
/// Handles the moving of objects from border to border.
/// </summary>
public class BorderColliderMap : MonoBehaviour
{
    public GameObject otherBorder;

    Vector2 cameraPos;
    
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag != "Player" && col.tag != "NPC") return;

        float difference = col.transform.position.x - transform.position.x;

        // Move the object to the other border
        Vector2 newPosition = new Vector2(otherBorder.transform.position.x + difference, col.transform.position.y);
        col.transform.position = newPosition;

        /* Old rigidbody mover (had problems)
        // If collider has parent (which should mean it's a character)
        if (col.transform.parent)
        {
            // Disable the rigidbody before changing position to prevent weird movement
            Rigidbody2D rb = col.transform.parent.GetComponent<Rigidbody2D>();
            Vector2 velocity = rb.velocity;
            rb.Sleep();
            rb.gameObject.SetActive(false); // Disabling will disable animator in child -> causes incorrect split second animation

            // Move the object to the other border
            Vector2 newPosition = new Vector2(otherBorder.transform.position.x + difference, col.transform.position.y);
            col.transform.parent.position = newPosition;
            //rb.transform.position = newPosition;
            //col.transform.parent.Translate(newPosition);
            //rb.position = newPosition;

            // Enable the rigidbody with the previous velocity
            rb.gameObject.SetActive(true);
            rb.WakeUp();
            rb.velocity = velocity;
        }
        // If collider has no parent (which should mean it's some object, like a projectile)
        else
        {
            // Move the object to the other border
            Vector2 newPosition = new Vector2(otherBorder.transform.position.x + difference, col.transform.position.y);
            col.transform.position = newPosition;
        }
        */
    }
}

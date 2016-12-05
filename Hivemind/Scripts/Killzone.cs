using UnityEngine;
using System.Collections;

public class Killzone : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.GetComponent<Entity>()) {
            collider.transform.position = Vector2.zero;
        }
    }
}

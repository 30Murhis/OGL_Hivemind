using UnityEngine;
using System.Collections;

/// <summary>
/// Entity class used to control and manage character it is attached to.
/// </summary>
[System.Serializable]
public class Entity : MonoBehaviour
{
    [Tooltip("Data class containing all info of and for this entity.")]
    public EntityData entityData;

    bool colSizeSet = false;

    void Start () {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        entityData.lastPosition = transform.position;

        // Set the collider's size to approximately match the sprite's size, if it is not set yet
        if (!colSizeSet)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

            entityData.height = sr.sprite.bounds.size.y;
            entityData.width = 2.5f; // sr.sprite.bounds.size.x provides false width due to sprites being squares. 2.5f seems OK hard-coded value now.
            GetComponentInChildren<BoxCollider2D>().size = new Vector2(entityData.width, entityData.height);

            if (entityData.height > 0 || entityData.width > 0)
            {
                colSizeSet = true;
            }
        }
    }

    public void Die()
    {
        //if (OnDeath != null)
        //    OnDeath();

        // Death initiated.
        //FindObjectOfType<CameraController>().transform.SetParent(null);
        //Destroy(gameObject);

    }
}

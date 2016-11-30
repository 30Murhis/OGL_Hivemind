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

    void Start () {
        DontDestroyOnLoad(gameObject);
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

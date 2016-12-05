using System;
using UnityEngine;

namespace UnityStandardAssets._2D
{
    public class Restarter : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.tag == "Player")
            {
                //Application.LoadLevel(Application.loadedLevelName);
            }
        }
    }
}

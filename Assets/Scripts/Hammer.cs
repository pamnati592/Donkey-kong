using UnityEngine;
using System.Collections;

public class Hammer : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
            Destroy(other.gameObject);
    }
}
   


using System.Reflection.Metadata.Ecma335;
using UnityEngine;

public class Pickups : MonoBehaviour
{
    [SerializeField] private AudioClip coinPickSFX;

    bool wasCollected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(wasCollected)
        {
            return;
        }

        wasCollected = true;

        AudioSource.PlayClipAtPoint(coinPickSFX, Camera.main.transform.position);

        Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] AudioClip coinPickupSound;
    [SerializeField] int pointsForCoinPickup     = 100;
    [SerializeField] int pointsForBigCoinPickup  = 200;
    [SerializeField] int pointsForMegaCoinPickup = 300;


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            int coinsToAdd = pointsForCoinPickup;

            if (gameObject.name.Contains("Big Coin"))
            {
                coinsToAdd = pointsForBigCoinPickup;
            }
            else if (gameObject.name.Contains("Mega Coin"))
            {
                coinsToAdd = pointsForMegaCoinPickup;
            }
            FindObjectOfType<GameSession>().AddToScore(coinsToAdd);

            AudioSource.PlayClipAtPoint(coinPickupSound, transform.position);
            Destroy(gameObject);
        }
    }
}

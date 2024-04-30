using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float bulletSpeed = 12f;

    [SerializeField] ParticleSystem bulletSmoke;

    Rigidbody2D myRigidbody;
    Player      player;
    float       xSpeed;

    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<Player>();
        xSpeed = player.transform.localScale.x * bulletSpeed;

        SetSmokeRotEqualsBulletRot();
    }

    void SetSmokeRotEqualsBulletRot()
    {
        var bulletRot = bulletSmoke.transform.rotation;
        if (xSpeed < 0)
            bulletSmoke.transform.rotation = new Quaternion(bulletRot.x, bulletRot.y + 180, bulletRot.z, bulletRot.w);
    }

    void Update() => myRigidbody.velocity = new Vector2 (xSpeed, 0f);

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Buba"))
        {
            Destroy(other.gameObject, .2f);
        }
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D other) => Destroy(gameObject);
}

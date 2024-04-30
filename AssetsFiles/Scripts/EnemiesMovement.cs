using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1.15f;
    [SerializeField] float animDelay = .35f;

    Rigidbody2D     myRigidbody;
    SpriteRenderer  mySpriteRenderer;
    Animator        myAnimator;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        myAnimator = GetComponent<Animator>();
    }

    void Update() => myRigidbody.velocity = new Vector2(moveSpeed, 0);

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            moveSpeed = -moveSpeed;
            FlipFace();
        }
        if (collision.CompareTag("Player"))
        {
            myAnimator.speed = animDelay;
            Invoke(nameof(ResetMovement), 2f);
        }
    }
    void FlipFace()
    {
        if (mySpriteRenderer.flipX == false) mySpriteRenderer.flipX = true;
        else mySpriteRenderer.flipX = false;
    }

    void ResetMovement() => myAnimator.speed = 1.15f;
}

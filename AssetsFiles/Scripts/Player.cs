using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;


public class Player : MonoBehaviour
{
    [SerializeField] float runSpeed = 6f;
    [SerializeField] float jumpSpeed = 11f;
    [SerializeField] GameObject gun;
    [SerializeField] GameObject bullet;
    [SerializeField] ParticleSystem gunSmoke;

    [SerializeField] TilemapCollider2D climbingTops;

    [Header("AUDIO")]
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip runSound;
    [SerializeField] AudioClip bounceSound;
    [SerializeField] AudioSource audioSource;

    private const float HideTheGunDelay = 0.2f;

    readonly float onJumpUIisPressed = -1;

    GameObject      bulletInstance;
    Vector2         moveInput;
    SpriteRenderer  mySpriteRenderer;
    Rigidbody2D     myRigidbody;
    CapsuleCollider2D myBodyCollider;
    CapsuleCollider2D myFeetCollider;
    Animator        myAnimator;

    float startingGravity;
    bool isAlive = true;
    bool isJumping = false;

    Exit exitLevelScript;

    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        myRigidbody = GetComponent<Rigidbody2D>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GameObject.Find("Feet").GetComponent<CapsuleCollider2D>();
        myAnimator = GetComponent<Animator>();
        exitLevelScript = FindObjectOfType<Exit>();
        startingGravity = myRigidbody.gravityScale;
    }

    void Update()
    {
        if (!isAlive || exitLevelScript.IsLevelCompleted) { return; }

        Run();
        FlipSprite();
        ClimbLadder();
        Die();
    }



    void OnMove(InputValue value)
    {
        if (!isAlive || exitLevelScript.IsLevelCompleted) { return; }
        moveInput = value.Get<Vector2>();
    }

    #region Running
    void Run()
    {
        if (!isAlive || exitLevelScript.IsLevelCompleted) { return; }
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

        if (PlayerHasHorizontalSpeed && !isJumping)
        {
            myAnimator.SetBool("isRunning", true);
            if (!audioSource.isPlaying
                && myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground", "ClimbingTops")))
            {
                audioSource.PlayOneShot(runSound);
            }
        }
        else
        {
            myAnimator.SetBool("isRunning", false);
            if (audioSource.time == Mathf.Epsilon)
                audioSource.Stop(); //wait till the end of the sound
        }
    }
    void FlipSprite()
    {
        if (PlayerHasHorizontalSpeed)
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.velocity.x), 1f);
    }
    bool PlayerHasHorizontalSpeed => Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
    #endregion

    #region Jumping
    void OnJump(InputValue value)
    {
        if (!isAlive || exitLevelScript.IsLevelCompleted) { return; }

        if (IsAllowedToJump)
        {
            isJumping = true;
            if (IsJumpButtonPressed(value))
                myRigidbody.velocity += new Vector2(0, jumpSpeed);
            Invoke(nameof(ResetJumpState), .2f);
        }
    }
    bool IsJumpButtonPressed(InputValue value) => value.isPressed || onJumpUIisPressed > 0;
    bool IsAllowedToJump => myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground", "Climbing", "ClimbingTops"));
    void ResetJumpState() => isJumping = false;
    #endregion

    #region Shooting
    void OnFire()
    {
        if (!isAlive || exitLevelScript.IsLevelCompleted) { return; }
        ShowTheGun();
        InstantiateBullet();
        FixGunSmokePosition();
        Invoke(nameof(HideTheGun), HideTheGunDelay);
    }
    void ShowTheGun() => gun.SetActive(true);
    void InstantiateBullet()
    {
        AudioSource.PlayClipAtPoint(shootSound, transform.position);
        bulletInstance = Instantiate(bullet, gun.transform.position, transform.rotation);
        bulletInstance.transform.localScale = transform.localScale;
    }
    void FixGunSmokePosition()
    {
        var gunPos = GameObject.Find("Gun").GetComponent<Transform>().position;
        if (transform.localScale.x < 0) gunSmoke.transform.position = new Vector3(gunPos.x - 0.6f, gunPos.y, gunPos.z);
        else if (transform.localScale.x > 0) gunSmoke.transform.position = gunPos;
    }
    void HideTheGun() => gun.SetActive(false);
    #endregion

    #region Clinming
    void ClimbLadder()
    {
        SetClimbingTopsState();

        if (!IsClimbilg)
        {
            SetGravityScale(myRigidbody, startingGravity);
            myAnimator.speed = 1;
            myAnimator.SetBool("isClimbing", false);
            return;
        }

        SetGravityScale(myRigidbody, 0);
        myAnimator.SetBool("isRunning", false);
        myAnimator.SetBool("isClimbing", true);

        if (IsPassingOrJumpingFromLadder)
        {
            SetGravityScale(myRigidbody, startingGravity);
            return;
        }

        SlowDownIfClimbing();
    }
    bool IsClimbilg => myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing", "ClimbingTops"))
                            && myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Climbing", "ClimbingTops"));
    void SetClimbingTopsState()
    {
        //set the climbing top to act as ground or ladder/rope
        if (moveInput.y != 0)
        {
            if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("ClimbingTops")) ||
        myBodyCollider.IsTouchingLayers(LayerMask.GetMask("ClimbingTops")))
            {
                climbingTops.isTrigger = true; // move over it
            }
        }
        else if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("ClimbingTops")) &&
                    !myBodyCollider.IsTouchingLayers(LayerMask.GetMask("ClimbingTops")))
        {
            climbingTops.isTrigger = false; // move through it
        }
    }
    bool IsPassingOrJumpingFromLadder => PlayerHasHorizontalSpeed && isJumping && moveInput.y == 0;
    void SetGravityScale(Rigidbody2D rBody, float value) => rBody.gravityScale = value;
    void SlowDownIfClimbing()
    {
        if (moveInput.y != 0)
        {
            myAnimator.speed = 0.7f;
        }
        else myAnimator.speed = 0;
        Vector2 playerClimbingVelocity = new Vector2(myRigidbody.velocity.x, moveInput.y * runSpeed / 2);
        myRigidbody.velocity = playerClimbingVelocity;
    }
    #endregion

    void OnCollisionEnter2D(Collision2D other)
    {
        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Bouncing")))
        {
            AudioSource.PlayClipAtPoint(bounceSound, transform.position);
        }
    }

    void Die()
    {
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards", "Water")))
        {
            isAlive = false;
            myAnimator.SetTrigger("Dying");

            if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Water")))
            {
                Color color = mySpriteRenderer.color;
                color.a = mySpriteRenderer.color.a / 2;
                mySpriteRenderer.color = color;
                myBodyCollider.enabled = false; //if not, my color.a goes to 0 and better not

            }

            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }


    #region Mobile
    void OnPointerX(int inputValue) => moveInput.x = inputValue;
    void OnPointerY(int inputValue) => moveInput.y = inputValue;
    void OnJumpUI()
    {
        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground", "Climbing", "ClimbingTops")))
        {
            myRigidbody.velocity = new Vector2(0, jumpSpeed);
        }
    }
    void OnFireUI() => OnFire();
    #endregion
}

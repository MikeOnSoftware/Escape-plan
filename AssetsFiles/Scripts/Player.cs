using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;


public class Player : MonoBehaviour
{
    [SerializeField] float       runSpeed  = 6f;
    [SerializeField] float       jumpSpeed = 11f;
    [SerializeField] GameObject  bullet;
    [SerializeField] GameObject  gun;

    [SerializeField] TilemapCollider2D climbingTops;

    [Header("AUDIO")]
    [SerializeField] AudioClip   shootSound;
    [SerializeField] AudioClip   runSound;
    [SerializeField] AudioClip   bounceSound;
    [SerializeField] AudioSource audioSource;

   // [Header("MOBILE only")]
   // [SerializeField] Canvas canvasMobile;


    float onJumpUIisPressed = -1;


    GameObject        bulletInstance;
    Vector2           moveInput;
    SpriteRenderer    mySpriteRenderer;
    Rigidbody2D       myRigidbody;
    CapsuleCollider2D myBodyCollider;
    CapsuleCollider2D myFeetCollider;
    Animator          myAnimator;

    float startingGravity;
    bool  isAlive   = true;
    bool  isJumping = false;

    Exit exitLevelScript;


  //  void Awake()
  //  {
  //      if (Application.isMobilePlatform)
  //      {
  //          canvasMobile.gameObject.SetActive(true);
  //          // Application.Quit();
  //      }
  //  }


    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        myRigidbody      = GetComponent<Rigidbody2D>();
        myBodyCollider   = GetComponent<CapsuleCollider2D>();
        myFeetCollider   = GameObject.Find("Feet").GetComponent<CapsuleCollider2D>();
        myAnimator       = GetComponent<Animator>();
        exitLevelScript  = FindObjectOfType<Exit>();
        startingGravity  = myRigidbody.gravityScale;
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
        Debug.Log(value.Get<Vector2>());
        if (!isAlive || exitLevelScript.IsLevelCompleted) { return; }
        moveInput = value.Get<Vector2>();
    }

    void Run()
    {
        if (!isAlive || exitLevelScript.IsLevelCompleted) { return; }
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

        if (moveInput.x != 0 && !isJumping)
        {
            myAnimator.SetBool("isRunning", true);
            if (!audioSource.isPlaying && myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground", "ClimbingTops")))
            {
                audioSource.PlayOneShot(runSound);
            }
        }
        else
        {
            myAnimator.SetBool("isRunning", false);
            if (audioSource.time == Mathf.Epsilon) audioSource.Stop(); //wait till the end of the sound
        }

    }
    
    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.velocity.x), 1f);
        }
    }


    void OnJump(InputValue value)
    {
        if (!isAlive || exitLevelScript.IsLevelCompleted) { return; }

        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground", "Climbing", "ClimbingTops")))
        {
            isJumping = true;

            if (value.isPressed || onJumpUIisPressed > 0)
                myRigidbody.velocity += new Vector2(0, jumpSpeed);

            Invoke("ResetJumpState", .2f);
        }
    }

    void OnFire()
    {
        if (!isAlive || exitLevelScript.IsLevelCompleted) { return; }
        ShowTheGun();
        AudioSource.PlayClipAtPoint(shootSound, transform.position);
        bulletInstance = Instantiate(bullet, gun.transform.position, transform.rotation);
        bulletInstance.transform.localScale = transform.localScale;
        Invoke("HideTheGun", 0.2f);
    }

    void ClimbLadder()
    {
        //set the climbing top to act as ground or climbing object
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
        //--------------

        bool noContactFeetClimbing = !myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing", "ClimbingTops"));
        bool noContactBodyClimbing = !myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Climbing", "ClimbingTops"));

        if (noContactFeetClimbing || noContactBodyClimbing)
        {
            myRigidbody.gravityScale = startingGravity;
            myAnimator.speed = 1;
            myAnimator.SetBool("isClimbing", false);
            return;
        }

        myRigidbody.gravityScale = 0;
        myAnimator.SetBool("isRunning", false);
        myAnimator.SetBool("isClimbing", true);

        if (moveInput.y != 0)
        {
            myAnimator.speed = 0.7f;
        }
        else myAnimator.speed = 0;

        if (moveInput.x != 0 && isJumping && moveInput.y == 0) //for jumping from the ladder, and also when passing through
        {
            myRigidbody.gravityScale = startingGravity;
            return;
        }

        Vector2 playerVelocity = new Vector2(myRigidbody.velocity.x, moveInput.y * runSpeed / 2);
        myRigidbody.velocity = playerVelocity;
    }

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

    void ResetJumpState()
    {
        isJumping = false;
    }
    void ShowTheGun()
    {
        gun.SetActive(true);
    }
    void HideTheGun()
    {
        gun.SetActive(false);
    }

    //INPUT MOBILE
    public void OnPointerX(int inputValue)
    {
        moveInput.x = inputValue;
       
    }
    public void OnPointerY(int inputValue)
    {
        moveInput.y = inputValue;

    }
    public void OnJumpUI()
    {
        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground", "Climbing", "ClimbingTops")))
        {
            myRigidbody.velocity = new Vector2(0, jumpSpeed);
        }
    }
    public void OnFireUI()
    {
        OnFire();
    }
}


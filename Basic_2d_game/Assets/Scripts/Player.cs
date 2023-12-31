using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;



public class Player : MonoBehaviour
{
    
    Rigidbody2D myRigidbody2D;
    Animator myAnimator;
    BoxCollider2D myBoxCollider2d;
    PolygonCollider2D myPolygonCollider2d;
    AudioSource myAudioSource;

    float startingGravityScale;
    bool isHurting = false;

    [SerializeField]
    float runspeed = 2f;
    [SerializeField]
    float jumpspeed = 2f;
    [SerializeField]
    float climbspeed = 8f;
    [SerializeField]
    float attackRadius = 3f;
    [SerializeField]
    Vector2 hitKick=new Vector2(50f,50f);
    [SerializeField]
    Transform hurtBox;
    [SerializeField] AudioClip jumpingSFX, attackingSFX,getHitSFX, walkingSFX;

    // Start is called before the first frame update
    void Start()
    {
        
        myRigidbody2D= GetComponent<Rigidbody2D>();
        myAnimator= GetComponent<Animator>();
        myBoxCollider2d= GetComponent<BoxCollider2D>();
        myPolygonCollider2d= GetComponent<PolygonCollider2D>();
        myAudioSource= GetComponent<AudioSource>();
        startingGravityScale=myRigidbody2D.gravityScale;

        myAnimator.SetTrigger("Appearing");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isHurting)
        {
            Run();
            Jump();
            climb();
            Attack();

            if (myBoxCollider2d.IsTouchingLayers(LayerMask.GetMask("Enemy")))
            {
                PlayerHit();
            }

            ExitLevel();
        }
    }

    private void ExitLevel()
    {
        if (!myBoxCollider2d.IsTouchingLayers(LayerMask.GetMask("Interactable")))
        {
            return;
        }

        if (Input.GetButtonDown("Vertical"))
        {
            myAnimator.SetTrigger("Disappearing");
           
        }
    }

    public void LoadNextLevel()
    {
        FindObjectOfType<ExitDoor>().StartLoadingNextLevel();
        TurnOffRenderer();
    }
    public void TurnOffRenderer()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    private void Attack()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            myAnimator.SetTrigger("Attacking");
            myAudioSource.PlayOneShot(attackingSFX);

            Collider2D[] enemiesToHit= Physics2D.OverlapCircleAll(hurtBox.position, attackRadius, LayerMask.GetMask("Enemy"));

            foreach(Collider2D enemy in enemiesToHit)
            {
                enemy.GetComponent<Enemy>().Dying();
            }
        }
    }

    public void Run()
    {
       
        float horizontalInput = Input.GetAxis("Horizontal");

        // Apply the input to the player's velocity.
        myRigidbody2D.velocity = new Vector2(horizontalInput * runspeed, myRigidbody2D.velocity.y);
        FlipSprite();
        ChangeRunAnimation();
        ChangeJumpAnimation();

       
    }

    void StepsSFX()
    {
        bool playerMovingHorizontally = Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon;

        if(playerMovingHorizontally)
        {
            if (myBoxCollider2d.IsTouchingLayers(LayerMask.GetMask("Ground")))
            {
                myAudioSource.PlayOneShot(walkingSFX);
            }
        }
        else
        {
            myAudioSource.Stop();
        }
    }

    public void PlayerHit()
    {
        myRigidbody2D.velocity = hitKick * new Vector2(-transform.localScale.x, 1f);

        myAnimator.SetTrigger("Hitting");
        myAudioSource.PlayOneShot(getHitSFX);
        isHurting= true;

        FindObjectOfType<GameSession>().ProcessPlayerDeath();

        StartCoroutine(StopHurting());

    }

    IEnumerator StopHurting()
    {
        yield return new WaitForSeconds(2f);

        isHurting = false;
    }

    private void climb()
    {
        if (myBoxCollider2d.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            Debug.Log("istouching");
            float verticalInput = Input.GetAxis("Vertical");
            Vector2 climbingV = new Vector2(myRigidbody2D.velocity.x, verticalInput * climbspeed);
            myRigidbody2D.velocity = climbingV;

            myRigidbody2D.gravityScale = 0f;
        }
        else { myRigidbody2D.gravityScale = startingGravityScale; }
    }

    public void Jump()
    {
        if (!myPolygonCollider2d.IsTouchingLayers(LayerMask.GetMask("Ground"))) { return; }
        bool isJumping = Input.GetButtonDown("Jump");
        if (isJumping)
        {
            myRigidbody2D.velocity = new Vector2(myRigidbody2D.velocity.x, jumpspeed);

            myAudioSource.PlayOneShot(jumpingSFX);
        }
    }

    private void ChangeRunAnimation()
    {
        bool runningHorizontally = Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("Running", runningHorizontally);
    }

    private void ChangeJumpAnimation()
    {
        bool jumping=Mathf.Abs(myRigidbody2D.velocity.y) > Mathf.Epsilon;
        myAnimator.SetBool("Climbing", jumping);
    }

    private void FlipSprite()
    {
        bool runningHorizontally = Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon;
        if (runningHorizontally)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody2D.velocity.x),1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(hurtBox.position, attackRadius);
    }

}

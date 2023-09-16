using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Public configuration
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;

    //Components
    private Rigidbody2D rb;

    private BoxCollider2D bCol2d;
    private Animator anim;
    //Private attributes
    [SerializeField] private bool isJumping = false;
    [SerializeField] private bool fallFromPlatformInput = false;
    [SerializeField] private bool fallingFromPlatform = false;
    private Collider2D currentPlatform;
    private ContactFilter2D filter2D;
    private float horizontalInput;
    public Transform groundCheck;
    [SerializeField] private bool isGrounded;
    //Allocate an array with just one element capacity to store the floor when hit
    RaycastHit2D[] hits = new RaycastHit2D[1];

    // Start is called before the first frame update
    void Start()
    {
        //Get components
        rb = GetComponent<Rigidbody2D>();
        bCol2d = GetComponent<BoxCollider2D>();
        anim = GameObject.Find("Melonie").GetComponent<Animator>();

        //Create a contactFilter configuration for the rays to check if the player is grounded
        filter2D = new ContactFilter2D
        {
            //Ignore trigger colliders
            useTriggers = false,
            //Use a layer mask
            useLayerMask = true
        };
        //Set the layer mask to hit only Floor and Platform layer
        filter2D.SetLayerMask(LayerMask.GetMask("Ground", "Platform"));

    }

    private void Update()
    {
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        //Keep down arrow pressed and press space
        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.Space))
        {
            //Enable falling down from a platform
            fallFromPlatformInput = true;
        }
        //Press only Space instead
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            //Let the player jump
            isJumping = true;
        }


    }

    void FixedUpdate()
    {
        // Get horizontal input
        horizontalInput = Input.GetAxis("Horizontal");

        // Move the player horizontally
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        // Flip the player sprite based on direction of movement
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);

        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // Play the walking animation if the player is moving horizontally
        if (horizontalInput != 0 && isGrounded)
        {
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);

        }



        //bool grounded = Grounded();
        //Check if the player ray is touching the ground and jump is enable
        if (isJumping && isGrounded)
        {
            //Jump
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            //rb.AddForce(new Vector2(0,jumpForce), ForceMode2D.Impulse);

        }
        //Reset the jump input
        isJumping = false;


        //Check for fallFromPlatform input and start falling only if the player is touching the ground
        if (fallFromPlatformInput && isGrounded)
        {
            if (currentPlatform != null)
            {
                //start falling from the platform 
                fallingFromPlatform = true;
            }
        }
        //Reset the fall input
        fallFromPlatformInput = false;
        //Check if the player is grounded on a platform and the should fall down
        if (CloudPlatformCheck() && fallingFromPlatform)
        {
            //Cast the ray above the player head to check 
            FallingFromPlatformCheck();
            if (currentPlatform != null && !currentPlatform.isTrigger)
            {
                //Reset the cloud platform to initial state (as trigger)
                currentPlatform.isTrigger = true;
                SpriteRenderer sprite = currentPlatform.gameObject.GetComponent<SpriteRenderer>();
                sprite.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                currentPlatform = null;
            }
            //If the platform has become a trigger now the updated Grounded will return false because the playes is falling
            //When it returns true again it means the player is touching the floor so disable the fallingFromPlatform check
            if (Grounded())
            {
                //disable the fallingFromPlatform check
                fallingFromPlatform = false;
            }
        }
        else
        {
            //Disable the fallingFromPlatform check
            fallingFromPlatform = false;
        }
    }

    void FixedUpdate2()
    {
        //Laser length
        float laserLength = 0.1f;
        //Right ray start X
        float startPositionX = transform.position.x + (bCol2d.size.x * transform.localScale.x / 2.0f) + (bCol2d.offset.x * transform.localScale.x) - 0.1f;
        //Hit only the objects of Platform layer
        //int layerMask = LayerMask.GetMask("Bonus");
        //Left ray start point
        Vector2 startPosition = new Vector2(startPositionX, transform.position.y - (bCol2d.bounds.extents.y + 0.05f));
        //The color of the ray for debug purpose
        Color rayColor = Color.red;
        //Check if the left laser hits something
        int totalObjectsHit = Physics2D.Raycast(startPosition, Vector2.down, filter2D, hits, laserLength);

        //Iterate the objects hit by the laser
        for (int i = 0; i < totalObjectsHit; i++)
        {
            //Get the object hit
            RaycastHit2D hit = hits[i];
            //Do something
            if (hit.collider != null)
            {
                SpriteRenderer sprite = hit.collider.GetComponent<SpriteRenderer>();
                sprite.color = Color.green;

            }

        }
    }

    bool Grounded()
    {
        //Laser length
        float laserLength = 0.8f;
        //Left ray start X
        float left = transform.position.x - (bCol2d.size.x * transform.localScale.x / 2.0f) + (bCol2d.offset.x * transform.localScale.x) + 0.1f;
        //Right ray start X
        float right = transform.position.x + (bCol2d.size.x * transform.localScale.x / 2.0f) + (bCol2d.offset.x * transform.localScale.x) - 0.1f;
        //Hit only the objects of Platform layer
        int layerMask = LayerMask.GetMask("Ground", "Platform");
        //Left ray start point
        Vector2 startPositionLeft = new Vector2(left, transform.position.y - (bCol2d.bounds.extents.y + 1f));
        //Right ray start point
        Vector2 startPositionRight = new Vector2(right, transform.position.y - (bCol2d.bounds.extents.y + 1f));
        //The color of the ray for debug purpose
        Color rayColor = Color.red;
        //Check if the left laser hits something
        int leftCount = Physics2D.Raycast(startPositionLeft, Vector2.down, filter2D, hits, laserLength);
        //Check if the right laser hits something
        int rightCount = Physics2D.Raycast(startPositionRight, Vector2.down, filter2D, hits, laserLength);

        Collider2D col2DHit = null;
        //If one of the lasers hits the floor
        //if ((leftCount > 0 && hitsLeft[0].collider != null) || (rightCount > 0 && hitsRight[0].collider != null))
        if ((leftCount > 0 || rightCount > 0) && hits[0].collider != null)
        {

            //Get the object hits collider
            col2DHit = hits[0].collider;
            //Change the color of the ray for debug purpose
            rayColor = Color.green;
        }
        //Draw the ray for debug purpose
        Debug.DrawRay(startPositionLeft, Vector2.down * laserLength, rayColor);
        Debug.DrawRay(startPositionRight, Vector2.down * laserLength, rayColor);
        //If the ray hits the floor returns true, false otherwise
        return col2DHit != null;
    }

    bool CloudPlatformCheck()
    {
        //While the player is checking from falling from a platform invalidate this check
        if (fallingFromPlatform) return true;
        //Laser length
        float laserLength = 0.8f;
        //Left ray start X
        float left = transform.position.x - (bCol2d.size.x * transform.localScale.x / 2.0f) + (bCol2d.offset.x * transform.localScale.x) + 0.1f;
        //Right ray start X
        float right = transform.position.x + (bCol2d.size.x * transform.localScale.x / 2.0f) + (bCol2d.offset.x * transform.localScale.x) - 0.1f;
        //Hit only the objects of Platform layer
        int layerMask = LayerMask.GetMask("Platform");
        //Left ray start point
        Vector2 startPositionLeft = new Vector2(left, transform.position.y - (bCol2d.bounds.extents.y + 1f));
        //Check if the left laser hit something
        RaycastHit2D hitLeft = Physics2D.Raycast(startPositionLeft, Vector2.down, laserLength, layerMask);
        //Right ray start point
        Vector2 startPositionRight = new Vector2(right, transform.position.y - (bCol2d.bounds.extents.y + 1f));
        //Check if the right laser hit something
        RaycastHit2D hitRight = Physics2D.Raycast(startPositionRight, Vector2.down, laserLength, layerMask);
        //The color of the ray for debug purpose
        Color rayColor = Color.red;

        Collider2D col2DHit = null;
        //If one of the lasers hit a cloud platform
        if (hitLeft.collider != null || hitRight.collider != null)
        {
            //Get the object hit collider
            col2DHit = hitLeft.collider != null ? hitLeft.collider : hitRight.collider;
            //Change the color of the ray for debug purpose
            rayColor = Color.green;
            //If the cloud platform collider is trigger
            if (col2DHit.isTrigger)
            {
                //Store the platform to reset later
                currentPlatform = col2DHit;
                //Disable trigger behaviour of collider
                currentPlatform.isTrigger = false;
                //Color the sprite of the cloud platform for debug purpose
                SpriteRenderer sprite = currentPlatform.gameObject.GetComponent<SpriteRenderer>();
                sprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
        else
        {
            //Change the color of the ray for debug purpose
            rayColor = Color.red;
            //If we stored previously a platform
            if (currentPlatform != null)
            {
                //Reset the platform properties
                currentPlatform.isTrigger = true;
                SpriteRenderer sprite = currentPlatform.gameObject.GetComponent<SpriteRenderer>();
                sprite.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                currentPlatform = null;
            }
        }

        //Draw the ray for debug purpose
        Debug.DrawRay(startPositionLeft, Vector2.down * laserLength, rayColor);
        Debug.DrawRay(startPositionRight, Vector2.down * laserLength, rayColor);
        //If the ray hits a platform returns true, false otherwise
        return col2DHit != null;
    }

    bool FallingFromPlatformCheck()
    {
        //Laser length
        float laserLength = 0.25f;
        //Ray start point
        Vector2 startPosition = new Vector2(transform.position.x, transform.position.y - (bCol2d.bounds.extents.y));
        //Hit only the objects of Platform layer
        int layerMask = LayerMask.GetMask("Platform");
        //Check if the laser hit something
        RaycastHit2D hit = Physics2D.Raycast(startPosition, Vector2.down, laserLength, layerMask);
        //The color of the ray for debug purpose
        Color rayColor = Color.red;
        if (hit.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
            //the player overcame the platform falling down, so disable the fallingFromPlatform check
            fallingFromPlatform = false;
        }

        //Draw the ray for debug purpose
        Debug.DrawRay(startPosition, Vector2.down * laserLength, rayColor);
        return hit.collider != null;
    }
}



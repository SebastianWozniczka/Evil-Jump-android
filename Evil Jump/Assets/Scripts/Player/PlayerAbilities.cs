using System;
using System.Collections;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    public AirJump airJump;
    public MegaAirJump megaAirJump;
    public Magic magic;
    public TimeLord timeLord;
    public WallRun1 wallRun;
    public MegaJump megaJump;

    public Animator animator;
    public PlayerControls playerControls;
    public CameraFollow cameraFollow;

    public bool hasInvincibility = false;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector3 startDist;

    private string airJumpKey = "AirJumped";
    private string megaAirJumpKey = "hasMegaAirJump";
    private string wallRunKey = "hasWallRun";
    private string wallslideKey = "wallSliding";
    private string teleportation = "hasStaff";
    private string lord = "isTimelord";

    private string wallTag = "wall";
    private bool dragging = false;
    private float distance;

    // Start is called before the first frame update
    void Start(){
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void OnEnable(){
        airJump.AirJumpEvent += AirJump;
    }

    void OnDisable(){
        airJump.AirJumpEvent -= AirJump;
    }

    void FixedUpdate(){

        if (magic.hasThis)
        {
            MagicStaff();  
        }

        if (timeLord.hasThis)
        {
            TimeLord();
        }

        
        if (megaAirJump.hasThis){
            MegaAirJump();
        }
    }

    

    void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag(wallTag)){ 
            //
            // Prevents player from passing through the wall.
            // Don't use 'is trigger = false' because that causes an issue that 
            // allows player to stop when turning the phone 90 degrees 
            // while touching the wall
            // 

            cameraFollow.state = CameraFollow.States.Normal;
            
            Vector2 velocity = rb.velocity;
            if(velocity.x < 0f){
                playerControls.canMoveLeft = false;
                rb.velocity = new Vector2(0,velocity.y);
            }
            else if(rb.velocity.x > 0f ){
                playerControls.canMoveRight = false;
                rb.velocity = new Vector2(0,velocity.y);
            }
        }
        else if(other.CompareTag("wallBottom")){ // Ends MegaAirjump skill
            megaAirJump.End();
            animator.SetBool(megaAirJumpKey, false);
        }
    }
    
    void OnTriggerStay2D(Collider2D other){
        if(other.CompareTag(wallTag)){ // Lets the player run through walls if they have the wallRun ability

           
            if(!megaAirJump.hasThis && cameraFollow.state != CameraFollow.States.Falling){
                cameraFollow.state = CameraFollow.States.Normal;
            }
            
            if(wallRun.hasThis && !PlayerControls.isDashing && !megaAirJump.hasThis){ 
                // Wall run
                rb.velocity = new Vector2(0, 7f);
                animator.SetBool(wallRunKey, true);
                animator.SetBool(wallslideKey, false);
            }
            else{ 
                animator.SetBool(wallRunKey, false);
                if(rb.velocity.y < 0f && !PlayerControls.isDashing){ // Wall slide
                    
                    rb.velocity = new Vector2(0, rb.velocity.y + 0.009f);
                    
                    animator.SetBool(wallslideKey, true);
                }
                else{
                    animator.SetBool(wallslideKey, false);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other){
        if(other.CompareTag(wallTag)){ 
            // Removes any movement restrictions
            playerControls.canMoveLeft = true;
            playerControls.canMoveRight = true;
            animator.SetBool(wallRunKey, false);
            animator.SetBool(wallslideKey, false);
        }
    }

    // Jumps the player when they are on the platform
    public void Jump(float jumpForce){
        cameraFollow.state = CameraFollow.States.Normal;
        
        if(PlayerControls.isDashing){
            playerControls.EndDash();
        }
        
        jumpForce = megaJump.hasThis ? jumpForce * megaJump.jumpMultiplier : jumpForce;

        rb.velocity = new Vector2(0f, jumpForce);
        playerControls.movementSpeed = 12f;
    }

    // Makes player invincible when they are shot by an enemy
    public void BecomeInvincible(float duration){
        StartCoroutine(Invinciblity(duration));
    }

    private IEnumerator Invinciblity(float duration){
        hasInvincibility =  true;
        yield return new WaitForSeconds(duration);
        hasInvincibility = false;
    }

    // Jumps the player in air when they uses the ability
    private void AirJump(){
        cameraFollow.state = CameraFollow.States.Normal;
        playerControls.movementSpeed = 12f;
        rb.velocity = new Vector2(0f, airJump.jumpForce);
        //wallRun.End();
        animator.SetBool(airJumpKey,true);
        animator.SetBool(wallRunKey,false);
    }

    private void MagicStaff()
    {
        animator.SetBool(teleportation, true);
        //magic.Duration();
        rb.bodyType = RigidbodyType2D.Static;
        if (col.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
        {
            
            OnMouseDown();
            playerDrag();
        }

        if (magic.isdurationFinished)
        {
            dragging = false;
            animator.SetBool(teleportation, false);
            rb.bodyType = RigidbodyType2D.Dynamic;
            magic.End();
           
        }
    }

    private void TimeLord()
    {
        animator.SetBool(lord, true);
        Time.timeScale = 0.7f;

        if (timeLord.isdurationFinished)
        {
            animator.SetBool(lord, false);
            Time.timeScale = 1f;
            timeLord.End();
        }
    }

    private void playerDrag()
    {


        //PlayerControls.hasStarted = false;

           

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayPoint = ray.GetPoint(distance);

            float velY = rb.velocity.y * 100;
            float velX = rb.velocity.x * 100;
          
            Vector3 vel = new Vector3(velX, velY, 0);
          
            transform.position = Vector3.SmoothDamp(transform.position, rayPoint, ref vel, 0.001f);

     
    }

    private void OnMouseDown()
    {

        

        distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        dragging = true;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 rayPoint = ray.GetPoint(distance);
        startDist = transform.position - rayPoint;
    }


    // Lets player fly for a certian duration when they pick up MegaAirJump power up 
    private void MegaAirJump(){
        cameraFollow.state = CameraFollow.States.HighSpeed;
        playerControls.movementSpeed = 12f;
        megaAirJump.Duration();
        rb.velocity = new Vector2(rb.velocity.x, megaAirJump.speed);
        animator.SetBool(megaAirJumpKey, true);

        if(megaAirJump.isdurationFinished){
            cameraFollow.state = CameraFollow.States.Normal;
            animator.SetBool(megaAirJumpKey, false);
            megaAirJump.End();
        }
    }

    public void EndAirJumpAnim(){
        animator.SetBool(airJumpKey,false);
    }
}

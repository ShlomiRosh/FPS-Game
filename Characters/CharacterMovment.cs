using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class CharacterMovment : MonoBehaviour
{
    // This class Holder the Anim setting [the options for the player]
    #region ClassAnimationSettings
    [System.Serializable]
    public class AnimationSettings
    {
        public string verticalVelocityFloat = "Forward";
        public string horizontalVelocityFloat = "Strafe";
        public string groundedBool = "IsGrounded";
        public string jumpBool = "IsJumping";
    }
    #endregion ClassAnimationSettings
    // This class Holder the Physics setting [gravity ect]
    #region ClassPhysicsSettings
    [System.Serializable]
    public class PhysicsSettings
    {
        public float gravityModifier = 9.81f;
        public float baseGravity = 50.0f;
        public float resetGravityValue = 1.2f;
        public LayerMask groundLayers;
        public float airSpeed = 2.5f;
    }
    #endregion ClassPhysicsSettings
    // This class Holder the movments setting [jump speed ect]
    #region ClassMovmentSettings
    [System.Serializable]
    public class MovmentSettings
    {
        public float jumpSpeed = 6.0f;
        public float jumpTime = 0.25f;
    }
    #endregion ClassMovmentSettings

    #region ClassCharacterMovment Variables // This Class
    [SerializeField] public AnimationSettings animations;
    [SerializeField] public PhysicsSettings physics;
    [SerializeField] public MovmentSettings movment;
    Animator animator;
    CharacterController characterController;
    bool jumping;
    bool resetGravity;
    float gravity;
    float forward;
    float strafe;
    Vector3 airControl;
    #endregion ClassCharacterMovment

    #region ClassCharacterMovment Functions // This Class
    void Awake() // Before the game start
    {
        animator = GetComponent<Animator>();
        SetUpAnimator();
    }

    void Start() // Start is called before the first frame update
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update() // Update is called once per frame
    {
        AirControl(forward, strafe);
        ApplyGravity();
        //isGrounded = characterController.isGrounded;
    }

    void AirControl(float forward, float strafe) // Controls player movments on the air
    {
        if (isGrounded() == false)
        {
            airControl.x = strafe;
            airControl.z = forward;
            airControl = transform.TransformDirection(airControl);
            airControl *= physics.airSpeed;
            characterController.Move(airControl * Time.deltaTime);
        }
    }

    bool isGrounded() // Check if player on the ground
    {
        RaycastHit hit;
        Vector3 start = transform.position + transform.up;
        Vector3 dir = Vector3.down;
        float radius = characterController.radius;
        if (Physics.SphereCast(start, radius, dir, out hit, characterController.height / 2, physics.groundLayers))
        {
            return true;
        }

        return false;
    }

    public void Animate(float forward, float strafe) // Animate the character and root motion handles the movment
    {
        this.forward = forward;
        this.strafe = strafe;
        animator.SetFloat(animations.verticalVelocityFloat, forward);
        animator.SetFloat(animations.horizontalVelocityFloat, strafe);
        animator.SetBool(animations.groundedBool, isGrounded());
        animator.SetBool(animations.jumpBool, jumping);
    }

    public void Jump() // Makes the character jump
    {
        if (jumping)
            return;
        if (isGrounded())
        {
            jumping = true;
            StartCoroutine(StopJump());
        }
    }

    IEnumerator StopJump() // Stops us from jumping
    {
        yield return new WaitForSeconds(movment.jumpTime);
        jumping = false;
    }

    void ApplyGravity() // Applys downard force to the character when we aren't jumping 
    {
        if (!isGrounded())
        {
            if (!resetGravity)
            {
                gravity = physics.resetGravityValue;
                resetGravity = true;
            }
            gravity += Time.deltaTime * physics.gravityModifier;
        }
        else
        {
            gravity = physics.baseGravity;
            resetGravity = false;
        }
        Vector3 gravityVector = new Vector3();
        if (!jumping)
        {
            gravityVector.y -= gravity;
        }
        else
        {
            gravityVector.y = movment.jumpSpeed;
        }
        characterController.Move(gravityVector * Time.deltaTime);
    }

    void SetUpAnimator() // Set up animator with child avatar
    {
        Animator wantedAnim = GetComponentsInChildren<Animator>()[1];
        Avatar wantedAvater = wantedAnim.avatar;
        animator.avatar = wantedAvater;
        Destroy(wantedAnim);
    }
    #endregion ClassCharacterMovment Functions
}

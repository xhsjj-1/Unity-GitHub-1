using UnityEngine;

public class Ai : MonoBehaviour
{
    private CharacterController controller;
    private Animator anim;

    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 10f;

    [Header("AI 设置")]
    public float switchDirectionTime = 2f;
    private float moveInput;
    private float timer;

    [Header("跳跃&重力")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("AI 随机跳跃")]
    public float jumpCheckInterval = 0.5f;
    [Range(0f, 1f)] public float jumpProbability = 0.3f;
    private float jumpTimer;

    [Header("AI 死亡特效")]
    public GameObject deathEffect;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        moveInput = 1f;
        timer = switchDirectionTime;
        jumpTimer = jumpCheckInterval;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        AutoSwitchDirection();
        AutoRandomJump();
        Move();
        Rotate();
        ApplyGravity();
        UpdateAnimation();
    }

    void AutoSwitchDirection()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            moveInput = -moveInput;
            timer = switchDirectionTime;
        }
    }

    void AutoRandomJump()
    {
        if (!isGrounded) return;
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0)
        {
            if (Random.value < jumpProbability) Jump();
            jumpTimer = jumpCheckInterval;
        }
    }

    void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void Move()
    {
        Vector3 move = Vector3.right * moveInput;
        float speed = isGrounded ? moveSpeed : moveSpeed * 0.8f;
        controller.Move(move * speed * Time.deltaTime);
    }

    void Rotate()
    {
        if (moveInput > 0.1f)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), rotateSpeed * Time.deltaTime);
        else if (moveInput < -0.1f)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 180, 0), rotateSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateAnimation()
    {
        if (anim == null) return;
        anim.SetFloat("Forward", Mathf.Abs(moveInput) > 0.01f ? 1 : 0);
        anim.SetBool("IsJumping", !isGrounded);
    }

    // AI 碰撞玩家（玩家不动也掉血）
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PlayerMovement player = hit.collider.GetComponent<PlayerMovement>();

        // 🔥 关键修复：玩家无敌时，AI 绝对不造成伤害
        if (player != null && !player.isDead && !player.isInvincible)
        {
            player.TakeDamage(1);
        }
    }

    public void Die()
    {
        if (deathEffect != null)
        {
            Vector3 effectPos = transform.position + Vector3.up * 1f;
            Instantiate(deathEffect, effectPos, transform.rotation);
        }
        Destroy(gameObject);
    }
}
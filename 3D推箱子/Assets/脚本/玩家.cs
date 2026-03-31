using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家移动控制器
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    private PlayerControl playerControl;
    private CharacterController controller;
    private Animator anim;
    private Renderer playerRenderer;

    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 10f;
    [Range(0.1f, 1f)] public float grabMoveSpeedMultiplier = 0.6f; // 抓取减速

    [Header("跳跃&重力")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("玩家血量")]
    public int maxHealth = 3;
    public int currentHealth;
    public bool isDead = false;

    [Header("踩怪检测")]
    public float rayDistance = 0.7f;

    [Header("受击无敌")]
    public float invincibleDuration = 1f;
    public bool isInvincible = false;
    private Color originalColor;

    [Header("死亡特效")]
    public GameObject explosionEffect;

    [Header("变大效果")]
    public float sizeDuration = 5f;       // 效果时间
    public float sizeMultiplier = 2f;    // 变大倍数
    private bool isBig = false;
    private Vector3 originalScale;       // 原始大小

    private PlayerGrab playerGrab;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerControl = GetComponent<PlayerControl>();
        anim = GetComponent<Animator>();
        playerRenderer = GetComponentInChildren<Renderer>();
        playerGrab = GetComponent<PlayerGrab>();
        originalScale = transform.localScale;
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;
    }

    void Update()
    {
        if (isDead) return;

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        CheckStepOnAI();
        Move();
        Rotate();
        Jump();
        ApplyGravity();
        UpdateAnimation();
    }

    // ====================== 【变大=无敌+闪白】 ======================
    public void StartBigSize()
    {
        if (isBig) return;
        StartCoroutine(BigSizeCoroutine());
    }

    IEnumerator BigSizeCoroutine()
    {
        isBig = true;
        // 变大
        transform.localScale = originalScale * sizeMultiplier;

        // 🔥 变大期间：开启无敌 + 一直闪白
        isInvincible = true;
        StartCoroutine(FlashWhileBig());

        // 维持 sizeDuration 秒
        yield return new WaitForSeconds(sizeDuration);

        // 恢复大小
        transform.localScale = originalScale;
        isBig = false;

        // 关闭无敌
        isInvincible = false;
        // 恢复颜色
        if (playerRenderer != null)
            playerRenderer.material.color = originalColor;
    }

    // 变大期间一直闪白
    IEnumerator FlashWhileBig()
    {
        while (isBig && playerRenderer != null)
        {
            playerRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            playerRenderer.material.color = originalColor;
            yield return new WaitForSeconds(0.15f);
        }
    }
    // ==============================================================

    void CheckStepOnAI()
    {
        Vector3[] rayOrigins = new Vector3[]
        {
        transform.position + new Vector3(0, 0.2f, 0),
        transform.position + new Vector3(0.4f, 0.2f, 0),
        transform.position + new Vector3(-0.4f, 0.2f, 0),
        transform.position + new Vector3(0, 0.2f, 0.4f),
        transform.position + new Vector3(0, 0.2f, -0.4f),
        transform.position + new Vector3(0.4f, 0.2f, 0.4f),
        transform.position + new Vector3(-0.4f, 0.2f, 0.4f),
        transform.position + new Vector3(0.4f, 0.2f, -0.4f),
        transform.position + new Vector3(-0.4f, 0.2f, -0.4f)
        };

        foreach (Vector3 origin in rayOrigins)
        {
            Debug.DrawLine(origin, origin + Vector3.down * rayDistance, Color.blue, 0.1f);
        }

        if (velocity.y >= 0) return;

        foreach (Vector3 origin in rayOrigins)
        {
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance))
            {
                Ai ai = hit.collider.GetComponent<Ai>();
                if (ai != null)
                {
                    ai.Die();
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    return;
                }
            }
        }
    }
    void Move()
    {
        float input = playerControl.MoveInput;
        Vector3 move = Vector3.right * input;

        float currentSpeed = moveSpeed;
        if (playerGrab != null && playerGrab.isGrabbing)
        {
            currentSpeed *= grabMoveSpeedMultiplier;
        }

        currentSpeed = isGrounded ? currentSpeed : currentSpeed * 0.8f;

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void Rotate()
    {
        float input = playerControl.MoveInput;
        if (input > 0.1f)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), rotateSpeed * Time.deltaTime);
        else if (input < -0.1f)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 180, 0), rotateSpeed * Time.deltaTime);
    }

    void Jump()
    {
        if (playerControl.JumpInput && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateAnimation()
    {
        if (anim == null) return;
        float forward = Mathf.Abs(playerControl.MoveInput) > 0.01f ? 1f : 0f;
        anim.SetFloat("Forward", forward);
        anim.SetBool("IsJumping", !isGrounded);
    }

    public void TakeDamage(int damage)
    {
        // 🔥 变大期间完全无敌，不受伤害
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        Debug.Log("玩家血量：" + currentHealth);

        StartCoroutine(InvincibleFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        if (explosionEffect != null)
        {
            Vector3 explosionPos = transform.position + Vector3.up * 1f;
            Instantiate(explosionEffect, explosionPos, transform.rotation);
        }
        gameObject.SetActive(false);
    }

    IEnumerator InvincibleFlash()
    {
        isInvincible = true;

        for (int i = 0; i < 2; i++)
        {
            if (playerRenderer != null) playerRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.25f);
            if (playerRenderer != null) playerRenderer.material.color = originalColor;
            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.5f);
        isInvincible = false;
    }
}
using UnityEngine;

// AI自动控制脚本（替代玩家输入，自动左右移动 + 概率跳跃）
public class AIControl : MonoBehaviour
{
    // 对外提供移动输入值（和PlayerControl完全一样）
    public float MoveInput { get; private set; }

    // 跳跃输入
    public bool JumpInput { get; private set; }

    [Header("AI设置")]
    public float switchTime = 2f;       // 每2秒切换方向
    public float jumpCheckInterval = 0.5f; // 跳跃检测间隔（0.5秒）
    [Range(0f, 1f)]
    public float jumpProbability = 0.3f;  // 跳跃概率（30%）

    private float timer;         // 方向切换计时器
    private float jumpTimer;     // 跳跃检测计时器

    void Start()
    {
        // 初始默认向右
        MoveInput = 1f;
        timer = switchTime;
        jumpTimer = jumpCheckInterval;
    }

    void Update()
    {
        // ==================== 自动左右移动逻辑（不变） ====================
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            MoveInput = -MoveInput; // 切换方向
            timer = switchTime;     // 重置计时器
        }

        // ==================== 概率跳跃逻辑（新增） ====================
        JumpInput = false; // 每帧先重置跳跃状态
        jumpTimer -= Time.deltaTime;

        // 每0.5秒检测一次
        if (jumpTimer <= 0f)
        {
            // 生成0~1随机数，小于0.3则触发跳跃
            if (Random.value < jumpProbability)
            {
                JumpInput = true;
            }

            jumpTimer = jumpCheckInterval; // 重置检测计时器
        }
    }
}
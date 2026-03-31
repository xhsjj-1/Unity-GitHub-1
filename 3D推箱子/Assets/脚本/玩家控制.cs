using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float MoveInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool GrabInput { get; private set; }

    // 🔥 新增：玩家当前朝向（1=右，-1=左）
    public int FaceDirection { get; private set; } = 1;

    void Update()
    {
        MoveInput = Input.GetAxis("Horizontal");
        JumpInput = Input.GetKeyDown(KeyCode.Space);
        GrabInput = Input.GetMouseButtonDown(0);

        // 🔥 自动更新朝向
        if (MoveInput > 0.1f) FaceDirection = 1;
        else if (MoveInput < -0.1f) FaceDirection = -1;
    }
}
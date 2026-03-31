using UnityEngine;

[RequireComponent(typeof(PlayerControl))]
public class PlayerGrab : MonoBehaviour
{
    [Header("抓取设置")]
    public float grabDistance = 3f;
    public float holdDistance = 1.2f;
    public float smoothSpeed = 15f;
    public float raycastHeight = 1f;

    [Header("动画控制")]
    public string grabBoolName = "Take"; // 你动画控制器里的布尔参数名

    // 组件引用
    private PlayerControl playerControl;
    private Animator anim; // 直接使用玩家身上原本的animator
    private GameObject grabbedObject;
    private Rigidbody grabbedRb;
    public bool isGrabbing = false;

    // 你要的控制开关
    public bool take = false;

    void Awake()
    {
        playerControl = GetComponent<PlayerControl>();
        anim = GetComponent<Animator>(); // 直接获取玩家自身的animator
    }

    void Update()
    {
        // 鼠标左键抓取/放下
        if (playerControl.GrabInput)
        {
            if (!isGrabbing)
                TryGrab();
            else
                Release();
        }

        // 自动同步动画：take 为 true 激活抓取动画
        if (anim != null)
        {
            anim.SetBool(grabBoolName, take);
        }
    }

    void FixedUpdate()
    {
        if (isGrabbing && grabbedObject != null)
            HoldObject();
    }

    void TryGrab()
    {
        Vector3 rayStart = transform.position + Vector3.up * raycastHeight;
        Vector3 direction = transform.right; // 玩家自身X轴方向
        Ray ray = new Ray(rayStart, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
        {
            if (hit.collider.CompareTag("Box"))
            {
                grabbedObject = hit.collider.gameObject;
                grabbedRb = grabbedObject.GetComponent<Rigidbody>();

                if (grabbedRb != null)
                    grabbedRb.isKinematic = true;

                isGrabbing = true;
                take = true; // 开启抓取动画
                Debug.Log("✅ 抓取成功");
            }
        }
    }

    void HoldObject()
    {
        // 物体永远跟随玩家本地 X 轴
        Vector3 targetPos = transform.position
                            + transform.right * holdDistance
                            + Vector3.up * raycastHeight;

        grabbedObject.transform.position = Vector3.Lerp(
            grabbedObject.transform.position,
            targetPos,
            smoothSpeed * Time.fixedDeltaTime
        );

        grabbedObject.transform.rotation = transform.rotation;
    }

    void Release()
    {
        if (grabbedRb != null)
            grabbedRb.isKinematic = false;

        isGrabbing = false;
        take = false; // 关闭抓取动画
        grabbedObject = null;
        grabbedRb = null;
        Debug.Log("✅ 已放下");
    }

    // 射线可视化（选中玩家可见）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 rayStart = transform.position + Vector3.up * raycastHeight;
        Gizmos.DrawLine(rayStart, rayStart + transform.right * grabDistance);
    }
}
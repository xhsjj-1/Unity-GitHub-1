using UnityEngine;

public class Potion : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 通知玩家变大
            other.GetComponent<PlayerMovement>().StartBigSize();

            // 药水立即消失
            Destroy(gameObject);
        }
    }
}
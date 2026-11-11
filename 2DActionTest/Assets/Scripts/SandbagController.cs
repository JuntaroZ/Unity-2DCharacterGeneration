using UnityEngine;

public class SandboxController : MonoBehaviour
{
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log("KarenController Awake");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LeftJabHitBox"))
        {
            Debug.Log("左ジャブが当たった");
            // 左ジャブのダメージ処理
            animator.SetTrigger("upper_damage");
        }
        else if (other.CompareTag("MiddleKickHitBox"))
        {
            Debug.Log("ミドルキックが当たった");
            // ミドルキックのダメージ処理
            animator.SetTrigger("middle_damage");
        }
        else if (other.CompareTag("LowKickHitBox") || other.CompareTag("LowKickHitBox2"))
        {
            Debug.Log("ローキックが当たった");
            // ローキックのダメージ処理
            animator.SetTrigger("low_damage");
        }
    }    
}

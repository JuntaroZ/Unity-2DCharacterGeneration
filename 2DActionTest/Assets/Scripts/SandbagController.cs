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
        animator.SetTrigger("upper_damage");
        Debug.Log($"ジャブ接触：{other.name}");
    }    
}

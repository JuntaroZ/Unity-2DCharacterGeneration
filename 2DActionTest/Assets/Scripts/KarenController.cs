using UnityEngine;

public class KarenController : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        Debug.Log("KarenController Awake");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed");
        }
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            animator.SetTrigger("left_jab");
            Debug.Log("Left jab triggered");
        }
    }
}
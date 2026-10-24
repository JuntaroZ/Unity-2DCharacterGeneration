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
            Debug.Log("left_jab triggered");
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            animator.SetTrigger("right_middle_kick");
            Debug.Log("right_middle_kick triggered");
        }
    }
}
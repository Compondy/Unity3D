using UnityEngine;

public class SlideStateBehaviour : StateMachineBehaviour
{
    [SerializeField] private float  slideHeight = 1f;
    [SerializeField] private float standHeight = 2f;

    private CapsuleCollider _capsule;

    // Вызывается при входе в состояние
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_capsule == null)
            _capsule = animator.GetComponent<CapsuleCollider>();

        if (_capsule != null)
        {
            _capsule.height = slideHeight;
            _capsule.center = new Vector3(0, slideHeight / 2, 0);
        }
        animator.SetBool("IsSliding", true);
    }

    // Вызывается при выходе из состояния
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_capsule == null)
            _capsule = animator.GetComponent<CapsuleCollider>();

        if (_capsule != null)
        {
            _capsule.height = standHeight;
            _capsule.center = new Vector3(0, 0.7f, 0);
        }
        animator.SetBool("IsSliding", false);
    }
}
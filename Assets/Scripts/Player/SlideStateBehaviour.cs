using UnityEngine;

public class SlideStateBehaviour : StateMachineBehaviour
{
    [SerializeField] private float  slideHeight = 1f;
    [SerializeField] private float standHeight = 2f;

    private CapsuleCollider _capsule;
    private float _originalHeight;
    private Vector3 _originalCenter;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_capsule == null)
            _capsule = animator.GetComponent<CapsuleCollider>();

        if (_capsule != null)
        {
            _originalHeight = _capsule.height;
            _originalCenter = _capsule.center;
            _capsule.height = slideHeight;
            _capsule.center = new Vector3(0, slideHeight / 2, 0);
        }
        animator.SetBool("IsSliding", true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_capsule != null)
        {
            _capsule.height = standHeight;
            _capsule.center = new Vector3(0, 0.7f, 0);
        }
        animator.SetBool("IsSliding", false);
        var pc = animator.GetComponent<PlayerController>();
        if (pc != null) pc.ForceStopSlide();
    }
}
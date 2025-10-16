using UnityEngine;

public class Player_CController : MonoBehaviour
{
    Vector3 moveDirection;
    CharacterController characterCnt;
    public float speed = 5.0f;

    private void Start()
    {
        characterCnt = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // �L�����N�^�[�̃��[�J�����W���l�����Ĉړ��������v�Z
        Vector3 forwardMovement = transform.forward * verticalInput;
        Vector3 rightMovement = transform.right * horizontalInput;

        moveDirection = (forwardMovement + rightMovement).normalized; // ���K�����Ď΂߈ړ��̑��x�����ɕۂ�

        moveDirection.y += Physics.gravity.y * Time.deltaTime; // �d�͕͂ʓr���Z

        // CharacterController.Move�͏d�͂̉e����ʓr�l������K�v������܂�
        // �����moveDirection.y�ɒ��ډ��Z���Ă��܂�
        characterCnt.Move(moveDirection * speed * Time.deltaTime);

        if (characterCnt.isGrounded)
        {
            moveDirection.y = 0; // �n�ʂɂ���Ƃ���Y���̑��x�����Z�b�g
        }
    }
}

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

        // キャラクターのローカル座標を考慮して移動方向を計算
        Vector3 forwardMovement = transform.forward * verticalInput;
        Vector3 rightMovement = transform.right * horizontalInput;

        moveDirection = (forwardMovement + rightMovement).normalized; // 正規化して斜め移動の速度を一定に保つ

        moveDirection.y += Physics.gravity.y * Time.deltaTime; // 重力は別途加算

        // CharacterController.Moveは重力の影響を別途考慮する必要があります
        // 今回はmoveDirection.yに直接加算しています
        characterCnt.Move(moveDirection * speed * Time.deltaTime);

        if (characterCnt.isGrounded)
        {
            moveDirection.y = 0; // 地面にいるときはY軸の速度をリセット
        }
    }
}

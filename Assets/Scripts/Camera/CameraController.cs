using UnityEngine;

public class CaneraRotation : MonoBehaviour
{
    Vector3 diff; //ターゲット距離の差
    GameObject player;

    public float followSpeed = 8.0f; //カメラの追従スピード

    //カメラの初期位置/初期回転
    public Vector3 defaultPos = new Vector3(0, 2.5f, -2.0f);
    public Vector3 defaultRotate = new Vector3(20, 0, 0);

    //マウス感度
    float mouseSensitivity = 3.0f;

    //カメラ回転用変数
    float minVerticalAngle = -15.0f; //カメラ角度上の限界値
    float maxVerticalAngle = 20.0f; //カメラ角度下の限界値

    //プレイ中の角度
    public float verticalRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //画面中心にカーソルをロック
        Cursor.visible = false; //カーソルを非表示
        player = GameObject.FindGameObjectWithTag("Player");
        transform.position = player.transform.position + defaultPos; //プレイヤーから相対的な位置
        transform.rotation = Quaternion.Euler(defaultRotate); //角度を3D回転にする

        diff = player.transform.position - transform.position; //プレイヤーとカメラの距離
    }

    void LateUpdate()
    {
        if (GameManager.gameState != GameState.playing || player == null)
            return;

        //マウスの動き
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity; //横のマウスの動きの量に速さをかけて代入
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity; //上下のマウスの動きの量に速さをかけて代入


        //プレイヤーの回転を生成
        player.transform.Rotate(Vector3.up * mouseX);


        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        //プレイヤーの角度を含めた位置とカメラの位置の差分
        Vector3 targetCameraPosition = player.transform.position - (player.transform.rotation * diff);

        //カメラの位置
        transform.position = Vector3.Lerp(transform.position, targetCameraPosition, followSpeed * Time.deltaTime);

        //x成分のオイラー角を3D回転にしたものとy成分のオイラー角を3D回転にしたものをかけて回転とする
        Quaternion targetRotation = Quaternion.Euler(0, player.transform.eulerAngles.y, 0) * Quaternion.Euler(verticalRotation, 0, 0);

        //角度をtargetRotationに滑らかに回転させる
        transform.rotation = Quaternion.Slerp(transform.rotation,
        targetRotation,
        followSpeed * Time.deltaTime);
    }
}
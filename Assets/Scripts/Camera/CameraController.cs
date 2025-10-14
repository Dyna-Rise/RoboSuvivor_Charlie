using UnityEngine;

public class CaneraRotation : MonoBehaviour
{
    GameObject player;
    public float sensitivity = 3f; //カメラの動く速度

    //カメラ回転用変数
    float verticalRotation = 100f; //カメラの動く角度上下の値
    float horizontalRotation = 0; //カメラの動く角度左右の値

    float minVerticalRotationAngle = -15.0f; //カメラ角度上の限界値
    float maxVerticalRotationAngle = 30.0f; //カメラ角度下の限界値

    // public float minHorizontalAngle = -60.0f; //カメラ角度左の限界値
    // public float maxHorizontalAngle = 60.0f; //カメラ角度右の限界値

    //カメラズーム用変数
    public float zoomSpeed = 5f; //カメラのズームスピード
    public float minZoom = 2f; //ズーム限界
    public float maxZoom = 10f; //ズームアウト限界
    float currentZoom = 5f; //ズーム初期値

    public float currentZoomHeight = 3.0f; // プレイヤーの頭の少し上


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //画面中心にカーソルをロック
        Cursor.visible = false; //カーソルを非表示
        
        
        player = GameObject.FindGameObjectWithTag("Player");
        
    }

    // Update is called once per frame
    void Update()
    {
        //プレイ状態でなければ動かせないようにしておく
        if (GameManager.gameState != GameState.playing) return;

        //マウスの動き
        float mouseX = Input.GetAxis("Mouse X") * sensitivity; //横のマウスの動きの量に速さをかけて代入
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity; //上下のマウスの動きの量に速さをかけて代入
        float scroll = Input.GetAxis("Mouse ScrollWheel"); //マウススクロールのホイール量を取得して代入

        horizontalRotation += mouseX; //横のマウスの動きの量を加算
        // horizontalRotation = Mathf.Clamp(horizontalRotation, minHorizontalAngle, maxHorizontalAngle); //横方向のカメラの動きの制限、最小、最大

        verticalRotation += mouseY; //縦のマウスの動きの量を加算
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalRotationAngle, maxVerticalRotationAngle); //縦方向のカメラの動きの制限、最小、最大

        //カメラの回転を生成、カメラ自身の回転
        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);

        //プレイヤーの回転を生成
        player.transform.Rotate(Vector3.up * mouseX);

        //マウスホイール上下させるとズームインアウトする処理
        currentZoom -= scroll * zoomSpeed;

        //ズームアウトとズームインの限界値を代入
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        // new Vector3(0, currentZoomHeight, -currentZoom)
        //プレイヤーが回転してもカメラが後ろに来るようにoffsetを定義
        // Y軸方向に currentZoomHeight → カメラを上に持ち上げる
        // Z軸方向に -currentZoom → プレイヤーの後ろに下がる
        
        // Quaternion.Euler(verticalRotation, horizontalRotation, 0f)
        //カメラ回転を offset に反映
        //verticalRotationで上下方向、horizontalRotationで左右方向にoffsetが回転
        Vector3 offset = Quaternion.Euler(verticalRotation, horizontalRotation, 0f) * new Vector3(0, currentZoomHeight, -currentZoom);

        //カメラの位置をoffset分ずらす
        transform.position = player.transform.position + offset;
    }
}

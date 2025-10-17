using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject bulletPrefab; //バレットプレハブ
    Transform player; //プレイヤーのトランスフォーム情報
    GameObject gate; //プレイヤーについているgateオブジェクトの情報

    public float shootPower = 100f; //弾速
    public int RecoverySeconds = 3; //残弾数回復秒数
    bool enableShot; //ショット可能フラグ
    bool isAttack; //攻撃中フラグ

    bool bulletRecover; //弾回復中フラグ

    Camera cam; //カメラ情報の取得

    AudioSource audio;
    public AudioClip se_shot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //プレイヤーのtransform情報の取得
        player = GameObject.FindGameObjectWithTag("Player").transform;

        //プレイヤー子要素のGateオブジェクトのオブジェクト情報の取得
        gate = player.transform.Find("Gate").gameObject;

        //カメラ情報の取得
        cam = Camera.main;

        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.gameState != GameState.playing)
        {
            enableShot = false;
            return;
        }

        enableShot = true;
        if (Input.GetMouseButtonDown(0))
        {
            if (enableShot)
            {
                Shot();
            }
        }

        //通常時かつ打っていない時にリロードかつ回復中に回復したくないので
        if (!bulletRecover && !isAttack)
            StartCoroutine(RecoverBullet()); //回復コルーチン

    }

    //弾発射
    void Shot()
    {
        //プレイヤーがいない、残弾数が0、攻撃中の時、何もせずリターンする
        if (player == null || GameManager.shotRemainingNum <= 0 || isAttack)
            return;

        //プレイヤーの位置にBulletを作成、gateの角度に生成 → camの角度に生成に変更、気持ち弾道の正面から見えるようになる
        GameObject obj = Instantiate(bulletPrefab, gate.transform.position, cam.transform.rotation * Quaternion.Euler(90, 0, 0));
        


        //生成したBulletのRigidbodyを取得
        Rigidbody rbody = obj.GetComponent<Rigidbody>();

        //カメラの角度を考慮した方向を生成
        // Vector3 v = new Vector3(
        //        cam.transform.forward.x * shootPower,
        //        cam.transform.forward.y,
        //        cam.transform.forward.z * shootPower
        //  );


        //  カメラの向きに飛ばすように変更
        // Vector3 v = (cam.transform.forward + Vector3.up * 0.3f).normalized * shootPower; //別の書き方
        Vector3 v = cam.transform.forward * shootPower;

        //生成した球のAddForceの力でシュート
        // rbody.AddForce(v + new Vector3(0, 20, 0), ForceMode.Impulse);
        rbody.AddForce(v + new Vector3(0,20,0), ForceMode.Impulse);
        
        isAttack = true;
        SEPlay(SEType.Shot);

        ConsumeBullet();

        Invoke("ResetAttack", 0.2f);

    }

    //残弾消費
    void ConsumeBullet()
    {
        GameManager.shotRemainingNum--; //消費

    }

    //残弾回復コルーチン
    IEnumerator RecoverBullet()
    {
        bulletRecover = true; //弾回復フラグオン
        //RecoverySeconds秒まつ
        yield return new WaitForSeconds(RecoverySeconds);

        if (GameManager.shotRemainingNum < 20)
        {
            GameManager.shotRemainingNum++; //1つ回復
            //bulletRecover = false; //弾回復フラグオフ
        }
        bulletRecover = false; //弾回復フラグオフ
    }

    //攻撃中解除
    void ResetAttack()
    {
        isAttack = false;
    }

    public void SEPlay(SEType type)
    {
        audio.PlayOneShot(se_shot);
        
    }

}

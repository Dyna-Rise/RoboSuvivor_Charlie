using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Bossコントロール・クラス
/// </summary>
public class BossController : MonoBehaviour
{
    //<<< 定数設定値 >>>
    const float StunDuration = 0.5f;                // スタン状態の時間

    //<<< パブリック変数 >>>
    public int bossHP = 30;                          // ヒットポイント
    public GameObject body;                         // 自身のボディ(点滅対象)
    public float closeRange = 3f;                   // プレイヤーとの距離が近いと判断する距離(近接攻撃をする距離)
    public float barrierDeploymentDistance = 5f;    // パリアを展開させるプレイヤーとの距離 
    public float attackInterval = 5f;               // 攻撃のクールダウン
    public GameObject bulletPrefab;                 // 飛ばす弾のプレハブ
    public float bulletSpeed = 100.0f;              // 弾の速度
    public bool barrierDeployment;                  // バリアを展開しているか(false=消去中/true=展開中)
    public float moveSpeed = 0.5f;                  // 移動スピード(タックルの移動速度)
    public GameObject gate;                         // 弾を生成する位置
    public GameObject barrierPrefab;                // パリアープレハブ

    //<<< ローカル変数 >>>
    GameObject player;                  // プレイヤー情報
    float barrierEffecticeCount;	    // バリアー発生時間カウンタ

    Rigidbody rbody;                    // Rigidbody
                                        // Animator animator;                // Animator
    bool isFar = false;                 // プレイヤーとの距離が遠いと判断しているフラグ(false=近い/true=遠い)    private float distance;             // プレイヤーとの距離
    float playerDistance;               // プレイヤーとの距離
    float recoverTime = 0.0f;
    bool isDamage;　                     //ダメージフラグ
    float timer;                         // 時間経過
    bool isAttacking;                    // 攻撃中かどうか

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // オブジェクトの取得
        rbody = GetComponent<Rigidbody>();    // Rigidbodyを得る
                                              //        animator = GetComponent<Animator>();    //Animatorを得る
        player = GameObject.FindGameObjectWithTag("Player"); //プレイヤー情報を得る
                                                             // バリア子オブジェクトを取得する
                                                             //barrierPrefab = transform.Find("Barrier").gameObject;

        // パリアを非表示にする
        //        barrierPrefab.SetActive(false);
        //        animator.SetBool("Active", true);

    }


    // Update is called once per frame
    void Update()
    {
        // playingモードでないと何もしない
        if (GameManager.gameState != GameState.playing) return;

        //プレイヤーがいない時は何もしない
        if (player == null) return;

        // もしスタン中かLifeが0なら点滅処理
        if (IsStun())
        {
            //moveDirection.x = 0;
            //moveDirection.z = 0;
            // 復活までの時間をカウント
            recoverTime -= Time.deltaTime;

            // 点滅処理
            Blinking();
        }
        else
        {
            // 自身の位置をプレイヤーに向ける
            transform.LookAt(player.transform);

            // プレイヤーと自身の位置の差
            //            playerDistance = Vector3.Distance(transform.position, player.transform.position);
        }
    }

    private void FixedUpdate()
    {
        //playingモードでないと何もしない
        if (GameManager.gameState != GameState.playing) return;

        //プレイヤーがいない時は何もしない
        if (player == null) return;

        //<<< 動作タイマー処理 >>>
        // 攻撃処理中は何もしない
        // タイマーもクリア
        if (isAttacking)
        {
            timer = 0.0f;
            return;
        }
        else
        {
            // タイマーが攻撃のクールダウンカウントアップするまで何もしない
//            Debug.Log($"AttackingInterval={attackInterval} Timer={timer}");
            timer += Time.deltaTime;
            if (timer <= attackInterval)
            {
                return;
            }
        }

        //<<< 攻撃処理 >>>
        // プレイヤーとの距離を求める
        playerDistance = Vector3.Distance(transform.position, player.transform.position);
        Debug.Log($"longDistance={closeRange} playerDistance={playerDistance}");
        // プレイヤーとの距離を判定する
        // 遠距離判定距離よりもプレイヤーとの距離が近いか？
        if (closeRange >= playerDistance)  // プレイヤーとの距離が近い時
        {
            Debug.Log("Is Near");
            // 攻撃実行中でなければ近距離攻撃を行う
            if (!isAttacking)
            {
                // 攻撃中フラグをセットする
                isAttacking = true;                    // 攻撃中かどうか

                // バリア展開コルーチンを呼び出す
                Debug.Log($"longDistance={closeRange} playerDistance={playerDistance}");
                StartCoroutine(Barrier());
            }
        }
        else  // プレイヤーとの距離が遠い時
        {
            Debug.Log("Is Far");
            //////Debug.Log($"Is Attacking={isAttacking}");
            // 現在プレイヤーとの距離が近いと判断している           
            // 距離が遠い判定フラグをセットする
            isFar = true;

            // ランダムで遠距離処理選択用の0,1の2択の値を作成する
            int rnd = Random.Range(0, 2); // ※ 0～1の範囲でランダムな整数値が返る
//            rnd = 1;    // debag
            rnd = 0;
            // 攻撃中では無い？
            if (!isAttacking)
            {
                // 攻撃中フラグをセットする
                isAttacking = true;                    // 攻撃中かどうか

                //Debug.Log($"Is Attacking={isAttacking}");
                if (rnd == 0)
                {
                    Debug.Log("Tackle()");
                    // タックルコルーチンを呼び出す
                    StartCoroutine(Tackle());
                }
                else
                {
                    //Debug.Log("Shot()");
                    // ショットコルーチンを呼び出す
                    StartCoroutine(Shot());
                }
            }
        }

        // 移動
        // ※常に移動する場合はコメントアウトを外す
        //       rbody.linearVelocity = new Vector3(axisH, axisV).normalized;

    }

    /// <summary>
    /// バリア処理コルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator Barrier()
    {
        Debug.Log("Barrier()");

        // バリア発生までのウエイト(プレイヤーが斬撃するための隙を作る)
        yield return new WaitForSeconds(3.0f);

        // バリア発生中時間カウンタをクリアする
        //barrierEffecticeCount = 0.0f;       // バリアー発生時間カウンタ

        // バリアプレアブを生成する
        Vector3 barrierPos = transform.position;
        barrierPos.y = 2.0f;
//        GameObject barrier = Instantiate(barrierPrefab, transform.position, Quaternion.identity); // 新しい回転を適用
        GameObject barrier = Instantiate(barrierPrefab, barrierPos, Quaternion.identity); // 新しい回転を適用
        Debug.Log($"transform.position={transform.position}");
        Debug.Log("Barrier() Instantiate");
        Vector3 barrierSize = barrier.transform.localScale;
//        barrier.transform.localScale = new Vector3(barrierSize.x / 10.0f, barrierSize.y / 10.0f, barrierSize.z / 10.0f);

        Debug.Log($"Barrier() barrierSize={barrierSize}");
        //<<< バリアを表示する >>>
        // バリアが生成されて範囲が少しずつ広がるイメージ
        barrier.SetActive(true);
        //barrier.transform.localScale = Vector3.Lerp(barrier.transform.localScale, barrierSize, Time.deltaTime);

        //while (barrier.transform.localScale.x < barrierSize.x)
        //{
        //    //            barrier.transform.localScale = Vector3.Lerp(barrier.transform.localScale, barrierSize, Time.deltaTime * 2.0f);  // 一直線上で補間
        //    barrier.transform.localScale = Vector3.Slerp(barrier.transform.localScale, barrierSize, Time.deltaTime * 2.0f); // 円弧を描くように補間
        //    // 一定時間ウエイト
        //    yield return null;    // 1フレーム処理を待ちます。
        //}

        //<<< バリア発生時間が経過するまでループ >>>
        //while (barrierEffecticeCount < barrierEffectiveTime)
        //{
        //    // 他の処理でバリアが解除されたらバリア処理を終了する
        //    if (!barrierDeployment) break;

        //    // 一定時間ウエイト
        //    yield return null;    // 1フレーム処理を待ちます。

        //    // バリア展開中カウンタを加算する
        //    barrierEffecticeCount += Time.deltaTime;	    // バリアー発生時間カウンタ
        //}

        // パリアを廃棄する
        //        Destroy(barrier);

        // バリアが消滅するのを待つ
        while (barrier != null)
        {
            yield return null;
        }

        // 攻撃中フラグをクリアする
        isAttacking = false;                    // 攻撃中かどうか
    }

    /// <summary>
    /// タックルコルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator Tackle()
    {
        Debug.Log("Tackle()");


        float maxSpeed = 10f;       // 最大速度
        float stopDistance = 10.5f;  // 接近停止距離
        float currentSpeed = 0f;

        Vector3 barrierSize = barrierPrefab.transform.localScale;
        //Debug.Log($"Tackle barrierSize={barrierSize}");

        // プレイヤーの位置を取得する
        Vector3 playerPosition = player.transform.position;

        //        while (Vector3.Distance(transform.position, playerPosition) > stopDistance)
        while (Vector3.Distance(transform.position, playerPosition) >  closeRange)             // プレイヤーとの距離が近いと判断する距離(近接攻撃をする距離)
        {
            transform.position = Vector3.Lerp(transform.position, playerPosition, Time.deltaTime);
            // 次のフレームまで待機
            yield return null;
        }
        // 攻撃中フラグをクリアする
        isAttacking = false;           // 攻撃中かどうか
        Debug.Log("isAttacking = fales");
        //// 対象への方向
        //Vector3 direction = (playerPosition - transform.position).normalized;

        //    // 加速
        //    currentSpeed += moveSpeed * Time.deltaTime;
        //    currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

        //    // 移動
        //    transform.position += direction * currentSpeed * Time.deltaTime;

        //    //            barrier.transform.localScale = Vector3.Lerp(barrier.transform.localScale, barrierSize, Time.deltaTime * 2.0f);  // 一直線上で補間
        //    //barrier.transform.position = Vector3.Slerp(barrier.transform.position, playerPosition, Time.deltaTime * currentSpeed); // 円弧を描くように補間

        //    // 次のフレームまで待機
        //    yield return null;
    }


    //※コルーチンの外でRigidBody.AddForce()を使用して加速移動する方法
    //// 対象への方向ベクトルを正規化
    //Vector3 direction = (player.transform.position - transform.position).normalized;

    //// 加速度を加える
    //rbody.AddForce(direction * speed, ForceMode.Acceleration);

    //// 最大速度を制限
    //if (rbody.angularVelocity.magnitude > 10f)
    //{
    //    rbody.angularVelocity = rbody.angularVelocity.normalized * 10.0f;
    //}



    /// <summary>
    /// ショットコルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator Shot()
{
        //Debug.Log("Shot()");
    //        Transform gate = transform.Find("Gate");
    int burstCount = 3; // バーストショット回数
    int shotCount = 0;

    Vector3 currentPosityon = transform.position;

    // バーストショット処理
    while (burstCount > shotCount)
    {
        //            transform.position = currentPosityon;

        // ゲートを攻撃対象に向ける
        gate.transform.LookAt(player.transform);

        // 射出方向の回転に加えて、Bulletが横向きになるような回転を適用
        // gate.transform.rotationは、Gateが向いている方向にBulletのZ軸を合わせる
        // Quaternion.Euler(90, 0, 0)は、X軸を90度回転させることで、シリンダーのY軸（長い方）をZ軸方向（前）に倒す
        Quaternion bulletRotation = gate.transform.rotation * Quaternion.Euler(90, 0, 0);
        GameObject bullet = Instantiate(bulletPrefab, gate.transform.position, bulletRotation); // 新しい回転を適用

        // 弾のRigidbodyを読みだす
        Rigidbody bulletRbody = bullet.GetComponent<Rigidbody>();

        // 弾を打ち出す
        shotCount++;
        bulletRbody.AddForce(gate.transform.forward * 100.0f, ForceMode.Impulse);

        rbody.AddRelativeForce(transform.forward * -0.5f, ForceMode.Impulse);

        // とりあえず弾を消去する
        StartCoroutine(DestroyBullet(bullet));

        // バーストショットで次弾を打ち出す迄のウエイト
        yield return new WaitForSeconds(0.2f);
    }

    // 次弾発射可能になるまでのインターバル
    yield return new WaitForSeconds(3f);

    // 攻撃中フラグをクリアする
    isAttacking = false;           // 攻撃中かどうか
}

/// <summary>
/// とりあえず弾を消去する
/// </summary>
/// <param name="bullet"></param>
/// <returns></returns>
IEnumerator DestroyBullet(GameObject bullet)
{
    // とりあえず0.5秒後に弾を消去する
    yield return new WaitForSeconds(0.5f);
    Destroy(bullet);
}

/// <summary>
/// スタン（気絶状態）かどうか
/// </summary>
/// <returns></returns>
bool IsStun()
{
    // recoverTimeが稼働中かHPが0になった場合はsturnフラグがON
    bool stun = recoverTime > 0.0f || bossHP <= 0;

    // ※StunフラグがOFFの場合はボディを確実に表示
    if (!stun) body.SetActive(true);

    // Stunフラグをリターン
    return stun;
}

/// <summary>
/// 点滅処理 
/// </summary>
void Blinking()
{
    // 点滅演出
    // Sinメソッドの角度情報にゲーム開始からの経過時間を与える
    float val = Mathf.Sin(Time.time * 50);

    if (val > 0)
    {
        body.SetActive(true);
        //// 描画機能を有効
        //gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }
    else
    {
        body.SetActive(false);
        //// 描画機能を無効
        //gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }
}

/// <summary>
/// プレイヤーの弾又はソードとの接触
/// ブリンク表示してHPを減少させる
/// </summary>
/// <param name="collision"></param>
private void OnTriggerEnter(Collider collision)
{
    bool playerSword = false;   // ソードに接触フラグ

    // 接触したのがプレイヤーの弾だったら
    if (collision.gameObject.CompareTag("PlayerBullet"))
    {
        playerSword = false;
    }
    else if (collision.gameObject.CompareTag("PlayerSword"))
    {
        playerSword = true;
    }
    else
    {
        return;
    }

    // 既にプレイヤーの弾と接触中ならば何もしない
    if (IsStun()) return;

    if (playerSword)
    {
        // HPの減算(3倍)更新
        bossHP -= 3;
    }
    else
    {
        // HPの減算更新
        bossHP--;
    }


    // HPの残量による処理
    if (bossHP > 0)
    {
        //recoverTimeの時間を設定
        recoverTime = StunDuration;
    }
    else
    {
        // ゲームオーバー処理
        GameOver();
        //                if (GameManager.gameState == GameState.playing) StartCoroutine(StartEnding());
        // とりあえずゲーム終了
        Application.Quit();//ゲームプレイ終了
    }

}


/// <summary>
/// ゲームオーバー処理
/// </summary>
void GameOver()
{
    Debug.Log("Game over");
    GameManager.gameState = GameState.gameover;

    //recoverTimeの時間をクリア
    recoverTime = 0.0f;

}

/// <summary>
/// エンディングへの切り替え
/// </summary>
/// <returns></returns>
IEnumerator StartEnding()
{
    //ゲームエンド
    //        animator.SetTrigger("Dead");
    rbody.linearVelocity = Vector2.zero;
    //        GameManager.gameState = GameState.ending;
    yield return new WaitForSeconds(10);
    SceneManager.LoadScene("Ending");
}

}

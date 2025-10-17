using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class EnemyController : MonoBehaviour
{
    GameObject player;      // プレイヤーをInspectorから設定
    NavMeshAgent navMeshAgent;     // NavMeshAgentコンポーネント

    //private Animator animator; // Animatorコンポーネント

    public float detectionRange = 80f; // プレイヤーを検知する距離
    public float attackRange = 30f; // 攻撃を開始する距離
    public float stopRange = 5f; //接近限界距離

    public float shootSpeed = 100f; //発射速度

    public int enemyHP = 5; //敵のHP
    public float enemySpeed = 5.0f; //敵のスピード

    bool isDamage; //ダメージ中フラグ
    public GameObject body; //点滅されるbody

    bool isAttack; //攻撃中フラグ

    bool lockOn = true; //ターゲットを向くべき

    float timer; //時間経過

    GameObject gameMgr; //ゲームマネージャー

    public Animator animator;



    //削除される基準のY座標値
    public float deletePosY = -50f;

    public GameObject bulletPrefab;    // 発射する弾のPrefab
    public GameObject gate;            // 弾を発射する位置
    public float bulletSpeed = 100f;   // 発射する弾の速度 
    public float fireInterval = 2.0f;  // 弾を発射するインターバル
    //public float rotationSpeed = 5   // プレイヤーの方向へ回転する速度 (スムーズなLookAt用)

    public GameObject flamePrefab; //倒された時の炎エフェクト


    //音にまつわるコンポーネントとSE音情報
    AudioSource audio;
    public AudioClip se_shot;
    public AudioClip se_damage;
    public AudioClip se_exprosion;

    void Start()
    {
        //playingモードでないと何もしない
        if (GameManager.gameState != GameState.playing) return;
        gameMgr = GameObject.FindGameObjectWithTag("GM");

        audio = GetComponent<AudioSource>();

        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");


        isAttack = false;
        isDamage = false;

        timer = 0f; //経過時間を計測
    }


    void Update()
    {
        //playingモードでないと何もしない
        if (GameManager.gameState != GameState.playing) return;

        //エネミーのHPが0の時は何もしない
        if (enemyHP <= 0) return;

        // プレイヤーオブジェクトが存在しない場合、全ての動作を停止
        if (player == null)
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true; // ナビゲーションを停止
            }
            isAttack = false; // 攻撃中フラグもリセット
            lockOn = false;   // ロックオンも解除
            // Debug.Log("Playerオブジェクトが見つかりません。EnemyControllerを停止します。");
            return; // 以降のUpdate処理をスキップ
        }

        //ステージ外に落ちたら消滅
        if (transform.position.y <= deletePosY)
        {
            Destroy(gameObject);
            return;
        }

        if (isDamage)
        {
            Blinking(); //ダメージ受けたら点滅処理
        }

        //プレイヤーとの距離を常に測る
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= detectionRange) //索敵範囲に入ったら変数enemySpeedのスピードで近づいてくる
        {
            if (lockOn) //ターゲットを向く
            {
                transform.LookAt(player.transform.position); // これだと瞬時に回転
                //LookAtPlayer(); // スムーズな回転
            }
           

            //射程範囲に入ったら攻撃しつつゆっくり近づいいてくる
            if (distance <= attackRange)
            {
                if (isAttack) //攻撃中なら何もしない
                {
                    return;
                }
                
                if (Time.time >= timer) //攻撃間隔
                {
                    StartCoroutine(Attack());

                    //timer = Time.time + 1f / fireInterval; // 次の発射可能時間を更新
                }
                if (distance <= stopRange) //接近限界だったら止まる
                {
                    if (navMeshAgent != null) // navMeshAgentがnullでないことを確認
                    {
                        navMeshAgent.isStopped = true;
                    }
                }
                else //攻撃範囲以内で接近限界より距離があるなら動く
                {
                    if (navMeshAgent != null) // navMeshAgentがnullでないことを確認
                    {
                        navMeshAgent.isStopped = false;
                        navMeshAgent.SetDestination(player.transform.position);
                        navMeshAgent.speed = enemySpeed * 0.5f; //半分の速度にする
                    }
                }
            }
            else　//索敵範囲以内で攻撃範囲より距離があるなら動く
            {
                if (navMeshAgent != null) // navMeshAgentがnullでないことを確認
                {
                    navMeshAgent.isStopped = false;
                    navMeshAgent.SetDestination(player.transform.position);
                    navMeshAgent.speed = enemySpeed;
                }

            }
        }
        else // 索敵範囲外に出たら停止
        {
            if (navMeshAgent != null) // navMeshAgentがnullでないことを確認
            {
                navMeshAgent.isStopped = true;
            }

            lockOn = false; // 索敵範囲外ならロックオン解除
        }


    }


    IEnumerator Attack()
    {
        isAttack = true;
        lockOn = false; // 攻撃中もプレイヤーの方を向かせたいならコメントアウト
        yield return new WaitForSeconds(fireInterval); //プレイヤーの逃げる猶予
        Shot();
        isAttack = false;
        lockOn = true;
        timer = 0f;
    }


    // 弾を発射するメソッド
    void Shot()
    {
        if (GameManager.gameState != GameState.playing) return;

        if (player != null)
        {
            //プレイヤーの位置にBulletを生成
            GameObject obj = Instantiate(
                bulletPrefab,
                gate.transform.position,
                gate.transform.rotation * Quaternion.Euler(90, 0, 0));

            //生成したBulletのRigidbodyを取得
            Rigidbody rbody = obj.GetComponent<Rigidbody>();

            //方向を生成
            Vector3 v = new Vector3(
                gate.transform.forward.x * shootSpeed,
                gate.transform.forward.y * shootSpeed,
                gate.transform.forward.z * shootSpeed);

            //生成した球のAddForceの力でshoot
            rbody.AddForce(v, ForceMode.Impulse);

            SEPlay(SEType.Shot); //射撃音
        }
    }


    //接触判定
    private void OnTriggerEnter(Collider hit)
    {
        if (GameManager.gameState != GameState.playing) return;

        //プレイヤーがいない時 or エネミーのHPが0の時は何もしない
        if (player == null || enemyHP <= 0) return;

        if (isDamage) return; // ダメージ中は処理しない (無敵判定)

        //ぶつかった相手がPlayerBulletなら
        if (hit.gameObject.CompareTag("PlayerBullet") || hit.gameObject.CompareTag("PlayerSword"))
        {
            isDamage = true;

            //体力をマイナス
            enemyHP--;

            //ぶつかった相手がPlsyerSwordなら
            if (hit.gameObject.CompareTag("PlayerSword"))
            {
                //体力を3マイナス
                enemyHP -= 3;
            }

            if (enemyHP > 0) SEPlay(SEType.Damage); //ダメージ音

            //ダメージ音を鳴らす
            //SEPlay(SEType.Damage);

            if (enemyHP <= 0)
            {
                animator.SetTrigger("die"); //死亡アニメ
                Instantiate(flamePrefab, transform.position, flamePrefab.transform.rotation); //炎のエフェクトを発生
                SEPlay(SEType.Explosion); //倒した音
                Destroy(gameObject, 2f); //少し時間差で自分を消滅
                DestroyDeadEnemy();
            }

            StartCoroutine(Damaged());

        }
    }


    IEnumerator Damaged()
    {
        yield return new WaitForSeconds(1);
        isDamage = false; //無敵時間終了
        if (!isDamage) body.SetActive(true); // bodyを表示状態に戻す
    }

    //点滅処理
    void Blinking()
    {
        //その時のゲーム進行時間で正か負かの値を算出
        float val = Mathf.Sin(Time.time * 50);
        //正の周期なら表示
        if (val >= 0) body.SetActive(true);
        //負の周期なら非表示
        else body.SetActive(false);
    }

    //SE再生
    public void SEPlay(SEType type)
    {
        switch (type)
        {
            case SEType.Shot:
                audio.PlayOneShot(se_shot);
                break;
            case SEType.Damage:
                audio.PlayOneShot(se_damage);
                break;
            case SEType.Explosion:
                audio.PlayOneShot(se_exprosion);
                break;
        }
    }

    // ギズモで範囲を表示（デバッグ用）
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.DrawWireSphere(transform.position, stopRange);
    }

    // エネミーオブジェクトが破棄されるときにリストの先頭から削除する
    void DestroyDeadEnemy()
    {
        GameManager gm = gameMgr.GetComponent<GameManager>();
        GameObject deadEnemy = gm.enemyList[0];
        gm.enemyList.RemoveAt(0);
        //Destroy(deadEnemy,2f);
    }

}
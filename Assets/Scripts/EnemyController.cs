using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class EnemyController : MonoBehaviour
{
    GameObject player;      // プレイヤーをInspectorから設定
    NavMeshAgent navMeshAgent;     // NavMeshAgentコンポーネント

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



    //削除される基準のY座標値
    public float deletePosY = -50f;

    public GameObject bulletPrefab;    // 発射する弾のPrefab
    public GameObject gate;            // 弾を発射する位置
    public float bulletSpeed = 100f;   // 発射する弾の速度 
    public float fireInterval = 2.0f;  // 弾を発射するインターバル
    //public float rotationSpeed = 5   // プレイヤーの方向へ回転する速度 (スムーズなLookAt用)

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");

        isAttack = false;
        lockOn = false;

        timer = 0f; //経過時間を計測

    }


    void Update()
    {
        //playingモードでないと何もしない
        if (GameManager.gameState != GameState.playing) return;

        //プレイヤーがいない時 or エネミーのHPが0の時は何もしない
        if (player == null || enemyHP <= 0) return;

        //ステージ外に落ちたら消滅
        if (transform.position.y <= deletePosY)
        {
            Destroy(gameObject);
            return;
        }

        if (lockOn) //ターゲットを向く
        {
            transform.LookAt(player.transform.position); // これだと瞬時に回転
            //LookAtPlayer(); // スムーズな回転
        }

        if(isAttack) //攻撃中なら何もしない
        {
            return;
        }

        //プレイヤーとの距離を常に測る
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= detectionRange) //索敵範囲に入ったら変数enemySpeedのスピードで近づいてくる
        {
            lockOn = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(player.transform.position);
            navMeshAgent.speed = enemySpeed;

            //射程範囲に入ったら攻撃しつつゆっくり近づいいてくる
            if (distance <= attackRange)
            {
                
                navMeshAgent.speed = enemySpeed * 0.5f; //半分の速度にする

                if (Time.time >= timer)
                {
                    StartCoroutine(Attack());

                    //timer = Time.time + 1f / fireInterval; // 次の発射可能時間を更新
                }
            }
            if(distance <= stopRange)
            {
                navMeshAgent.isStopped = true;
            }
        }
        else
        {
            navMeshAgent.isStopped = true;
        }



        if (isDamage)
        {
            StartCoroutine(Damaged());
        }
        

    }

    
    IEnumerator Attack()
    {
        isAttack = true;
        lockOn = false; // 攻撃中もプレイヤーの方を向かせたいならコメントアウト
        Shot();
        yield return new WaitForSeconds(fireInterval);
        isAttack = false;
        lockOn = true;
        timer = 0f;  
    }


    // 弾を発射するメソッド
    void Shot()
    {
        //enemyが消滅していれば何もしない
        if (gameObject == null) return;

        if (player != null)
        {
            //プレイヤーの位置にBulletを生成
            GameObject obj = Instantiate(
                bulletPrefab,
                gate.transform.position,
                gate.transform.rotation * Quaternion.Euler(90,0,0));

            //生成したBulletのRigidbodyを取得
            Rigidbody rbody = obj.GetComponent<Rigidbody>();

            //方向を生成
            Vector3 v = new Vector3(
                gate.transform.forward.x * shootSpeed,
                gate.transform.forward.y * shootSpeed,
                gate.transform.forward.z * shootSpeed);

            //生成した球のAddForceの力でshoot
            rbody.AddForce(v, ForceMode.Impulse);
        }
    }


    //接触判定
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //ぶつかった相手がPlayerBulletなら
        if (hit.gameObject.CompareTag("PlayerBullet"))
        {
            isDamage = true;

            //体力をマイナス
            enemyHP--;

            //ダメージ音を鳴らす
            //SEPlay(SEType.Damage);

            if (enemyHP <= 0)
            {
                //Instantiate(bom, transform.position, Quaternion.identity); //爆発エフェクトの発生
                Destroy(gameObject, 0.5f); //少し時間差で自分を消滅
            }

            //接触したPlayerBulletを削除
            Destroy(hit.gameObject);
        }

        //ぶつかった相手がPlsyerSwordなら
        if (hit.gameObject.CompareTag("PlayerSword"))
        {
            isDamage = true;

            //体力を3マイナス
            enemyHP -= 3;

            //ダメージ音を鳴らす
            //SEPlay(SEType.Damage);

            if (enemyHP <= 0)
            {
                //Instantiate(bom, transform.position, Quaternion.identity); //爆発エフェクトの発生
                Destroy(gameObject, 0.5f); //少し時間差で自分を消滅
            }

        }
    }


    IEnumerator Damaged()
    {
        Blinking();
        yield return new WaitForSeconds(2);

        isDamage = false;
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

    // ギズモで範囲を表示（デバッグ用）
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.DrawWireSphere(transform.position, stopRange);
    }


}
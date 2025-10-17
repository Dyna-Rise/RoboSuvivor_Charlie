using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem.Processors;

public class PlayerController : MonoBehaviour
{
    // 公開変数 (インスペクターから設定可能)
    public GameObject body;

    // 移動設定
    public float moveSpeed = 40.0f;
    public float dashSpeed = 80.0f;
    public float gravity = 20.0f;
    public float jumpForce = 8.0f;

    // ダッシュ設定
    public float dashDuration = 0.5f;
    public float dashThreshold = 0.2f;

    // ダメージ設定
    public float invulnerabilityDuration = 2.0f;
    public float blinkInterval = 0.1f;

    // 移動
    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;

    // ダッシュ関連の変数
    private float dashTimer = 0f;
    private float lastMoveTime = 0f;
    private KeyCode lastMoveKey;

    // ダメージ状態フラグ (無敵時間中かどうか)
    public bool isDamage { get; private set; } = false;

    // PlayerAnimation参照
    private PlayerAnimation playerAnimation;

    // PlayerAnimationに渡すための最終的な入力方向（修正版PlayerAnimationで使用）
    [HideInInspector] public Vector3 lastMoveInput = Vector3.forward;


    AudioSource audioSource;

    public AudioClip se_Walk;
    public AudioClip se_Damage;
    public AudioClip se_Explosion;
    public AudioClip se_Jump;


    //足音判定
    float footstepInterval = 0.6f; //足音間隔
    float footstepTimer; //時間計測

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<CharacterController>();
        playerAnimation = GetComponent<PlayerAnimation>();

        if (controller == null)
        {
            Debug.LogError("PlayerController: CharacterControllerがアタッチされていません。");
            enabled = false;
        }
        if (playerAnimation == null)
        {
            Debug.LogWarning("PlayerController: PlayerAnimationが見つかりませんでした。アニメーションは動作しません。");
        }
    }

    void Update()
    {
        // 状態チェック: gameover, pause, option の場合、移動と入力を停止
        if (GameManager.gameState != GameState.playing && GameManager.gameState != GameState.gameclear)
            return;


        // ダメージ中の点滅処理
        if (isDamage)
        {
            Blinking();
        }


        if (controller.isGrounded)
        {
            //　水平入力
            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                moveDirection.x = Input.GetAxisRaw("Horizontal") * moveSpeed;
            }
            else
            {
                moveDirection.x = 0;
            }

            // 垂直入力
            if (Input.GetAxisRaw("Vertical") != 0)
            {
                moveDirection.z = Input.GetAxisRaw("Vertical") * moveSpeed;
            }
            else
            {
                moveDirection.z = 0;
            }

            // スペースキー ("Jump" ボタン) でジャンプ
            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpForce;
                audioSource.PlayOneShot(se_Jump);
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        // 実際の移動の実行
        Vector3 globalDirection = transform.TransformDirection(moveDirection);
        controller.Move(globalDirection * Time.deltaTime);

        //足音
        HandleFootsteps();
    }

    private void OnTriggerEnter(Collider hit)
    {
        // 衝突したオブジェクトのタグが"Enemy" または "EnemyBullet" で、現在無敵時間中（isDamage = true）でない場合
        if (!isDamage)
        {
            if (hit.gameObject.CompareTag("Enemy") || hit.gameObject.CompareTag("EnemyBullet") || hit.gameObject.CompareTag("Barrier"))
            {
                //SetDamageState(true);

                //ダメージ中なら何もしない
                if (isDamage) return;

                isDamage = true; //ダメージ中
                GameManager.playerHP--; //プレイヤーHP現象
                audioSource.PlayOneShot(se_Damage);

                if (GameManager.playerHP <= 0) //プレイヤーHPがなくなったら
                {                   
                    //ゲームオーバーへ
                    StartCoroutine(Dead());
                }

                //ダメージリセット
                StartCoroutine(DamageReset());
            }
        }
    }

    IEnumerator Dead()
    {
        audioSource.PlayOneShot(se_Explosion);
        yield return new WaitForSeconds(3);
        GameManager.gameState = GameState.gameover;
        Destroy(gameObject);
    }

    //ダメージリセットのコルーチン
    IEnumerator DamageReset()
    {
        yield return new WaitForSeconds(1.0f); // 点滅時間

        isDamage = false; //ダメージ中の解除
        body.SetActive(true); //明確に姿を表示
    }

    //点滅メソッド
    void Blinking()
    {
        float val = Mathf.Sin(Time.time * 50);
        if (val > 0) body.SetActive(true);
        else body.SetActive(false);
    }

    //足音
    void HandleFootsteps()
    {
        //プレイヤーが動いていれば
        if (moveDirection.x != 0 || moveDirection.z != 0)
        {
            footstepTimer += Time.deltaTime; //時間計測

            if (footstepTimer >= footstepInterval) //インターバルチェック
            {
                audioSource.PlayOneShot(se_Walk);
                footstepTimer = 0;
            }
        }
        else //動いていなければ時間計測リセット
        {
            footstepTimer = 0f;
        }
    }
}

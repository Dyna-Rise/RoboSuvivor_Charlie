using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // 公開変数 (インスペクターから設定可能)
    public GameObject body; 
    
    // 移動設定
    public float moveSpeed = 5.0f;
    public float dashSpeed = 10.0f; 
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

    void Start()
    {
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
        {
            moveDirection.x = 0;
            moveDirection.z = 0;
            
            // 垂直方向は重力で落下継続
            if (controller.isGrounded)
            {
                 moveDirection.y = -1.0f; // 押し付け
            }
            else
            {
                moveDirection.y -= gravity * Time.deltaTime; 
            }
            
            controller.Move(moveDirection * Time.deltaTime);

            // ゲームオーバー時のアニメーション非表示
            if (GameManager.gameState == GameState.gameover)
            {
                 if (body != null && body.TryGetComponent<Renderer>(out var renderer))
                 {
                     renderer.enabled = false; // 見た目を非表示
                 }
                 if (playerAnimation != null) playerAnimation.enabled = false;
            }

            return;
        }


        // --- 移動・ジャンプ・ダッシュ ---

        // ダッシュ状態の更新
        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
        }

        // ダッシュ中なら dashSpeed を使用
        float currentSpeed = (dashTimer > 0) ? dashSpeed : moveSpeed;
        
        
        // 水平方向の入力と移動計算
        
        float forwardMovement = Input.GetAxis("Vertical");
        float sidewaysMovement = Input.GetAxis("Horizontal");

        // カニ歩きを実現
        Vector3 forwardVector = transform.forward * forwardMovement;
        Vector3 rightVector = transform.right * sidewaysMovement;
        Vector3 horizontalMove = forwardVector + rightVector;
        
        if (horizontalMove.magnitude > 1)
        {
            horizontalMove.Normalize();
        }

        // PlayerAnimationのために、移動入力を記録
        if (horizontalMove.magnitude > 0.01f)
        {
            lastMoveInput = horizontalMove.normalized;
        }

        // ダッシュの連続入力判定
        CheckForDash(KeyCode.W);
        CheckForDash(KeyCode.S);
        CheckForDash(KeyCode.A);
        CheckForDash(KeyCode.D);
        
        
        // ジャンプ処理

        if (controller.isGrounded)
        {
            moveDirection.y = -1.0f; 

            // スペースキー ("Jump" ボタン) でジャンプ
            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpForce;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;


        // 最終的な移動方向と速度の設定

        moveDirection.x = horizontalMove.x * currentSpeed; 
        moveDirection.z = horizontalMove.z * currentSpeed;
        
        // 実際の移動の実行
        controller.Move(moveDirection * Time.deltaTime);
    }
    
    /// <summary>
    /// 方向キーの連続入力によるダッシュ判定ロジック
    /// </summary>
    private void CheckForDash(KeyCode key)
    {
        if (Input.GetKeyDown(key))
        {
            // 前回押されたキーと同じか、かつ時間閾値内か
            if (lastMoveKey == key && Time.time < lastMoveTime + dashThreshold)
            {
                // ダッシュ開始
                dashTimer = dashDuration;
            }
            
            // 最後に押されたキーと時間を更新
            lastMoveKey = key;
            lastMoveTime = Time.time;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 衝突したオブジェクトのタグが"Enemy" または "EnemyBullet" で、現在無敵時間中（isDamage = true）でない場合
        if (!isDamage)
        {
            if (hit.gameObject.CompareTag("Enemy") || hit.gameObject.CompareTag("EnemyBullet"))
            {
                SetDamageState(true);
            }
        }
    }


    /// <summary>
    /// ダメージ状態を設定し、HPを減らし、ゲームオーバー判定を行う
    /// </summary>
    public void SetDamageState(bool damageState)
    {
        // isDamage が false のときに damageState = true が来たら、ダメージ処理を開始
        if (damageState && !isDamage)
        {
            isDamage = true; // 無敵時間開始
            
            // ダメージを受けたときにHPを減らす処理
            if (GameManager.playerHP > 0)
            {
                GameManager.playerHP -= 1; // HPを減らす
            }
            
            // HPが0になったらゲームオーバーにする
            if (GameManager.playerHP <= 0 && GameManager.gameState == GameState.playing)
            {
                GameManager.gameState = GameState.gameover;
            }
            
            // ダメージを受けたとき点滅コルーチンを開始
            if (GameManager.gameState == GameState.playing || GameManager.gameState == GameState.gameclear)
            {
                 StartCoroutine(BlinkEffect());
            }
        }
        // damageState = false が来たとき (点滅コルーチン終了後)
        else if (!damageState)
        {
            isDamage = false; // 無敵時間終了
            // 無敵時間終了時に、ボディを表示状態に戻す
            if (body != null && body.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.enabled = true;
            }
        }
    }

    // 点滅処理（コルーチン）
    private IEnumerator BlinkEffect()
    {
        // 処理続行前にRendererの有無を確認
        if (body == null || !body.TryGetComponent<Renderer>(out var bodyRenderer))
        {
            SetDamageState(false);
            yield break;
        }

        float endTime = Time.time + invulnerabilityDuration;

        while (Time.time < endTime)
        {
            bodyRenderer.enabled = !bodyRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }

        // 点滅終了後、ダメージ状態を解除し、ボディを再表示
        SetDamageState(false);
    }
}

using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator animator;
    public enum GameState { playing, gameover }
    public static GameState currentGameState = GameState.playing;


    void Start()
    {
        // 最初は動かない
        if (animator != null)
        {
            animator.SetBool("walk", false);
        }
    }

    void Update()
    {
        // ゲームオーバー
            if (currentGameState == GameState.gameover)
        {

                animator.SetTrigger("die");
            return; 
        }

        // スペースキーを押した場合ジャンプ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (animator != null)
            {
                animator.SetTrigger("jump");
            }
        }

        bool isMoving = false;
        
        // 上を押した場合
        if (Input.GetAxisRaw("Vertical") > 0)
        {
            isMoving = true;
            animator.SetBool("walk", true);
            animator.SetInteger("direction", 0);
        }

        // 左を押した場合
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            isMoving = true;
            animator.SetBool("walk", true);
            animator.SetInteger("direction", 1);
        }

        // 下を押した場合
        if (Input.GetAxisRaw("Vertical") < 0)
        {
            isMoving = true;
            animator.SetBool("walk", true);
            animator.SetInteger("direction", 2);
        }
        
        // 右を押した場合
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            isMoving = true;
            animator.SetBool("walk", true);
            animator.SetInteger("direction", 3);
        }

        // どの移動キーも押されていない場合停止
        if (!isMoving)
        {
            animator.SetBool("walk", false);
        }
        
        // 移動キーの入力チェック
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        bool isMovingForShot = (Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f);
        
        bool isLeftClickDown = Input.GetMouseButtonDown(0);

        // 移動していない状態で左クリックが押された場合、shot
        if (!isMovingForShot && isLeftClickDown)
        {
            if (animator != null)
            {
                animator.SetTrigger("shot");
            }
        }
    }
}
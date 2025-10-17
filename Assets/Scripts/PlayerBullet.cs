using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBullet : MonoBehaviour
{

    public float deleteTime = 3.0f;
    public float bossDeleteTime = 0.7f;

    void Start()
    {
        //現在のシーン名取得
        Scene currentScene = SceneManager.GetActiveScene();


        //ボスステージの時
        if (currentScene.name == "BossStage")
            Destroy(gameObject, bossDeleteTime);

        //ボスステージ以外の時
        else
            Destroy(gameObject, deleteTime);

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Barrier"))
        {
            Destroy(gameObject);
        }
    }
}

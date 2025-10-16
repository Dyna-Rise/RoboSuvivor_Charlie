using UnityEngine;

public class Camera_CController : MonoBehaviour
{
    GameObject player;
    Vector3 diff;
    float followSpeed = 8f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        diff = player.transform.position - transform.position;
    }

    void LateUpdate()
    {
        Vector3 nextPos = player.transform.position - diff;
        transform.position = Vector3.Lerp(transform.position, nextPos, followSpeed * Time.deltaTime);
    }
}

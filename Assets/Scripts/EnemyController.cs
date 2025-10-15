using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class EnemyController : MonoBehaviour
{
    GameObject player;      // �v���C���[��Inspector����ݒ�
    NavMeshAgent navMeshAgent;     // NavMeshAgent�R���|�[�l���g

    public float detectionRange = 80f; // �v���C���[�����m���鋗��
    public float attackRange = 30f; // �U�����J�n���鋗��
    public float stopRange = 5f; //�ڋߌ��E����

    public float shootSpeed = 100f; //���ˑ��x

    public int enemyHP = 5; //�G��HP
    public float enemySpeed = 5.0f; //�G�̃X�s�[�h

    bool isDamage; //�_���[�W���t���O
    public GameObject body; //�_�ł����body

    bool isAttack; //�U�����t���O

    bool lockOn = true; //�^�[�Q�b�g�������ׂ�

    float timer; //���Ԍo��

    GameObject gameMgr; //�Q�[���}�l�[�W���[



    //�폜�������Y���W�l
    public float deletePosY = -50f;

    public GameObject bulletPrefab;    // ���˂���e��Prefab
    public GameObject gate;            // �e�𔭎˂���ʒu
    public float bulletSpeed = 100f;   // ���˂���e�̑��x 
    public float fireInterval = 2.0f;  // �e�𔭎˂���C���^�[�o��
    //public float rotationSpeed = 5   // �v���C���[�̕����։�]���鑬�x (�X���[�Y��LookAt�p)

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");

        isAttack = false;
        lockOn = false;

        timer = 0f; //�o�ߎ��Ԃ��v��

    }


    void Update()
    {
        //playing���[�h�łȂ��Ɖ������Ȃ�
        if (GameManager.gameState != GameState.playing) return;

        //�v���C���[�����Ȃ��� or �G�l�~�[��HP��0�̎��͉������Ȃ�
        if (player == null || enemyHP <= 0) return;

        //�X�e�[�W�O�ɗ����������
        if (transform.position.y <= deletePosY)
        {
            Destroy(gameObject);
            return;
        }

        if (lockOn) //�^�[�Q�b�g������
        {
            transform.LookAt(player.transform.position); // ���ꂾ�Əu���ɉ�]
            //LookAtPlayer(); // �X���[�Y�ȉ�]
        }

        if(isAttack) //�U�����Ȃ牽�����Ȃ�
        {
            return;
        }

        //�v���C���[�Ƃ̋�������ɑ���
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= detectionRange) //���G�͈͂ɓ�������ϐ�enemySpeed�̃X�s�[�h�ŋ߂Â��Ă���
        {
            lockOn = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(player.transform.position);
            navMeshAgent.speed = enemySpeed;

            //�˒��͈͂ɓ�������U�����������߂Â����Ă���
            if (distance <= attackRange)
            {
                
                navMeshAgent.speed = enemySpeed * 0.5f; //�����̑��x�ɂ���

                if (Time.time >= timer)
                {
                    StartCoroutine(Attack());

                    //timer = Time.time + 1f / fireInterval; // ���̔��ˉ\���Ԃ��X�V
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
        lockOn = false; // �U�������v���C���[�̕��������������Ȃ�R�����g�A�E�g
        Shot();
        yield return new WaitForSeconds(fireInterval);
        isAttack = false;
        lockOn = true;
        timer = 0f;  
    }


    // �e�𔭎˂��郁�\�b�h
    void Shot()
    {
        //enemy�����ł��Ă���Ή������Ȃ�
        if (gameObject == null) return;

        if (player != null)
        {
            //�v���C���[�̈ʒu��Bullet�𐶐�
            GameObject obj = Instantiate(
                bulletPrefab,
                gate.transform.position,
                gate.transform.rotation * Quaternion.Euler(90,0,0));

            //��������Bullet��Rigidbody���擾
            Rigidbody rbody = obj.GetComponent<Rigidbody>();

            //�����𐶐�
            Vector3 v = new Vector3(
                gate.transform.forward.x * shootSpeed,
                gate.transform.forward.y * shootSpeed,
                gate.transform.forward.z * shootSpeed);

            //������������AddForce�̗͂�shoot
            rbody.AddForce(v, ForceMode.Impulse);
        }
    }


    //�ڐG����
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //�Ԃ��������肪PlayerBullet�Ȃ�
        if (hit.gameObject.CompareTag("PlayerBullet"))
        {
            isDamage = true;

            //�̗͂��}�C�i�X
            enemyHP--;

            //�_���[�W����炷
            //SEPlay(SEType.Damage);

            if (enemyHP <= 0)
            {
                //Instantiate(bom, transform.position, Quaternion.identity); //�����G�t�F�N�g�̔���
                Destroy(gameObject, 0.5f); //�������ԍ��Ŏ���������
            }

            //�ڐG����PlayerBullet���폜
            Destroy(hit.gameObject);
        }

        //�Ԃ��������肪PlsyerSword�Ȃ�
        if (hit.gameObject.CompareTag("PlayerSword"))
        {
            isDamage = true;

            //�̗͂�3�}�C�i�X
            enemyHP -= 3;

            //�_���[�W����炷
            //SEPlay(SEType.Damage);

            if (enemyHP <= 0)
            {
                //Instantiate(bom, transform.position, Quaternion.identity); //�����G�t�F�N�g�̔���
                Destroy(gameObject, 0.5f); //�������ԍ��Ŏ���������
            }

        }
    }


    IEnumerator Damaged()
    {
        Blinking();
        yield return new WaitForSeconds(2);

        isDamage = false;
    }

    //�_�ŏ���
    void Blinking()
    {
        //���̎��̃Q�[���i�s���ԂŐ��������̒l���Z�o
        float val = Mathf.Sin(Time.time * 50);
        //���̎����Ȃ�\��
        if (val >= 0) body.SetActive(true);
        //���̎����Ȃ��\��
        else body.SetActive(false);
    }

    // �M�Y���Ŕ͈͂�\���i�f�o�b�O�p�j
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.DrawWireSphere(transform.position, stopRange);
    }


}
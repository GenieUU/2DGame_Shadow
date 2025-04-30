using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    public AudioClip jumpClip;
    public AudioClip runClip;
    public AudioClip slashClip;
    public AudioClip hitClip;
    public AudioClip treadClip;

    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isGrounded;

    private Rigidbody2D rigid;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    // 공격
    public float attackRange = 1f;
    public LayerMask MonsterLayer;
    public Transform attackPoint;

    public float attackCooldown = 0.5f; // 쿨타임
    private bool canAttack = true;

    // Hp관리
    public int maxHP = 3;
    private int currentHP;

    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;
    public Image[] hearts;

    private bool isInvincible = false;
    public float invincibleTime = 1f;

    // 추락
    private Vector2 lastCheckpointPosition;
    public float fallThresholdY = -20f;


    private void Start()
    {
        Time.timeScale = 1f;

        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        currentHP = maxHP;
        UpdateHearts();

        lastCheckpointPosition = transform.position;
    }

    private void Update()
    {
        // 바닥 감지
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        Vector2 moveDirection = Vector2.zero;

        if (isMovingLeft)
        {
            moveDirection = Vector2.left;
        }
        else if (isMovingRight)
        {
            moveDirection = Vector2.right;
        }

        rigid.velocity = new Vector2(moveDirection.x * moveSpeed, rigid.velocity.y);

        // 추락 감지
        if (transform.position.y < fallThresholdY)
        {
            HandleFallOff();
        }

        // 이동 처리
        if (isMovingLeft)
            moveDirection = Vector2.left;
        else if (isMovingRight)
            moveDirection = Vector2.right;

        rigid.velocity = new Vector2(moveDirection.x * moveSpeed, rigid.velocity.y);

        // 달리기 애니메이션
        if (animator != null)
        {
            bool isRunning = moveDirection.x != 0;
            animator.SetBool("isRunning", isRunning);
            if (isRunning && isGrounded && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(runClip);
            }
        }

        // 좌우 반전
        if (moveDirection.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (moveDirection.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Respawn"))
        {
            // 리스폰 위치를 살짝 위로 조정해서 저장 (ex: 0.5f 위로)
            Vector2 offset = new Vector2(0f, 0.5f);
            lastCheckpointPosition = (Vector2)other.transform.position + offset;

            Animator flowerAnimator = other.GetComponent<Animator>();
            if (flowerAnimator != null)
            {
                flowerAnimator.SetTrigger("Flower");
            }
        }
    }



    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            // 위에서 밟았을 경우만 공격
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
                OnAttack(collision.transform);
            else
                OnDamaged(collision.transform.position);
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            // 무조건 데미지
            OnDamaged(collision.transform.position);
        }
    }

    void OnAttack(Transform GroundMonster)
    {
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        
        if (treadClip != null)
            audioSource.PlayOneShot(treadClip);

        // Monster 죽음 처리
        GroundMonsterController monsterController = GroundMonster.GetComponent<GroundMonsterController>();
        if (monsterController != null)
            monsterController.OnDamaged();
    }

    void OnDamaged(Vector2 targetPos)
    {
        // 체력 감소
        if (isInvincible) return;
        {
            if (hitClip != null)
                audioSource.PlayOneShot(hitClip);

            currentHP--;
            UpdateHearts();
        }

        if (currentHP <= 0)
        {
            Debug.Log("플레이어 사망");
            FindObjectOfType<GameOverManager>().TriggerGameOver();
        }

        // Obstacle 부딪힘
        gameObject.layer = 13; // 레이어 변경
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);

        animator.SetTrigger("Damaged");
        Invoke("OffDamaged", 1);
    }

    void OffDamaged()
    {
        gameObject.layer = 10;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnLeftDown()
    {
        isMovingLeft = true;
    }

    public void OnLeftUp()
    {
        isMovingLeft = false;
    }

    public void OnRightDown()
    {
        isMovingRight = true;
    }

    public void OnRightUp()
    {
        isMovingRight = false;
    }

    public void OnJumpButton()
    {
        if (isGrounded)
        {
            rigid.velocity = new Vector2(rigid.velocity.x, jumpForce);

            if (animator != null)
            {
                animator.SetTrigger("isJumping");
                audioSource.PlayOneShot(jumpClip);
            }
        }
    }

    IEnumerator PlaySlashSoundRepeated()
    {
        for (int i = 0; i < 3; i++)
        {
            audioSource.PlayOneShot(slashClip);
            yield return new WaitForSeconds(0.5f); // 간격 조절 (0.1초 간격)
        }
    }


    public void OnAttackButton()
    {
        if (!canAttack) return; // 쿨타임 중이면 아무것도 하지 않음

        canAttack = false;
        StartCoroutine(ResetAttackCooldown());

        // 공격 애니메이션
        if (animator != null)
        {
            animator.SetTrigger("isAttacking");
        }

        // 공격 사운드
        StartCoroutine(PlaySlashSoundRepeated());

        // 몬스터 공격 판정
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, MonsterLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            GroundMonsterController monster = enemy.GetComponent<GroundMonsterController>();
            if (monster != null)
            {
                monster.OnDamaged();
            }
        }
    }

    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // Hp 모양 변경
    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHP)
                hearts[i].sprite = fullHeartSprite;
            else
                hearts[i].sprite = emptyHeartSprite;
        }
    }

    void HandleFallOff()
    {
        currentHP--;
        UpdateHearts();

        if (currentHP <= 0)
        {
            Debug.Log("추락으로 사망");
            FindObjectOfType<GameOverManager>().TriggerGameOver();
        }
        else
        {
            transform.position = lastCheckpointPosition;

            gameObject.layer = 13;
            spriteRenderer.color = new Color(1, 1, 1, 0.4f);
            Invoke("OffDamaged", 1f);
        }
    }
}
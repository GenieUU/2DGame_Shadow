using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class GroundMonsterController : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsulecollider;

    public int nextMove;

    void Awake()
    {

        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsulecollider = GetComponent<CapsuleCollider2D>();
        Invoke("Think", 3); // 3초 딜레이
    }

    void FixedUpdate()
    {
        // 움직이기
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        // 플랫폼 확인
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove*0.5f, rigid.position.y); // 앞 확인
        Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1);
        if (rayHit.collider == null)
        {
            Turn();
        }
    }

    void Think()
    {
        nextMove = Random.Range(-1, 2);

        // 애니메이션 결정
        anim.SetInteger("WalkSpeed", nextMove);

        // 방향 전환
        if (nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == -1; // 왼쪽 이동 시 FlipX ture
        }

        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    void Turn()
    {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove == -1;

        CancelInvoke();
        Invoke("Think", 3);
    }

    public void OnDamaged()
    {
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        spriteRenderer.flipY = true;
        capsulecollider.enabled = false;
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        Invoke("DeActive", 5);
    }

    void DeActive()
    {
        gameObject.SetActive(false);
    }
}
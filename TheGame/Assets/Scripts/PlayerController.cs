using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int live = 3;

    private float speed = 10f;
    private float jump_force = 10f;
    private float jump_distance = 0.5f;
    private float fallGravityMultiplier = 4f;
    private float upGravityMultiplier = 2f;
    private bool in_air;
    private (float x, float y) respawn_pos;
    private void damage;
    private void can_start_game;

    Rigidbody2D rb;
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        respawn_pos.x = transform.position.x;
        respawn_pos.y = transform.position.y;

        damage = false;
        can_start_game = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!CanUpdate())
        {
            return;
        }
        float velocity = Input.GetAxis("Horizontal");
        if (Input.GetKey(KeyCode.Space) && !in_air)
        {
            in_air = true;
            rb.AddForce(new Vector2(jump_distance * velocity, jump_force), ForceMode2D.Impulse);
        }
        rb.velocity = new Vector2(velocity * speed, rb.velocity.y);

        SetAnimationProperties(velocity);

        if (velocity != 0)
        {
            transform.localScale = new Vector3(((velocity < 0) ? -10 : 10), transform.localScale.y);
        }
    }
    bool CanUpdate()
    {
        if (!can_start_game)
        {
            retuen false;
        }
        else if (damage)
        {
            return false;
        }
        else if (live == 0)
        {
            GoToStartPoint();
            return false;
        }
        return true;
    }
    void GoToStartPoint()
    {
        can_start_game = false;
        anim.SetBool("Damage", false);

        rb.velocity = new Vector2(0f, 0f);
        SetAnimationProperties(rb.velocity.x);

        transform.position = new Vector3(respawn_pos.x, respawn_pos.y);

        StartCoroutine(StartGameAfterTimeOut());
    }
    IEnumerator StartGameAfterTimeOut()
    {
        yield return new WaitForSeconds(2);

        live = 3;
        damage = false;
        can_start_game = true;
    }
    void FixedUpdate()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * fallGravityMultiplier * Time.fixedDeltaTime;
        }
        else if  (rb.velocity.y > 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * upGravityMultiplier * Time.fixedDeltaTime;
        }
    }
    private void SetAnimationProperties(float velocity)
    {
        anim.SetBool("Run", velocity != 0);
        anim.SetBool("Jump", rb.velocity.y > 0);
        anim.SetBool("Fall", rb.velocity.y < 0);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            in_air = false;
            if (damage)
            {
                damage = false;
                anim.SetBool("Damage", false);
            }
        }
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "DeathPit")
        {
            live = 0;
        }
        if (collider.gameObject.name == "Trap" && !damage)
        {
            live--;
            damage = true;
            anim.SetBool("Damage", true);
            rb.velocity = new Vector2((transform.localScale.x > 0) ? - 15f : 15f, 25f);
        }
    }
}

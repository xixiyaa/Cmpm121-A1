using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform target;

    /* NEW public stats filled by spawner */
    public int   maxHP;
    public int   currentHP;
    public int   damage;
    public float speed = 5f;
    /* ---------------------------------- */

    public Hittable  hp;
    public HealthBar healthui;

    bool  dead;
    float last_attack;

    void Start()
    {
        target  = GameManager.Instance.player.transform;

        // initialise Hittable with currentHP if not supplied
        if (hp == null)
            hp = new Hittable(currentHP, Hittable.Team.MONSTERS, gameObject);

        hp.OnDeath += Die;
        healthui.SetHealth(hp);
    }

    void Update()
    {
        if (dead) return;
        if (hp.hp <= 0) { Die(); return; }

        Vector3 dir = target.position - transform.position;
        if (dir.magnitude < 2f)
            DoAttack();
        else
            GetComponent<Unit>().movement = dir.normalized * speed;
    }

    void DoAttack()
    {
        if (last_attack + 2f < Time.time)
        {
            last_attack = Time.time;
            target.GetComponent<PlayerController>()
                  .hp.Damage(new Damage(damage, Damage.Type.PHYSICAL));
        }
    }

    void Die()
    {
        if (dead) return;
        dead = true;

        var col = GetComponent<Collider2D>();  if (col) col.enabled = false;
        var rb  = GetComponent<Rigidbody2D>(); if (rb)  rb.simulated = false;
        GetComponent<Unit>().movement = Vector2.zero;

        GameManager.Instance.RemoveEnemy(gameObject);
        Destroy(gameObject);
    }
}

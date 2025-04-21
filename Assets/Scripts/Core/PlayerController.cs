// using UnityEngine;
// using UnityEngine.InputSystem;
// using Newtonsoft.Json.Linq;
// using Newtonsoft.Json;
// using System.IO;
// using System.Collections.Generic;

// public class PlayerController : MonoBehaviour
// {
//     public Hittable hp;
//     public HealthBar healthui;
//     public ManaBar manaui;

//     public SpellCaster spellcaster;
//     public SpellUI spellui;

//     public int speed;

//     public Unit unit;

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         unit = GetComponent<Unit>();
//         GameManager.Instance.player = gameObject;
//     }

//     public void StartLevel()
//     {
//         spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER);
//         StartCoroutine(spellcaster.ManaRegeneration());
        
//         hp = new Hittable(100, Hittable.Team.PLAYER, gameObject);
//         hp.OnDeath += Die;
//         hp.team = Hittable.Team.PLAYER;

//         // tell UI elements what to show
//         healthui.SetHealth(hp);
//         manaui.SetSpellCaster(spellcaster);
//         spellui.SetSpell(spellcaster.spell);
//         GetComponent<PlayerDeathHandler>().InitHP(100);   // or whatever max HP

//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }

//     void OnAttack(InputValue value)
//     {
//         if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
//         Vector2 mouseScreen = Mouse.current.position.value;
//         Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
//         mouseWorld.z = 0;
//         StartCoroutine(spellcaster.Cast(transform.position, mouseWorld));
//     }

//     void OnMove(InputValue value)
//     {
//         if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
//         unit.movement = value.Get<Vector2>()*speed;
//     }

//     void Die()
//     {
//         Debug.Log("You Lost");
//     }

// }


// PlayerController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;
    public SpellCaster spellcaster;
    public SpellUI spellui;
    public int speed;
    public Unit unit;

    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
    }

    public void StartLevel()
    {
        spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER);
        StartCoroutine(spellcaster.ManaRegeneration());

        // ← your one-and-only HP instance
        hp = new Hittable(100, Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);
        spellui.SetSpell(spellcaster.spell);

        // ← hook death handler into _this_ Hittable
        GetComponent<PlayerDeathHandler>().InitHP(hp);
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME ||
            GameManager.Instance.state == GameManager.GameState.GAMEOVER)
            return;

        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        StartCoroutine(spellcaster.Cast(transform.position, mouseWorld));
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME ||
            GameManager.Instance.state == GameManager.GameState.GAMEOVER)
            return;

        unit.movement = value.Get<Vector2>() * speed;
    }

    void Die()
    {
        Debug.Log("You Lost");
        // ← stop any lingering velocity
        unit.movement = Vector2.zero;
        // ← disable controller so even if state logic missed, no input!
        enabled = false;
    }
}

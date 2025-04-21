using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]                       // ← helper class for JSON
public class EnemyInfo
{
    public string name;
    public int    sprite;
    public int    hp;
    public float  speed;
    public int    damage;
}

public class GameManager
{
    public enum GameState
    {
        PREGAME,
        INWAVE,
        WAVEEND,
        COUNTDOWN,
        GAMEOVER
    }
    public GameState state;

    public  int countdown;

    /*──────── NEW ────────*/
    public static string SelectedLevelName = "Easy";   // set by menu / spawner
    private Dictionary<string, EnemyInfo> enemyDict;   // filled in LoadEnemyData()
    /*─────────────────────*/

    private static GameManager theInstance;
    public static GameManager Instance
    {
        get
        {
            if (theInstance == null)
                theInstance = new GameManager();
            return theInstance;
        }
    }

    public GameObject player;

    public ProjectileManager  projectileManager;
    public SpellIconManager   spellIconManager;
    public EnemySpriteManager enemySpriteManager;
    public PlayerSpriteManager playerSpriteManager;
    public RelicIconManager    relicIconManager;

    private List<GameObject> enemies;
    public  int enemy_count { get { return enemies.Count; } }

    public void AddEnemy(GameObject enemy)   { enemies.Add(enemy);    }
    public void RemoveEnemy(GameObject enemy){ enemies.Remove(enemy); }

    public GameObject GetClosestEnemy(Vector3 point)
    {
        if (enemies == null || enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];
        return enemies.Aggregate((a, b) =>
            (a.transform.position - point).sqrMagnitude <
            (b.transform.position - point).sqrMagnitude ? a : b);
    }

    /*──────── NEW ────────*/
    public void LoadEnemyData(TextAsset jsonAsset)
    {
        enemyDict = new Dictionary<string, EnemyInfo>();
        EnemyInfo[] list = JsonUtility.FromJson<Wrapper>( "{ \"arr\": " + jsonAsset.text + "}" ).arr;
        foreach (var e in list) enemyDict[e.name] = e;
    }
    public EnemyInfo GetEnemyInfo(string name)
    {
        return (enemyDict != null && enemyDict.ContainsKey(name)) ? enemyDict[name] : null;
    }
    [Serializable] private class Wrapper { public EnemyInfo[] arr; }
    /*─────────────────────*/

    private GameManager()
    {
        enemies = new List<GameObject>();
    }
}

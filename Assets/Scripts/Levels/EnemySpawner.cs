// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections;

// public class EnemySpawner : MonoBehaviour
// {
//     /* ---------- Inspector‑assigned fields ---------- */
//     [Header("UI")]
//     public Image      level_selector;      // panel holding the difficulty buttons
//     public GameObject buttonPrefab;        // prefab for Easy/Medium/Endless buttons
//     public TMP_Text   waveButtonLabel;     // OPTIONAL: drag in the Wave button's TMP_Text here
//     public GameObject rewardScreenPanel;   // the panel shown between waves

//     [Header("Spawn")]
//     public GameObject enemyPrefab;         // Enemy prefab to instantiate
//     public SpawnPoint[] SpawnPoints;       // Assign all spawn points here

//     /* ---------- internal state ---------- */
//     private int  waveIndex     = 0;        // current wave number (1-based)
//     private int  maxWaves      = 0;        // number of waves this level
//     private bool levelFinished = false;    // true once last wave of current level completes

//     /* ====================================================================== */
//     /*                         MAIN MENU SETUP                                */
//     /* ====================================================================== */
//     void Start()
//     {
//         // Build three difficulty buttons at runtime
//         string[] levels = { "Easy", "Medium", "Endless" };
//         float yStart = 130f, yStep = -50f;
//         for (int i = 0; i < levels.Length; i++)
//         {
//             GameObject btn = Instantiate(buttonPrefab, level_selector.transform);
//             btn.transform.localPosition = new Vector3(0, yStart + i * yStep, 0);

//             var m = btn.GetComponent<MenuSelectorController>();
//             m.spawner = this;
//             m.SetLevel(levels[i]);  // sets label text + stores the level name
//         }

//         // Load enemy stats once (from Resources/enemies.json)
//         TextAsset enemyJson = Resources.Load<TextAsset>("enemies");
//         GameManager.Instance.LoadEnemyData(enemyJson);
//     }

//     /* ====================================================================== */
//     /*                    START A NEW LEVEL (Menu button)                    */
//     /* ====================================================================== */
//     public void StartLevel(string levelName)
//     {
//         // Hide both the main menu and any reward screen
//         level_selector.gameObject.SetActive(false);
//         if (rewardScreenPanel != null)
//             rewardScreenPanel.SetActive(false);

//         GameManager.SelectedLevelName = levelName;

//         // Decide how many waves each difficulty runs
//         maxWaves = (levelName == "Easy")   ? 1 :
//                    (levelName == "Medium") ? 15 :
//                    int.MaxValue;               // Endless

//         waveIndex     = 0;
//         levelFinished = false;

//         // Kick off the player's StartLevel logic and first wave
//         GameManager.Instance.player
//             .GetComponent<PlayerController>().StartLevel();
//         StartCoroutine(SpawnWave());
//     }

//     /* ====================================================================== */
//     /*    NEXT‑WAVE BUTTON (RewardScreen) — either continue or next level     */
//     /* ====================================================================== */
//     public void NextWave()
//     {
//         if (levelFinished)
//         {
//             // Easy → Medium, Medium → Endless, Endless stays Endless
//             string next =
//                 (GameManager.SelectedLevelName == "Easy")   ? "Medium" :
//                 (GameManager.SelectedLevelName == "Medium") ? "Endless" :
//                                                                "Endless";

//             // Hide the reward panel before jumping to next level
//             if (rewardScreenPanel != null)
//                 rewardScreenPanel.SetActive(false);

//             StartLevel(next);
//         }
//         else
//         {
//             // Still in the same difficulty: spawn the next wave
//             StartCoroutine(SpawnWave());
//         }
//     }

//     // Overload retained for legacy calls (not used by the UI)
//     public void NextWave(string levelName) => StartCoroutine(SpawnWave());

//     /* ====================================================================== */
//     /*                        WAVE SPAWNING COROUTINE                         */
//     /* ====================================================================== */
//     IEnumerator SpawnWave()
//     {
//         waveIndex++;
//         Debug.Log($"Wave {waveIndex}/{maxWaves} — {GameManager.SelectedLevelName}");

//         // 3-second countdown
//         GameManager.Instance.state     = GameManager.GameState.COUNTDOWN;
//         GameManager.Instance.countdown = 3;
//         for (int i = 0; i < 3; i++)
//         {
//             yield return new WaitForSeconds(1);
//             GameManager.Instance.countdown--;
//         }

//         GameManager.Instance.state = GameManager.GameState.INWAVE;

//         // Choose enemy type by selected difficulty
//         string eType =
//             (GameManager.SelectedLevelName == "Medium")  ? "skeleton" :
//             (GameManager.SelectedLevelName == "Endless") ? "warlock"  :
//                                                            "zombie";

//         // Spawn 10 of that type
//         for (int i = 0; i < 10; i++)
//             yield return SpawnEnemy(eType);

//         // Wait until all spawned enemies are dead
//         yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);

//         // Mark whether this was the last wave
//         levelFinished = (waveIndex >= maxWaves);

//         // If level finished, update the button label
//         if (levelFinished)
//         {
//             if (waveButtonLabel != null)
//             {
//                 waveButtonLabel.text = "Next Level";
//             }
//             else
//             {
//                 // Fallback: find the WaveButton by name
//                 GameObject wb = GameObject.Find("WaveButton");
//                 if (wb != null)
//                 {
//                     var tmp = wb.GetComponentInChildren<TMP_Text>();
//                     if (tmp != null)
//                         tmp.text = "Next Level";
//                 }
//             }
//         }

//         // Signal end of wave so RewardScreen shows up
//         GameManager.Instance.state = GameManager.GameState.WAVEEND;
//     }

//     /* ====================================================================== */
//     /*                         SPAWN ONE ENEMY                                */
//     /* ====================================================================== */
//     IEnumerator SpawnEnemy(string type)
//     {
//         var info = GameManager.Instance.GetEnemyInfo(type);
//         if (info == null) yield break;

//         // Select a random spawn point + small offset
//         var sp  = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
//         var pos = sp.transform.position + (Vector3)(Random.insideUnitCircle * 1.8f);

//         // Instantiate and configure
//         var e = Instantiate(enemyPrefab, pos, Quaternion.identity);
//         e.GetComponent<SpriteRenderer>().sprite =
//             GameManager.Instance.enemySpriteManager.Get(info.sprite);

//         var ec = e.GetComponent<EnemyController>();
//         ec.maxHP     = ec.currentHP = info.hp;
//         ec.speed     = info.speed;
//         ec.damage    = info.damage;

//         GameManager.Instance.AddEnemy(e);
//         yield return new WaitForSeconds(0.5f);
//     }
// }


// New Version: Have a HudInfo that shows the current wave and kills and the level name,
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    /* ---------- Inspector UI ---------- */
    [Header("UI")]
    public Image      level_selector;      // menu panel
    public GameObject buttonPrefab;        // difficulty button prefab
    public TMP_Text   waveButtonLabel;     // text on reward‑screen button
    public GameObject rewardScreenPanel;   // panel shown between waves
    public TMP_Text   HUDInfo;             // drag HUDInfo (TMP) here

    /* ---------- Spawn ---------- */
    public GameObject  enemyPrefab;
    public SpawnPoint[] SpawnPoints;

    /* ---------- state ---------- */
    int waveIndex       = 0;
    int maxWaves        = 0;
    bool levelFinished  = false;
    int enemiesSpawned  = 0;   // per‑wave spawn count
    int killCount       = 0;   // per‑wave kill count  ⬅︎ NEW

    /* ====================================================================== */
    /*                          INITIALISATION                                */
    /* ====================================================================== */
    void Start()
    {
        // Auto‑fill SpawnPoints if left empty
#if UNITY_2022_2_OR_NEWER
        if (SpawnPoints == null || SpawnPoints.Length == 0)
            SpawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
#else
        if (SpawnPoints == null || SpawnPoints.Length == 0)
            SpawnPoints = FindObjectsOfType<SpawnPoint>();
#endif

        // Build three menu buttons
        string[] lvls = { "Easy", "Medium", "Endless" };
        float y0 = 130f, dy = -50f;
        for (int i = 0; i < lvls.Length; i++)
        {
            var b = Instantiate(buttonPrefab, level_selector.transform);
            b.transform.localPosition = new Vector3(0, y0 + i * dy, 0);
            var mc = b.GetComponent<MenuSelectorController>();
            mc.spawner = this;
            mc.SetLevel(lvls[i]);
        }

        // Hide HUD until game starts
        if (HUDInfo) HUDInfo.gameObject.SetActive(false);

        // Load enemy data
        var js = Resources.Load<TextAsset>("enemies");
        GameManager.Instance.LoadEnemyData(js);
    }

    /* ====================================================================== */
    /*                        START A NEW LEVEL                               */
    /* ====================================================================== */
    public void StartLevel(string levelName)
    {
        level_selector.gameObject.SetActive(false);
        if (rewardScreenPanel) rewardScreenPanel.SetActive(false);

        GameManager.SelectedLevelName = levelName;
        maxWaves = (levelName == "Easy")   ? 1  :
                   (levelName == "Medium") ? 15 :
                                             int.MaxValue;   // Endless

        waveIndex      = 0;
        levelFinished  = false;

        if (HUDInfo) HUDInfo.gameObject.SetActive(true);

        GameManager.Instance.player
                 .GetComponent<PlayerController>().StartLevel();

        StartCoroutine(SpawnWave());
    }

    /* ====================================================================== */
    /*                         WAVE COROUTINE                                 */
    /* ====================================================================== */
    IEnumerator SpawnWave()
    {
        waveIndex++;
        enemiesSpawned = 0;
        killCount      = 0;      // reset per wave
        UpdateHUD();

        /* ---------- 3‑second countdown ---------- */
        GameManager.Instance.state     = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }

        GameManager.Instance.state = GameManager.GameState.INWAVE;

        /* ---------- spawn ten of the chosen type ---------- */
        string type = (GameManager.SelectedLevelName == "Medium")  ? "skeleton" :
                      (GameManager.SelectedLevelName == "Endless") ? "warlock"  :
                                                                     "zombie";

        for (int i = 0; i < 10; i++)
        {
            yield return SpawnEnemy(type);
            enemiesSpawned++;
            UpdateHUD();
        }

        /* ---------- wait for player to kill everything ---------- */
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);

        levelFinished = (waveIndex >= maxWaves);
        UpdateHUD();                      // final kill tally

        if (levelFinished && waveButtonLabel)
            waveButtonLabel.text = "Next Level";

        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        if (rewardScreenPanel) rewardScreenPanel.SetActive(true);
    }

    /* ====================================================================== */
    /*                         SPAWN ONE ENEMY                                */
    /* ====================================================================== */
    IEnumerator SpawnEnemy(string type)
    {
        var info = GameManager.Instance.GetEnemyInfo(type);
        if (info == null) yield break;

        var sp  = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        var pos = sp.transform.position + (Vector3)Random.insideUnitCircle * 1.8f;

        var e = Instantiate(enemyPrefab, pos, Quaternion.identity);
        e.GetComponent<SpriteRenderer>().sprite =
            GameManager.Instance.enemySpriteManager.Get(info.sprite);

        var ec = e.GetComponent<EnemyController>();
        ec.hp        = new Hittable(info.hp, Hittable.Team.MONSTERS, e);
        ec.maxHP     = ec.currentHP = info.hp;
        ec.speed     = info.speed;
        ec.damage    = info.damage;

        /* ---------- count kills immediately on death ---------- */
        ec.hp.OnDeath += () =>
        {
            killCount++;
            UpdateHUD();
        };

        GameManager.Instance.AddEnemy(e);
        yield return new WaitForSeconds(0.5f);
    }

    /* ====================================================================== */
    /*                           HUD UPDATE                                   */
    /* ====================================================================== */
    void UpdateHUD()
    {
        if (!HUDInfo) return;

        string lvl = GameManager.SelectedLevelName;
        string waveLine = (maxWaves == int.MaxValue)
                            ? $"Wave {waveIndex}"
                            : $"Wave {waveIndex} / {maxWaves}";

        HUDInfo.text = $"Level: {lvl}\n{waveLine}\nKills: {killCount}";
    }

    /* ====================================================================== */
    /*                        REWARD‑SCREEN BUTTON                            */
    /* ====================================================================== */
    public void NextWave()
    {
        if (rewardScreenPanel) rewardScreenPanel.SetActive(false);

        if (levelFinished)
        {
            if (GameManager.SelectedLevelName == "Endless")
            {
                level_selector.gameObject.SetActive(true); // back to main menu
                return;
            }

            string next = (GameManager.SelectedLevelName == "Easy")
                            ? "Medium"
                            : "Endless";
            StartLevel(next);
        }
        else
        {
            StartCoroutine(SpawnWave());
        }
    }
}

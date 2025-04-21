using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    /* ---------- Inspector‑assigned fields ---------- */
    [Header("UI")]
    public Image      level_selector;      // panel holding the difficulty buttons
    public GameObject buttonPrefab;        // prefab for Easy/Medium/Endless buttons
    public TMP_Text   waveButtonLabel;     // OPTIONAL: drag in the Wave button's TMP_Text here
    public GameObject rewardScreenPanel;   // the panel shown between waves

    [Header("Spawn")]
    public GameObject enemyPrefab;         // Enemy prefab to instantiate
    public SpawnPoint[] SpawnPoints;       // Assign all spawn points here

    /* ---------- internal state ---------- */
    private int  waveIndex     = 0;        // current wave number (1-based)
    private int  maxWaves      = 0;        // number of waves this level
    private bool levelFinished = false;    // true once last wave of current level completes

    /* ====================================================================== */
    /*                         MAIN MENU SETUP                                */
    /* ====================================================================== */
    void Start()
    {
        // Build three difficulty buttons at runtime
        string[] levels = { "Easy", "Medium", "Endless" };
        float yStart = 130f, yStep = -50f;
        for (int i = 0; i < levels.Length; i++)
        {
            GameObject btn = Instantiate(buttonPrefab, level_selector.transform);
            btn.transform.localPosition = new Vector3(0, yStart + i * yStep, 0);

            var m = btn.GetComponent<MenuSelectorController>();
            m.spawner = this;
            m.SetLevel(levels[i]);  // sets label text + stores the level name
        }

        // Load enemy stats once (from Resources/enemies.json)
        TextAsset enemyJson = Resources.Load<TextAsset>("enemies");
        GameManager.Instance.LoadEnemyData(enemyJson);
    }

    /* ====================================================================== */
    /*                    START A NEW LEVEL (Menu button)                    */
    /* ====================================================================== */
    public void StartLevel(string levelName)
    {
        // Hide both the main menu and any reward screen
        level_selector.gameObject.SetActive(false);
        if (rewardScreenPanel != null)
            rewardScreenPanel.SetActive(false);

        GameManager.SelectedLevelName = levelName;

        // Decide how many waves each difficulty runs
        maxWaves = (levelName == "Easy")   ? 1 :
                   (levelName == "Medium") ? 15 :
                   int.MaxValue;               // Endless

        waveIndex     = 0;
        levelFinished = false;

        // Kick off the player's StartLevel logic and first wave
        GameManager.Instance.player
            .GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
    }

    /* ====================================================================== */
    /*    NEXT‑WAVE BUTTON (RewardScreen) — either continue or next level     */
    /* ====================================================================== */
    public void NextWave()
    {
        if (levelFinished)
        {
            // Easy → Medium, Medium → Endless, Endless stays Endless
            string next =
                (GameManager.SelectedLevelName == "Easy")   ? "Medium" :
                (GameManager.SelectedLevelName == "Medium") ? "Endless" :
                                                               "Endless";

            // Hide the reward panel before jumping to next level
            if (rewardScreenPanel != null)
                rewardScreenPanel.SetActive(false);

            StartLevel(next);
        }
        else
        {
            // Still in the same difficulty: spawn the next wave
            StartCoroutine(SpawnWave());
        }
    }

    // Overload retained for legacy calls (not used by the UI)
    public void NextWave(string levelName) => StartCoroutine(SpawnWave());

    /* ====================================================================== */
    /*                        WAVE SPAWNING COROUTINE                         */
    /* ====================================================================== */
    IEnumerator SpawnWave()
    {
        waveIndex++;
        Debug.Log($"Wave {waveIndex}/{maxWaves} — {GameManager.SelectedLevelName}");

        // 3-second countdown
        GameManager.Instance.state     = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }

        GameManager.Instance.state = GameManager.GameState.INWAVE;

        // Choose enemy type by selected difficulty
        string eType =
            (GameManager.SelectedLevelName == "Medium")  ? "skeleton" :
            (GameManager.SelectedLevelName == "Endless") ? "warlock"  :
                                                           "zombie";

        // Spawn 10 of that type
        for (int i = 0; i < 10; i++)
            yield return SpawnEnemy(eType);

        // Wait until all spawned enemies are dead
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);

        // Mark whether this was the last wave
        levelFinished = (waveIndex >= maxWaves);

        // If level finished, update the button label
        if (levelFinished)
        {
            if (waveButtonLabel != null)
            {
                waveButtonLabel.text = "Next Level";
            }
            else
            {
                // Fallback: find the WaveButton by name
                GameObject wb = GameObject.Find("WaveButton");
                if (wb != null)
                {
                    var tmp = wb.GetComponentInChildren<TMP_Text>();
                    if (tmp != null)
                        tmp.text = "Next Level";
                }
            }
        }

        // Signal end of wave so RewardScreen shows up
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }

    /* ====================================================================== */
    /*                         SPAWN ONE ENEMY                                */
    /* ====================================================================== */
    IEnumerator SpawnEnemy(string type)
    {
        var info = GameManager.Instance.GetEnemyInfo(type);
        if (info == null) yield break;

        // Select a random spawn point + small offset
        var sp  = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        var pos = sp.transform.position + (Vector3)(Random.insideUnitCircle * 1.8f);

        // Instantiate and configure
        var e = Instantiate(enemyPrefab, pos, Quaternion.identity);
        e.GetComponent<SpriteRenderer>().sprite =
            GameManager.Instance.enemySpriteManager.Get(info.sprite);

        var ec = e.GetComponent<EnemyController>();
        ec.maxHP     = ec.currentHP = info.hp;
        ec.speed     = info.speed;
        ec.damage    = info.damage;

        GameManager.Instance.AddEnemy(e);
        yield return new WaitForSeconds(0.5f);
    }
}

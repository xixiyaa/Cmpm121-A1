// // PlayerDeathHandler.cs
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class PlayerDeathHandler : MonoBehaviour
// {
//     public Canvas gameOverPanel;   // Drag GameOverPanel here

//     // Make sure the panel starts hidden
//     void Awake()
//     {
//         if (gameOverPanel) gameOverPanel.gameObject.SetActive(false);
//     }

//     /// <summary>Attach a fresh HP component and listen for death.</summary>
//     public void InitHP(int maxHP)
//     {
//         var hp = new Hittable(maxHP, Hittable.Team.PLAYER, gameObject);
//         hp.OnDeath += OnPlayerDeath;
//     }

//     /// <summary>Called once when the player dies.</summary>
//     void OnPlayerDeath()
//     {
//         if (GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;

//         GameManager.Instance.state = GameManager.GameState.GAMEOVER;

//         Time.timeScale = 0f;                                   // freeze gameplay
//         if (gameOverPanel) gameOverPanel.gameObject.SetActive(true);
//     }

//     /// <summary>Wired to RestartButton → On Click()</summary>
//     public void RestartGame()
//     {
//         Time.timeScale = 1f;                                   // un‑freeze
//         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//     }
// }




// // PlayerDeathHandler.cs
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class PlayerDeathHandler : MonoBehaviour
// {
//     public Canvas gameOverPanel;   // Drag GameOverPanel here

//     void Awake()
//     {
//         if (gameOverPanel) gameOverPanel.gameObject.SetActive(false);
//     }

//     // ← CHANGE: accept the existing Hittable
//     public void InitHP(Hittable hp)
//     {
//         hp.OnDeath += OnPlayerDeath;
//     }

//     void OnPlayerDeath()
//     {
//         if (GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;

//         GameManager.Instance.state = GameManager.GameState.GAMEOVER;
//         Time.timeScale = 0f;                                   // freeze gameplay
//         if (gameOverPanel) gameOverPanel.gameObject.SetActive(true);

//         // ← NEW: disable player input entirely
//         var pc = GetComponent<PlayerController>();
//         if (pc) pc.enabled = false;
//     }

//     public void RestartGame()
//     {
//         Time.timeScale = 1f;
//         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//     }
// }


// PlayerDeathHandler.cs (edited)
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathHandler : MonoBehaviour
{
    // ← allow dragging in the Panel itself
    public GameObject gameOverPanel;

    void Awake()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void InitHP(Hittable hp)
    {
        hp.OnDeath += OnPlayerDeath;
    }

    void OnPlayerDeath()
    {
        if (GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;

        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
        Time.timeScale = 0f;

        if (gameOverPanel) gameOverPanel.SetActive(true);

        // disable input if you haven’t already
        var pc = GetComponent<PlayerController>();
        if (pc) pc.enabled = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

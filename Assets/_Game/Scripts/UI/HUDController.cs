using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LowAmmo.Level;

namespace LowAmmo.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private TMP_Text levelNameText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private Button restartButton;

        [Header("Screens")]
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button retryButton;

        private float elapsedTime = 0f;
        private bool isTimerRunning = true;

        private void Start()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnAmmoChanged += UpdateAmmoDisplay;
                LevelManager.Instance.OnLevelCompleted += ShowLevelComplete;
                LevelManager.Instance.OnPlayerDied += ShowGameOver;

                UpdateAmmoDisplay(LevelManager.Instance.CurrentAmmo, LevelManager.Instance.StartingAmmo);

                if (levelNameText != null)
                {
                    levelNameText.text = LevelManager.Instance.LevelName;
                }
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRestartClicked);
            }

            if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnAmmoChanged -= UpdateAmmoDisplay;
                LevelManager.Instance.OnLevelCompleted -= ShowLevelComplete;
                LevelManager.Instance.OnPlayerDied -= ShowGameOver;
            }
        }

        private void Update()
        {
            if (isTimerRunning && timerText != null)
            {
                elapsedTime += Time.deltaTime;
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                int seconds = Mathf.FloorToInt(elapsedTime % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        public void UpdateAmmoDisplay(int current, int max)
        {
            if (ammoText != null)
            {
                ammoText.text = $"AMMO: {current} / {max}";
            }
        }

        private void ShowLevelComplete()
        {
            isTimerRunning = false;
            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(true);
            }
        }

        private void ShowGameOver()
        {
            isTimerRunning = false;
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }

        private void OnRestartClicked()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartLevel();
            }
        }

        private void OnNextLevelClicked()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadNextLevel();
            }
        }
    }
}

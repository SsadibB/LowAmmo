using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace LowAmmo.Level
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Settings")]
        [SerializeField] private string levelName = "Level 1";
        [SerializeField] private int startingAmmo = 2;
        [SerializeField] private string nextSceneName = "";

        public int CurrentAmmo { get; private set; }
        public int StartingAmmo => startingAmmo;
        public string LevelName => levelName;
        public bool IsLevelCompleted { get; private set; }
        public bool IsGameOver { get; private set; }

        public event Action<int, int> OnAmmoChanged;
        public event Action OnLevelCompleted;
        public event Action OnPlayerDied;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CurrentAmmo = startingAmmo;
            IsLevelCompleted = false;
            IsGameOver = false;
        }

        private void Start()
        {
            OnAmmoChanged?.Invoke(CurrentAmmo, startingAmmo);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.rKey.wasPressedThisFrame)
            {
                RestartLevel();
            }
        }

        public bool TryConsumeAmmo()
        {
            if (IsGameOver || IsLevelCompleted) return false;

            if (CurrentAmmo > 0)
            {
                CurrentAmmo--;
                OnAmmoChanged?.Invoke(CurrentAmmo, startingAmmo);
                return true;
            }

            return false;
        }

        public void TriggerPlayerDeath()
        {
            if (IsGameOver || IsLevelCompleted) return;
            IsGameOver = true;
            OnPlayerDied?.Invoke();
        }

        public void CompleteLevel()
        {
            if (IsGameOver || IsLevelCompleted) return;
            IsLevelCompleted = true;
            OnLevelCompleted?.Invoke();
        }

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadNextLevel()
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
                if (nextIndex < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(nextIndex);
                }
                else
                {
                    // Loop or reload current if at end
                    SceneManager.LoadScene(0);
                }
            }
        }
    }
}

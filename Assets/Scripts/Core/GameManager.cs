using System;
using UnityEngine;

namespace CosmicRunner.Core
{
    public enum GameState
    {
        Menu,
        Playing,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Menu;

        [Header("Speed Settings")]
        [SerializeField] private float initialSpeed = 12f;
        [SerializeField] private float maxSpeed = 35f;
        [SerializeField] private float speedAcceleration = 0.25f; // Tốc độ tăng mỗi giây

        public float CurrentSpeed { get; private set; }
        public float DistanceScore { get; private set; }
        public int CoinsCollected { get; private set; }
        public int HighScore { get; private set; }

        public event Action OnGameStarted;
        public event Action OnGameOver;
        public event Action<int> OnScoreUpdated;
        public event Action<int> OnCoinsUpdated;

        private const string HIGH_SCORE_KEY = "CosmicRunner_HighScore";
        private const string TOTAL_COINS_KEY = "CosmicRunner_TotalCoins";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadData();
        }

        private void Start()
        {
            CurrentSpeed = initialSpeed;
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                // Tăng tốc độ theo thời gian
                if (CurrentSpeed < maxSpeed)
                {
                    CurrentSpeed += speedAcceleration * Time.deltaTime;
                }

                // Tính điểm theo quãng đường
                DistanceScore += CurrentSpeed * Time.deltaTime;
                OnScoreUpdated?.Invoke((int)DistanceScore);
            }
        }

        public void StartGame()
        {
            DistanceScore = 0f;
            CoinsCollected = 0;
            CurrentSpeed = initialSpeed;
            CurrentState = GameState.Playing;

            OnGameStarted?.Invoke();
            OnScoreUpdated?.Invoke(0);
            OnCoinsUpdated?.Invoke(0);
        }

        public void BackToMenu()
        {
            CurrentState = GameState.Menu;
        }

        public void AddCoin(int amount = 1)
        {
            if (CurrentState != GameState.Playing) return;

            CoinsCollected += amount;
            OnCoinsUpdated?.Invoke(CoinsCollected);
        }

        public void TriggerGameOver()
        {
            if (CurrentState != GameState.Playing) return;

            CurrentState = GameState.GameOver;

            int finalScore = (int)DistanceScore;
            if (finalScore > HighScore)
            {
                HighScore = finalScore;
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, HighScore);
            }

            int savedCoins = PlayerPrefs.GetInt(TOTAL_COINS_KEY, 0) + CoinsCollected;
            PlayerPrefs.SetInt(TOTAL_COINS_KEY, savedCoins);
            PlayerPrefs.Save();

            OnGameOver?.Invoke();
        }

        public void RestartGame()
        {
            StartGame();
        }

        private void LoadData()
        {
            HighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        }
    }
}

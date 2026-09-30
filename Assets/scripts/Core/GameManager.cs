using System;
using UnityEngine;

namespace RiseToDominance.Core
{
    public enum GameState
    {
        MainMenu,
        GeneratingMap,
        PlayerTurn,
        AITurn,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }

        public static event Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ChangeState(GameState.MainMenu);
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            Debug.Log($"[GameManager] Состояние игры изменено на: {newState}");

            OnGameStateChanged?.Invoke(newState);
        }

        public void StartGame()
        {
            ChangeState(GameState.GeneratingMap);
        }
    }
}
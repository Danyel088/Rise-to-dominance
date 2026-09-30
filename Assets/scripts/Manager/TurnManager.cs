using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Настройки ходов")]
    [SerializeField] private int currentTurn = 1;
    [SerializeField] private int currentFaction = 1; // 1 — Игрок, 2 — ИИ/Противник
    [SerializeField] private int totalFactions = 2;

    // Событие перехода хода: передает (номер хода, номер фракции)
    public static event Action<int, int> OnTurnChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        OnTurnChanged?.Invoke(currentTurn, currentFaction);
    }

    private void Update()
    {
        // Временная проверка: нажатие Space завершает ход
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        currentFaction++;

        if (currentFaction > totalFactions)
        {
            currentFaction = 1;
            currentTurn++;
        }

        Debug.Log($"--- Наступил {currentTurn} ход. Сейчас ходит Фракция: {currentFaction} ---");
        OnTurnChanged?.Invoke(currentTurn, currentFaction);
    }

    public int CurrentTurn => currentTurn;
    public int CurrentFaction => currentFaction;
}
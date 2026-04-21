using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    public enum TankState { Garage, Driving, Combat, Dead }
    public enum CombatPhase { None, PlayerAiming, ProjectileInFlight, EnemyTurn }

    [Header("State Configuration")]
    public TankState currentState = TankState.Garage;
    public CombatPhase currentPhase = CombatPhase.None;

    [Header("Environment Control")]
    public float environmentSpeed = 5f;
    public AimingSystem aimingSystem;
    public static bool shouldAutoStart = false;

    public static float CurrentGlobalSpeed { get; private set; }

    private float currentAimTimer;
    private GameObject activeProjectile;
    private EnemyAI currentTarget;
    private Health targetHealth;

    private void Start()
    {
        if (shouldAutoStart)
        {
            SetState(TankState.Driving);

            shouldAutoStart = false;

            if (UIManager.Instance != null) UIManager.Instance.InGamePanel.SetActive(true);
            if (UIManager.Instance != null) UIManager.Instance.GaragePanel.SetActive(false);
        }
        else
        {
            SetState(TankState.Garage);
        }
    }

    private void Update() => ProcessState();

    private void SetState(TankState newState)
    {
        currentState = newState;
        CurrentGlobalSpeed = (currentState == TankState.Driving) ? environmentSpeed : 0f;
    }

    private void ProcessState()
    {
        if (currentState != TankState.Combat) return;

        switch (currentPhase)
        {
            case CombatPhase.PlayerAiming:
                currentAimTimer -= Time.deltaTime;

                bool isFired = false;
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) isFired = true;
                else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) isFired = true;

                if (isFired)
                {
                    ExecuteFire();
                }
                else if (currentAimTimer <= 0f)
                {
                    aimingSystem.CancelAiming();
                    InitiateEnemyTurn();
                }
                break;

            case CombatPhase.ProjectileInFlight:
                break;

            case CombatPhase.EnemyTurn:
                break;
        }
    }

    public void StartBattleFromGarage()
    {
        SetState(TankState.Driving);
    }

    public void EnterCombatState(EnemyAI enemy)
    {
        if (currentState == TankState.Driving)
        {
            currentTarget = enemy;
            targetHealth = enemy.GetComponent<Health>();
            SetState(TankState.Combat);
            StartPlayerTurn();
        }
    }

    public void StartPlayerTurn()
    {
        currentPhase = CombatPhase.PlayerAiming;

        currentAimTimer = DifficultyManager.Instance.GetPlayerAimTime();

        aimingSystem.StartAiming();
    }

    private void ExecuteFire()
    {
        activeProjectile = aimingSystem.ExecuteShot(HandleShotResult);
        currentPhase = CombatPhase.ProjectileInFlight;
    }

    private void HandleShotResult(bool isHit)
    {
        if (isHit)
        {
            if (currentTarget == null || targetHealth == null || targetHealth.currentHealth <= 0)
            {
                DifficultyManager.Instance.AddKill();

                currentPhase = CombatPhase.None;
                ResumeDriving();
            }
            else
            {
                InitiateEnemyTurn();
            }
        }
        else
        {
            InitiateEnemyTurn();
        }
    }

    private void InitiateEnemyTurn()
    {
        if (currentPhase == CombatPhase.EnemyTurn) return;

        currentPhase = CombatPhase.EnemyTurn;
        if (currentTarget != null)
        {
            currentTarget.ExecutePerfectShot(this);
        }
    }

    public void ResumeDriving() => SetState(TankState.Driving);

    public void TriggerGameOver()
    {
        SetState(TankState.Dead);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }
        else
        {
            Debug.LogError("UIManager.Instance не знайдено!");
        }
    }
}
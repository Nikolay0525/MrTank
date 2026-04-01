using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    public enum TankState { Driving, Combat, Dead }
    public enum CombatPhase { None, PlayerAiming, ProjectileInFlight, EnemyTurn }

    [Header("State Configuration")]
    public TankState currentState = TankState.Driving;
    public CombatPhase currentPhase = CombatPhase.None;

    [Header("Environment Control")]
    public float environmentSpeed = 5f;
    public AimingSystem aimingSystem;

    [Header("Timer Equation Parameters")]
    public float baseAimTime = 5f;
    public float minAimTime = 1.5f;
    public float timeReductionCoefficient = 0.01f;

    public static float CurrentGlobalSpeed { get; private set; }

    private float currentAimTimer;
    private GameObject activeProjectile;
    private EnemyAI currentTarget;
    private Health targetHealth;

    private void Start() => SetState(TankState.Driving);

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
                // Пасивне очікування.
                // Перехід до наступної фази ініціюється виключно через делегат HandleShotResult.
                break;

            case CombatPhase.EnemyTurn:
                // Пасивне очікування завершення алгоритму штучного інтелекту.
                break;
        }
    }

    // Метод викликається тригером CombatZoneTrigger, який передає посилання на ворога
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
        currentAimTimer = Mathf.Max(minAimTime, baseAimTime - (timeReductionCoefficient * ChunkManager.TotalDistanceTraveled));
        aimingSystem.StartAiming();
    }

    private void ExecuteFire()
    {
        // Передача методу HandleShotResult як зворотного виклику
        activeProjectile = aimingSystem.ExecuteShot(HandleShotResult);
        currentPhase = CombatPhase.ProjectileInFlight;
    }

    // Новий метод, який відповідає сигнатурі Action<bool>
    // Викликатиметься зсередини снаряда при знищенні/колізії
    private void HandleShotResult(bool isHit)
    {
        if (isHit)
        {
            // Якщо снаряд повідомив про влучання у ворога
            if (currentTarget == null || targetHealth == null || targetHealth.currentHealth <= 0)
            {
                currentPhase = CombatPhase.None;
                ResumeDriving();
            }
            else
            {
                // Ворог отримав шкоду, але вижив (опціонально для майбутнього)
                InitiateEnemyTurn();
            }
        }
        else
        {
            // Якщо снаряд повідомив про промах (влучання в ландшафт або виліт за межі)
            InitiateEnemyTurn();
        }
    }

    private void InitiateEnemyTurn()
    {
        currentPhase = CombatPhase.EnemyTurn;
        if (currentTarget != null)
        {
            currentTarget.ExecutePerfectShot(this);
        }
    }

    public void ResumeDriving() => SetState(TankState.Driving);

    public void TriggerGameOver() => SetState(TankState.Dead);
}
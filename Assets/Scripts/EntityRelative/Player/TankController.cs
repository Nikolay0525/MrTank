using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class TankController : MonoBehaviour
    {
        public enum TankState { Garage, Driving, Combat, Dead }
        public enum CombatPhase { None, PlayerAiming, ProjectileInFlight, EnemyTurn }

        [Header("State Configuration")]
        public TankState currentState = TankState.Garage;
        public CombatPhase currentPhase = CombatPhase.None;

        [Header("Environment Control")]
        public float environmentSpeed = 4f;
        public AimingSystem aimingSystem;
        public static bool shouldAutoStart = false;

        [Header("Movement Dynamics")]
        public float accelerationRate = 2.5f;
        public float decelerationRate = 5f;

        private float targetGlobalSpeed = 0f;

        private CombatTimerUI combatTimerUI;

        [Header("Visual Effects")]
        public Animator fireEffectAnimator;
        public GameObject gunshotObj;
        public string fireAnimationName = "Gunshot";

        public static float CurrentGlobalSpeed { get; private set; }

        private float currentAimTimer;
        private GameObject activeProjectile;
        private EnemyAI currentTarget;
        private Health targetHealth;

        private void Awake()
        {
            if (UIManager.Instance.CombatTimer == null)
            {
                combatTimerUI = FindAnyObjectByType<CombatTimerUI>();
            }
            else combatTimerUI = UIManager.Instance.CombatTimer;

            TerrainChunk.hasGarageSpawned = false;

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

        private void Update()
        {
            if (Mathf.Approximately(Time.timeScale, 0f))
                return;

            ProcessState();
            UpdateGlobalSpeed();
        }
        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return EventSystem.current.IsPointerOverGameObject();
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
                return EventSystem.current.IsPointerOverGameObject(touchId);
            }

            return false;
        }
        private void UpdateGlobalSpeed()
        {
            if (Mathf.Approximately(CurrentGlobalSpeed, targetGlobalSpeed))
                return;

            float currentRate = (targetGlobalSpeed > CurrentGlobalSpeed) ? accelerationRate : decelerationRate;

            CurrentGlobalSpeed = Mathf.MoveTowards(CurrentGlobalSpeed, targetGlobalSpeed, currentRate * Time.deltaTime);
        }

        private void SetState(TankState newState)
        {
            currentState = newState;

            targetGlobalSpeed = (currentState == TankState.Driving) ? environmentSpeed : 0f;

            if (currentState == TankState.Garage)
            {
                CurrentGlobalSpeed = 0f;
                targetGlobalSpeed = 0f;
            }
        }

        private void ProcessState()
        {
            if (currentState != TankState.Combat) return;

            switch (currentPhase)
            {
                case CombatPhase.PlayerAiming:
                    currentAimTimer -= Time.deltaTime;

                    if (combatTimerUI != null)
                    {
                        combatTimerUI.UpdateTimer(currentAimTimer);
                    }

                    bool isFired = false;
                    if (!IsPointerOverUI())
                    {
                        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) isFired = true;
                        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) isFired = true;
                    }

                    if (isFired)
                    {
                        if (combatTimerUI != null) combatTimerUI.HideTimer();
                        ExecuteFire();
                    }
                    else if (currentAimTimer <= 0f)
                    {
                        if (combatTimerUI != null) combatTimerUI.HideTimer();
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

            if (combatTimerUI != null && currentTarget != null)
            {
                combatTimerUI.ShowTimer(currentTarget.transform, currentAimTimer);
            }

            aimingSystem.StartAiming();
        }

        private void ExecuteFire()
        {
            if (fireEffectAnimator != null)
            {
                gunshotObj.SetActive(true);
                fireEffectAnimator.Play(fireAnimationName, 0, 0f);
            }

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
                    DifficultyManager.Instance.EnemiesPassedSinceLastStation++;
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

        public void ResumeDriving()
        {
            SetState(TankState.Driving);
            if (aimingSystem != null)
            {
                aimingSystem.ResetGunRotation();
            }
        }

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
}

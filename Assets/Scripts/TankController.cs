using UnityEngine;

public class TankController : MonoBehaviour
{
    public enum TankState
    {
        Driving,
        Combat,
        Dead
    }

    [Header("State Configuration")]
    public TankState currentState = TankState.Driving;

    [Header("Environment Control")]
    public float environmentSpeed = 5f;

    // Статическая переменная для доступа из скриптов генерации и параллакса
    public static float CurrentGlobalSpeed { get; private set; }

    private void Start()
    {
        SetState(TankState.Driving);
    }

    private void Update()
    {
        ProcessState();
    }

    private void SetState(TankState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case TankState.Driving:
                CurrentGlobalSpeed = environmentSpeed;
                break;
            case TankState.Combat:
                CurrentGlobalSpeed = 0f;
                // Вызов метода инициализации параболы прицеливания
                break;
            case TankState.Dead:
                CurrentGlobalSpeed = 0f;
                // Вызов метода инициализации экрана Game Over
                break;
        }
    }

    private void ProcessState()
    {
        switch (currentState)
        {
            case TankState.Driving:
                // Логика состояния движения. Танк статичен по оси X.
                // Вертикальное позиционирование контролируется компонентом Rigidbody2D.
                break;
            case TankState.Combat:
                // Фиксация пользовательского ввода для инициализации выстрела
                if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                {
                    ExecuteFire();
                }
                break;
            case TankState.Dead:
                // Остановка всех процессов, ожидание перезапуска
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == TankState.Driving)
        {
            if (collision.CompareTag("Enemy"))
            {
                SetState(TankState.Combat);
            }
        }
    }

    private void ExecuteFire()
    {
        // Инстанцирование снаряда и передача ему текущего вектора параболы.
        // Возврат в состояние Driving (при попадании) или Dead (при промахе) 
        // должен вызываться через публичный метод SetState из скрипта логики снаряда.
    }

    public void ResumeDriving()
    {
        SetState(TankState.Driving);
    }

    public void TriggerGameOver()
    {
        SetState(TankState.Dead);
    }
}

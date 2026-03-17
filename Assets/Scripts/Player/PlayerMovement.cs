using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{

    public static PlayerMovement Instance { get; private set; }

    [SerializeField] private float movingSpeed = 5f;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private LayerMask wallLayer; // Добавляем слой для стен

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    private float minMovingSpeed = 0.1f;
    private bool isRunning = false;

    // Добавленные переменные для направления
    private bool isUp = false;
    private bool isDown = false;
    private bool isLeft = false;
    private bool isRight = false;

    // Новая переменная для последнего направления
    private Vector2 lastDirection = Vector2.zero;
    private Vector2 nextDirection = Vector2.zero;

    // Для отслеживания состояний клавиш в прошлом кадре
    private bool wasLeftPressed = false;
    private bool wasRightPressed = false;
    private bool wasUpPressed = false;
    private bool wasDownPressed = false;

    // Для сброса isRunning при смене направления
    private Vector2 previousDirection = Vector2.zero;
    private float directionChangeTime = 0f;
    private float resetDuration = 0.05f;

    // Для сеточного движения
    private Vector2 targetPosition;
    private bool isMovingToTarget = false;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        targetPosition = rb.position;
        lastDirection = Vector2.down;
    }

    private void Update()
    {
        Vector2 rawInput = GameInput.Instance.GetMovementVector();

        // Определяем, какие клавиши нажаты в этом кадре
        bool isLeftPressed = rawInput.x < -minMovingSpeed;
        bool isRightPressed = rawInput.x > minMovingSpeed;
        bool isUpPressed = rawInput.y > minMovingSpeed;
        bool isDownPressed = rawInput.y < -minMovingSpeed;

        // Проверяем момент нажатия (было false, стало true)
        if (isLeftPressed && !wasLeftPressed)
        {
            nextDirection = Vector2.left;
        }
        if (isRightPressed && !wasRightPressed)
        {
            nextDirection = Vector2.right;
        }
        if (isUpPressed && !wasUpPressed)
        {
            nextDirection = Vector2.up;
        }
        if (isDownPressed && !wasDownPressed)
        {
            nextDirection = Vector2.down;
        }

        // Проверяем момент отпускания (было true, стало false)
        if (!isLeftPressed && wasLeftPressed)
        {
            if (isRightPressed) nextDirection = Vector2.right;
            else if (isUpPressed) nextDirection = Vector2.up;
            else if (isDownPressed) nextDirection = Vector2.down;
            else nextDirection = Vector2.zero;
        }

        if (!isRightPressed && wasRightPressed)
        {
            if (isLeftPressed) nextDirection = Vector2.left;
            else if (isUpPressed) nextDirection = Vector2.up;
            else if (isDownPressed) nextDirection = Vector2.down;
            else nextDirection = Vector2.zero;
        }

        if (!isUpPressed && wasUpPressed)
        {
            if (isLeftPressed) nextDirection = Vector2.left;
            else if (isRightPressed) nextDirection = Vector2.right;
            else if (isDownPressed) nextDirection = Vector2.down;
            else nextDirection = Vector2.zero;
        }

        if (!isDownPressed && wasDownPressed)
        {
            if (isLeftPressed) nextDirection = Vector2.left;
            else if (isRightPressed) nextDirection = Vector2.right;
            else if (isUpPressed) nextDirection = Vector2.up;
            else nextDirection = Vector2.zero;
        }

        // Если все клавиши отпущены
        if (rawInput == Vector2.zero)
        {
            nextDirection = Vector2.zero;
        }

        // Запоминаем состояния для следующего кадра
        wasLeftPressed = isLeftPressed;
        wasRightPressed = isRightPressed;
        wasUpPressed = isUpPressed;
        wasDownPressed = isDownPressed;

        // Проверяем, можно ли двигаться в выбранном направлении
        if (nextDirection != Vector2.zero && !isMovingToTarget)
        {
            // Проверяем, есть ли стена на пути
            if (!IsWallInDirection(nextDirection))
            {
                targetPosition = rb.position + nextDirection * gridSize;
                isMovingToTarget = true;
                lastDirection = nextDirection;
            }
            else
            {
                // Если стена есть - просто сбрасываем nextDirection? 
                // Или оставляем как есть? Оставляем как есть, чтобы запомнить направление
                // но не начинаем движение
                // Debug.Log("Стена!");
            }
        }
    }

    // Проверка на стену
    private bool IsWallInDirection(Vector2 direction)
    {
        // Создаем луч из центра персонажа в направлении движения
        Vector2 origin = rb.position;
        Vector2 size = boxCollider.size * 0.95f; // Немного уменьшаем размер для точности

        // Пускаем BoxCast в направлении движения на расстояние gridSize
        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, direction, gridSize, wallLayer);

        return hit.collider != null; // Если есть попадание - значит стена
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Движение к целевой клетке
        if (isMovingToTarget)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, movingSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            // Проверяем, достигли ли цели
            if (Vector2.Distance(rb.position, targetPosition) < 0.01f)
            {
                rb.MovePosition(targetPosition);
                isMovingToTarget = false;

                // Если есть следующее направление - проверяем и начинаем движение
                if (nextDirection != Vector2.zero)
                {
                    // НОВОЕ: Снова проверяем стену перед следующим движением
                    if (!IsWallInDirection(nextDirection))
                    {
                        targetPosition = rb.position + nextDirection * gridSize;
                        isMovingToTarget = true;
                        lastDirection = nextDirection;
                    }
                    // Если стена - просто не начинаем движение
                }
            }
        }

        // Проверяем, изменилось ли направление (для сброса isRunning)
        if (lastDirection != previousDirection)
        {
            directionChangeTime = resetDuration;
            previousDirection = lastDirection;
        }

        // Обновляем таймер сброса
        if (directionChangeTime > 0)
        {
            directionChangeTime -= Time.fixedDeltaTime;
        }

        // ЛОГИКА АНИМАЦИИ
        if (isMovingToTarget)
        {
            if (directionChangeTime > 0)
            {
                isRunning = false;
            }
            else
            {
                isRunning = true;
            }

            isUp = lastDirection == Vector2.up;
            isDown = lastDirection == Vector2.down;
            isLeft = lastDirection == Vector2.left;
            isRight = lastDirection == Vector2.right;
        }
        else
        {
            isRunning = false;
        }
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public bool IsUp()
    {
        return isUp;
    }

    public bool IsDown()
    {
        return isDown;
    }

    public bool IsLeft()
    {
        return isLeft;
    }

    public bool IsRight()
    {
        return isRight;
    }

    // Контроль состояния бега у персонажа
    public void SetRunning(bool running)
    {
        isRunning = running;
    }

}
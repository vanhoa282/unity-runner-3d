using CosmicRunner.Audio;
using CosmicRunner.Core;
using UnityEngine;

namespace CosmicRunner.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Lane Settings")]
        [SerializeField] private float laneDistance = 3.0f; // Khoảng cách giữa các làn
        [SerializeField] private float laneChangeSpeed = 16.0f; // Tốc độ lướt đổi làn

        [Header("Jump & Gravity")]
        [SerializeField] private float jumpHeight = 2.2f;
        [SerializeField] private float gravity = 25.0f;

        [Header("Touch Swipe Settings")]
        [SerializeField] private float minSwipeDistance = 40.0f; // Ngưỡng cảm ứng vuốt (pixel)

        private CharacterController controller;
        private int currentLane = 0; // -1: Trái, 0: Giữa, 1: Phải
        private Vector3 verticalVelocity;
        private bool isGrounded;

        // Cảm ứng
        private Vector2 touchStartPos;
        private bool isSwiping = false;

        private Vector3 startPosition;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            startPosition = transform.position;
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStarted += ResetPlayer;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStarted -= ResetPlayer;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            HandleInput();
            ApplyMovement();
        }

        private void HandleInput()
        {
            // --- 1. XỬ LÝ CẢM ỨNG VUỐT TRÊN MÀN HÌNH ANDROID ---
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                    isSwiping = true;
                }
                else if (touch.phase == TouchPhase.Ended && isSwiping)
                {
                    Vector2 delta = touch.position - touchStartPos;

                    if (delta.magnitude >= minSwipeDistance)
                    {
                        float xDiff = delta.x;
                        float yDiff = delta.y;

                        if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
                        {
                            // Vuốt ngang
                            if (xDiff > 0) MoveLane(1); // Phải
                            else MoveLane(-1);          // Trái
                        }
                        else
                        {
                            // Vuốt dọc
                            if (yDiff > 0) Jump(); // Vuốt lên
                        }
                    }
                    isSwiping = false;
                }
            }

            // --- 2. FALLBACK BÀN PHÍM (TESTING) ---
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveLane(-1);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveLane(1);
            }

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                Jump();
            }
        }

        private void MoveLane(int direction)
        {
            int targetLane = Mathf.Clamp(currentLane + direction, -1, 1);
            if (targetLane != currentLane)
            {
                currentLane = targetLane;
                SoundManager.Instance?.PlayLaneChangeSound();
            }
        }

        private void Jump()
        {
            if (controller.isGrounded)
            {
                verticalVelocity.y = Mathf.Sqrt(2 * jumpHeight * gravity);
                SoundManager.Instance?.PlayJumpSound();
            }
        }

        private void ApplyMovement()
        {
            float forwardSpeed = GameManager.Instance.CurrentSpeed;

            // Tính vị trí mục tiêu theo làn
            float targetX = currentLane * laneDistance;
            float currentX = transform.position.x;
            float newX = Mathf.MoveTowards(currentX, targetX, laneChangeSpeed * Time.deltaTime);

            // Xử lý trọng lực
            if (controller.isGrounded)
            {
                if (verticalVelocity.y < 0) verticalVelocity.y = -2f;
            }
            else
            {
                verticalVelocity.y -= gravity * Time.deltaTime;
            }

            Vector3 move = new Vector3(newX - currentX, verticalVelocity.y * Time.deltaTime, forwardSpeed * Time.deltaTime);
            controller.Move(move);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Obstacle"))
            {
                SoundManager.Instance?.PlayCrashSound();
                GameManager.Instance.TriggerGameOver();
            }
            else if (other.CompareTag("Coin"))
            {
                SoundManager.Instance?.PlayCoinSound();
                GameManager.Instance.AddCoin(1);
                other.gameObject.SetActive(false);
            }
        }

        public void ResetPlayer()
        {
            currentLane = 0;
            verticalVelocity = Vector3.zero;
            transform.position = startPosition;
            if (controller != null)
            {
                controller.enabled = false;
                transform.position = startPosition;
                controller.enabled = true;
            }
        }
    }
}

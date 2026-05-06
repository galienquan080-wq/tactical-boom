using UnityEngine;

/// <summary>
/// Điều khiển chuyển động và hành động chính của Player
/// </summary>
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerHealth playerHealth;
    private WeaponSystem weaponSystem;
    private BombDefuseSystem bombDefuse;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crawlSpeed = 1f;
    [SerializeField] private float jumpForce = 5f;

    private float currentSpeed;
    private Vector3 velocity;
    private bool isSprinting = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>();
        weaponSystem = GetComponent<WeaponSystem>();
        bombDefuse = FindObjectOfType<BombDefuseSystem>();

        // Subscribe to events
        playerHealth.OnInjuryStateChanged += HandleInjuryStateChanged;
    }

    void Update()
    {
        if (!playerHealth.IsAlive()) return;

        HandleMovement();
        HandleWeaponInput();
        HandleDefuseInput();
    }

    void HandleMovement()
    {
        // Lấy input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;

        // Xác định tốc độ di chuyển
        isSprinting = Input.GetKey(KeyCode.LeftShift) && !playerHealth.IsCrawling();
        
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        
        // Áp dụng multiplier nếu bị thương chân
        currentSpeed *= playerHealth.GetMoveSpeedMultiplier();

        // Di chuyển
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Gravity
        velocity.y -= 9.8f * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded && !playerHealth.IsCrawling())
        {
            velocity.y = jumpForce;
        }
        else if (characterController.isGrounded)
        {
            velocity.y = 0;
        }
    }

    void HandleWeaponInput()
    {
        // Bắn
        if (Input.GetMouseButton(0))
        {
            Vector3 shootDirection = Camera.main.transform.forward;
            weaponSystem.Fire(shootDirection);
        }

        // Reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            weaponSystem.Reload();
        }

        // Đổi vũ khí
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleWeapon();
        }
    }

    void HandleDefuseInput()
    {
        // Bắt đầu gỡ bom
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!bombDefuse.IsDefusing())
            {
                bombDefuse.StartDefuse();
            }
            else
            {
                bombDefuse.InterruptDefuse();
            }
        }
    }

    void ToggleWeapon()
    {
        WeaponSystem.WeaponType currentType = weaponSystem.GetCurrentWeapon().type;
        WeaponSystem.WeaponType nextType = currentType == WeaponSystem.WeaponType.M4A1 
            ? WeaponSystem.WeaponType.G13 
            : WeaponSystem.WeaponType.M4A1;
        
        weaponSystem.EquipWeapon(nextType);
    }

    void HandleInjuryStateChanged(PlayerHealth.InjuryState newState)
    {
        switch (newState)
        {
            case PlayerHealth.InjuryState.BothLegsInjured:
                Debug.Log("[PLAYER] Entered CRAWL mode!");
                // TODO: Play crawl animation
                break;
            case PlayerHealth.InjuryState.LeftArmInjured:
            case PlayerHealth.InjuryState.RightArmInjured:
                Debug.Log("[PLAYER] Arm injured - increased recoil!");
                break;
        }
    }
}

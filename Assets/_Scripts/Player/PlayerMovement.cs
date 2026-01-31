using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerStats MoveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;
    private SpriteRenderer _spriteRenderer;
    private PartyMemberController _partyMemberController;

    [Header("AfterImage")]
    [SerializeField] private GameObject _afterImageGameObject;
    private bool _canCreateAfterImage;

    [Header("Knockback")]
    [SerializeField] private float knockbackTime;
    [SerializeField] private float hitDirectionForce;
    [SerializeField] private float constantForce;
    [SerializeField] private AnimationCurve knockbackCurve;
    private Coroutine knockbackCoroutine;
    public bool isBeingKnockedBack { get; private set; }

    private CinemachineImpulseSource _impulseSource;
    private Animator _animator;
    private Rigidbody2D _rb;

    // Party Vars
    public bool isLeader;

    // Movement Vars
    public float HorizontalVelocity { get; private set; }
    public bool _isFacingRight { get; private set; }
    public bool _canAct;

    // Collision Vars
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private RaycastHit2D _wallHit;
    private RaycastHit2D _lastWallHit;
    public bool _isGrounded { get; private set; }
    private bool _bumpedHead;
    private bool _isTouchingWall;

    // Jump Vars
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    // Apex Vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    // Jump Buffer Vars
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    // Coyote Time Vars
    private float _coyoteTimer;

    // Wall Slide Vars
    private bool _isWallSliding;
    private bool _isWallSlideFalling;

    // Wall Jump Vars
    private bool _useWallJumpMoveStats;
    private bool _isWallJumping;
    private float _wallJumpTime;
    private bool _isWallJumpFastFalling;
    private bool _isWallJumpFalling;
    private float _wallJumpFastFallTime;
    private float _wallJumpFastFallReleaseSpeed;

    private float _wallJumpPostBufferTimer;

    private float _wallJumpApexPoint;
    private float _timePastWallJumpApexThreshold;
    private bool _isPastWallJumpApexThreshold;

    // Dash Vars
    private bool _isDashing;
    private bool _isAirDashing;
    private float _dashTimer;
    private float _dashOnGroundTimer;
    private int _numberOfDashesUsed;
    private Vector2 _dashDirection;
    private bool _isDashFastFalling;
    private float _dashFastFallTime;
    private float _dashFastFallReleaseSpeed;

    // Attack Vars
    private bool _isGroundAttacking;
    private bool _isAirAttacking;

    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _partyMemberController = GetComponent<PartyMemberController>();
        _isGroundAttacking = false;
        _isAirAttacking = false;
        _canAct = true;
        _canCreateAfterImage = true;
        isBeingKnockedBack = false;
    }

    private void Update()
    {
        if (_canAct && !isBeingKnockedBack)
        {
            CountTimers();
            JumpChecks();
            LandCheck();
            WallSlideCheck();
            WallJumpCheck();
            DashCheck();
        } 
    }

    private void FixedUpdate()
    {
        if (_canAct && !isBeingKnockedBack)
        {
            CollisionChecks();
            Jump();
            Fall();
            WallSlide();
            WallJump();
            Dash();

            if (_isGrounded)
            {
                Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
            }
            else
            {
                if (_useWallJumpMoveStats)
                {
                    Move(MoveStats.WallJumpMoveAcceleration, MoveStats.WallJumpMoveDeveleration, InputManager.Movement);
                }
                else
                {
                    Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
                }

            }
            ApplyVelocity();
        }       
    }

    private void ApplyVelocity()
    {
        if (!_isDashing)
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
        }
        else
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -50f, 50f);

            if (_canCreateAfterImage)
            {
                GameObject afterImageObj = ObjectPoolManager.SpawnObject(_afterImageGameObject, gameObject.transform.position, gameObject.transform.rotation, ObjectPoolManager.PoolType.Sprites);
                afterImageObj.GetComponent<SpriteRenderer>().sprite = _spriteRenderer.sprite;
                afterImageObj.gameObject.transform.localScale = transform.localScale;
                StartCoroutine(CreateAfterImageCooldown());
            }
        }

        if (!_isGroundAttacking)
        {
            _rb.velocity = new Vector2(HorizontalVelocity * MoveStats.MovementReductionAttack, VerticalVelocity);
        }
        else
        {
            _rb.velocity = Vector2.zero;
        }
        
    }

    private IEnumerator CreateAfterImageCooldown()
    {
        _canCreateAfterImage = false;
        yield return new WaitForSeconds(0.06f);
        _canCreateAfterImage = true;
    }

    public void SetVelocities(float horizontal, float vertical)
    {
        HorizontalVelocity = horizontal;
        VerticalVelocity = vertical;
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (!_isDashing)
        {
            if (Mathf.Abs(moveInput.x) >= MoveStats.MoveThreshold)
            {
                TurnCheck(moveInput);

                float targetVelocity = 0f;
                if (InputManager.RunIsHeld)
                {
                    targetVelocity = moveInput.x * MoveStats.MaxRunSpeed;
                }
                else
                {
                    targetVelocity = moveInput.x * MoveStats.MaxWalkSpeed;
                }

                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else if (Mathf.Abs(moveInput.x) < MoveStats.MoveThreshold)
            {
                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, 0f, deceleration * Time.fixedDeltaTime);
            }
            if (!InputManager.RunIsHeld && Mathf.Abs(HorizontalVelocity) > 0)
            {
                _animator.SetFloat("moveSpeed", Mathf.Abs(MoveStats.MaxWalkSpeed));
            }
            else
            {
                _animator.SetFloat("moveSpeed", Mathf.Abs(HorizontalVelocity));
            }
        }   
    }


    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0f)
        {
            Turn(false);
        }
        else if (!_isFacingRight && moveInput.x > 0f) {
            {
                Turn(true);
            }
        }
    }
    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }
    #endregion

    #region Fall/Land

    private void LandCheck()
    {
        if ((_isJumping || _isFalling || _isWallJumpFalling || _isWallJumping || _isWallSlideFalling || _isWallSliding || _isDashFastFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            ResetJumpValues();
            StopWallSlide();
            ResetWallJumpValues();
            ResetDashes();

            _numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;

            if (_isDashFastFalling && _isGrounded)
            {
                ResetDashValues();
                return;
            }

            ResetDashValues();
        }
    }

    private void Fall()
    {
        // Normal gravity while falling
        if (!_isGrounded && !_isJumping && !_isWallSliding && !_isWallJumping && !_isDashing && !_isDashFastFalling)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }
            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }
    }
    #endregion

    #region Jump

    private void ResetJumpValues()
    {
        _isJumping = false;
        _isFalling = false;
        _isFastFalling = false;
        _fastFallTime = 0f;
        _isPastApexThreshold = false;
    }
    private void JumpChecks()
    {
        // When we press jump button
        if (InputManager.JumpWasPressed && !_isGroundAttacking)
        {
            if(_isWallSlideFalling && _wallJumpPostBufferTimer >= 0f)
            {
                return;
            }else if(_isWallSliding || (_isTouchingWall && !_isGrounded))
            {
                return;
            }
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        // When we release jump button
        if (InputManager.JumpWasReleased)
        {
            if(_jumpBufferTimer > 0f)
            {
                _jumpReleasedDuringBuffer = true;
            }
            if(_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        // Inititate jump with jump buffering and coyote time, double jump, air jump after coyote time lapsed
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InititiateJump(1);
            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        } else if (_jumpBufferTimer > 0f && (_isJumping || _isWallJumping || _isWallSlideFalling || _isAirDashing || _isDashFastFalling) && !_isTouchingWall && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InititiateJump(1);
            if (_isDashFastFalling)
            {
                _isDashFastFalling = false;
            }

        } else if (_jumpBufferTimer > 0f && _isFalling && !_isWallSlideFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {
            InititiateJump(2);
            _isFastFalling = false;
        }
        

    }

    private void InititiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
        { 
            _isJumping = true;
            _animator.SetTrigger("jumpStart");
        }

        ResetWallJumpValues();

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        // Apply gravity while jumping
        if (_isJumping)
        {
            // Check for head bump
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }
            // Gravity Ascending
            if(VerticalVelocity >= 0f)
            {
                // Apex Controls
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);
                if(_apexPoint > MoveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
                // Gravity on ascending but not past apex threshold
                else if(!_isFastFalling)
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }   
            }
            // Gravity on descending
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        // Jump Cut
        if (_isFastFalling)
        {
            if(_fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(_fastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MoveStats.TimeForUpwardsCancel));
            }
            _fastFallTime += Time.fixedDeltaTime;
        }
        _animator.SetFloat("verticalVelocity", VerticalVelocity);
    }

    #endregion

    #region Wall Slide

    private void WallSlideCheck()
    {
        if(_isTouchingWall && !_isDashing && !_isGrounded)
        {
            if (VerticalVelocity < 0f && !_isWallSliding)
            {
                ResetJumpValues();
                ResetWallJumpValues();
                ResetDashValues();

                if (MoveStats.ResetDashOnWallSlide)
                {
                    ResetDashes();
                }

                _isWallSlideFalling = false;
                _isWallSliding = true;

                if (MoveStats.ResetJumpsOnWallSlide)
                {
                    _numberOfJumpsUsed = 0;
                }
            }
        }else if(_isWallSliding && !_isTouchingWall && !_isGrounded && !_isWallSlideFalling)
        {
            _isWallSlideFalling = true;
            StopWallSlide();
        }
        else
        {
            StopWallSlide();
        }
    }

    private void StopWallSlide()
    {
        if (_isWallSliding)
        {
            _numberOfJumpsUsed++;
            _isWallSliding = false;
            _animator.SetBool("isWallSliding", false);
        }
    }

    private void WallSlide()
    {
        if (_isWallSliding)
        {
            VerticalVelocity = Mathf.Lerp(VerticalVelocity, -MoveStats.WallSlideSpeed, MoveStats.WallSlideDecelerationSpeed * Time.fixedDeltaTime);
            _animator.SetBool("isWallSliding", true);
        }
    }

    #endregion

    #region Wall Jump

    private void WallJumpCheck()
    {
        if (ShouldApplyPostWallJumpBuffer())
        {
            _wallJumpPostBufferTimer = MoveStats.WallJumpPostBufferTime;
        }

        // Wall Jump Fast Falling
        if(InputManager.JumpWasReleased && !_isWallSliding && !_isTouchingWall && _isWallJumping)
        {
            if(VerticalVelocity > 0f)
            {
                if (_isPastWallJumpApexThreshold)
                {
                    _isPastWallJumpApexThreshold = false;
                    _isWallJumpFastFalling = true;
                    _wallJumpFastFallTime = MoveStats.TimeForUpwardsCancel;

                    VerticalVelocity = 0f;
                }
                else
                {
                    _isWallJumpFastFalling = true;
                    _wallJumpFastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        // Actual Jump With Post Wall Jump Buffer Time
        if(InputManager.JumpWasPressed && _wallJumpPostBufferTimer > 0f)
        {
            InitiateWallJump();
        }
    }

    private void InitiateWallJump()
    {
        if (!_isWallJumping)
        {
            _isWallJumping = true;
            _useWallJumpMoveStats = true;
        }

        StopWallSlide();
        ResetJumpValues();
        _wallJumpTime = 0f;

        VerticalVelocity = MoveStats.InitialWallJumpVelocity;

        int dirMultiplier = 0;
        Vector2 hitPoint = _lastWallHit.collider.ClosestPoint(_bodyColl.bounds.center);

        if (hitPoint.x > transform.position.x)
        {
            dirMultiplier = -1;
        }
        else
        {
            dirMultiplier = 1;
        }

        HorizontalVelocity = Mathf.Abs(MoveStats.WallJumpDirection.x) * dirMultiplier;
    }

    private void WallJump()
    {
        // Apply Wall Jump Gravity
        if (_isWallJumping)
        {
            // Time to take over movement controls whill wall jumping
            _wallJumpTime += Time.fixedDeltaTime;
            if (_wallJumpTime >= MoveStats.TimeTillJumpApex)
            {
                _useWallJumpMoveStats = false;
            }

            // Hit Head
            if (_bumpedHead)
            {
                _isWallJumpFastFalling = true;
                _useWallJumpMoveStats = false;
            }

            // Gravity Ascending
            if (VerticalVelocity >= 0f)
            {
                // Apex Controls
                _wallJumpApexPoint = Mathf.InverseLerp(MoveStats.WallJumpDirection.y, 0f, VerticalVelocity);

                if (_wallJumpApexPoint > MoveStats.ApexThreshold)
                {
                    if (!_isPastWallJumpApexThreshold)
                    {
                        if (!_isPastWallJumpApexThreshold)
                        {
                            _isPastWallJumpApexThreshold = true;
                            _timePastWallJumpApexThreshold = 0f;
                        }

                        if (_isPastWallJumpApexThreshold)
                        {
                            _timePastWallJumpApexThreshold += Time.fixedDeltaTime;
                            if (_timePastWallJumpApexThreshold < MoveStats.ApexHangTime)
                            {
                                VerticalVelocity = 0f;
                            }
                            else
                            {
                                VerticalVelocity = -0.01f;
                            }
                        }
                    }


                }
                // Gravity in ascending but not past apex threshold
                else if (!_isWallJumpFastFalling)
                {
                    VerticalVelocity += MoveStats.WallJumpGravity * Time.fixedDeltaTime;
                    if (_isPastWallJumpApexThreshold)
                    {
                        _isPastWallJumpApexThreshold = false;
                    }
                }
            }
            // Gravity Descending
            else if (!_isWallJumpFastFalling)
            {
                VerticalVelocity += MoveStats.WallJumpGravity * Time.fixedDeltaTime;
            }
            else if(VerticalVelocity < 0f)
            {
                if (!_isWallJumpFalling)
                    _isWallJumpFalling = true;
            }
        }

        // Handle WallJumpCut time
        if (_isWallJumpFastFalling)
        {
            if(_wallJumpFastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MoveStats.WallJumpGravity * MoveStats.WallJumpGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(_wallJumpFastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_wallJumpFastFallReleaseSpeed, 0f, (_wallJumpFastFallTime / MoveStats.TimeForUpwardsCancel));
            }
            _wallJumpFastFallTime += Time.fixedDeltaTime;
        }
    }

    private bool ShouldApplyPostWallJumpBuffer()
    {
        if (!_isGrounded && (_isTouchingWall || _isWallSliding))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ResetWallJumpValues()
    {
        _isWallSlideFalling = false;
        _useWallJumpMoveStats = false;
        _isWallJumping = false;
        _isWallJumpFastFalling = false;
        _isWallJumpFalling = false;
        _isPastWallJumpApexThreshold = false;

        _wallJumpFastFallTime = 0f;
        _wallJumpTime = 0f;
    }

    #endregion

    #region Dash

    private void DashCheck()
    {
        if (InputManager.DashWasPressed)
        {
            if(_isGrounded && _dashOnGroundTimer < 0 && !_isDashing)
            {
                InitiateDash();
            }else if(!_isGrounded && !_isDashing && _numberOfDashesUsed < MoveStats.NumberOfDashes)
            {
                _isAirDashing = true;
                InitiateDash();

                if(_wallJumpPostBufferTimer > 0f)
                {
                    _numberOfJumpsUsed--;
                    if (_numberOfJumpsUsed < 0f)
                    {
                        _numberOfJumpsUsed = 0;
                    }
                }
            }
        }
    }

    private void InitiateDash()
    {
        _dashDirection = InputManager.Movement;

        Vector2 closestDirection = Vector2.zero;
        float minDistance = Vector2.Distance(_dashDirection, MoveStats.DashDirections[0]);

        for (int i = 0; i < MoveStats.DashDirections.Length; i++)
        {
            if (_dashDirection == MoveStats.DashDirections[i])
            {
                closestDirection = _dashDirection;
                break;
            }
            float distance = Vector2.Distance(_dashDirection,MoveStats.DashDirections[i]);
            bool isDiagonal = (Mathf.Abs(MoveStats.DashDirections[i].x) == 1 && Mathf.Abs(MoveStats.DashDirections[i].y) == 1);
            if (isDiagonal)
            {
                distance -= MoveStats.DashDiagonallyBias;
            }else if(distance < minDistance)
            {
                minDistance = distance;
                closestDirection = MoveStats.DashDirections[i];
            }
        }

        if(closestDirection == Vector2.zero)
        {
            if (_isFacingRight)
            {
                closestDirection = Vector2.right;
            }
            else
            {
                closestDirection = Vector2.left;
            }
        }

        _dashDirection = closestDirection;

        _dashDirection = closestDirection;
        _numberOfDashesUsed++;
        _isDashing = true;
        _dashTimer = 0f;
        _dashOnGroundTimer = MoveStats.TimeBtwDashesOnGround;

        CameraShakeManager.Instance.CameraShake(_impulseSource, CameraShakeManager.GeneralShakeIntensity.Light);
        StartCoroutine(DelayInvincibilityFromDash(MoveStats.DelayedDashInvincibility));

        ResetJumpValues();
        ResetWallJumpValues();
        StopWallSlide();
    }

    private IEnumerator DelayInvincibilityFromDash(float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        _partyMemberController.InitiateInvincibility(MoveStats.DashInvincibilityTimeFrame);
    }

    private void Dash()
    {
        if (_isDashing)
        {
            _dashTimer += Time.fixedDeltaTime;
            if(_dashTimer >= MoveStats.DashTime)
            {
                if (_isGrounded)
                {
                    ResetDashes();
                }
                _isAirDashing = false;
                _isDashing = false;
                if(!_isJumping && !_isWallJumping)
                {
                    _dashFastFallTime = 0f;
                    _dashFastFallReleaseSpeed = VerticalVelocity;
                    if (!_isGrounded)
                    {
                        _isDashFastFalling = true;
                    }
                }
                return;
            }

            HorizontalVelocity = MoveStats.DashSpeed * _dashDirection.x;
            if (_dashDirection.y != 0f || _isAirDashing)
            {
                VerticalVelocity = MoveStats.DashSpeed * _dashDirection.y;
            }
        }
        else if (_isDashFastFalling)
        {
            if(VerticalVelocity > 0f)
            {
                if(_dashFastFallTime < MoveStats.DashTimeForUpwardsCancel)
                {
                    VerticalVelocity = Mathf.Lerp(_dashFastFallReleaseSpeed, 0f, (_dashFastFallTime / MoveStats.DashTimeForUpwardsCancel));
                }
                else if(_dashFastFallTime >= MoveStats.DashTimeForUpwardsCancel)
                {
                    VerticalVelocity += MoveStats.Gravity * MoveStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }

                _dashFastFallTime += Time.fixedDeltaTime;
            }
            else
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
        }
        
    }

    private void ResetDashValues()
    {
        _isDashFastFalling = false;
        _dashOnGroundTimer = -0.01f;
    }

    private void ResetDashes()
    {
        _numberOfDashesUsed = 0;
    }

    #endregion

    #region Knockback
    IEnumerator KnockbackAction(Vector2 hitDirection, Vector2 constantForceDirection)
    {
        isBeingKnockedBack = true;
        Vector2 _hitForce;
        Vector2 _constantForce;
        Vector2 _knockbackForce;

        float _time = 0f;

        
        _constantForce = constantForce * constantForceDirection;

        float elapsedTime = 0f;
        while (elapsedTime < knockbackTime)
        {
            elapsedTime += Time.fixedDeltaTime;
            _time += Time.fixedDeltaTime;
            _hitForce = hitDirection * hitDirectionForce * knockbackCurve.Evaluate(_time);
            _knockbackForce = _hitForce + _constantForce;
            _rb.velocity = _knockbackForce;
            yield return new WaitForFixedUpdate();
        }
        isBeingKnockedBack = false;
    }

    public void CallKnockback(Vector2 hitDirection, Vector2 consantForceDirection)
    {
        knockbackCoroutine = StartCoroutine(KnockbackAction(hitDirection, consantForceDirection));
    }
    #endregion

    #region Collision Checks
    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        if (_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
        _animator.SetBool("isGrounded", _isGrounded);
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        if(_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else
        {
            _bumpedHead = false;
        }
    }

    private void IsTouchingWall()
    {
        float originEndPoint = 0f;
        if (_isFacingRight)
        {
            originEndPoint = _bodyColl.bounds.max.x;
        }
        else
        {
            originEndPoint = _bodyColl.bounds.min.x;
        }

        float adjustHeight = _bodyColl.bounds.size.y * MoveStats.WallDetectionRayHeightMultiplier;

        Vector2 boxCastOrigin = new Vector2(originEndPoint, _bodyColl.bounds.center.y);
        Vector2 boxCastSize = new Vector2(MoveStats.WallDetectionRayLength, adjustHeight);

        _wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, MoveStats.WallDetectionRayLength, MoveStats.GroundLayer);
        if(_wallHit.collider != null)
        {
            _lastWallHit = _wallHit;
            _isTouchingWall = true;
        }
        else
        {
            _isTouchingWall = false;
        }
    }

    public void IsGroundAttacking(bool isGroundAttacking)
    {
        _isGroundAttacking = isGroundAttacking;
        // Debug.Log("Is Ground Attacking: " + _isGroundAttacking.ToString());
    }

    public void IsAirAttacking(bool isAirAttacking)
    {
        _isAirAttacking = isAirAttacking;
        // Debug.Log("Is Air Attacking: " + _isAirAttacking.ToString());
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
        IsTouchingWall();
    }
    #endregion

    #region Timers
    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;
        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer = MoveStats.JumpCoyoteTime;
        }

        if (!ShouldApplyPostWallJumpBuffer())
        {
            _wallJumpPostBufferTimer -= Time.deltaTime;
        }

        if (_isGrounded)
        {
            _dashOnGroundTimer -= Time.deltaTime;
        }
    }
    #endregion
}

using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;

public class Player : NetworkBehaviour
{
    [HideInInspector] public Rigidbody rb;

    #region Stats
    [Tooltip("Determined max speed")]
    private float speed;
    [Tooltip("Reduces knockback distance and damage taken")]
    private float weight;
    [Tooltip("Reduces charge cooldown")]
    private float charge_up;
    [Tooltip("Movement acceleration")]
    private float handling;
    #endregion

    #region CharacterParts
    public GameObject head;
    public GameObject body;
    public GameObject legs;

    void InstantiateParts()
    {
        Transform head_slot = model.transform.GetChild(0);
        GameObject headObject = Instantiate(head, head.transform.position, head.transform.rotation);
        headObject.transform.parent = head_slot;

        Transform body_slot = model.transform.GetChild(1);
        GameObject bodyObject = Instantiate(body, body.transform.position, body.transform.rotation);
        bodyObject.transform.parent = body_slot;

        Transform legs_slot = model.transform.GetChild(2);
        GameObject legsObject = Instantiate(legs, legs.transform.position, legs.transform.rotation);
        legsObject.transform.parent = legs_slot;
    }

    void GenerateStats()
    {
        Part head_stats = head.GetComponent<Part>();
        Part body_stats = body.GetComponent<Part>();
        Part legs_stats = legs.GetComponent<Part>();

        speed = head_stats.speed + body_stats.speed + legs_stats.speed;
        Debug.Log(String.Format("Speed set to {0}", speed));
        weight = head_stats.weight + body_stats.weight + legs_stats.weight;
        Debug.Log(String.Format("Weight set to {0}", weight));
        charge_up = head_stats.charge_up + body_stats.charge_up + legs_stats.charge_up;
        Debug.Log(String.Format("Charge Up set to {0}", charge_up));
        handling = head_stats.handling + body_stats.handling + legs_stats.handling;
        Debug.Log(String.Format("Handling set to {0}", handling));
    }
    #endregion

    #region PlayerMovement
    [Header("Movement")]
    [Tooltip("The max movement speed when not dashing.")]
    public float moveSpeed;
    [Tooltip("The acceleration for movement when not dashing.")]
    public float accel;
    [Tooltip("The max speed for falling.")]
    public float fallSpeed;
    [Tooltip("The acceleration for falling.")]
    public float gravityAccel;
    [Tooltip("The max speed for rising in an updraft.")]
    public float updraftMaxSpeed;
    [Tooltip("The acceleration for rising in an updraft.")]
    public float updraftAccel;

    [Header("Attacking")]
    [Tooltip("The base dash speed.")]
    public float dashSpeed;
    [Tooltip("The acceleration for dashing.")]
    public float dashAccel;
    [Tooltip("The amount of time it takes to fully charge a dash.")]
    public float dashChargeTime;
    [Tooltip("The amount of time the dash lasts for (when fully charged).")]
    public float dashDuration;

    [Header("Spinning")]
    [Tooltip("The character model.")]
    public Transform model;
    [Tooltip("The spin speed (rotations per second) when not moving.")]
    public float idleSpinSpeed;
    [Tooltip("The spin speed (rotations per second) when moving normally.")]
    public float movingSpinSpeed;
    [Tooltip("The spin speed (rotations per second) when dashing.")]
    public float dashingSpinSpeed;

    private Vector2 moveInput;
    private Vector2 oldMoveInput;
    private bool firePressed;
    private bool oldFirePressed;

    private readonly NetworkVariable<Vector3> Velocity = new();
    private Vector3 velocity {
        get => Velocity.Value;
        set => Velocity.Value = value;
    }
    private Vector2 horVelocity 
    { 
        get => new(velocity.x, velocity.z); 
        set => velocity = new(value.x, velocity.y, value.y);
    }
    private float verVelocity 
    { 
        get => velocity.y; 
        set => velocity = new(velocity.x, value, velocity.z);
    }

    private Vector3 lookDirection;
    private Vector3 oldLookDirection;

    private Vector3 dashDirection;
    private NetworkVariable<float> DashAmount = new();
    private float dashAmount {
        get => DashAmount.Value;
        set => DashAmount.Value = value;
    }
    private float maxDashAmount;
    private bool dashing;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        InstantiateParts();
        GenerateStats();
    }

    void Start()
    {
        if(IsLocalPlayer)
        {
            CinemachineFreeLook freeLook = FindObjectOfType<CinemachineFreeLook>();
            freeLook.m_LookAt = transform;
            freeLook.m_Follow = transform;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if(IsClient)
        {
            UpdateClient();
        }

        if(IsServer)
        {
            UpdateServer();
        }
    }

    private void UpdateServer()
    {
        if(dashing)
        {
            dashAmount -= Time.deltaTime/dashDuration;

            if(dashAmount <= 0)
            {
                dashAmount = 0;
                dashing = false;
            }
        }
        else
        {
            if(firePressed)
            {
                dashAmount += Time.deltaTime / dashChargeTime;
                if(dashAmount > 1)
                    dashAmount = 1;
            }
            else if(dashAmount > 0)
            {
                dashing = true;
                maxDashAmount = dashAmount;
                dashDirection = lookDirection;
            }
        }
    }

    private void UpdateClient()
    {
        if(IsOwner)
        {
            lookDirection = Camera.main.transform.forward;
            if(lookDirection != oldLookDirection)
            {
                oldLookDirection = lookDirection;
                LookDirectionServerRpc(lookDirection);
            }
        }

        float spinSpeed;
        if(velocity.magnitude > moveSpeed)
        {
            spinSpeed = Mathf.Lerp(movingSpinSpeed, dashingSpinSpeed, Mathf.InverseLerp(moveSpeed, dashSpeed, velocity.magnitude));
        }
        else
        {
            spinSpeed = Mathf.Lerp(idleSpinSpeed, movingSpinSpeed, Mathf.InverseLerp(0, moveSpeed, velocity.magnitude));
        }
        model.Rotate(0, 360 * spinSpeed * Time.deltaTime, 0);
    }

    void FixedUpdate()
    {
        if(IsServer)
        {
            FixedUpdateServer();
        }
    }

    private void FixedUpdateServer()
    {
        if(dashing)
        {
            velocity += dashDirection.normalized * dashAccel * Time.fixedDeltaTime;

            float dashBonus = Mathf.InverseLerp(1, -1, Vector3.Dot(Vector3.up, dashDirection));     // 1 for all the way down, 0 for all the way up
            float minDashSpeed = dashSpeed * Mathf.Lerp(0.2f, 0.8f, dashBonus);
            float currentDashSpeed = Mathf.Lerp(minDashSpeed, dashSpeed, dashAmount/maxDashAmount);

            if(velocity.magnitude > currentDashSpeed)
            {
                if(velocity.magnitude > currentDashSpeed + 2*dashAccel*Time.fixedDeltaTime)
                {
                    velocity = velocity.normalized * (velocity.magnitude - 2*dashAccel*Time.fixedDeltaTime);
                }
                else
                {
                    velocity = velocity.normalized * currentDashSpeed;
                }
            }
        }
        else
        {
            // Horizontal movement
            if(moveInput == Vector2.zero)
            {
                if(horVelocity.magnitude < accel * Time.fixedDeltaTime)
                {
                    horVelocity = Vector3.zero;
                }
                else
                {
                    horVelocity = horVelocity.normalized * (horVelocity.magnitude - accel * Time.fixedDeltaTime);
                }
            }
            else
            {
                Vector3 rotatedInput = Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(lookDirection, Vector3.up)) * new Vector3(moveInput.x, 0, moveInput.y);

                // If the velocity in the direction of input is already too big, don't add the input vector in the direction of velocity
                if(Vector3.Project(velocity, rotatedInput).magnitude > moveSpeed)
                    rotatedInput -= Vector3.Project(rotatedInput, velocity);
                    
                velocity += rotatedInput * accel * Time.fixedDeltaTime;

                if(horVelocity.magnitude > moveSpeed)
                {
                    if(horVelocity.magnitude > moveSpeed + accel*Time.fixedDeltaTime)
                    {
                        horVelocity = horVelocity.normalized * (horVelocity.magnitude - accel*Time.fixedDeltaTime);
                    }
                    else
                    {
                        horVelocity = horVelocity.normalized * moveSpeed;
                    }
                }
            }

            // Vertical movement
            if(verVelocity > -fallSpeed)
            {
                verVelocity -= gravityAccel * Time.deltaTime;
            }
            else
            {
                if(verVelocity < -fallSpeed - 2*gravityAccel*Time.fixedDeltaTime)
                {
                    verVelocity += 2*gravityAccel*Time.fixedDeltaTime;
                }
                else
                {
                    verVelocity = -fallSpeed;
                }
            }
        }

        rb.velocity = velocity;
    }

    void OnMove(InputValue inputValue)
    {
        if(IsOwner)
        {
            Vector2 input = inputValue.Get<Vector2>();
            if(input != oldMoveInput)
            {
                MoveInputServerRpc(input);
                oldMoveInput = input;
            }
        }
    }

    void OnFire(InputValue inputValue)
    {
        if(IsOwner)
        {
            bool input = inputValue.Get<float>() > 0;
            if(input != oldFirePressed)
            {
                FireInputServerRpc(input);
                oldFirePressed = input;

                if(!input)
                {
                    LookDirectionServerRpc(lookDirection);
                }
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(IsServer && other.gameObject.layer == LayerMask.NameToLayer("Updraft") && verVelocity < updraftMaxSpeed)
        {
            verVelocity += updraftAccel * Time.fixedDeltaTime;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(dashing)
        {
            dashDirection = Vector3.Reflect(dashDirection, collision.GetContact(0).normal);
        }

        if(dashing || horVelocity.magnitude > moveSpeed)
        {
            horVelocity = Vector3.Reflect(horVelocity.normalized, collision.GetContact(0).normal) * horVelocity.magnitude * 0.6f;
        }
    }

    [ServerRpc]
    private void MoveInputServerRpc(Vector2 input)
    {
        moveInput = input;
    }

    [ServerRpc]
    private void FireInputServerRpc(bool input)
    {
        firePressed = input;
    }

    [ServerRpc]
    private void LookDirectionServerRpc(Vector3 dir)
    {
        lookDirection = dir;
    }
}
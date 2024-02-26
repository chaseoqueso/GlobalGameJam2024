using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;
using System.Collections.Specialized;

public class Player : NetworkBehaviour
{
    [HideInInspector] public Rigidbody rb;

    #region Stats
    public static readonly float maxHealth = 100f;
    private NetworkVariable<float> currHealth = new();

    private bool isDead = false;
    private float respawnTimer = 5f;
    private float currRespawnTimer;

    public int score { get => kills.Value - deaths.Value; }
    public NetworkVariable<int> deaths = new(0);
    public NetworkVariable<int> kills = new(0);
    private float recentHit = 0f;
    private Player nemesis;  // Last player that hit you
    private float recentHitTimer = 1f;
    private float currRecentHitTimer;

    private float speed;
    private float weight;
    private float chargeUp;
    private float handling;

    private float BASE_DAMAGE = 10f;  // Tuning lever for damage


    public void takeDamage(float impactSpeed, Vector3 direction, bool isDashHit, bool isWall)
    {
        // Weight reduces damage up to 50%.
        float weightMod = 0.5f * (weight / 10f);
        float damage = 0f;

        // If this wall hit was *likely* caused by an enemy player dash
        if (isWall && recentHit > 0f)
        {
            damage = impactSpeed * weightMod * recentHit;  //TODO: Tune Me!
            recentHit = 0f;
        }
        
        else
        {
            damage = impactSpeed * weightMod * BASE_DAMAGE;  //TODO: Tune Me!
        }

        if (damage < 1f)
        {
            return;
        }
        currHealth.Value -= damage;

        Debug.Log(string.Format("Took {0} damage. Health left: {1}", damage, currHealth.Value));
        knockback(impactSpeed, direction);


        // If this is a dash hit, set recentHit so we can use it in case the player collides with a wall
        if (isDashHit)
        {
            recentHit = damage;
            currRecentHitTimer = recentHitTimer;
        }

        if (currHealth.Value <= 0)
        {
            die();
        }
    }

    public void knockback(float impactMagnitude, Vector3 pushBackDir)
    {
        // Tune Me!
        float weightMod = 0.5f * (weight / 10f);
        float knockbackModifier = 1f;

        Vector3 knockBackVector = impactMagnitude * knockbackModifier * weightMod * pushBackDir;
        horVelocity += new Vector2(knockBackVector.x, knockBackVector.z);
        verVelocity += knockBackVector.y;
    }

    public void die()
    {
        Debug.Log("Dead... Cue the Wilhelm Scream.");
        currRespawnTimer = respawnTimer;
        isDead = true;
        loseScore();
        gameObject.GetComponent<SphereCollider>().enabled = false;
    }

    public void respawn()
    {
        Debug.Log("Back from the grave.");
        currHealth.Value = maxHealth;
        isDead = false;
        gameObject.GetComponent<SphereCollider>().enabled = true;

        //TODO: respawn at the correct place
        gameObject.transform.position = getRandomSpawn();
    }

    private Vector3 getRandomSpawn()
    {
        float r = UnityEngine.Random.Range(0f, 20f);
        float angle = UnityEngine.Random.Range(0f, 360f);

        Vector3 spawnPoint = Quaternion.Euler(0, angle, 0) * new Vector3(r, 40, 0);
        return spawnPoint;
    }

    public void loseScore()
    {
        deaths.Value++;
        if(nemesis != null)
            nemesis.kills.Value++;
        Debug.Log($"Score transfer complete. Current Score: {score}");
    }
    #endregion

    #region CharacterParts
    public GameObject head;
    public GameObject body;
    public GameObject legs;

    private void InstantiateParts()
    {
        PlayerModels models = GameManager.Instance.GetPlayerModels(OwnerClientId);

        Transform head_slot = model.transform.GetChild(0);
        head = GameManager.Instance.headDatabase[models.head];
        GameObject headObject = Instantiate(head, head.transform.position + 10 * Vector3.up, head.transform.rotation);
        headObject.transform.parent = head_slot;
        Debug.Log(headObject.transform.position);

        Transform body_slot = model.transform.GetChild(1);
        body = GameManager.Instance.torsoDatabase[models.body];
        GameObject bodyObject = Instantiate(body, body.transform.position + 10 * Vector3.up, body.transform.rotation);
        bodyObject.transform.parent = body_slot;
        Debug.Log(bodyObject.transform.position);

        Transform legs_slot = model.transform.GetChild(2);
        legs = GameManager.Instance.legsDatabase[models.legs];
        GameObject legsObject = Instantiate(legs, legs.transform.position + 10 * Vector3.up, legs.transform.rotation);
        legsObject.transform.parent = legs_slot;
        Debug.Log(legsObject.transform.position);
    }

    private void GenerateStats()
    {
        Part head_stats = head.GetComponent<Part>();
        Part body_stats = body.GetComponent<Part>();
        Part legs_stats = legs.GetComponent<Part>();

        float[] stats = Part.GetCombinedStats(head_stats, body_stats, legs_stats);

        speed = stats[0];
        Debug.Log(String.Format("Speed set to {0}", speed));
        weight = stats[1];
        Debug.Log(String.Format("Weight set to {0}", weight));
        chargeUp = stats[2];
        Debug.Log(String.Format("Charge Up set to {0}", chargeUp));
        handling = stats[3];
        Debug.Log(String.Format("Handling set to {0}", handling));

        // Speed increases or reduces base speed by up to 30%
        float speedMod = 0.7f + 0.6f * speed/10f;  // Tune Me!
        moveSpeed = speedMod * moveSpeed;

        // ChargeUP increases or reduces time to max charge by up to 30%
        float chargeMod = 0.7f + 0.6f * chargeUp/10f;  // Tune Me!
        dashChargeTime = chargeMod * dashChargeTime;
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

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        InstantiateParts();
        GenerateStats();

        if(IsServer)
        {
            currHealth.Value = maxHealth;
        }
        
        GameManager.Instance.AddPlayer(OwnerClientId, this);
        GameManager.Instance.UpdatePlayerScore(OwnerClientId, 0);
        kills.OnValueChanged += (int old, int current) => GameManager.Instance.UpdatePlayerScore(OwnerClientId, current);
        deaths.OnValueChanged += (int old, int current) => GameManager.Instance.UpdatePlayerScore(OwnerClientId, current);
        GetComponent<NetworkObject>().DestroyWithScene = true;

        if(IsLocalPlayer)
        {
            CinemachineFreeLook freeLook = FindObjectOfType<CinemachineFreeLook>();
            freeLook.m_LookAt = transform;
            freeLook.m_Follow = transform;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            currHealth.OnValueChanged += (float _, float current) => GameUI.Instance.SetHealthBar(current);
            DashAmount.OnValueChanged += (float _, float current) => GameUI.Instance.SetCharge(current);
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
        if (isDead)
        {
            currRespawnTimer -= Time.deltaTime;
            if (currRespawnTimer <= 0)
            {
                respawn();
            }
            return;  //No shenanigans if you're dead
        }

        if (recentHit != 0f)
        {
            currRecentHitTimer -= Time.deltaTime;
            if (currRecentHitTimer <= 0)
            {
                recentHit = 0f;
            }
        }

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
        if(IsServer)
        {
            if(dashing)
            {
                dashDirection = Vector3.Reflect(dashDirection, collision.GetContact(0).normal);
            }

            if(dashing || horVelocity.magnitude > moveSpeed)
            {
                horVelocity = Vector3.Reflect(horVelocity.normalized, collision.GetContact(0).normal) * horVelocity.magnitude * 0.6f;
            }

            // Hit a player
            Player collidingPlayer = collision.gameObject.GetComponent<Player>();
            if (collidingPlayer != null)
            {
                Debug.Log("Hit a player");
                Vector3 collidingPlayerVelocity = collidingPlayer.GetComponent<Rigidbody>().velocity;

                // Get the speed of the colliding player in the direction of *this* player
                float incomingForce = Vector3.Dot(collidingPlayerVelocity, collision.GetContact(0).normal);

                if (incomingForce > 0)
                {
                    Debug.Log("or rather, they hit us.");
                    nemesis = collidingPlayer;
                    takeDamage(incomingForce, collision.GetContact(0).normal, collidingPlayer.dashing, false);
                }
            }

            // Hit a wall
            else
            {
                Debug.Log("Hit a wall");
                if (recentHit > 0)
                {
                    Debug.Log("...hard");
                    takeDamage(collision.relativeVelocity.magnitude, collision.GetContact(0).normal, false, true);
                }
                else
                {
                    Debug.Log("Take damage anyway for testing");
                    takeDamage(collision.relativeVelocity.magnitude, collision.GetContact(0).normal, false, true);
                }
            }
        }
    }   

    #region Server
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
    #endregion
}
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    public Rigidbody rb;
    public float moveSpeed;
    public float accel;

    private Vector2 moveInput;
    private Vector2 oldMoveInput;

    private Vector3 velocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if(IsServer)
        {
            FixedUpdateServer();
        }
    }

    void FixedUpdateServer()
    {
        if(moveInput == Vector2.zero)
        {
            if(velocity.magnitude < accel * Time.deltaTime)
            {
                velocity = Vector3.zero;
            }
            else
            {
                velocity = velocity.normalized * (velocity.magnitude - accel * Time.deltaTime);
            }
        }
        else
        {
            velocity += new Vector3(moveInput.x, 0, moveInput.y) * accel * Time.deltaTime;
            if(velocity.magnitude > moveSpeed)
                velocity = velocity.normalized * moveSpeed;
        }

        rb.velocity = velocity;
    }

    void OnMove(InputValue inputValue)
    {
        Vector2 input = inputValue.Get<Vector2>();
        if(input != oldMoveInput)
        {
            UpdateInputServerRpc(input);
            oldMoveInput = input;
        }
    }

    [ServerRpc]
    private void UpdateInputServerRpc(Vector2 input)
    {
        moveInput = input;
    }
}
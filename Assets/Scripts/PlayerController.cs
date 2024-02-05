using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{   
    public Camera camera;
    public AudioListener audioListener;

    // create a list of colors
    public List<Color> colors = new List<Color>();

    // getting the reference to the prefab
    [SerializeField]
    private GameObject spawnedPrefab;
    // save the instantiated prefab
    private GameObject instantiatedPrefab;


    private CharacterController controller;
    private PlayerInput playerInput;
    
    private Vector3 playerVelocity;
    
    private bool groundedPlayer;

    private float playerSpeed = 11.0f;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;

    private InputAction moveAction;
    private InputAction jumpAction;

    private Transform cameraTransform;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        cameraTransform = camera.transform;

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

        if (!IsOwner) return;

        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0, input.y);

        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0.0f;

        controller.Move(move * Time.deltaTime * playerSpeed);

        // Changes the height position of the player..
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // Rotate model towards camera look
        Quaternion playerRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, playerRotation, 5f * Time.deltaTime);


        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // this method is called when the object is spawned
    // we will change the color of the objects
    public override void OnNetworkSpawn()
    {
        GetComponent<MeshRenderer>().material.color = colors[(int)OwnerClientId];

        // check if the player is the owner of the object
        if (!IsOwner) return;
        // if the player is the owner of the object
        // enable the camera and the audio listener
        audioListener.enabled = true;
        camera.enabled = true;
    }


    void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;

        Debug.Log("Collision Detected");
        Debug.Log(hitObject.tag);
    }
}

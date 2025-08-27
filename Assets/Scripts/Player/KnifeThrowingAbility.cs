using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeThrowingAbility : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float KnifeCount=1;
    public Transform knifeSpawnPoint; // Child object where knives spawn
    public GameObject knifePrefab;    // Prefab for the knife
    public float knifeCooldown = 0.5f; // Time between knife throws
    private float nextKnifeTime = 0f; // Time tracking for cooldown
    private Vector2 currentVelocity; // Stores player's movement velocity
    private float angle = 0;
    // Update Loop
    private void Update()
    {
        HandleMovement();
        HandleKnifeThrowing();
    }

    // Handle Player Movement
    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 moveDirection = new Vector2(moveX, moveY).normalized;

        currentVelocity = moveDirection * moveSpeed; // Update the player's velocity

        if (moveDirection != Vector2.zero)
        {
            transform.Translate(currentVelocity * Time.deltaTime);
        }
    }

    // Handle Knife Throwing
    private void HandleKnifeThrowing()
    {
        if (Time.time >= nextKnifeTime)
        {
            ThrowKnife();
            nextKnifeTime = Time.time + knifeCooldown;
        }
    }

    // Throw a Knife
    private void ThrowKnife()
    {
        for (int i = 0; i < KnifeCount; i++)
        {
            Vector2 aimDirection = GetAimDirection();

            // Spawn the knife
            GameObject knife = Instantiate(knifePrefab, knifeSpawnPoint.position, Quaternion.identity);

            // Calculate the final velocity of the knife (player's velocity + aim direction)
            Vector2 knifeVelocity = aimDirection.normalized * knifePrefab.GetComponent<Knife>().speed + currentVelocity;

            // Initialize the knife with the calculated velocity
            knife.GetComponent<Knife>().Initialize(knifeVelocity, this.angle);
        }
    }

    // Get the Direction to Throw the Knife
    private Vector2 GetAimDirection()
    {
        // Example for aiming based on WASD or arrow keys
        Vector2 direction = Vector2.right; // Default to right
        angle = 0;
        if (Input.GetKey(KeyCode.W)) { angle = 90; direction = Vector2.up; }
        else if (Input.GetKey(KeyCode.S)) { direction = Vector2.down; angle = -90; }
        else if (Input.GetKey(KeyCode.A)) { direction = Vector2.left; angle = 180; }
        else if (Input.GetKey(KeyCode.D)) { direction = Vector2.right; angle = 0; }
        return direction;
    }
}
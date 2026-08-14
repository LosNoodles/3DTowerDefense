using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int playerHealth = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoseHealth(int amount)
    {
        playerHealth -= amount;
        Debug.Log($"Player lost health! Remaining Health: {playerHealth}");
        if (playerHealth <= 0)
        {
            Debug.Log("Game Over!");
        }
    }
}
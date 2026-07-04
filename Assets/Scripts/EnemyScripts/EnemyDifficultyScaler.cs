using UnityEngine;

public class EnemyDifficultyScaler : MonoBehaviour
{
    [Header("Difficulty Settings")]
    [SerializeField] private float secondsPerDifficultyLevel = 120f;
    [SerializeField] private int maxDifficultyLevel = 3;

    [Header("Movement Scaling")]
    [SerializeField] private float moveSpeedIncreasePerLevel = 0.5f;

    [Header("Accuracy Scaling")]
    [SerializeField] private float accuracyBonusPerLevel = 0.05f;

    [Header("Flying Enemy Homing Scaling")]
    [SerializeField] private float homingTurnSpeedIncreasePerLevel = 1f;

    public int CurrentDifficultyLevel
    {
        get
        {
            int level = Mathf.FloorToInt(Time.timeSinceLevelLoad / secondsPerDifficultyLevel);
            return Mathf.Clamp(level, 0, maxDifficultyLevel);
        }
    }

    public void ApplyScaling(GameObject enemy)
    {
        if (enemy == null) return;

        int difficultyLevel = CurrentDifficultyLevel;

        float moveSpeedBonus = moveSpeedIncreasePerLevel * difficultyLevel;
        float accuracyBonus = accuracyBonusPerLevel * difficultyLevel;
        float homingTurnSpeedMultiplier = homingTurnSpeedIncreasePerLevel * difficultyLevel;

        GroundEnemyAI groundEnemy = enemy.GetComponent<GroundEnemyAI>();

        if (groundEnemy != null)
        {
            groundEnemy.ApplyDifficultyScaling(
                moveSpeedBonus,
                accuracyBonus
            );
        }

        FlyingEnemyAI flyingEnemy = enemy.GetComponent<FlyingEnemyAI>();

        if (flyingEnemy != null)
        {
            flyingEnemy.ApplyDifficultyScaling(
                moveSpeedBonus,
                accuracyBonus,
                homingTurnSpeedMultiplier
            );
        }
    }
}
using Fusion;
using FusionExamples.Utility;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public GameUI hudPrefab;
    public Powerup[] powerups;
    public Powerup noPowerup;

    public static ResourceManager Instance => Singleton<ResourceManager>.Instance;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}

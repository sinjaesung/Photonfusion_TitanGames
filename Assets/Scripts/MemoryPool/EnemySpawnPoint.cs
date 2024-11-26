using System.Collections;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField]
    private float fadeSpeed = 4;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        StartCoroutine("OnFadeEffect");
    }

    private void OnDisable()
    {
        StopCoroutine("OnFadeEffect");
    }

    private IEnumerator OnFadeEffect()
    {
        while (true)
        {
            Color color = meshRenderer.material.color;
            float pingPongvalue = Mathf.PingPong(Time.time * fadeSpeed, 1);
            float alphaValue = Mathf.Lerp(1, 0, pingPongvalue);
            color.a = alphaValue;
            meshRenderer.material.color = color;

            yield return null;
        }
    }
}
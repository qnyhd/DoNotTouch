using UnityEngine;
using System.Collections;

public class CharacterStunVFXController : MonoBehaviour
{
    public GameObject stunVFXRoot;

    [Header("Timing")]
    [Tooltip("显示后再等多久才出画面。0=立刻，正值=更晚。")]
    public float showDelay = 0f;
    [Tooltip("隐藏前再等多久。0=立刻消失。")]
    public float hideDelay = 0f;
    [Tooltip("粒子播放速度。大于1更快。")]
    public float playbackSpeed = 1f;
    [Tooltip("跳过特效开头空白。越大越早看到完整效果。")]
    public float startOffset = 0f;

    private Coroutine showCoroutine;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        SetStunActive(false);
    }

    public void ShowStun()
    {
        StopRunningCoroutines();

        if (showDelay > 0f)
            showCoroutine = StartCoroutine(ShowAfterDelay());
        else
            SetStunActive(true);
    }

    public void HideStun()
    {
        StopRunningCoroutines();

        if (hideDelay > 0f)
            hideCoroutine = StartCoroutine(HideAfterDelay());
        else
            SetStunActive(false);
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(showDelay);
        SetStunActive(true);
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        SetStunActive(false);
    }

    private void SetStunActive(bool active)
    {
        if (stunVFXRoot == null)
            return;

        stunVFXRoot.SetActive(active);

        if (active)
            ConfigureParticles(stunVFXRoot, playbackSpeed, startOffset);
    }

    private void ConfigureParticles(GameObject root, float speed, float offset)
    {
        ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem particle in particles)
        {
            ParticleSystem.MainModule main = particle.main;
            main.simulationSpeed = speed;

            if (offset > 0f)
            {
                particle.Simulate(offset, false, true, true);
                particle.Play();
            }
        }
    }

    private void StopRunningCoroutines()
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }
}
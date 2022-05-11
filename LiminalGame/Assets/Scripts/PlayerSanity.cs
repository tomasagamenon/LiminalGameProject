using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerSanity : MonoBehaviour
{
    [SerializeField]
    Volume volume;
    [SerializeField]
    int sanity;
    int sanitySave;

    [Header("SanityUpAndDown")]
    [SerializeField]
    private float darkness;
    private float darknessCurrent;
    [SerializeField]
    private bool darknessBool;
    [SerializeField]
    private float seeEnemy;
    private float seeEnemyCurrent;
    [SerializeField]
    private bool seeEnemyBool;
    [SerializeField]
    private float rangeEnemy;
    [SerializeField]
    private float angleEnemy;
    [SerializeField]
    private float pursued;
    private float pursuedCurrent;
    [SerializeField]
    public bool pursuedBool;
    [SerializeField]
    private float quiet;
    private float quietCurrent;

    [Header("PostProcessing")]
    [SerializeField]
    private float multiplierSanityPerPoint;
    private float multiplierSanity;

    private ChromaticAberration chromatic;
    [SerializeField]
    private int SanityStartCAInt;
    [SerializeField]
    private float MaxCAIntensity;
    [SerializeField]
    private float MinCAIntensity;
    private float ValueCAIntensity;

    private LensDistortion lens;
    [SerializeField]
    private int SanityStartLDInt;
    [SerializeField]
    private float MaxLDIntensity;
    [SerializeField]
    private float MinLDIntensity;
    private float ValueLDIntensity;

    private Vignette vignette;
    [SerializeField]
    private int SanityStartVInt;
    [SerializeField]
    private float MaxVIntensity;
    [SerializeField]
    private float MinVIntensity;
    private float ValueVIntensity;
    [SerializeField]
    private int SanityStartVSmooth;
    [SerializeField]
    private float MaxVSmoothness;
    [SerializeField]
    private float MinVSmoothness;
    private float ValueVSmoothness;

    void Awake()
    {
        sanity = 100;
        multiplierSanity = 1;
        volume.profile.TryGet<Vignette>(out vignette);
        volume.profile.TryGet<LensDistortion>(out lens);
        volume.profile.TryGet<ChromaticAberration>(out chromatic);
        vignette.intensity.value = MinVIntensity;
        vignette.smoothness.value = MinVSmoothness;
        lens.intensity.value = MinLDIntensity;
        chromatic.intensity.value = MinCAIntensity;
        seeEnemyCurrent = seeEnemy;
        pursuedCurrent = pursued;
        darknessCurrent = darkness;
        quietCurrent = quiet;
    }

    void Update()
    {
        if (sanity < sanitySave)
        {
            sanitySave = sanity;
            if (sanity <= SanityStartVInt)
                vignette.intensity.value = Calculate(MaxVIntensity, MinVIntensity, vignette.intensity.value, SanityStartVInt, 1);
            if (sanity <= SanityStartVSmooth)
                vignette.smoothness.value = Calculate(MaxVSmoothness, MinVSmoothness, vignette.smoothness.value, SanityStartVSmooth, 1);
            if (sanity <= SanityStartLDInt)
                lens.intensity.value = Calculate(MaxLDIntensity, MinLDIntensity, lens.intensity.value, SanityStartLDInt, 1);
            if (sanity <= SanityStartCAInt)
                chromatic.intensity.value = Calculate(MaxCAIntensity, MinCAIntensity, chromatic.intensity.value, SanityStartCAInt, 1);
            multiplierSanity += multiplierSanityPerPoint;
        }
        else if (sanity > sanitySave)
        {
            sanitySave = sanity;
            if (sanity <= SanityStartVInt)
                vignette.intensity.value = Calculate(MaxVIntensity, MinVIntensity, vignette.intensity.value, SanityStartVInt, -1);
            if (sanity <= SanityStartVSmooth)
                vignette.smoothness.value = Calculate(MaxVSmoothness, MinVSmoothness, vignette.smoothness.value, SanityStartVSmooth, -1);
            if (sanity <= SanityStartLDInt)
                lens.intensity.value = Calculate(MaxLDIntensity, MinLDIntensity, lens.intensity.value, SanityStartLDInt, -1);
            if (sanity <= SanityStartCAInt)
                chromatic.intensity.value = Calculate(MaxCAIntensity, MinCAIntensity, chromatic.intensity.value, SanityStartCAInt, -1);
            multiplierSanity -= multiplierSanityPerPoint;
        }
        if (GetComponent<StarterAssets.FirstPersonController>().IsInSight(FindObjectOfType<AIFollow>().transform, 
            rangeEnemy, angleEnemy) && sanity > 0)
            seeEnemyBool = true;
        else seeEnemyBool = false;
        if (seeEnemyBool && sanity > 0)
            SeeEnemySanity();
        if (pursuedBool && sanity > 0)
            PursuedSanity();
        if (darknessBool && sanity > 0)
            DarknessSanity();
        if (!seeEnemyBool && !darknessBool && !pursuedBool && sanity < 100)
            QuietSanity();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
            darknessBool = true;
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.gameObject.layer == 9)
            darknessBool = false;
    }

    public void SeeEnemySanity()
    {
        seeEnemyCurrent -= Time.deltaTime;
        if (seeEnemyCurrent <= 0)
        {
            seeEnemyCurrent = seeEnemy;
            sanity -= 1;
        }
    }

    public void PursuedSanity()
    {
        pursuedCurrent -= Time.deltaTime;
        if (pursuedCurrent <= 0)
        {
            pursuedCurrent = pursued;
            sanity -= 1;
        }
    }

    public void DarknessSanity()
    {
        darknessCurrent -= Time.deltaTime;
        if (darknessCurrent <= 0)
        {
            darknessCurrent = darkness;
            sanity -= 1;
        }
    }

    public void QuietSanity()
    {
        quietCurrent -= Time.deltaTime;
        if (quietCurrent <= 0)
        {
            quietCurrent = quiet;
            sanity += 1;
        }
    }

    float Calculate(float maxValue, float minValue, float value, int sanityStart, int mult)
    {
        if (value >= maxValue && mult > 0)
            return value;
        else if (value <= minValue && mult < 0)
            return value;
        else
        {
            var multiplier = 1 + (multiplierSanityPerPoint * sanity);
            var one = ((maxValue / sanityStart) / multiplier) * mult;
            return (one + value);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * rangeEnemy);
        Gizmos.DrawWireSphere(transform.position, rangeEnemy);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, angleEnemy / 2, 0) * transform.forward * rangeEnemy);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, -angleEnemy / 2, 0) * transform.forward * rangeEnemy);
    }
}

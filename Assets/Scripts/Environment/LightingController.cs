using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class LightingController : MonoBehaviour
{
    public Light environmentLight;
    private LiftGammaGain liftGammaGain;
    private Volume renderingVolume;

    void Awake()
    {
        renderingVolume = GetComponent<Volume>();
        if (!renderingVolume.profile.TryGet(out liftGammaGain))
            throw new System.NullReferenceException(nameof(liftGammaGain));
    }

    void Update()
    {
        environmentLight.intensity = GameInfo.options.GetFloat(OptionName.EnvironmentLightIntensity);
        liftGammaGain.gamma.Override(new Vector4(
            1f, 1f, 1f, GameInfo.options.GetFloat(OptionName.Gamma)
        ));
        liftGammaGain.gain.Override(new Vector4(
            1f, 1f, 1f, GameInfo.options.GetFloat(OptionName.Gain)
        ));
    }
}
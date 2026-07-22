using UnityEngine;

internal sealed class RtsAudioFeedbackSystem
{
    private readonly GameObject audioObject;
    private readonly AudioSource source;
    private readonly PresentationPrefabCatalog catalog;

    public bool HasAudioClips => catalog != null &&
        catalog.AttackClip != null &&
        catalog.HitClip != null &&
        catalog.ProductionCompleteClip != null;

    public RtsAudioFeedbackSystem(Transform parent)
    {
        catalog = Resources.Load<PresentationPrefabCatalog>("PresentationPrefabCatalog");
        audioObject = new GameObject("AudioFeedback");
        audioObject.transform.SetParent(parent, false);
        source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = 0.65f;
    }

    public void PlayCombat(CombatFeedbackEvent feedback)
    {
        if (catalog == null)
        {
            return;
        }

        Play(catalog.AttackClip, 0.42f);
        Play(catalog.HitClip, feedback.IsLethal ? 0.65f : 0.48f);
    }

    public void PlayProductionComplete()
    {
        if (catalog != null)
        {
            Play(catalog.ProductionCompleteClip, 0.72f);
        }
    }

    public void Destroy()
    {
        if (audioObject == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(audioObject);
        }
        else
        {
            Object.DestroyImmediate(audioObject);
        }
    }

    private void Play(AudioClip clip, float volumeScale)
    {
        if (clip != null && source != null)
        {
            source.PlayOneShot(clip, volumeScale);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

internal sealed class RtsWorldFeedbackSystem
{
    private const float ProjectileDuration = 0.14f;
    private const float HitFlashDuration = 0.12f;
    private const float DeathEffectDuration = 0.42f;

    private sealed class ProjectileView
    {
        public GameObject GameObject;
        public Vector2 Start;
        public Vector2 End;
        public float Age;
    }

    private sealed class FlashView
    {
        public SpriteRenderer Renderer;
        public Color OriginalColor;
        public float Remaining;
    }

    private sealed class DeathView
    {
        public GameObject GameObject;
        public SpriteRenderer Renderer;
        public Vector3 InitialScale;
        public Color InitialColor;
        public float Age;
    }

    private readonly EntityPresentationFactory presentation;
    private readonly Transform root;
    private readonly List<ProjectileView> projectiles = new List<ProjectileView>();
    private readonly List<FlashView> flashes = new List<FlashView>();
    private readonly List<DeathView> deaths = new List<DeathView>();

    public int ActiveEffectCount => projectiles.Count + flashes.Count + deaths.Count;

    public RtsWorldFeedbackSystem(EntityPresentationFactory presentationFactory, Transform parent)
    {
        presentation = presentationFactory;
        root = parent;
    }

    public void PlayCombatFeedback(CombatFeedbackEvent feedback)
    {
        Color teamColor = feedback.SourceTeam == Team.Player
            ? new Color(0.25f, 0.85f, 1f, 1f)
            : new Color(1f, 0.35f, 0.2f, 1f);

        GameObject projectile = presentation.CreateCircle(
            "AttackProjectile",
            feedback.SourcePosition,
            0.08f,
            teamColor,
            80,
            root
        );
        projectiles.Add(new ProjectileView
        {
            GameObject = projectile,
            Start = feedback.SourcePosition,
            End = feedback.TargetPosition
        });

        PlayHitFlash(feedback.TargetObject);

        if (feedback.IsLethal)
        {
            PlayDeathEffect(feedback.TargetPosition, teamColor);
        }
    }

    public void Tick(float deltaTime)
    {
        TickProjectiles(deltaTime);
        TickFlashes(deltaTime);
        TickDeaths(deltaTime);
    }

    public void Clear()
    {
        foreach (ProjectileView projectile in projectiles)
        {
            Destroy(projectile.GameObject);
        }

        foreach (FlashView flash in flashes)
        {
            if (flash.Renderer != null)
            {
                flash.Renderer.color = flash.OriginalColor;
            }
        }

        foreach (DeathView death in deaths)
        {
            Destroy(death.GameObject);
        }

        projectiles.Clear();
        flashes.Clear();
        deaths.Clear();
    }

    private void PlayHitFlash(GameObject target)
    {
        SpriteRenderer renderer = target != null ? target.GetComponent<SpriteRenderer>() : null;

        if (renderer == null)
        {
            return;
        }

        foreach (FlashView flash in flashes)
        {
            if (flash.Renderer == renderer)
            {
                flash.Remaining = HitFlashDuration;
                renderer.color = Color.white;
                return;
            }
        }

        flashes.Add(new FlashView
        {
            Renderer = renderer,
            OriginalColor = renderer.color,
            Remaining = HitFlashDuration
        });
        renderer.color = Color.white;
    }

    private void PlayDeathEffect(Vector2 position, Color color)
    {
        color.a = 0.7f;
        GameObject deathObject = presentation.CreateCircle(
            "DeathPulse",
            position,
            0.18f,
            color,
            75,
            root
        );
        SpriteRenderer renderer = deathObject.GetComponent<SpriteRenderer>();
        deaths.Add(new DeathView
        {
            GameObject = deathObject,
            Renderer = renderer,
            InitialScale = deathObject.transform.localScale,
            InitialColor = color
        });
    }

    private void TickProjectiles(float deltaTime)
    {
        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            ProjectileView projectile = projectiles[i];
            projectile.Age += deltaTime;
            float progress = Mathf.Clamp01(projectile.Age / ProjectileDuration);

            if (projectile.GameObject != null)
            {
                projectile.GameObject.transform.position = Vector2.Lerp(
                    projectile.Start,
                    projectile.End,
                    progress
                );
            }

            if (progress < 1f)
            {
                continue;
            }

            Destroy(projectile.GameObject);
            projectiles.RemoveAt(i);
        }
    }

    private void TickFlashes(float deltaTime)
    {
        for (int i = flashes.Count - 1; i >= 0; i--)
        {
            FlashView flash = flashes[i];
            flash.Remaining -= deltaTime;

            if (flash.Remaining > 0f)
            {
                continue;
            }

            if (flash.Renderer != null)
            {
                flash.Renderer.color = flash.OriginalColor;
            }

            flashes.RemoveAt(i);
        }
    }

    private void TickDeaths(float deltaTime)
    {
        for (int i = deaths.Count - 1; i >= 0; i--)
        {
            DeathView death = deaths[i];
            death.Age += deltaTime;
            float progress = Mathf.Clamp01(death.Age / DeathEffectDuration);

            if (death.GameObject != null)
            {
                death.GameObject.transform.localScale = death.InitialScale * Mathf.Lerp(1f, 4f, progress);
            }

            if (death.Renderer != null)
            {
                Color color = death.InitialColor;
                color.a *= 1f - progress;
                death.Renderer.color = color;
            }

            if (progress < 1f)
            {
                continue;
            }

            Destroy(death.GameObject);
            deaths.RemoveAt(i);
        }
    }

    private static void Destroy(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(gameObject);
        }
        else
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}

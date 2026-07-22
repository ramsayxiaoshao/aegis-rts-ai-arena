using UnityEngine;

internal readonly struct CombatFeedbackEvent
{
    public Vector2 SourcePosition { get; }
    public Vector2 TargetPosition { get; }
    public GameObject TargetObject { get; }
    public Team SourceTeam { get; }
    public int Damage { get; }
    public bool IsLethal { get; }

    public CombatFeedbackEvent(
        Vector2 sourcePosition,
        Vector2 targetPosition,
        GameObject targetObject,
        Team sourceTeam,
        int damage,
        bool isLethal
    )
    {
        SourcePosition = sourcePosition;
        TargetPosition = targetPosition;
        TargetObject = targetObject;
        SourceTeam = sourceTeam;
        Damage = damage;
        IsLethal = isLethal;
    }
}

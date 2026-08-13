namespace NeiroHelp.Combat
{
    public enum AttackContext
    {
        Ground = 0,
        Air = 1,
        Both = 2
    }

    public enum CombatButton
    {
        None = 0,
        Light = 1,
        Heavy = 2,
        Special = 3
    }

    public enum ChargeKind
    {
        Tap = 0,
        Hold = 1
    }

    public enum AimDirection
    {
        Neutral = 0,
        TowardTarget = 1,
        AwayFromTarget = 2,
        Up = 3,
        Down = 4
    }

    public enum HitCastShape
    {
        Box = 0,
        Sphere = 1,
        Capsule = 2
    }

    public enum HitProbeMode
    {
        /// <summary>Cast from previous socket position to current each active frame (preferred for fists).</summary>
        SweepSocket = 0,
        /// <summary>One-shot overlap at an offset — convenient for AoE slam / shockwave.</summary>
        OverlapAtOffset = 1
    }

    public enum CombatEffectType
    {
        None = 0,
        Knockback = 1,
        LaunchUp = 2,
        SlamDown = 3,
        SpawnProjectile = 4,
        DashToTarget = 5,
        PullTargetToSelf = 6,
        ApplyStun = 7
    }
}

using UnityEngine;

namespace NeiroHelp.Combat
{
    /// <summary>
    /// Builds a full example combo graph in memory (no asset files required).
    /// Includes starter/chain edges for light, heavy, hold-launch, air, slam, special variants.
    /// </summary>
    public static class CombatExampleFactory
    {
        public const string LightPunch1 = "LightPunch1";
        public const string LightPunch2 = "LightPunch2";
        public const string LightPunch3 = "LightPunch3";
        public const string HeavyKick1 = "HeavyKick1";
        public const string UppercutLaunch = "UppercutLaunch";
        public const string AirPunch1 = "AirPunch1";
        public const string GroundSlam = "GroundSlam";
        public const string ProjectileShot = "ProjectileShot";
        public const string DashToTarget = "DashToTarget";
        public const string PullEnemyIn = "PullEnemyIn";
        public const string StunPulse = "StunPulse";

        public static ComboGraphDefinition CreateExampleGraph(LayerMask hitMask)
        {
            ComboGraphDefinition graph = ScriptableObject.CreateInstance<ComboGraphDefinition>();

            AttackDefinition light1 = CreateLightPunch1(hitMask);
            AttackDefinition light2 = CreateChainedPunch(LightPunch2, 1.4f, 12f, 5f, hitMask);
            AttackDefinition light3 = CreateChainedPunch(LightPunch3, 1.8f, 16f, 7f, hitMask);
            AttackDefinition heavy1 = CreateHeavyKick(hitMask);
            AttackDefinition uppercut = CreateUppercut(hitMask);
            AttackDefinition airPunch = CreateAirPunch(hitMask);
            AttackDefinition slam = CreateGroundSlam(hitMask);
            AttackDefinition projectile = CreateProjectile();
            AttackDefinition dash = CreateDash();
            AttackDefinition pull = CreatePull();
            AttackDefinition stun = CreateStun();

            graph.Attacks.Add(light1);
            graph.Attacks.Add(light2);
            graph.Attacks.Add(light3);
            graph.Attacks.Add(heavy1);
            graph.Attacks.Add(uppercut);
            graph.Attacks.Add(airPunch);
            graph.Attacks.Add(slam);
            graph.Attacks.Add(projectile);
            graph.Attacks.Add(dash);
            graph.Attacks.Add(pull);
            graph.Attacks.Add(stun);

            // Starters (FromAttackId empty = no current attack)
            AddEdge(graph, null, LightPunch1, CombatButton.Light, ChargeKind.Tap, AimDirection.Neutral, AttackContext.Ground, 0);
            AddEdge(graph, null, HeavyKick1, CombatButton.Heavy, ChargeKind.Tap, AimDirection.Neutral, AttackContext.Ground, 0);
            AddEdge(graph, null, UppercutLaunch, CombatButton.Light, ChargeKind.Hold, AimDirection.Neutral, AttackContext.Ground, 10);
            AddEdge(graph, null, AirPunch1, CombatButton.Light, ChargeKind.Tap, AimDirection.Neutral, AttackContext.Air, 0);
            AddEdge(graph, null, GroundSlam, CombatButton.Heavy, ChargeKind.Hold, AimDirection.Down, AttackContext.Air, 10);
            AddEdge(graph, null, ProjectileShot, CombatButton.Special, ChargeKind.Tap, AimDirection.Neutral, AttackContext.Both, 0);
            AddEdge(graph, null, DashToTarget, CombatButton.Special, ChargeKind.Tap, AimDirection.TowardTarget, AttackContext.Both, 5);
            AddEdge(graph, null, PullEnemyIn, CombatButton.Special, ChargeKind.Tap, AimDirection.AwayFromTarget, AttackContext.Both, 5);
            AddEdge(graph, null, StunPulse, CombatButton.Special, ChargeKind.Hold, AimDirection.Neutral, AttackContext.Both, 8);

            // Light combo chain
            AddEdge(graph, LightPunch1, LightPunch2, CombatButton.Light, ChargeKind.Tap, AimDirection.Neutral, AttackContext.Ground, 0);
            AddEdge(graph, LightPunch2, LightPunch3, CombatButton.Light, ChargeKind.Tap, AimDirection.Neutral, AttackContext.Ground, 0);
            AddEdge(graph, LightPunch1, UppercutLaunch, CombatButton.Light, ChargeKind.Hold, AimDirection.Neutral, AttackContext.Ground, 5);
            AddEdge(graph, LightPunch1, HeavyKick1, CombatButton.Heavy, ChargeKind.Tap, AimDirection.Neutral, AttackContext.Ground, 0);

            // After launch into air string
            AddEdge(graph, UppercutLaunch, AirPunch1, CombatButton.Light, ChargeKind.Tap, AimDirection.Neutral, AttackContext.Air, 0);
            AddEdge(graph, AirPunch1, GroundSlam, CombatButton.Heavy, ChargeKind.Hold, AimDirection.Down, AttackContext.Air, 0);

            return graph;
        }

        public static AttackDefinition CreateLightPunch1(LayerMask hitMask)
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            attack.Id = LightPunch1;
            attack.Context = AttackContext.Ground;
            attack.DurationSeconds = 0.4f;
            attack.Phases = new AttackPhaseWindows
            {
                StartupEnd = 0.2f,
                ActiveEnd = 0.7f,
                ComboOpen = 0.35f,
                ComboClose = 0.95f
            };
            attack.Motion = new AttackMotionData
            {
                ForwardDisplacement = new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.35f, 0.85f),
                    new Keyframe(1f, 1f)),
                HeightDisplacement = AnimationCurve.Constant(0f, 1f, 0f),
                ForwardDistance = 1.1f,
                HeightDistance = 0f,
                OverrideGravity = true
            };
            attack.Hit = new AttackHitData
            {
                Enabled = true,
                Mode = HitProbeMode.SweepSocket,
                Shape = HitCastShape.Box,
                BoxHalfExtents = new Vector3(0.18f, 0.18f, 0.18f),
                HitMask = hitMask,
                Damage = 10f,
                KnockbackForce = 3.5f,
                KnockbackLocalDirection = new Vector3(0f, 0.2f, 1f),
                HitSameTargetOncePerAttack = true
            };
            return attack;
        }

        private static AttackDefinition CreateChainedPunch(
            string id,
            float forwardDistance,
            float damage,
            float knockback,
            LayerMask hitMask)
        {
            AttackDefinition attack = CreateLightPunch1(hitMask);
            attack.Id = id;
            attack.Motion.ForwardDistance = forwardDistance;
            attack.Hit.Damage = damage;
            attack.Hit.KnockbackForce = knockback;
            attack.DurationSeconds = 0.38f;
            return attack;
        }

        private static AttackDefinition CreateHeavyKick(LayerMask hitMask)
        {
            AttackDefinition attack = CreateLightPunch1(hitMask);
            attack.Id = HeavyKick1;
            attack.DurationSeconds = 0.55f;
            attack.Motion.ForwardDistance = 1.6f;
            attack.Hit.Damage = 18f;
            attack.Hit.KnockbackForce = 6f;
            attack.Hit.BoxHalfExtents = new Vector3(0.22f, 0.22f, 0.25f);
            return attack;
        }

        private static AttackDefinition CreateUppercut(LayerMask hitMask)
        {
            AttackDefinition attack = CreateLightPunch1(hitMask);
            attack.Id = UppercutLaunch;
            attack.DurationSeconds = 0.5f;
            attack.Motion.ForwardDistance = 0.4f;
            attack.Motion.HeightDistance = 1.8f;
            attack.Motion.HeightDisplacement = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.4f, 1f),
                new Keyframe(1f, 0.7f));
            attack.Hit.Damage = 14f;
            attack.Hit.KnockbackForce = 5f;
            attack.Hit.KnockbackLocalDirection = new Vector3(0f, 1f, 0.35f);
            attack.Effects.Add(new AttackEffectEntry
            {
                Type = CombatEffectType.LaunchUp,
                FireAtNormalizedTime = 0.25f,
                FloatParameter = 8f
            });
            return attack;
        }

        private static AttackDefinition CreateAirPunch(LayerMask hitMask)
        {
            AttackDefinition attack = CreateLightPunch1(hitMask);
            attack.Id = AirPunch1;
            attack.Context = AttackContext.Air;
            attack.Motion.ForwardDistance = 0.8f;
            attack.Motion.HeightDistance = 0.2f;
            return attack;
        }

        private static AttackDefinition CreateGroundSlam(LayerMask hitMask)
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            attack.Id = GroundSlam;
            attack.Context = AttackContext.Air;
            attack.DurationSeconds = 0.6f;
            attack.Phases = AttackPhaseWindows.DefaultPunch;
            attack.Motion = new AttackMotionData
            {
                ForwardDisplacement = AnimationCurve.Constant(0f, 1f, 0f),
                HeightDisplacement = new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.7f, -1f),
                    new Keyframe(1f, -1f)),
                ForwardDistance = 0f,
                HeightDistance = 3f,
                OverrideGravity = true
            };
            attack.Hit = new AttackHitData
            {
                Enabled = true,
                Mode = HitProbeMode.OverlapAtOffset,
                Shape = HitCastShape.Sphere,
                Radius = 1.5f,
                LocalOffset = new Vector3(0f, 0.2f, 0.5f),
                HitMask = hitMask,
                Damage = 22f,
                KnockbackForce = 8f,
                KnockbackLocalDirection = new Vector3(0f, 0.5f, 0.5f)
            };
            attack.Effects.Add(new AttackEffectEntry
            {
                Type = CombatEffectType.SlamDown,
                FireAtNormalizedTime = 0.2f
            });
            return attack;
        }

        private static AttackDefinition CreateProjectile()
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            attack.Id = ProjectileShot;
            attack.Context = AttackContext.Both;
            attack.DurationSeconds = 0.35f;
            attack.Phases = AttackPhaseWindows.DefaultPunch;
            attack.Motion = new AttackMotionData
            {
                ForwardDisplacement = AnimationCurve.Constant(0f, 1f, 0f),
                HeightDisplacement = AnimationCurve.Constant(0f, 1f, 0f)
            };
            attack.Hit.Enabled = false;
            attack.Effects.Add(new AttackEffectEntry
            {
                Type = CombatEffectType.SpawnProjectile,
                FireAtNormalizedTime = 0.3f
            });
            return attack;
        }

        private static AttackDefinition CreateDash()
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            attack.Id = DashToTarget;
            attack.Context = AttackContext.Both;
            attack.DurationSeconds = 0.35f;
            attack.Phases = AttackPhaseWindows.DefaultPunch;
            attack.Motion = new AttackMotionData
            {
                ForwardDisplacement = new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.5f, 1f),
                    new Keyframe(1f, 1f)),
                ForwardDistance = 4f,
                HeightDistance = 0f,
                OverrideGravity = true
            };
            attack.Hit.Enabled = false;
            attack.Effects.Add(new AttackEffectEntry
            {
                Type = CombatEffectType.DashToTarget,
                FireAtNormalizedTime = 0.05f
            });
            return attack;
        }

        private static AttackDefinition CreatePull()
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            attack.Id = PullEnemyIn;
            attack.Context = AttackContext.Both;
            attack.DurationSeconds = 0.4f;
            attack.Phases = AttackPhaseWindows.DefaultPunch;
            attack.Motion = new AttackMotionData();
            attack.Hit.Enabled = false;
            attack.Effects.Add(new AttackEffectEntry
            {
                Type = CombatEffectType.PullTargetToSelf,
                FireAtNormalizedTime = 0.25f
            });
            return attack;
        }

        private static AttackDefinition CreateStun()
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            attack.Id = StunPulse;
            attack.Context = AttackContext.Both;
            attack.DurationSeconds = 0.45f;
            attack.Phases = AttackPhaseWindows.DefaultPunch;
            attack.Motion = new AttackMotionData();
            attack.Hit.Enabled = false;
            attack.Effects.Add(new AttackEffectEntry
            {
                Type = CombatEffectType.ApplyStun,
                FireAtNormalizedTime = 0.3f,
                FloatParameter = 1.5f
            });
            return attack;
        }

        private static void AddEdge(
            ComboGraphDefinition graph,
            string from,
            string to,
            CombatButton button,
            ChargeKind charge,
            AimDirection aim,
            AttackContext context,
            int priority)
        {
            graph.Edges.Add(new ComboEdge
            {
                FromAttackId = from,
                ToAttackId = to,
                Chord = new InputChord(button, charge, aim),
                Context = context,
                Priority = priority,
                RequireHitConfirm = false
            });
        }
    }
}

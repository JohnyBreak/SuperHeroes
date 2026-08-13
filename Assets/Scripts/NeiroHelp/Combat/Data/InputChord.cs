using System;

namespace NeiroHelp.Combat
{
    [Serializable]
    public struct InputChord : IEquatable<InputChord>
    {
        public CombatButton Button;
        public ChargeKind Charge;
        public AimDirection Aim;

        public InputChord(CombatButton button, ChargeKind charge, AimDirection aim)
        {
            Button = button;
            Charge = charge;
            Aim = aim;
        }

        public bool Equals(InputChord other)
        {
            return Button == other.Button
                   && Charge == other.Charge
                   && Aim == other.Aim;
        }

        public override bool Equals(object obj)
        {
            return obj is InputChord other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)Button;
                hash = (hash * 397) ^ (int)Charge;
                hash = (hash * 397) ^ (int)Aim;
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{Button}/{Charge}/{Aim}";
        }
    }
}

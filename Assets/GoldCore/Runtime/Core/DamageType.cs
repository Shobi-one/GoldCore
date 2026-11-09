namespace GoldCore
{
    /// <summary>What caused the damage, like Source's DMG_* flags. Combine them with |.</summary>
    [System.Flags]
    public enum DamageType
    {
        Generic = 0,
        Bullet = 1 << 0,
        Slash = 1 << 1,
        Club = 1 << 2,
        Blast = 1 << 3,
        Fall = 1 << 4,
        Burn = 1 << 5,
        Shock = 1 << 6,
        Drown = 1 << 7,
        Crush = 1 << 8,
        Poison = 1 << 9,
        /// <summary>Ignores armor, like trigger_hurt set to kill.</summary>
        BypassArmor = 1 << 10,
    }
}

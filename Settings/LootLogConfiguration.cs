﻿using MapAssist.Files;
using MapAssist.Types;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace MapAssist.Settings
{
    public class LootLogConfiguration
    {
        public static Dictionary<string, List<ItemFilter>> Filters { get; set; }

        public static void Load()
        {
            Filters = ConfigurationParser<Dictionary<string, List<ItemFilter>>>.ParseConfigurationFile($"./{MapAssistConfiguration.Loaded.ItemLog.FilterFileName}");
        }
    }

    public class ItemFilter
    {
        public object this[Stat stat]
        {
            get { return GetType().GetProperty(stat.ToString()).GetValue(this, null); }
            set { GetType().GetProperty(stat.ToString()).SetValue(this, value, null); }
        }

        public ItemQuality[] Qualities { get; set; }
        public int[] Sockets { get; set; }
        public bool? Ethereal { get; set; }
        public int? Defense { get; set; }
        public int? Strength { get; set; }
        public int? Dexterity { get; set; }
        public int? Vitality { get; set; }
        public int? Energy { get; set; }

        [YamlMember(Alias = "All Attributes")]
        public int? AllAttributes { get; set; }

        [YamlMember(Alias = "Max Life")]
        public int? MaxLife { get; set; }

        [YamlMember(Alias = "Max Mana")]
        public int? MaxMana { get; set; }

        [YamlMember(Alias = "Attack Rating")]
        public int? AttackRating { get; set; }

        [YamlMember(Alias = "Min Damage")]
        public int? MinDamage { get; set; }

        [YamlMember(Alias = "Max Damage")]
        public int? MaxDamage { get; set; }

        [YamlMember(Alias = "Damage Reduced")]
        public int? DamageReduced { get; set; }

        [YamlMember(Alias = "Life Steal")]
        public int? LifeSteal { get; set; }

        [YamlMember(Alias = "Mana Steal")]
        public int? ManaSteal { get; set; }

        [YamlMember(Alias = "Cold Skill Damage")]
        public int? ColdSkillDamage { get; set; }

        [YamlMember(Alias = "Lightning Skill Damage")]
        public int? LightningSkillDamage { get; set; }

        [YamlMember(Alias = "Fire Skill Damage")]
        public int? FireSkillDamage { get; set; }

        [YamlMember(Alias = "Poison Skill Damage")]
        public int? PoisonSkillDamage { get; set; }

        [YamlMember(Alias = "Increased Attack Speed")]
        public int? IncreasedAttackSpeed { get; set; }

        [YamlMember(Alias = "Faster Run Walk")]
        public int? FasterRunWalk { get; set; }

        [YamlMember(Alias = "Faster Hit Recovery")]
        public int? FasterHitRecovery { get; set; }

        [YamlMember(Alias = "Faster Cast Rate")]
        public int? FasterCastRate { get; set; }

        [YamlMember(Alias = "Magic Find")]
        public int? MagicFind { get; set; }

        [YamlMember(Alias = "Gold Find")]
        public int? GoldFind { get; set; }

        [YamlMember(Alias = "Cold Resist")]
        public int? ColdResist { get; set; }

        [YamlMember(Alias = "Lightning Resist")]
        public int? LightningResist { get; set; }

        [YamlMember(Alias = "Fire Resist")]
        public int? FireResist { get; set; }

        [YamlMember(Alias = "Poison Resist")]
        public int? PoisonResist { get; set; }

        [YamlMember(Alias = "All Resist")]
        public int? AllResist { get; set; }

        [YamlMember(Alias = "All Skills")]
        public int? AllSkills { get; set; }

        [YamlMember(Alias = "Class Skills")]
        public Dictionary<Structs.PlayerClass, int?> ClassSkills { get; set; } = new Dictionary<Structs.PlayerClass, int?>();

        [YamlMember(Alias = "Class Skill Tree")]
        public Dictionary<ClassTabs, int?> ClassTabSkills { get; set; } = new Dictionary<ClassTabs, int?>();

        [YamlMember(Alias = "Skills")]
        public Dictionary<Skill, int?> Skills { get; set; } = new Dictionary<Skill, int?>();

        [YamlMember(Alias = "Skill Charges")]
        public Dictionary<Skill, int?> SkillCharges { get; set; } = new Dictionary<Skill, int?>();
    }
}

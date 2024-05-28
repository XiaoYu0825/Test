namespace Config
{
    public class ConfigManager
    {
        public ConfigManager()
        {
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<GlobalConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<CharacterConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<CombatFormulaConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<PlayerConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<BulletConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<InterruptStackActionConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<BuffConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<SkillConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<DungeonConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<DungeonMapConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<SkillCDConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<AttributeMapConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<ExperienceGrowthConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<EquipmentQualityConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<EquipmentRandomAttributeConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<PlayerForge>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<StrengthenConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<EquipmentBasePropertyConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<RandomAttributeGroupConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<DropRewardGroupConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<PropConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<SpellFieldConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<FXConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<AudioConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<SpriteConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<UIConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<ShopConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<I18NConfig>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<BuffHitData>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<SkillCastLimit>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<SkillHitData>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<RandomAttributeId>();
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<RandomAttributeData>();
        }
        public static void LoadAllConfigsByAddressable(string directory)
        {
            Config.GlobalConfig.DeserializeByAddressable(directory);
            Config.CharacterConfig.DeserializeByAddressable(directory);
            Config.CombatFormulaConfig.DeserializeByAddressable(directory);
            Config.PlayerConfig.DeserializeByAddressable(directory);
            Config.BulletConfig.DeserializeByAddressable(directory);
            Config.InterruptStackActionConfig.DeserializeByAddressable(directory);
            Config.BuffConfig.DeserializeByAddressable(directory);
            Config.SkillConfig.DeserializeByAddressable(directory);
            Config.DungeonConfig.DeserializeByAddressable(directory);
            Config.DungeonMapConfig.DeserializeByAddressable(directory);
            Config.SkillCDConfig.DeserializeByAddressable(directory);
            Config.AttributeMapConfig.DeserializeByAddressable(directory);
            Config.ExperienceGrowthConfig.DeserializeByAddressable(directory);
            Config.EquipmentQualityConfig.DeserializeByAddressable(directory);
            Config.EquipmentRandomAttributeConfig.DeserializeByAddressable(directory);
            Config.PlayerForge.DeserializeByAddressable(directory);
            Config.StrengthenConfig.DeserializeByAddressable(directory);
            Config.EquipmentBasePropertyConfig.DeserializeByAddressable(directory);
            Config.RandomAttributeGroupConfig.DeserializeByAddressable(directory);
            Config.DropRewardGroupConfig.DeserializeByAddressable(directory);
            Config.PropConfig.DeserializeByAddressable(directory);
            Config.SpellFieldConfig.DeserializeByAddressable(directory);
            Config.FXConfig.DeserializeByAddressable(directory);
            Config.AudioConfig.DeserializeByAddressable(directory);
            Config.SpriteConfig.DeserializeByAddressable(directory);
            Config.UIConfig.DeserializeByAddressable(directory);
            Config.ShopConfig.DeserializeByAddressable(directory);
            Config.I18NConfig.DeserializeByAddressable(directory);
        }
        public static void LoadAllConfigsByFile(string directory)
        {
            Config.GlobalConfig.DeserializeByFile(directory);
            Config.CharacterConfig.DeserializeByFile(directory);
            Config.CombatFormulaConfig.DeserializeByFile(directory);
            Config.PlayerConfig.DeserializeByFile(directory);
            Config.BulletConfig.DeserializeByFile(directory);
            Config.InterruptStackActionConfig.DeserializeByFile(directory);
            Config.BuffConfig.DeserializeByFile(directory);
            Config.SkillConfig.DeserializeByFile(directory);
            Config.DungeonConfig.DeserializeByFile(directory);
            Config.DungeonMapConfig.DeserializeByFile(directory);
            Config.SkillCDConfig.DeserializeByFile(directory);
            Config.AttributeMapConfig.DeserializeByFile(directory);
            Config.ExperienceGrowthConfig.DeserializeByFile(directory);
            Config.EquipmentQualityConfig.DeserializeByFile(directory);
            Config.EquipmentRandomAttributeConfig.DeserializeByFile(directory);
            Config.PlayerForge.DeserializeByFile(directory);
            Config.StrengthenConfig.DeserializeByFile(directory);
            Config.EquipmentBasePropertyConfig.DeserializeByFile(directory);
            Config.RandomAttributeGroupConfig.DeserializeByFile(directory);
            Config.DropRewardGroupConfig.DeserializeByFile(directory);
            Config.PropConfig.DeserializeByFile(directory);
            Config.SpellFieldConfig.DeserializeByFile(directory);
            Config.FXConfig.DeserializeByFile(directory);
            Config.AudioConfig.DeserializeByFile(directory);
            Config.SpriteConfig.DeserializeByFile(directory);
            Config.UIConfig.DeserializeByFile(directory);
            Config.ShopConfig.DeserializeByFile(directory);
            Config.I18NConfig.DeserializeByFile(directory);
        }
        public static System.Collections.IEnumerator LoadAllConfigsByBundle(string directory, string subFolder)
        {
            yield return Config.GlobalConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.CharacterConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.CombatFormulaConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.PlayerConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.BulletConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.InterruptStackActionConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.BuffConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.SkillConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.DungeonConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.DungeonMapConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.SkillCDConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.AttributeMapConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.ExperienceGrowthConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.EquipmentQualityConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.EquipmentRandomAttributeConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.PlayerForge.DeserializeByBundle(directory, subFolder);
            yield return Config.StrengthenConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.EquipmentBasePropertyConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.RandomAttributeGroupConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.DropRewardGroupConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.PropConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.SpellFieldConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.FXConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.AudioConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.SpriteConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.UIConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.ShopConfig.DeserializeByBundle(directory, subFolder);
            yield return Config.I18NConfig.DeserializeByBundle(directory, subFolder);
        }
    }
}

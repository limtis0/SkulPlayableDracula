using Characters;
using Characters.Cooldowns;
using Characters.Gear;
using Characters.Gear.Weapons;
using Characters.Gear.Weapons.Gauges;
using GameResources;
using HarmonyLib;
using Services;
using Singletons;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Characters.CharacterStatus;

namespace PlayableDracula
{
    public static class PlayableDracula
    {
        const string PlaceholderSkill = "TriplePierce_4";

        private class Config
        {
            public const int MinDamage = 11;
            public const int MaxDamage = 15;

            public const float AbilityCooldown = 3f;

            public const int OnBleedHealing = 1;
            public const float FullGaugeHealingMultiply = 3f;

            public const int BleedChancePercent = 10;
        }

        private static bool IsDracula<T>(T gear) where T : Gear
        {
            return gear.name == "Dracula" || gear.name == "Dracula(Clone)";
        }

        public static void PatchAll(Harmony harmony)
        {
            // DropGearPatch
            MethodInfo DropGearMethod = AccessTools.Method(typeof(Weapon), "InitializeSkills");
            MethodInfo DropGearPrefix = AccessTools.Method(typeof(PlayableDracula), nameof(PrefixDropGear));

            harmony.Patch(DropGearMethod, prefix: new HarmonyMethod(DropGearPrefix));


            // SetCurrentSkills
            MethodInfo SetSkillsMethod = AccessTools.Method(typeof(Weapon), "SetCurrentSkills");
            MethodInfo SetSkillsPrefix = AccessTools.Method(typeof(PlayableDracula), nameof(PrefixSetSkills));
            MethodInfo SetSkillsPostfix = AccessTools.Method(typeof(PlayableDracula), nameof(PostfixSetSkills));

            harmony.Patch(SetSkillsMethod, prefix: new HarmonyMethod(SetSkillsPrefix), postfix: new HarmonyMethod(SetSkillsPostfix));


            // SetObtainability
            MethodInfo GetWeaponMethod = AccessTools.Method(typeof(GearManager), nameof(GearManager.GetWeaponToTake), new Type[] { typeof(System.Random), typeof(Rarity) });
            MethodInfo GetWeaponPrefix = AccessTools.Method(typeof(PlayableDracula), nameof(PrefixGetWeapon));

            harmony.Patch(GetWeaponMethod, prefix: new HarmonyMethod(GetWeaponPrefix));


            MethodInfo QuitMethod = AccessTools.Method(typeof(Application), nameof(Application.Quit), new Type[] { typeof(int) });
            MethodInfo QuitPrefix = AccessTools.Method(typeof(PlayableDracula), nameof(OnQuitPrefix));

            harmony.Patch(QuitMethod, prefix: new HarmonyMethod(QuitPrefix));
        }

        #region DropGearPatch

        private static void PrefixDropGear(Weapon __instance)
        {
            if (IsDracula(__instance))
            {
                SetSkillInfo(__instance);
                SetDamage(__instance);
                SetHealingFunc();
                SetBleedChance();
            }
        }

        private static void SetSkillInfo(Weapon weapon)
        {
            weapon.transform.Find("Equipped/Skill").gameObject.AddComponent<SkillInfo>();  // Magic by MrBacanudo

            new Traverse(weapon).Field("_skillSlots").SetValue(1);  // There is only one valid skill, this fixes all errors
        }

        private static void SetDamage(Weapon weapon)
        {
            AttackDamage damage = weapon.GetComponent<AttackDamage>();
            damage.minAttackDamage = Config.MinDamage;
            damage.maxAttackDamage = Config.MaxDamage;
        }

        private static void SetHealingFunc()
        {
            Singleton<Service>.Instance.levelManager.player.status.onApplyBleed += new OnTimeDelegate(HealOnBleed);
        }

        private static void HealOnBleed(Character attacker, Character target)
        {
            Weapon weapon = attacker.playerComponents.inventory.weapon.current;

            if (IsDracula(weapon))
            {
                if (target.type == Character.Type.Dummy)
                    return;

                ValueGauge gauge = (ValueGauge)weapon.gauge;

                bool multiplyHealing = false;
                if (gauge.currentValue >= gauge.maxValue)
                {
                    multiplyHealing = true;
                    gauge.Clear();
                }

                attacker.health.Heal(multiplyHealing ? Config.OnBleedHealing * Config.FullGaugeHealingMultiply : Config.OnBleedHealing);
            }
        }

        private static void SetBleedChance()
        {
            Singleton<Service>.Instance.levelManager.player.onGaveDamage += new GaveDamageDelegate(ApplyBleedWithChance);
        }


        private static readonly ApplyInfo applyInfo = new(Kind.Wound);
        private static void ApplyBleedWithChance(ITarget target,
                                                 in Damage originalDamage,
                                                 in Damage gaveDamage,
                                                 double damageDealt)
        {
            Character player = Singleton<Service>.Instance.levelManager.player;

            if (!IsDracula(player.playerComponents.inventory.weapon.current))
                return;

            if (target.character == null || target.character.health.dead)
                return;

            if (originalDamage.attackType != Damage.AttackType.Melee)
                return;

            if (!MMMaths.PercentChance(Config.BleedChancePercent))
                return;

            player.GiveStatus(target.character, applyInfo);
        }

        #endregion

        #region SetSkillsPatch

        private static void PrefixSetSkills(Weapon __instance)
        {
            if (IsDracula(__instance))
            {
                new Traverse(__instance.skills[0]).Field("_key").SetValue(PlaceholderSkill);  // Set icon
            }
        }

        private static void PostfixSetSkills(Weapon __instance)
        {
            if (IsDracula(__instance))
            {
                CooldownSerializer cooldown = new();
                Traverse traverse = new(cooldown);
                traverse.Field("_maxStack").SetValue(1);
                traverse.Field("_streakCount").SetValue(0);
                traverse.Field("_streakTimeout").SetValue(0);
                traverse.Field("_cooldownTime").SetValue(Config.AbilityCooldown);
                traverse.Field("_type").SetValue(CooldownSerializer.Type.Time);

                new Traverse(__instance.currentSkills[0].action).Field("_cooldown").SetValue(cooldown);
            }
        }

        #endregion

        #region SetObtainability

        private static bool setObtainable = false;
        private static void PrefixGetWeapon(GearManager __instance)
        {
            if (setObtainable)
                return;

            setObtainable = true;

            SetDraculaObtainability(__instance, true);
        }

        private static void OnQuitPrefix()
        {
            SetDraculaObtainability(Singleton<Service>.Instance.gearManager, false);
        }

        private static void SetDraculaObtainability(GearManager gearManager, bool obtainability)
        {
            EnumArray<Rarity, WeaponReference[]> gearByRarity = new Traverse(gearManager).Field("_weapons").GetValue() as EnumArray<Rarity, WeaponReference[]>;

            WeaponReference reference = gearByRarity[Rarity.Legendary].First(gearRef => gearRef.name == "Dracula");

            reference.obtainable = obtainability;
        }

        #endregion
    }
}

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
using UnityEngine;
using static Characters.CharacterStatus;

namespace PlayableDracula
{
    [HarmonyPatch]
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

        #region DropGearPatch

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Weapon), "InitializeSkills")]
        private static void PrefixInitSkills(Weapon __instance)
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
            weapon._skillSlots = 1;
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Weapon), "SetCurrentSkills")]
        private static void PrefixSetSkills(Weapon __instance)
        {
            if (IsDracula(__instance))
            {
                __instance.skills[0]._key = PlaceholderSkill;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Weapon), "SetCurrentSkills")]
        private static void PostfixSetSkills(Weapon __instance)
        {
            if (IsDracula(__instance))
            {
                CooldownSerializer cooldown = new();
                cooldown._maxStack = 1;
                cooldown._streakCount = 0;
                cooldown._streakTimeout = 0;
                cooldown._cooldownTime = Config.AbilityCooldown;
                cooldown._type = CooldownSerializer.Type.Time;

                __instance.currentSkills[0].action._cooldown = cooldown;
            }
        }

        #endregion

        #region SetObtainability

        private static bool setObtainable = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GearManager), nameof(GearManager.GetWeaponToTake), new Type[] { typeof(System.Random), typeof(Rarity) })]
        private static void PrefixGetWeapon(GearManager __instance)
        {
            if (setObtainable)
                return;

            setObtainable = true;

            SetDraculaObtainability(__instance, true);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Application), nameof(Application.Quit), new Type[] { typeof(int) })]
        private static void OnQuitPrefix()
        {
            SetDraculaObtainability(Singleton<Service>.Instance.gearManager, false);
        }

        private static void SetDraculaObtainability(GearManager gearManager, bool obtainability)
        {

            EnumArray<Rarity, WeaponReference[]> gearByRarity = gearManager._weapons;

            WeaponReference reference = gearByRarity[Rarity.Legendary].First(gearRef => gearRef.name == "Dracula");

            reference.obtainable = obtainability;
        }

        #endregion
    }
}

using Characters;
using Characters.Gear;
using Characters.Gear.Weapons;
using HarmonyLib;
using Level;
using System.Reflection;
using UnityEngine;
using System;
using Characters.Cooldowns;

namespace PlayableDracula
{
    public static class PlayableDracula
    {
        const int MinDamage = 11;
        const int MaxDamage = 15;
        const float AbilityCooldown = 3f;

        private static bool IsDracula<T>(T gear) where T : Gear
        {
            return gear.name == "Dracula" || gear.name == "Dracula(Clone)";
        }

        public static void PatchAll(Harmony harmony)
        {
            // DropGearPatch
            MethodInfo DropGearMethod = AccessTools.Method(typeof(LevelManager), nameof(LevelManager.DropGear), new Type[] { typeof(Gear), typeof(Vector3) });
            MethodInfo DropGearPrefix = AccessTools.Method(typeof(PlayableDracula), nameof(PrefixDropGear));

            harmony.Patch(DropGearMethod, prefix: new HarmonyMethod(DropGearPrefix));


            // SetCurrentSkills
            MethodInfo SetSkillsMethod = AccessTools.Method(typeof(Weapon), "SetCurrentSkills");
            MethodInfo SetSkillsPrefix = AccessTools.Method(typeof(PlayableDracula), nameof(PrefixSetSkills));
            MethodInfo SetSkillsPostfix = AccessTools.Method(typeof(PlayableDracula), nameof(PostfixSetSkills));

            harmony.Patch(SetSkillsMethod, prefix: new HarmonyMethod(SetSkillsPrefix), postfix: new HarmonyMethod(SetSkillsPostfix));

        }

        #region SpawnPlayerPatch

        private static readonly GiveDamageDelegate giveDamageDelegate = new(DraculaSetBaseDamage);

        private static bool DraculaSetBaseDamage(ITarget target, ref Damage damage)
        {
            if (IsDracula(damage.attacker.character.playerComponents.inventory.weapon.current))
            {
                damage.@base = 10;
            }

            return false;
        }

        private static void PrefixSpawnPlayer(LevelManager __instance, ref bool __state)
        {
            __state = __instance.player is not null;
        }

        private static void PostfixSpawnPlayer(LevelManager __instance, bool __state)
        {
            // If created new player
            if (__state)
            {
                __instance.player.onGiveDamage.Add(0, giveDamageDelegate);
            }
        }

        #endregion

        #region DropGearPatch

        private static void PrefixDropGear(Gear gear)
        {
            if (IsDracula(gear))
            {
                SetSkillInfo((Weapon)gear);
                SetDamage((Weapon)gear);
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
            damage.minAttackDamage = MinDamage;
            damage.maxAttackDamage = MaxDamage;
        }

        #endregion

        #region SetSkillsPatch

        private static void PrefixSetSkills(Weapon __instance)
        {
            if (IsDracula(__instance))
            {
                new Traverse(__instance.skills[0]).Field("_key").SetValue("TriplePierce_4");  // Set icon
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
                traverse.Field("_cooldownTime").SetValue(AbilityCooldown);
                traverse.Field("_type").SetValue(CooldownSerializer.Type.Time);

                new Traverse(__instance.currentSkills[0].action).Field("_cooldown").SetValue(cooldown);
            }
        }

        #endregion
    }
}

using Characters;
using Characters.Gear;
using Characters.Gear.Weapons;
using HarmonyLib;
using Level;
using System.Reflection;
using UnityEngine;
using System;

namespace PlayableDracula
{
    public static class PlayableDracula
    {
        private static bool IsDracula<T>(T gear) where T : Gear
        {
            return gear.name == "Dracula";
        }

        public static void PatchAll(Harmony harmony)
        {
            // SpawnPlayerPatch
            MethodInfo SpawnPlayerMethod = AccessTools.Method(typeof(LevelManager), nameof(LevelManager.SpawnPlayerIfNotExist));
            MethodInfo SpawnPlayerPrefix = AccessTools.Method(typeof(PlayableDracula), nameof(PrefixSpawnPlayer));
            MethodInfo SpawnPlayerPostfix = AccessTools.Method(typeof(PlayableDracula), nameof(PostfixSpawnPlayer));

            harmony.Patch(SpawnPlayerMethod, prefix: new HarmonyMethod(SpawnPlayerPrefix), postfix: new HarmonyMethod(SpawnPlayerPostfix));


            // DropGearPatch
            MethodInfo DropGearMethod = AccessTools.Method(typeof(LevelManager), nameof(LevelManager.DropGear), new Type[] { typeof(Gear), typeof(Vector3) });
            MethodInfo DropGearPrefix = AccessTools.Method(typeof(PlayableDracula), nameof(PrefixDropGear));

            harmony.Patch(DropGearMethod, prefix: new HarmonyMethod(DropGearPrefix));

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
            }
        }

        private static void SetSkillInfo(Weapon weapon)
        {
            weapon.transform.Find("Equipped/Skill").gameObject.AddComponent<SkillInfo>();  // Magic by MrBacanudo

            new Traverse(weapon).Field("_skillSlots").SetValue(1);  // There is only one valid skill, this fixes all errors
        }

        #endregion
    }
}

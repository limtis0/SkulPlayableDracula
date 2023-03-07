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
        private static readonly Sprite droppedSkull;
        private static readonly Sprite hudIcon;

        const string DraculaSkillKey = "DraculaSkill";
        const string PlaceholderSkillKey = "TriplePierce_4";

        static PlayableDracula()
        {
            // Load droppedSkull sprite
            string skullB64 =
                "iVBORw0KGgoAAAANSUhEUgAAABsAAAAWCAMAAAAGlBe5AAAAAXNSR0IArs4c6QAAAH5QTFRFAAAAprHSxc/o6/T3ipOvuAAg2OLvyNLkbnaNOQAhnnx/gGJ" +
                "kpgAafQQR9PDxaQA8oH+CvZeawwASXgANsQYXBAQEfQQSzC09S1Jl/wk0OgAPBgcI/9po13kNLxkx5+3wHgwG//jhNhIE2MPF////0NviNAYSEwsUPEVKoL" +
                "bFgkZQuwAAACp0Uk5TAP//////////////////////////////////////////////////////J0/sFAAAALZJREFUKJGN0O0OgyAMQFE+ZymK6xIS9/4vu" +
                "lJwiM5k94dRTiBUpf5Mty7LpsYvJyxkJaPNiJ0ausGs7Wi1c52OxugG02XRPybOGgfgG4UQoNhDe0ScJohxhkrLUpAPhdljSggRcQ5CiWNkC+uK5Ss9cQ1E" +
                "okQpUbEg9Cp7A+2XYeUBMXNiGeJ3vo2o2Zsx5Qx99m0To7pxNEb+zfUi0tEE70yxRboxZQw/YpKR4ZftneyIZ7r2ARadCpbgjX+gAAAAAElFTkSuQmCC";

            Texture2D skullT = LoadTextureB64(skullB64);
            droppedSkull = Sprite.Create(skullT, new Rect(0.0f, 0.0f, skullT.width, skullT.height), new Vector2(0.5f, 0.5f), 35.0f);


            // Load mainIcon sprite
            string hudIconB64 =
                "iVBORw0KGgoAAAANSUhEUgAAACMAAAAXCAMAAAC27AbQAAAAAXNSR0IArs4c6QAAAH5QTFRFAAAAprHSxc/o6/T3ipOvuAAg2OLvyNLkbnaNOQAhnnx/gGJkpg" +
                "AafQQR9PDxaQA8oH+CvZeawwASXgANsQYXBAQEfQQSzC09S1Jl/wk0OgAPBgcI/9po13kNLxkx5+3wHgwG//jhNhIE2MPF////0NviNAYSEwsUPEVKoLbFgkZQ" +
                "uwAAACp0Uk5TAP//////////////////////////////////////////////////////J0/sFAAAARxJREFUKJGd0dlugzAQBVAwELzEpu6YpQUUVWpR+v8/2D" +
                "uGhEDIS6+EWeZoGNlJ8q+kS16WxRw8vEBMshiRimO0kgXlhybLVpSl+RNKNwYoPzQpF4tTiWQil7LYEaWUZHNKC611WUpjznJLrGWEn8lzoZ3T0mh9VhviECAY" +
                "VVWa39ybrpT3j12s9855NiqSd+6lPK0ihBAVNkjXSDS1NNTcWoTQtkCd94v5AHJ1LRcSVcuxXReNnxuxadom2Sg06nCc88AxEl9viJA7OjZEfM1ICOMfDCadEd" +
                "17BStE3/fGYemdlCEJYZ2I1TAM4yhoQv1ywUK7o0iSEeRrGIUF+kZ6svRkGA3jhNL1B8H9c0ewkSMTpYiuvwiRmubSHw3gGedUu+/ZAAAAAElFTkSuQmCC";

            Texture2D hudIconT = LoadTextureB64(hudIconB64);
            hudIcon = Sprite.Create(hudIconT, new Rect(0.0f, 0.0f, hudIconT.width, hudIconT.height), new Vector2(0.5f, 0.3f), 20.0f);
        }
        
        // Thanks to MrBacanudo for this method of loading Sprites!
        private static Texture2D LoadTextureB64(string b64String)
        {
            byte[] skullIconBytes = Convert.FromBase64String(b64String);

            Texture2D texture = new(2, 2);
            texture.LoadImage(skullIconBytes);
            texture.filterMode = FilterMode.Point;

            return texture;
        }

        private static bool IsDracula<T>(T gear) where T : Gear
        {
            return gear.name == "Dracula" || gear.name == "Dracula(Clone)";
        }

        #region OnInit

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Weapon), "InitializeSkills")]
        private static void PrefixInitSkills(Weapon __instance)
        {
            if (IsDracula(__instance))
            {
                SetDroppedSprite(__instance);
                SetSkillInfo(__instance);
                SetDamage(__instance);
                SetHealingFunc();
                SetBleedChance();
            }
        }

        private static void SetDroppedSprite(Weapon weapon) => weapon._dropped.spriteRenderer.sprite = droppedSkull;

        private static void SetSkillInfo(Weapon weapon)
        {
            weapon.transform.Find("Equipped/Skill").gameObject.AddComponent<SkillInfo>();  // Magic by MrBacanudo
            weapon._skillSlots = 1;
        }

        private static void SetDamage(Weapon weapon)
        {
            AttackDamage damage = weapon.GetComponent<AttackDamage>();
            damage.minAttackDamage = Plugin.BaseMinDamage.Value;
            damage.maxAttackDamage = Plugin.BaseMaxDamage.Value;
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

                attacker.health.Heal(multiplyHealing ? Plugin.OnBleedHealing.Value * Plugin.FullGaugeHealingMultiplyBy.Value : Plugin.OnBleedHealing.Value);
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

            if (!MMMaths.PercentChance(Plugin.BleedChancePercent.Value))
                return;

            player.GiveStatus(target.character, applyInfo);
        }

        #endregion

        #region SetSkills

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Weapon), "SetCurrentSkills")]
        private static void PrefixSetSkills(Weapon __instance)
        {
            if (IsDracula(__instance))
            {
                __instance.skills[0]._key = DraculaSkillKey;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Weapon), "SetCurrentSkills")]
        private static void PostfixSetSkills(Weapon __instance)
        {
            if (IsDracula(__instance))
            {
                CooldownSerializer cooldown = new()
                {
                    _maxStack = 1,
                    _streakCount = 0,
                    _streakTimeout = 0,
                    _cooldownTime = Plugin.AbilityCooldown.Value,
                    _type = CooldownSerializer.Type.Time
                };

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

        #region SetIconsAndDescriptions

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Weapon), nameof(Weapon.mainIcon), MethodType.Getter)]
        [HarmonyPatch(typeof(Weapon), nameof(Weapon.subIcon), MethodType.Getter)]
        private static bool PrefixHudIcon(Weapon __instance, ref Sprite __result)
        {
            if (IsDracula(__instance))
            {
                __result = hudIcon;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Gear), nameof(Gear.displayName), MethodType.Getter)]
        [HarmonyPatch(typeof(Weapon), nameof(Weapon.activeName), MethodType.Getter)]
        private static bool PrefixSkullName(Gear __instance, ref string __result)
        {
            if (IsDracula(__instance))
            {
                __result = "Dracula";
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Gear), nameof(Gear.description), MethodType.Getter)]
        private static bool PrefixSkullDescription(Gear __instance, ref string __result)
        {
            if (IsDracula(__instance))
            {
                __result =
                    $"Normal attacks have a {Plugin.BleedChancePercent.Value}% chance to inflict <color=#C30012>Wound</color>.\n" +
                    $"Restore {Plugin.OnBleedHealing.Value} HP when inflicting an enemy with <color=#C30012>Bleed</color>.\n" +
                    $"This skull's gauge fills up on dealing damage to enemies.\n" +
                    $"When gauge is full, next inflicted <color=#C30012>Bleed</color> will restore {Plugin.FullGaugeHealingMultiplyBy.Value} times more HP.";
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Weapon), nameof(Weapon.activeDescription), MethodType.Getter)]
        private static bool PrefixActiveDescription(Weapon __instance, ref string __result)
        {
            if (IsDracula(__instance))
            {
                __result = "Does nothing on swap :)";
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillInfo), nameof(SkillInfo.displayName), MethodType.Getter)]
        private static bool PrefixSkillName(SkillInfo __instance, ref string __result)
        {
            if (__instance.key == DraculaSkillKey)
            {
                __result = "Swift Strike";
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillInfo), nameof(SkillInfo.description), MethodType.Getter)]
        private static bool PrefixSkillDescription(SkillInfo __instance, ref string __result)
        {
            if (__instance.key == DraculaSkillKey)
            {
                __result = "Dash forward and barrage enemies with piercing strikes, dealing <color=#F25D1C>Physical damage</color>.";
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GearResource), nameof(GearResource.GetSkillIcon))]
        private static void PrefixGetSkillIcon(ref string name)
        {
            if (name == DraculaSkillKey)
                name = PlaceholderSkillKey;
        }

        #endregion
    }
}

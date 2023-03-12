using Characters;
using Characters.Cooldowns;
using Characters.Gear;
using Characters.Gear.Weapons;
using Characters.Gear.Weapons.Gauges;
using Characters.Player;
using FX.SpriteEffects;
using GameResources;
using HarmonyLib;
using Services;
using Singletons;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static Characters.CharacterStatus;

namespace PlayableDracula
{
    [HarmonyPatch]
    public static class PlayableDracula
    {
        private static readonly Sprite droppedSkull;
        private static readonly Sprite hudIcon;
        private static readonly Sprite thumbnail;

        const string DraculaSkillKey = "DraculaSkill";
        const string PlaceholderSkillKey = "TriplePierce_4";

        private static readonly GenericSpriteEffect swapSpriteEffect;

        static PlayableDracula()
        {
            // Load droppedSkull sprite
            string skullB64 =
                "iVBORw0KGgoAAAANSUhEUgAAABsAAAAWCAMAAAAGlBe5AAAAAXNSR0IArs4c6QAAAH5QTFRFAAAAprHSxc/o6/T3ipOvuAAg2OLvyNLkbnaNOQAhnnx/gG" +
                "JkpgAafQQR9PDxaQA8oH+CvZeawwASXgANsQYXBAQEfQQSzC09S1Jl/wk0OgAPBgcI/9po13kNLxkx5+3wHgwG//jhNhIE2MPF////0NviNAYSEwsUPEVK" +
                "oLbFgkZQuwAAACp0Uk5TAP//////////////////////////////////////////////////////J0/sFAAAALBJREFUKJGF0QEOgyAMQFFB0FK0rktI3P" +
                "0vupYRrbhlP8aoDwnoMPzPtb6Sl+NTN0CfOz/WOgyGOgzBjScJXixYUjswAgQ/ztIU1ex6YckZ5hkRo5vUIKXULC2IGZAI4wIypdK6qjCnDR+k4bYlMSG9" +
                "q5j0nWfFpMZEzG3ODKVakVC2d4oupghSeTVj3m8mcbXdGh2hfGxLnV3IGGexyy8CqJuhLCfvh6uZ694M3uhHby7BCpaHxESsAAAAAElFTkSuQmCC";

            Texture2D skullT = LoadTextureB64(skullB64);
            droppedSkull = Sprite.Create(skullT, new Rect(0.0f, 0.0f, 27, 22), new Vector2(0.5f, 0.5f), 35.0f);


            // Load mainIcon sprite
            string hudIconB64 =
                "iVBORw0KGgoAAAANSUhEUgAAACMAAAAdCAMAAAAXdyW2AAAAAXNSR0IArs4c6QAAAIRQTFRFAAAAprHSxc/o6/T3ipOvuAAg2OLvyNLkbnaNOQAhnnx/gGJk" +
                "pgAafQQR9PDxaQA8oH+CvZeawwASXgANsQYXBAQEfQQSzC09S1Jl/wk0OgAPBgcI/9po13kNLxkx5+3wHgwG//jhNhIE2MPF////0NviNAYSEwsUPEVKoLbF" +
                "66o7/+20XWiY3gAAACx0Uk5TAP////////////////////////////////////////////////////////947V5JAAABW0lEQVQ4jZ3RfXeCIBgF8EfUBRg0" +
                "BqabWmedzZfv/wV3Qct86Z9dT2Tx84JK9K9EU15OszE4eYE8iUNYxPbRTCaU7Jo4nlEcJRsULQxQsmsiP5m+HZCYJZynKyKE4N68RamU8nDgWXbkS6KUR1iM" +
                "H1OpteSZlEexIBoBghGnk/S/9Ls8CWOeW5QxWhtvRCAfvksYOwvnXFB4QDJHgsl5Zs/3CueKAqg0ZjKfQDrP+USCKnxUWQZjxiJvzsWZFgpFJV7nuOEQjn/v" +
                "yCIPtG+s9Z8RMZaZJ4Odjsg+upxirKqqTGOoNOeOnJt35FVd103DbIv56xWDXb0Kogbku26YAvpBKqvsxnhUNy2m+l8E319r41zjiRDW9gNirWjXhlSL1F13" +
                "ufQDDUNPHak1wX22HXUdXXrqe8IFmxosVuDSjugSnhiBtMsi54dAOrrhoFGtex5nNw9Dz3a1F/kD2Ygg2ZsOfKMAAAAASUVORK5CYII=";

            Texture2D hudIconT = LoadTextureB64(hudIconB64);
            hudIcon = Sprite.Create(hudIconT, new Rect(0.0f, 0.0f, 35, 29), new Vector2(0.5f, 0.5f), 35.0f);


            // Load thumbnail sprite
            string thumbnailB64 = "iVBORw0KGgoAAAANSUhEUgAAAC8AAAAnCAYAAACfdBHBAAAAAXNSR0IArs4c6QAACMVJREFUWIXVmXtwVNUdxz+sbIIX24Ww2xBIs6AQWVYJ" +
                "wUBcnMRHiMSBwKAYHpVhoKMVxqYSUYeO9DHURzpFR2HQaRFEIRiaYHEFCjHYhNLIa0iA9EIiwc0Ly91AUnVlE8ntH5d7cm/2keBjOv5m7txzz/2d3/l+f+d3fvecc+" +
                "EHLAO+CyMlJSVqbGwsAIqiAOBwOEQZYOnSpd9JX0b5xga9Xq8a7b2iKCEEdPmuiFy3kf6ANoqRQO9ybm7utyIR0rho18mIyolD29WOjg5sNhsdHR1hdT7vdkbt8Erb" +
                "MdOz/8uhIRiWL7qPzy+GjphREpLHMDCqhkESh7arADabDZfLhSzLNF8eAoAz6SdCL65XO1/jRaHja7zIoGFpJgL2wZfVcAT6EBUY0C/w2VMTVFlux+FwAIQFnjzWFr" +
                "F9XwRcowaq8qdf95eAyrWIsfSlmZpsVSsqKoCeeL4e4MljbQK0sY3/y6FCR1EU7IMvR51LvYH3CT412SoMOhwO4fne4PoSIwGjvk5At9sHARPwqOBTk61qbW0tAG63" +
                "W9SfqOvqE2w40QmUldfT3Hguol4EAiHAIQz4WXc7yZ6aoIIZtKIoIWlQDwWjHL/JRdulK5xraDNdujQ3nqO5qYHmpgYm/HKFqDeO6o6dpUYCYYED4bON3+8HoKqqCg" +
                "CPx4PL5WKWY4rQmfzME9x5V7apXdulK1za9T4kpdL63GJRPz7vUc41tFG8vZg4v0wckLxpD3ueeYrsa7lfB98rNCMChzCe//uHZaokSVRVVZGVlYXH46G2tpbXEpKZ" +
                "NrBd6B3943o+PlRGXX1Pvt+wfhPMzhHPeSsLyFtZwL93/IWirRrwpBEjSRoxUtjoDdgwulGBQwTPBwIB0z1nWjY5/k8BeBbIHjJRdD5vwc9MhLq4GYDJBnt5KwvIu1" +
                "YueeVVuq9e5SA3Ay1IzRe0vhIThP69GZl8dLCSezMyo2EP9bzb7UaSJDwejwibQvsoCu2jOLy9mMPbiylrr6asvZo/vL4qxGA1LUx+Lou8lQWm+hNDRgMwd8WvALA+" +
                "M0e827LsKUAbgbwHH2LHztKooHUxDYvP5xMTRfd60e1TSbPEAHCsuxOAe9YVApC+YB41578CtGzifeklHnvpeVMH7f42AT61/TwAQ+zDWF5QSJxfpmFTMc6BVgCe9Z" +
                "9nx85SEUaKopD34ENhQ+emIbbIywNJkth4y0QBfOStyXC2jmPdnbxcdpSC7Mkc3l7MoDtnRfRMl3UiVvsw2v1tJuC66MBnu9yMO/gBhfbRTHlvi3jvcDgotI+OaD9s" +
                "ng8EAqHADXeAl8uOhjW4b90GUbZ2VUfsGDABP5Mxk9kuN0fmLI7aJip4SZJMoWIEDJBmiWHS+yUhBPSsk75gHu3+NtNlFGPdbJebjuWLOJMxk5azdbScrSPNYhUEwu" +
                "0FooJ/LSGZNEsMyTX/oLDzCvmnTopLJ2Mk8MKa1VE7iCTLbvgdg2dWY9vwDvmnTlLYeQVL6UYspRsFgXDLkYjgJUki/0IdU1tOAzCGbsbQzZvyx4yhW5DoTUCXxtYW" +
                "Dm8v7hN4U2GiKOefOskq71bG0M1fcxdqoEo3MuW9LX2GUMS1zYsu7Wu6xqcRWSUfEWQiEXhh21YeeDKfuPh4/lSgpb+4+Hji4uNpKkwkLj4egOnrgoxbon0fVnm3Eg" +
                "wGedhbBEAwGBT2jJO33+D1NLnGd1qUdQKr5CMA3FHqZtbpOSx8+mvxfn5QS3muOBfv7T1OXHw8vtbPqCnQ6msKrDQVJpL4xY04J21CeWA3APrm/WFvEe/PXYLP58Pn" +
                "80UFDmG+sIFAgNXO21irNBAIBLDb7QQCAROJQ3ULtELSKsYtgTObx1K3VuIXJ+ohdSzvXpIBCF7RvgHDX/gv1C8T7d9kGxALe2fAA7tNW8op61/E6dS2kn0RMHne4X" +
                "AMkCRJhIokSXi9XsrLy7Hb7fD2FMbd8GpPg8YXRXENXfjTU3goRmJ+0Mr8oJWaT5qo+aSJ3//8UZyTNkFSzxdZDxvH3hmAtr3sLf958jdR1zZhw0aSJAKBAF6vVyzO" +
                "/H4/9oIGzmzeZtI9s3kbi9ZaGd/ZxVLrVZZar7J7cCwN2RlU1zexOHM2e/72Fvv3fsSBzfs04L89bSIw5l8LhfedTuc3D5twRADKy8vxeDwsWmvlHcwExnd2sXuwFr" +
                "ex1kE4buxJcbHBU7y9S9tGjmh8CntqLP/M3ybivWPqDGw2GzYwEQgGgzzx9aWo2MIOi6IoYo2jex+0+VB0+1T2WszNKsrvISmn0gT8R7cl0Hl8P8u3asDTD+aQ9boW" +
                "Go980cnTF2qRZVnY0E8kuEaiP2c6fW7Ac3NzAW0E9FFYjZU3bnVz3mqhVZK4O+sY/sa3TO0+P32BmDvu59cL55F+MIdFa4cyIhC+O5fLZXoOF//9Bu9wOAZAT8rU54" +
                "C+3hl5azLTG86Q+OOfCk+PvOVxWj47JGykW08w/uweav7sFB4HQgjoHpdlWZDIzMzs1zFIxJh3OBwDFEVRjSnSKLHW2JC6kcPvouXcG9D2AZCOv/oUKY+FTrwDlkGQ" +
                "4GbmgRIRKjqJ/gKPCt5IAHrWPBPrj6DMfAQA5SuFEQELrVJ3D4FbHgcg0fLZtTs0dw+nVeqOGDa6XA9w6MdB6+qBQ8m/UGcagUAgQJYnmxEBC13DhvHugSLxLmViqij" +
                "XVJ/glekuVuyTmX+ftm6xtmkryr0tx4Xe9Xr8ukVRFHVXTLzq8/nElZKQqqYkpKo5KdNUWZbVnJRpqqIoqizL4lIURY2JHaSWlJSoKQmpaklJiaooipoWk6RWVlaqlZ" +
                "WV/TkpCyv9Zuv1epkwYYLoyJNyFzX1J8QxiT6p9eXE8xlOVuzrSYV2ux2AaRPu58OT+0W9nhy+V/C6+Hw+VU+ZvYFHErvdTkVFBW63W5CAbwccvsWfEZ2EvmgzEjAeV" +
                "ungQSNbW1vL3Llz/z9/RsKJLMsinKqqqgRoY9nlcn1/E/KHKP8DWqvfDeZ7WtoAAAAASUVORK5CYII=";

            Texture2D thumbnailT = LoadTextureB64(thumbnailB64);
            thumbnail = Sprite.Create(thumbnailT, new Rect(0.0f, 0.0f, 47, 39), new Vector2(0.5f, 0.5f), 19.0f);
            


            // Create an effect for swap
            var colorOverlay = new GenericSpriteEffect.ColorOverlay()
            {
                _enabled = true,
                _curve = new Curve(AnimationCurve.Linear(0.5f, 1, 0, 0)),
                _startColor = Color.red,
                _endColor = new Color(1, 0, 0, 0),
            };
            var colorBlend = new GenericSpriteEffect.ColorBlend() { _enabled = false };
            var outline = new GenericSpriteEffect.Outline()
            {
                _enabled = true,
                _colorChange = true,
                _color = Color.red,
                _endColor = new Color(1, 0, 0, 0f),
                _duration = Plugin.DummySwapVFXDuration.Value,
                _width = 3,
            };
            var grayScale = new GenericSpriteEffect.GrayScale() { _enabled = false };

            swapSpriteEffect = new(0, Plugin.DummySwapVFXDuration.Value, 1f, colorOverlay, colorBlend, outline, grayScale);
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
                if (gauge.currentValue >= gauge.maxValue && attacker.health.currentHealth < attacker.health.maximumHealth)
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

        #region SetSwapAbility

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Weapon), nameof(Weapon.activeName), MethodType.Getter)]
        private static bool PrefixActiveName(Gear __instance, ref string __result)
        {
            if (IsDracula(__instance))
            {
                __result = Plugin.DummySwapName.Value;
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
                __result = Plugin.DummySwapDescription.Value;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Weapon), nameof(Weapon.GetChildActions))]
        private static void SetSwapAction(Weapon __instance)
        {
            if (IsDracula(__instance))
            {
                var dummyAction = CloneSwapActions(Plugin.DummySwapSkullName.Value);

                var action = UnityEngine.Object.Instantiate(dummyAction);
                action.motion._length = 0;
                action.motion.length = 0;
                action.motion._blockLook = false;
                action.motion._blockMovement = false;
                action.motion._stay = false;
                action.name = "Swap_Dracula";

                action.transform.SetParent(__instance.transform);
            }
        }

        private static Characters.Actions.SimpleAction CloneSwapActions(string dummySkullName)
        {
            WeaponReference weaponRef = GearResource.instance.weapons.Where(wr => wr.name == dummySkullName).First();

            WeaponRequest weaponReq = weaponRef.LoadAsync();
            weaponReq.WaitForCompletion();

            Weapon weapon = weaponReq.asset;
            weapon.name = dummySkullName;

            if (string.IsNullOrEmpty(Plugin.DummySwapDescription.Value))
            {
                Plugin.DummySwapDescription.Value = weapon.activeDescription;
            }

            return weapon.GetComponentsInChildren<Characters.Actions.SimpleAction>(includeInactive: true)
                .Where(action => action.type == Characters.Actions.Action.Type.Swap).First();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(WeaponInventory), nameof(WeaponInventory.NextWeapon))]
        private static void PostfixNextWeapon(WeaponInventory __instance, bool __result)
        {
            if (__result is false)
                return;

            if (IsDracula(__instance.current))
            {
                swapSpriteEffect.Reset();
                Singleton<Service>.Instance.levelManager.player.spriteEffectStack.Add(swapSpriteEffect);
            }
            else
            {
                Singleton<Service>.Instance.levelManager.player.spriteEffectStack.Remove(swapSpriteEffect);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(WeaponInventory), nameof(WeaponInventory.Equip))]
        private static void PostfixEquipAt(WeaponInventory __instance)
        {
            if (!IsDracula(__instance.current))
            {
                Singleton<Service>.Instance.levelManager.player.spriteEffectStack.Remove(swapSpriteEffect);
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
        [HarmonyPatch(typeof(Gear), nameof(Gear.thumbnail), MethodType.Getter)]
        private static bool PrefixThumbnail(Weapon __instance, ref Sprite __result)
        {
            if (IsDracula(__instance))
            {
                __result = thumbnail;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Gear), nameof(Gear.displayName), MethodType.Getter)]
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

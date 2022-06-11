using System;
using System.Reflection;
using MelonLoader;
using NinjaTabi;
using UnityEngine;
using VRC.SDKBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using VRC.Core;

[assembly: MelonInfo(typeof(MelonEntry), "NinjaTabi", "1.0.0", "Dotlezz", "https://github.com/Dotlezz/NinjaTabi")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace NinjaTabi;

public class MelonEntry : MelonMod
{
    private static readonly MelonPreferences_Category Category = MelonPreferences.CreateCategory("NinjaTabi");
    private readonly MelonPreferences_Entry<bool> _entry1 = Category.CreateEntry("Enabled", true);
    private readonly MelonPreferences_Entry<float> _entry2 = Category.CreateEntry("Multiplier", 1.5f);

    public override void OnApplicationStart()
    {
        HarmonyInstance.Patch(
            typeof(RoomManager).GetMethod(nameof(RoomManager
                .Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0)),
            typeof(MelonEntry).GetMethod(nameof(Check), BindingFlags.NonPublic | BindingFlags.Static)
                .ToNewHarmonyMethod());
    }

    public override void OnUpdate()
    {
        if (!_entry1.Value || !_isAllowed || Networking.LocalPlayer == null ||
            Networking.LocalPlayer.IsPlayerGrounded() ||
            !VRCInputManager.Method_Public_Static_VRCInput_String_0("Jump").prop_Boolean_0)
            return;

        var old = Networking.LocalPlayer.GetVelocity();
        Networking.LocalPlayer.SetVelocity(new Vector3(old.x, Networking.LocalPlayer.GetJumpImpulse() * _entry2.Value,
            old.z));
    }

    private static bool _isAllowed;

    private static readonly List<string> BList = new List<string>
    {
        "author_tag_game", "author_tag_games", "admin_game", "author_tag_club",
    };

    private static void Check(ApiWorld __0)
    {
        var name = __0.name.ToLower();

        var tags = new List<string>();
        foreach (var tag in __0.tags) tags.Add(tag.ToLower());

        var hasTag = BList.Any(t => tags.Contains(t));
        var isAllowed = !name.Contains("club") && !name.Contains("game") && !hasTag;

        var gameobjects = SceneManager.GetActiveScene().GetRootGameObjects();
        if (gameobjects.Any(o => o.name is "eVRCRiskFuncDisable" or "UniversalRiskyFuncDisable"))
        {
            isAllowed = false;
        }
        else if (gameobjects.Any(o => o.name is "eVRCRiskFuncEnable" or "UniversalRiskyFuncEnable"))
        {
            isAllowed = true;
        }

        _isAllowed = isAllowed;
    }
}
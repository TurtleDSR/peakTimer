namespace peaktimer;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using HarmonyLib;

using UnityEngine;

using System;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Peak.exe")]

[BepInDependency("BepInEx-BepInExPack_PEAK-5.4.2403")] //require bepinexpack
public class Plugin : BaseUnityPlugin {
  internal static new ManualLogSource Logger;

  private static ConfigEntry<int> goalConfig;
  private static ConfigEntry<int> startConfig;
  private static ConfigEntry<bool> timingMethodConfig; //true = igt : false = rta

  private static GameObject mountainProgress;
  private static Component progressScript;

  private static Campfire beachCampfire;
  private static Campfire jungleCampfire;
  private static Campfire snowCampfire;
  private static Campfire volcanoCampfire;

  private static bool timing;

  private static bool timingMethod;

  private static int goalPoint;
  private static int startPoint;

  private static float time;
  private static string timeString;
  private static string fpsString;

  private static Vector2 timerPos;
  private static Vector2 fpsPos;

  private static float timerStartDelay;

  private void Awake() {
    Init();

    Logger = base.Logger;

    var harmony = new Harmony("com.turtledsr.peaktimer");
    harmony.PatchAll();

    startConfig = Config.Bind("Timing", "Start", 1, "Start point for the timer, 1 = Blink ... 5 = Volcano Campfire");
    startPoint = startConfig.Value;

    goalConfig = Config.Bind("Timing", "Goal", 5, "Goal point for the timer, 1 = Tropics ... 5 = Peak");
    goalPoint = goalConfig.Value;

    timingMethodConfig = Config.Bind("Timing", "Timing Method", false, "true = igt : false = rta");
    timingMethod = timingMethodConfig.Value;

    Logger.LogInfo("Peak Timer is loaded!");
  }

  private void OnGUI() {
    GUIStyle style = new() {
      fontSize = 30,
      fontStyle = FontStyle.Bold,
    };
    style.normal.textColor = Color.white;

    GUI.Label(new Rect(timerPos.x, timerPos.y, 200, 30), timeString, style);
    GUI.Label(new Rect(fpsPos.x, fpsPos.y, 200, 30), fpsString, style);
  }

  private void Update() {
    fpsString = $"FPS: {Math.Floor(1 / Time.deltaTime)}";

    if(timerStartDelay > 0) {
      timerStartDelay -= Time.deltaTime;

      if(timerStartDelay <= 0) {
        timerStartDelay = -1;
        timing = true;
      }
    }

    if(timing && mountainProgress != null) {

      time += Time.deltaTime;
      timeString = GetTimeString();

      switch (goalPoint) {
        case 1:
          if(beachCampfire != null && beachCampfire.state != Campfire.FireState.Off) {timing = false;}
          break;
        case 2:
          if(jungleCampfire != null && jungleCampfire.state != Campfire.FireState.Off) {timing = false;}
          break;
        case 3:
          if(snowCampfire != null && snowCampfire.state != Campfire.FireState.Off) {timing = false;}
          break;
        case 4:
          if(volcanoCampfire != null && volcanoCampfire.state != Campfire.FireState.Off) {timing = false;}
          break;
        default: break;
      }
    } else if(!timing && mountainProgress != null) {
      switch (startPoint) {
        case 2:
          if(beachCampfire != null && beachCampfire.state != Campfire.FireState.Off) {timing = true;}
          break;
        case 3:
          if(jungleCampfire != null && jungleCampfire.state != Campfire.FireState.Off) {timing = true;}
          break;
        case 4:
          if(snowCampfire != null && snowCampfire.state != Campfire.FireState.Off) {timing = true;}
          break;
        case 5:
          if(volcanoCampfire != null && volcanoCampfire.state != Campfire.FireState.Off) {timing = true;}
          break;
        default: break;
      }
    }
  }

  private void Init() {
    timing = false;
    time = 0f;
    timeString = "00:00:00.000";
    timerPos = new (1710, 60);
    fpsPos = new (1710, 30);
    timerStartDelay = -1;
  }

  private static void Bind() {
    goalPoint = goalConfig.Value;
    timingMethod = timingMethodConfig.Value;

    mountainProgress = GameObject.Find("MountainProgress");
    timing = false;

    if (mountainProgress != null) {
      timeString = "00:00:00.000";
      time = 0f;

      Logger.LogInfo("Bound MountainProgress!");
      progressScript = mountainProgress.GetComponentAtIndex(1);

      if(startPoint == 1) {timerStartDelay = 6f;}

      if(progressScript == null) {
        Logger.LogWarning("Couldn't bind progressScript");
      }

      LoadCampfires();
    } else {
      Logger.LogWarning("Couldn't bind MountainProgress :(");
    }
  }

  private static void Unbind() {
    mountainProgress = null;
    progressScript = null;

    timing = false;
    Logger.LogInfo("Unbound RunManager!");
  }

  public static void LoadCampfires() {
    Logger.LogInfo("Loading Campfires");

    try {
      beachCampfire = GameObject.Find("Map").transform.GetChild(1).GetChild(3).GetChild(0).gameObject.GetComponentAtIndex<Campfire>(1);
      if(beachCampfire == null) {
        Logger.LogError("Could not find Beach_Campfire");
      } else {
        Logger.LogInfo("Found Beach_Campfire");
      }
    } catch (System.Exception) {Logger.LogError("Could not find Beach_Campfire");}

    try {
      jungleCampfire = GameObject.Find("Map").transform.GetChild(3).GetChild(3).GetChild(0).gameObject.GetComponentAtIndex<Campfire>(1);
      if(jungleCampfire == null) {
        Logger.LogError("Could not find Jungle_Campfire");
      } else {
        Logger.LogInfo("Found Jungle_Campfire");
      }
    } catch (System.Exception) {Logger.LogError("Could not find Jungle_Campfire");}

    try {
      snowCampfire = GameObject.Find("Map").transform.GetChild(5).GetChild(3).GetChild(0).gameObject.GetComponentAtIndex<Campfire>(1);
      if(snowCampfire == null) {
        Logger.LogError("Could not find Snow_Campfire");
      } else {
        Logger.LogInfo("Found Snow_Campfire");
      }
    } catch (System.Exception) {Logger.LogError("Could not find Snow_Campfire");}

    try {
      volcanoCampfire = GameObject.Find("Map").transform.GetChild(6).GetChild(0).GetChild(7).GetChild(1).GetChild(1).GetChild(0).gameObject.GetComponentAtIndex<Campfire>(1);
      if(volcanoCampfire == null) {
        Logger.LogError("Could not find Volcano_Campfire");
      } else {
        Logger.LogInfo("Found Volcano_Campfire");
      }
    } catch (System.Exception) {Logger.LogError("Could not find Volcano_Campfire");}
  }

  private string GetTimeString() {
    int hrs = (int) (time / 3600);
    float timeLeft = time - (hrs * 3600);
    int min = (int) (timeLeft / 60);
    timeLeft -= min * 60;
    int sec = (int) timeLeft;
    int ms = (int) ((timeLeft - Math.Truncate(timeLeft)) * 1000);

    return $"{hrs.ToString("00")}:{min.ToString("00")}:{sec.ToString("00")}.{ms.ToString("000")}";
  } 

  [HarmonyPatch(typeof(PauseOptionsMenu), "OnOpen")]
  static class PauseoptionsMenu_OnOpen_Patch {
    static bool Prefix() {
      if(timingMethod) { //if using igt pause the timer
        Logger.LogInfo("Timer Paused (GAME PAUSE)");
        timing = false;
      }
      return true; //still run original function
    }
  }

  [HarmonyPatch(typeof(PauseOptionsMenu), "OnClose")]
  static class PauseoptionsMenu_OnClose_Patch {
    static bool Prefix() {
      if(timingMethod) { //if using igt pause the timer
        Logger.LogInfo("Timer Unpaused (GAME UNPAUSE)");
        timing = true;
      }
      return true; //still run original function
    }
  }

  [HarmonyPatch(typeof(SteamLobbyHandler), "LeaveLobby")]
  static class SteamLobbyHandler_LeaveLobby_Patch {
    static bool Prefix() {
      Logger.LogInfo("Lobby Left");

      Unbind();
      return true;
    }
  }

  [HarmonyPatch(typeof(RunManager), "StartRun")]
  static class RunManager_StartRun_Patch {
    static bool Prefix() {
      Bind();
      return true;
    }
  }

  [HarmonyPatch(typeof(GlobalEvents), "TriggerRunEnded")]
  static class GlobalEvents_TriggerRunEnded_Patch {
    static bool Prefix() {
      if(goalPoint == 5) {
        Logger.LogInfo("Run Ended By game End");
        timing = false;
      }
      return true;
    }
  }
}

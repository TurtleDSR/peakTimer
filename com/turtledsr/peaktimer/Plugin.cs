﻿namespace peaktimer;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using HarmonyLib;

using UnityEngine;

using System;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Peak.exe")]

public class Plugin : BaseUnityPlugin {
  public enum StartLocation {
    Shore = 0,
    Tropics = 1,
    Alpine = 2,
    Caldera = 3,
    Kiln = 4
  }

  public enum StopLocation {
    Tropics = 0,
    Alpine = 1,
    Caldera = 2,
    Kiln = 3,
    Peak = 4
  }

  public enum TimingMethod {
    RTA = 0,
    IGT = 1
  }

  internal static new ManualLogSource Logger;

  private static ConfigEntry<StopLocation> goalConfig;
  private static ConfigEntry<StartLocation> startConfig;
  private static ConfigEntry<TimingMethod> timingMethodConfig; //true = igt : false = rta

  private static ConfigEntry<String> versionConfig;

  private static GameObject mountainProgress;
  private static Component progressScript;

  private static Campfire beachCampfire;
  private static Campfire jungleCampfire;
  private static Campfire snowCampfire;
  private static Campfire volcanoCampfire;

  private static bool timing;

  private static TimingMethod timingMethod;

  private static StopLocation goalPoint;
  private static StartLocation startPoint;
  private static float time;
  private static string timeString;

  private static Vector2 timerPos;

  private static float timerStartDelay;

  private static Version currentVersion = new Version(MyPluginInfo.PLUGIN_VERSION);

  private void Awake() {
    Init();

    Logger = base.Logger;

    var harmony = new Harmony("com.turtledsr.peaktimer");
    harmony.PatchAll();

    ConfigDefinition versionDef = new ConfigDefinition("info", "version");

    bool resetConfigs = false;

    versionConfig = Config.Bind("Info", "version", "");

    if(versionConfig.Value != "") {
      Version upd = new Version("1.2.2");

      Logger.LogInfo(versionConfig.Value);
      Logger.LogInfo(upd.ToString());
      Logger.LogInfo(new Version(versionConfig.Value).CompareTo(upd));

      if(new Version(versionConfig.Value).CompareTo(upd) < 0) { //most recent config update
        Logger.LogInfo("New mod version detected: Resetting config");
        resetConfigs = true;
      }

      Config.Save();
      Config.Reload();
    } else {
      Logger.LogInfo("New mod version detected: Resetting config");
      resetConfigs = true;
    }

    versionConfig = null;
    Config.Remove(versionDef);

    Config.Reload();

    startConfig = Config.Bind("Timing", "Start", StartLocation.Shore, "Start point for the timer");
    startPoint = startConfig.Value;

    startConfig.SettingChanged += delegate {
      startPoint = startConfig.Value;
    };

    goalConfig = Config.Bind("Timing", "Goal", StopLocation.Peak, "Goal point for the timer");
    goalPoint = goalConfig.Value;

    goalConfig.SettingChanged += delegate {
      goalPoint = goalConfig.Value;
    };

    timingMethodConfig = Config.Bind("Timing", "Timing Method", TimingMethod.RTA, "Timing method for the timer");
    timingMethod = timingMethodConfig.Value;

    timingMethodConfig.SettingChanged += delegate {
      timingMethod = timingMethodConfig.Value;
    };

    if(resetConfigs) {
      startConfig.Value = StartLocation.Shore;
      goalConfig.Value = StopLocation.Peak;
      timingMethodConfig.Value = TimingMethod.RTA;
      Config.Save();
    }

    Logger.LogInfo("Peak Timer is loaded!");
  }

  private void OnGUI() {
    GUIStyle style = new() {
      fontSize = 30,
      fontStyle = FontStyle.Bold,
    };
    style.normal.textColor = Color.white;

    GUI.Label(new Rect(timerPos.x, timerPos.y, 200, 30), timeString, style);
  }

  private void Update() {
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
        case StopLocation.Tropics:
          if(beachCampfire != null && beachCampfire.state != Campfire.FireState.Off) {timing = false;}
          break;
        case StopLocation.Alpine:
          if(jungleCampfire != null && jungleCampfire.state != Campfire.FireState.Off) {timing = false;}
          break;
        case StopLocation.Caldera:
          if(snowCampfire != null && snowCampfire.state != Campfire.FireState.Off) {timing = false;}
          break;
        case StopLocation.Kiln:
          if(volcanoCampfire != null && volcanoCampfire.state != Campfire.FireState.Off) {timing = false;}
          break;
        default: break;
      }
    } else if(!timing && mountainProgress != null) {
      switch (startPoint) {
        case StartLocation.Tropics:
          if(beachCampfire != null && beachCampfire.state != Campfire.FireState.Off) {timing = true;}
          break;
        case StartLocation.Alpine:
          if(jungleCampfire != null && jungleCampfire.state != Campfire.FireState.Off) {timing = true;}
          break;
        case StartLocation.Caldera:
          if(snowCampfire != null && snowCampfire.state != Campfire.FireState.Off) {timing = true;}
          break;
        case StartLocation.Kiln:
          if(volcanoCampfire != null && volcanoCampfire.state != Campfire.FireState.Off) {timing = true;}
          break;
        default: break;
      }
    }
  }
  
  public void OnApplicationQuit() {
    Logger.LogInfo("Application Quitting!");
    ConfigEntry<string> ver = Config.Bind("Info", "version", currentVersion.ToString());
    ver.Value = currentVersion.ToString();
    Config.Save();
  }

  private void Init() {
    timing = false;
    time = 0f;
    timeString = "00:00:00.000";
    timerPos = new Vector2(Screen.currentResolution.width - 210, 10);
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

      if(startPoint == StartLocation.Shore) {timerStartDelay = 6f;}

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
      if(timingMethod == TimingMethod.IGT) { //if using igt pause the timer
        Logger.LogInfo("Timer Paused (GAME PAUSE)");
        timing = false;
      }
      return true; //still run original function
    }
  }

  [HarmonyPatch(typeof(PauseOptionsMenu), "OnClose")]
  static class PauseoptionsMenu_OnClose_Patch {
    static bool Prefix() {
      if(timingMethod == TimingMethod.IGT) { //if using igt pause the timer
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
      if(goalPoint == StopLocation.Peak) {
        Logger.LogInfo("Run Ended By game End");
        timing = false;
      }
      return true;
    }
  }
}

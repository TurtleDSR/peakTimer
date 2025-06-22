using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using PeakTimer.include.cs;
using UnityEngine.UIElements;
using System;
using BepInEx.Configuration;

namespace PeakTimer;

[BepInPlugin("com.turtledsr.peaktimer", "Peak Timer", "0.0.1.0")]
[BepInProcess("Peak.exe")]
public class Plugin : BaseUnityPlugin {
  private ConfigEntry<int> goalConfig;
  internal static new ManualLogSource Logger;
  private GameObject mountProg;
  private Component progScr;
  private LogListener bind;
  private LogListener unbind;
  private bool timing;
  private int goalPoint;

  private float time = 0f;
  private string timeString = "";

  private Vector2 timerPos = new(1725, 10);

  private void Awake() {
    Logger = base.Logger;

    goalConfig = Config.Bind("General", "goal", 5, "Goal point for the timer, 1 = Tropics ... 6 = Peak");
    goalPoint = goalConfig.Value;

    bind = new LogListener(Bind, "RUN STARTED"); //run started
    unbind = new LogListener(Unbind, "Switched State to: DefaultConnectionState"); //main menu

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
    if(timing && mountProg != null) {
      time += Time.deltaTime;
      timeString = GetTimeString();

      if(goalPoint <= (int) progScr.GetType().GetProperty("maxProgressPointReached").GetValue(progScr)) {
        timing = false;
      }
    }
  }

  private void Bind() {
    mountProg = GameObject.Find("MountainProgress");
    timing = true;

    if (mountProg != null) {
      Logger.LogInfo("Bound MountainProgress!");
      progScr = mountProg.GetComponentAtIndex(1);
      if(progScr == null) {
        Logger.LogError("Couldn't bind progressScript");
      }
    } else {
      Logger.LogError("Couldn't bind MountainProgress :(");
    }
  }

  private void Unbind() {
    mountProg = null;
    progScr = null;

    timeString = "";
    timing = false;
    Logger.LogInfo("Unbound RunManager!");
  }

  private string GetTimeString() {
    int hrs = (int) (time / 3600);
    float timeLeft = time - (hrs * 3600);
    int min = (int) (timeLeft / 60);
    timeLeft -= min * 60;
    int sec = (int) timeLeft;
    int ms = (int) ((timeLeft - Math.Truncate(timeLeft)) * 100);

    return $"{hrs.ToString("00")}:{min.ToString("00")}:{sec.ToString("00")}.{ms.ToString("00")}";
  }
}

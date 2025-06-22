using System;
using BepInEx.Logging;
using PeakTimer;

namespace PeakTimer.include.cs;

public class LogListener : ILogListener {
  private readonly Action func;
  private readonly string key;

  public LogListener(Action func, string key) {
    this.func = func;
    this.key = key;

    Logger.Listeners.Add(this);
  }

  public void Awake() {}

  public void Dispose() {
    Logger.Listeners.Remove(this);
  }

  public void LogEvent(object sender, LogEventArgs eventArgs) {
    if(eventArgs.Data.ToString().Equals(key)) {
      func();
    }
  }
}
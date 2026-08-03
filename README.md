## RunInBackground v1.0.0

Keeps TLD running in the background, instead of pausing the gameplay when the window is not focused

### Background FPS cap

While the window is not focused the game's frame rate is limited, so that running in the background
does not cost more GPU than actually playing does. Without a cap it costs considerably more: vsync
stops throttling a window that is not in front, and Unity ignores `Application.targetFrameRate` while
vsync is enabled, which leaves nothing limiting the frame rate at all.

The limit defaults to 30 and can be changed in `UserData/MelonPreferences.cfg`:

```
[RunInBackground]
BackgroundFpsCap = 30
```

Set it to `0` for no limit, which restores the behaviour of earlier versions.
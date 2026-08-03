using MelonLoader;
using UnityEngine;

namespace RunInBackground
{
    /// <summary>
    /// Caps the frame rate while the game window is not focused.
    ///
    /// Without this the game renders *faster* in the background than it does while you are
    /// playing, because two things line up:
    ///
    /// 1. Vsync stops throttling once the window is no longer the foreground window. The game
    ///    presents through the DXGI flip path, and the compositor stops gating the flip queue
    ///    for a window that is not in front, so Present() returns immediately instead of
    ///    blocking until the next scan-out.
    /// 2. Unity ignores Application.targetFrameRate whenever QualitySettings.vSyncCount != 0,
    ///    because it expects vsync to do the limiting.
    ///
    /// TLD ships vSyncCount = 1 and targetFrameRate = -1, so in the background there is no
    /// frame limiter left at all and the render loop runs as fast as the GPU allows.
    ///
    /// Measured on TLD 2.55, 3840x2160 @ 60 Hz, RX 9070 XT:
    ///
    ///   unfocused, as shipped          143.2 fps (n=1206)   GPU 3D 95.5%
    ///   unfocused, targetFrameRate=60  142.7 fps (n=1713)   GPU 3D 95.7%   <- no effect, see (2)
    ///   unfocused, vSync=0 + cap 30     30.0 fps (n=360)    GPU 3D 31.7%
    ///
    /// Note the middle row: setting a cap on its own does nothing. vSyncCount has to go to 0
    /// for Unity to honour the cap, and the cap has to be set or the game free-runs -- which is
    /// the very problem being fixed. The two always change together.
    /// </summary>
    internal static class BackgroundThrottle
    {
        private static bool _lastFocused = true;
        private static bool _hasFocusState;

        private static bool _throttled;
        private static int _savedVSync;
        private static int _savedTargetFrameRate;

        /// <summary>
        /// Polled once per frame. Application.isFocused is read directly rather than hooking
        /// GameManager.OnApplicationFocus, because this mod's own prefix suppresses that method
        /// for the whole no-pause behaviour. Polling is also self-correcting: if the throttle
        /// ever ends up applied at the wrong moment, the next frame puts it back.
        /// </summary>
        internal static void Poll()
        {
            bool focused = Application.isFocused;

            if (_hasFocusState && focused == _lastFocused)
            {
                return;
            }

            _lastFocused = focused;
            _hasFocusState = true;

            if (focused)
            {
                Restore();
            }
            else
            {
                Apply();
            }
        }

        private static void Apply()
        {
            if (_throttled)
            {
                return;
            }

            int cap = Settings.BackgroundFpsCap;
            if (cap <= 0)
            {
                // 0 means "no limit" -- the behaviour of every release before this one.
                return;
            }

            // Remember what the player actually had, rather than assuming the defaults. Someone
            // playing with vsync turned off in the game's own video options must not have it
            // silently switched back on when they alt-tab back in.
            _savedVSync = QualitySettings.vSyncCount;
            _savedTargetFrameRate = Application.targetFrameRate;
            _throttled = true;

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = cap;
        }

        private static void Restore()
        {
            if (!_throttled)
            {
                return;
            }

            _throttled = false;
            QualitySettings.vSyncCount = _savedVSync;
            Application.targetFrameRate = _savedTargetFrameRate;
        }
    }
}

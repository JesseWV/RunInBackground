using MelonLoader;

namespace RunInBackground
{
    /// <summary>
    /// MelonPreferences is used here rather than ModSettings so that the mod keeps having no
    /// dependencies -- nobody has to install a second mod to keep using this one. The value
    /// lives in UserData/MelonPreferences.cfg under [RunInBackground].
    /// </summary>
    internal static class Settings
    {
        private const int DefaultBackgroundFpsCap = 30;

        private static MelonPreferences_Entry<int>? _backgroundFpsCap;

        internal static int BackgroundFpsCap
        {
            get { return _backgroundFpsCap == null ? DefaultBackgroundFpsCap : _backgroundFpsCap.Value; }
        }

        internal static void Init()
        {
            MelonPreferences_Category category = MelonPreferences.CreateCategory("RunInBackground");

            _backgroundFpsCap = category.CreateEntry(
                "BackgroundFpsCap",
                DefaultBackgroundFpsCap,
                "Background FPS cap",
                "Frame rate limit applied while the game window is not focused. Set to 0 for no limit, which restores the behaviour of earlier versions.");
        }
    }
}

using System;
using System.Globalization;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace QuickSwap
{
    /// <summary>
    /// Decides which copy of the mod runs when more than one is loaded at once.
    /// Newest wins, so a version under development takes over from whatever is installed.
    /// </summary>
    /// <remarks>
    /// BepInEx's chainloader rejects a duplicate <c>BepInPlugin</c> GUID inside
    /// <c>plugins/</c>, and ScriptEngine has a duplicate check of its own — but it reads
    /// <c>Chainloader.PluginInfos</c> before the chainloader has finished filling it, and
    /// registers its own entry a frame later. Load early enough and the check passes, so a
    /// Thunderstore install of this mod and a hot-reload build of it go live together.
    ///
    /// Two instances is not a half-broken mod, it is two working mods: two <c>Update</c>
    /// loops, two Harmony instances, and — because ScriptEngine renames the assembly on
    /// load — two independent copies of every static field. Both record every
    /// <c>UseHotbarItem</c> call, including the other's, so each key press fires two swaps
    /// against a history the other copy already moved. What that looks like in game is one
    /// keypress equipping and unequipping, two equip animations, or the swap sticking on
    /// the wrong slot until the two histories happen to realign. It reads as a
    /// state-machine bug in <see cref="SwapController"/> rather than as a packaging
    /// accident, which is why this gets a guard and a loud log line rather than a note in
    /// the readme.
    /// </remarks>
    internal static class SingleInstance
    {
        /// <summary>
        /// Whether <paramref name="self"/> is the copy that should run. Shuts down any
        /// older copy it finds on the way, so the newest build always ends up in charge.
        /// </summary>
        internal static bool Claim(BaseUnityPlugin self)
        {
            Candidate mine = Candidate.For(self);

            // Not FindObjectsOfType: ScriptEngine hides its host object behind hide flags
            // that include DontSave, and that is exactly the copy this has to find.
            foreach (BaseUnityPlugin plugin in Resources.FindObjectsOfTypeAll<BaseUnityPlugin>())
            {
                if (plugin == null || ReferenceEquals(plugin, self))
                {
                    continue;
                }

                Candidate rival = Candidate.For(plugin);
                if (rival.Id != QuickSwapPlugin.ModGuid)
                {
                    continue;
                }

                if (rival.IsAtLeastAsNewAs(mine))
                {
                    QuickSwapPlugin.Log.LogWarning(
                        "Another copy of " + QuickSwapPlugin.ModName + " is already running (" + rival
                        + ") and is not older than this one (" + mine + "), so this copy is standing down. "
                        + "Two copies fire every swap key twice - uninstall one.");
                    return false;
                }

                QuickSwapPlugin.Log.LogWarning(
                    "Another copy of " + QuickSwapPlugin.ModName + " was already running (" + rival
                    + "); this one (" + mine + ") is newer, so the older copy is being shut down. "
                    + "Two copies fire every swap key twice - uninstall the old one to silence this.");

                // Its own OnDestroy unpatches its Harmony and clears its markers, which is
                // why this destroys the component rather than merely disabling it.
                UnityEngine.Object.Destroy(plugin);
            }

            return true;
        }

        /// <summary>One loaded copy of the mod, and enough of its identity to rank it.</summary>
        private struct Candidate
        {
            internal string Id;
            /// <summary>Qualified: an unqualified <c>Version</c> binds to Valheim's own
            /// global <c>Version</c> class, which the enclosing namespaces reach before
            /// any using directive is considered.</summary>
            internal System.Version Version;
            internal DateTime Built;
            internal string Where;

            internal static Candidate For(BaseUnityPlugin plugin)
            {
                Type type = plugin.GetType();

                // Not plugin.Info.Metadata: BaseUnityPlugin's constructor takes Info from
                // Chainloader.PluginInfos when the GUID is already registered there, so in
                // the very case this class exists for, one copy reports the other's
                // version. The attribute on the type is the only per-assembly answer.
                BepInPlugin metadata = MetadataHelper.GetMetadata(plugin);

                string location = type.Assembly.Location;

                return new Candidate
                {
                    Id = metadata?.GUID,
                    Version = metadata?.Version,
                    Built = ReadBuildTime(type.Assembly),

                    // ScriptEngine loads its assemblies from a byte array, which leaves
                    // Location empty — so an empty one names the hot-reload build.
                    Where = string.IsNullOrEmpty(location) ? "loaded from memory, so BepInEx/scripts" : location,
                };
            }

            /// <summary>
            /// Version first, then the build stamp. A dead tie goes to the copy already
            /// running, so two identical builds still settle on one of them.
            /// </summary>
            internal bool IsAtLeastAsNewAs(Candidate other)
            {
                int byVersion = CompareVersions(Version, other.Version);

                return byVersion != 0 ? byVersion > 0 : Built >= other.Built;
            }

            public override string ToString()
            {
                return (Version == null ? "unknown version" : "v" + Version)
                       + ", " + (Built == DateTime.MinValue
                           ? "build time unknown"
                           : "built " + Built.ToString(BuildStampFormat, CultureInfo.InvariantCulture))
                       + ", " + Where;
            }
        }

        /// <summary>Matches the stamp the GenerateBuildInfo target writes.</summary>
        private const string BuildStampFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// This assembly's build stamp, read reflectively so the same code can read another
        /// copy's. <see cref="DateTime.MinValue"/> for a build too old to carry one, which
        /// ranks it below every build that does.
        /// </summary>
        private static DateTime ReadBuildTime(Assembly assembly)
        {
            Type buildInfo = assembly.GetType("QuickSwap.BuildInfo", throwOnError: false);
            FieldInfo field = buildInfo?.GetField(
                "BuildTime", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            return DateTime.TryParseExact(
                field?.GetValue(null) as string, BuildStampFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime built)
                ? built
                : DateTime.MinValue;
        }

        /// <summary><see cref="BepInPlugin.Version"/> is null when the string would not parse.</summary>
        private static int CompareVersions(System.Version left, System.Version right)
        {
            if (left == null)
            {
                return right == null ? 0 : -1;
            }

            return right == null ? 1 : left.CompareTo(right);
        }
    }
}

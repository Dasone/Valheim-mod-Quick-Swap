using System.IO;
using System.Reflection;
using UnityEngine;

namespace QuickSwap
{
    /// <summary>
    /// The swap-arrows glyph shown on the anchored slot, embedded in the assembly so the
    /// mod stays a single DLL with no loose files to deploy. Drawn white on transparent by
    /// art/make_icon.py, which is also what draws the arrows on the Thunderstore icon —
    /// white so the Image component can tint it to the configured marker colour.
    /// </summary>
    internal static class MarkerSprite
    {
        private const string ResourceName = "QuickSwap.anchor-arrows.png";

        private static Sprite _sprite;
        private static Texture2D _texture;
        private static bool _failed;

        internal static Sprite Get()
        {
            if (_sprite != null || _failed)
            {
                return _sprite;
            }

            byte[] bytes = ReadResource();
            if (bytes == null)
            {
                _failed = true;
                QuickSwapPlugin.Log.LogError(
                    "Embedded resource '" + ResourceName + "' is missing; the anchored slot will show a plain marker.");
                return null;
            }

            _texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };

            if (!LoadImage(_texture, bytes))
            {
                Object.Destroy(_texture);
                _texture = null;
                _failed = true;
                QuickSwapPlugin.Log.LogError("Could not decode '" + ResourceName + "'.");
                return null;
            }

            _sprite = Sprite.Create(
                _texture,
                new Rect(0f, 0f, _texture.width, _texture.height),
                new Vector2(0.5f, 0.5f));

            return _sprite;
        }

        /// <summary>Frees the texture on unload; a hot reload would otherwise leak one per reload.</summary>
        internal static void Unload()
        {
            if (_sprite != null)
            {
                Object.Destroy(_sprite);
            }

            if (_texture != null)
            {
                Object.Destroy(_texture);
            }

            _sprite = null;
            _texture = null;
            _failed = false;
        }

        /// <summary>
        /// Calls <c>ImageConversion.LoadImage(Texture2D, byte[])</c> through reflection.
        /// </summary>
        /// <remarks>
        /// Unity 6 added ReadOnlySpan overloads of LoadImage, and the compiler has to resolve
        /// every overload in the set to pick one — which it cannot do on net462, where
        /// ReadOnlySpan does not exist and neither Valheim's mscorlib nor its netstandard
        /// facade supplies it. Naming the byte[] overload explicitly sidesteps the overload
        /// set entirely; that overload is present at runtime, which is where this binds.
        /// </remarks>
        private static bool LoadImage(Texture2D texture, byte[] bytes)
        {
            MethodInfo method = typeof(ImageConversion).GetMethod(
                "LoadImage",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(Texture2D), typeof(byte[]) },
                modifiers: null);

            if (method == null)
            {
                QuickSwapPlugin.Log.LogError("ImageConversion.LoadImage(Texture2D, byte[]) not found.");
                return false;
            }

            return (bool)method.Invoke(null, new object[] { texture, bytes });
        }

        private static byte[] ReadResource()
        {
            Assembly assembly = typeof(MarkerSprite).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(ResourceName))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var buffer = new MemoryStream())
                {
                    stream.CopyTo(buffer);
                    return buffer.ToArray();
                }
            }
        }
    }
}

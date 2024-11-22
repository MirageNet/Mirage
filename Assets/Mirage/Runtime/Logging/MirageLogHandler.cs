using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage.Logging
{
    /// <summary>
    /// Log handler that adds prefixes to logging
    /// </summary>
    public class MirageLogHandler : ILogHandler
    {
        private readonly Settings _settings;
        private readonly ILogHandler _inner;
        private readonly string _label;

        public MirageLogHandler(Settings settings, string fullTypeName = null, ILogHandler inner = null)
        {
            _inner = inner ?? Debug.unityLogger;
            _settings = settings;

            if (_settings.Label && !string.IsNullOrEmpty(fullTypeName))
            {
                var (name, _) = LogSettingsSO.LoggerSettings.GetNameAndNameSpaceFromFullname(fullTypeName);
                _label = $"[{name}]";

                if (_settings.ColoredLabel)
                {
                    _label = _settings.AllowColorToLabel(fullTypeName, _label);
                }
            }
            else
            {
                _label = null;
            }
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            _inner.LogException(exception, context);
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            // add label before frame nummber
            if (!string.IsNullOrEmpty(_label))
            {
                format = $"{_label} {format}";
            }

            format = AddTimePrefix(format);

            _inner.LogFormat(logType, context, format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string AddTimePrefix(string format)
        {
            string timePrefix;
            switch (_settings.TimePrefix)
            {
                default:
                    return format;
                case TimePrefix.FrameCount:
                    try
                    {
                        // need try/catch for unity function because unity can throw if called from side thread 
                        timePrefix = Time.frameCount.ToString();
                    }
                    catch
                    {
                        timePrefix = "0";
                    }
                    break;
                case TimePrefix.UnscaledTime:
                    timePrefix = Time.unscaledTime.ToString("0.000");
                    break;
                case TimePrefix.DateTimeMilliSeconds:
                    timePrefix = DateTime.Now.ToString("HH:mm:ss.fff");
                    break;
                case TimePrefix.DateTimeSeconds:
                    timePrefix = DateTime.Now.ToString("HH:mm:ss");
                    break;
            }

            return $"{timePrefix}: {format}";
        }

        public enum TimePrefix
        {
            None,
            FrameCount,
            UnscaledTime,
            DateTimeMilliSeconds,
            DateTimeSeconds,
        }
        [Serializable]
        public class Settings
        {
            public TimePrefix TimePrefix;
            public readonly bool ColoredLabel;
            public readonly bool Label;

            /// <summary>
            /// Used to change the colors of names
            /// <para>number is multiple by hash unchecked, so small changes to seed will cause large changes in result</para>
            /// <para>403 seems like a good starting seed, common class like NetworkServer and NetworkClient have different colors</para>
            /// </summary>
            public int ColorSeed = 403;
            public float ColorSaturation = 0.6f;
            public float ColorValue = 0.8f;

            public Settings(TimePrefix timePrefix, bool coloredLabel, bool label)
            {
                TimePrefix = timePrefix;
                ColoredLabel = coloredLabel;
                Label = label;
            }

            public string AllowColorToLabel(string fullname, string label)
            {
                var color = ColorFromName(fullname);
                var colorHex = ColorUtility.ToHtmlStringRGB(color);
                return $"<color=#{colorHex}>{label}</color>";
            }

            public Color ColorFromName(string fullName)
            {
                var hash = fullName.GetStableHashCode();
                if (ColorSeed != 0)
                    hash = unchecked(ColorSeed * hash);

                var hue = Mathf.Abs((float)hash / (float)int.MaxValue);
                return Color.HSVToRGB(hue, ColorSaturation, ColorValue);
            }
        }
    }
}

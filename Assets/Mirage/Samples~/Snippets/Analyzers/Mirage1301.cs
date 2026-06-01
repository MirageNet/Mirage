using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1301.Triggering
    {
        // CodeEmbed-Start: mirage1301-triggering
        public class TargetInfo
        {
            public int x;
            public int y;
        }

        [NetworkMessage]
        public struct FireMessage
        {
            // Warns: NetworkMessage field 'info' is a class type 'TargetInfo'.
            public TargetInfo info;
        }
        // CodeEmbed-End: mirage1301-triggering
    }

    namespace M1301.StructOption
    {
        // CodeEmbed-Start: mirage1301-struct-option
        public struct TargetInfo
        {
            public int x;
            public int y;
        }

        [NetworkMessage]
        public struct FireMessage
        {
            public TargetInfo info;
        }
        // CodeEmbed-End: mirage1301-struct-option
    }

    namespace M1301.ClassOption
    {
        // CodeEmbed-Start: mirage1301-class-option
        [WeaverSafeClass]
        public class TargetInfo
        {
            public int x;
            public int y;
        }
        // CodeEmbed-End: mirage1301-class-option
    }

    namespace M1301.SuppressOption
    {
        // CodeEmbed-Start: mirage1301-suppress-option
        [NetworkMessage]
        public struct FireMessage
        {
            [WeaverSafeClass]
            public TargetInfo info;
        }
        // CodeEmbed-End: mirage1301-suppress-option

        public class TargetInfo
        {
            public int x;
            public int y;
        }
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1304
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            // Traversal for MonoBehaviour checking is combined with Mirage1301's field/parameter checks
            // to optimize compilation start/symbol visitation overhead.
        }
    }
}

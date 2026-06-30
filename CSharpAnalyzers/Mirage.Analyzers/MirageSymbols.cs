using Microsoft.CodeAnalysis;

namespace Mirage.Analyzers
{
    public class MirageSymbols
    {
        // Attributes
        public INamedTypeSymbol? SyncVarAttribute { get; }
        public INamedTypeSymbol? ServerAttribute { get; }
        public INamedTypeSymbol? ClientAttribute { get; }
        public INamedTypeSymbol? HasAuthorityAttribute { get; }
        public INamedTypeSymbol? LocalPlayerAttribute { get; }
        public INamedTypeSymbol? ServerRpcAttribute { get; }
        public INamedTypeSymbol? ClientRpcAttribute { get; }
        public INamedTypeSymbol? NetworkMethodAttribute { get; }
        public INamedTypeSymbol? WeaverSafeClassAttribute { get; }
        public INamedTypeSymbol? NetworkMessageAttribute { get; }
        public INamedTypeSymbol? BitCountAttribute { get; }
        public INamedTypeSymbol? BitCountFromRangeAttribute { get; }
        public INamedTypeSymbol? VarIntAttribute { get; }
        public INamedTypeSymbol? VarIntBlocksAttribute { get; }
        public INamedTypeSymbol? FloatPackAttribute { get; }
        public INamedTypeSymbol? Vector2PackAttribute { get; }
        public INamedTypeSymbol? Vector3PackAttribute { get; }
        public INamedTypeSymbol? QuaternionPackAttribute { get; }
        public INamedTypeSymbol? RateLimitAttribute { get; }

        // Types
        public INamedTypeSymbol? NetworkBehaviour { get; }
        public INamedTypeSymbol? GameObject { get; }
        public INamedTypeSymbol? Transform { get; }
        public INamedTypeSymbol? MonoBehaviour { get; }
        public INamedTypeSymbol? NetworkIdentity { get; }
        public INamedTypeSymbol? NetworkServer { get; }
        public INamedTypeSymbol? NetworkClient { get; }
        public INamedTypeSymbol? ISyncObject { get; }
        public INamedTypeSymbol? NetworkWriter { get; }
        public INamedTypeSymbol? NetworkReader { get; }
        public INamedTypeSymbol? INetworkPlayer { get; }
        public INamedTypeSymbol? NetworkPlayer { get; }
        public INamedTypeSymbol? NetworkConnection { get; }
        public INamedTypeSymbol? IEnumerable { get; }
        public INamedTypeSymbol? UniTask { get; }
        public INamedTypeSymbol? SyncList { get; }
        public INamedTypeSymbol? SyncDictionary { get; }
        public INamedTypeSymbol? SyncIDictionary { get; }
        public INamedTypeSymbol? Vector2 { get; }
        public INamedTypeSymbol? Vector3 { get; }
        public INamedTypeSymbol? Quaternion { get; }

        public System.Collections.Generic.Dictionary<ITypeSymbol, IMethodSymbol> CustomWriters { get; } = new System.Collections.Generic.Dictionary<ITypeSymbol, IMethodSymbol>(SymbolEqualityComparer.Default);
        public System.Collections.Generic.Dictionary<ITypeSymbol, IMethodSymbol> CustomReaders { get; } = new System.Collections.Generic.Dictionary<ITypeSymbol, IMethodSymbol>(SymbolEqualityComparer.Default);
        public System.Collections.Generic.HashSet<ITypeSymbol> CustomSerializableTypes { get; } = new System.Collections.Generic.HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        public MirageSymbols(Compilation compilation)
        {
            // Resolve attributes
            SyncVarAttribute = compilation.GetTypeByMetadataName("Mirage.SyncVarAttribute");
            ServerAttribute = compilation.GetTypeByMetadataName("Mirage.ServerAttribute");
            ClientAttribute = compilation.GetTypeByMetadataName("Mirage.ClientAttribute");
            HasAuthorityAttribute = compilation.GetTypeByMetadataName("Mirage.HasAuthorityAttribute");
            LocalPlayerAttribute = compilation.GetTypeByMetadataName("Mirage.LocalPlayerAttribute");
            ServerRpcAttribute = compilation.GetTypeByMetadataName("Mirage.ServerRpcAttribute");
            ClientRpcAttribute = compilation.GetTypeByMetadataName("Mirage.ClientRpcAttribute");
            NetworkMethodAttribute = compilation.GetTypeByMetadataName("Mirage.NetworkMethodAttribute");
            WeaverSafeClassAttribute = compilation.GetTypeByMetadataName("Mirage.WeaverSafeClassAttribute");
            NetworkMessageAttribute = compilation.GetTypeByMetadataName("Mirage.NetworkMessageAttribute");
            BitCountAttribute = compilation.GetTypeByMetadataName("Mirage.Serialization.BitCountAttribute");
            BitCountFromRangeAttribute = compilation.GetTypeByMetadataName("Mirage.Serialization.BitCountFromRangeAttribute");
            VarIntAttribute = compilation.GetTypeByMetadataName("Mirage.Serialization.VarIntAttribute");
            VarIntBlocksAttribute = compilation.GetTypeByMetadataName("Mirage.Serialization.VarIntBlocksAttribute");
            FloatPackAttribute = compilation.GetTypeByMetadataName("Mirage.Serialization.FloatPackAttribute");
            Vector2PackAttribute = compilation.GetTypeByMetadataName("Mirage.Serialization.Vector2PackAttribute");
            Vector3PackAttribute = compilation.GetTypeByMetadataName("Mirage.Serialization.Vector3PackAttribute");
            QuaternionPackAttribute = compilation.GetTypeByMetadataName("Mirage.Serialization.QuaternionPackAttribute");
            RateLimitAttribute = compilation.GetTypeByMetadataName("Mirage.RateLimitAttribute");

            // Resolve types
            NetworkBehaviour = compilation.GetTypeByMetadataName("Mirage.NetworkBehaviour");
            GameObject = compilation.GetTypeByMetadataName("UnityEngine.GameObject");
            Transform = compilation.GetTypeByMetadataName("UnityEngine.Transform");
            NetworkIdentity = compilation.GetTypeByMetadataName("Mirage.NetworkIdentity");
            NetworkServer = compilation.GetTypeByMetadataName("Mirage.NetworkServer");
            NetworkClient = compilation.GetTypeByMetadataName("Mirage.NetworkClient");
            ISyncObject = compilation.GetTypeByMetadataName("Mirage.Collections.ISyncObject");
            NetworkWriter = compilation.GetTypeByMetadataName("Mirage.Serialization.NetworkWriter");
            NetworkReader = compilation.GetTypeByMetadataName("Mirage.Serialization.NetworkReader");
            INetworkPlayer = compilation.GetTypeByMetadataName("Mirage.INetworkPlayer");
            NetworkPlayer = compilation.GetTypeByMetadataName("Mirage.NetworkPlayer");
            NetworkConnection = compilation.GetTypeByMetadataName("Mirage.NetworkConnection");
            IEnumerable = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
            UniTask = compilation.GetTypeByMetadataName("Cysharp.Threading.Tasks.UniTask");
            SyncList = compilation.GetTypeByMetadataName("Mirage.Collections.SyncList`1");
            SyncDictionary = compilation.GetTypeByMetadataName("Mirage.Collections.SyncDictionary`2");
            SyncIDictionary = compilation.GetTypeByMetadataName("Mirage.Collections.SyncIDictionary`2");
            Vector2 = compilation.GetTypeByMetadataName("UnityEngine.Vector2");
            Vector3 = compilation.GetTypeByMetadataName("UnityEngine.Vector3");
            Quaternion = compilation.GetTypeByMetadataName("UnityEngine.Quaternion");
            MonoBehaviour = compilation.GetTypeByMetadataName("UnityEngine.MonoBehaviour");

            FindCustomSerializers(compilation);
        }

        private void FindCustomSerializers(Compilation compilation)
        {
            FindCustomSerializersInNamespace(compilation.GlobalNamespace);
        }

        private void FindCustomSerializersInNamespace(INamespaceSymbol ns)
        {
            foreach (var member in ns.GetMembers())
            {
                if (member is INamespaceSymbol nestedNs)
                {
                    FindCustomSerializersInNamespace(nestedNs);
                }
                else if (member is INamedTypeSymbol typeSymbol && typeSymbol.IsStatic)
                {
                    foreach (var method in typeSymbol.GetMembers().OfType<IMethodSymbol>())
                    {
                        if (method.IsStatic && method.IsExtensionMethod)
                        {
                            if (method.Parameters.Length == 2 && 
                                NetworkWriter != null && 
                                SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, NetworkWriter))
                            {
                                var type = method.Parameters[1].Type;
                                if (!CustomWriters.ContainsKey(type))
                                    CustomWriters.Add(type, method);
                                
                                CustomSerializableTypes.Add(type);
                            }
                            else if (method.Parameters.Length == 1 && 
                                     NetworkReader != null && 
                                     SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, NetworkReader))
                            {
                                var type = method.ReturnType;
                                if (!CustomReaders.ContainsKey(type))
                                    CustomReaders.Add(type, method);
                                
                                CustomSerializableTypes.Add(type);
                            }
                        }
                    }
                }
            }
        }

        public bool HasAttribute(ISymbol? symbol, INamedTypeSymbol? attributeSymbol)
        {
            if (symbol == null || attributeSymbol == null)
                return false;

            foreach (var attr in symbol.GetAttributes())
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol))
                    return true;

            return false;
        }

        public bool TryGetAttribute(ISymbol? symbol, INamedTypeSymbol? attributeSymbol, out AttributeData attributeData)
        {
            attributeData = null!;
            if (symbol == null || attributeSymbol == null)
                return false;

            foreach (var attr in symbol.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol))
                {
                    attributeData = attr;
                    return true;
                }
            }

            return false;
        }

        public bool IsOrInherits(ITypeSymbol? type, INamedTypeSymbol? targetType)
        {
            if (type == null || targetType == null)
                return false;

            var current = type;
            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, targetType))
                    return true;

                current = current.BaseType;
            }

            return false;
        }

        public bool Implements(ITypeSymbol? type, INamedTypeSymbol? interfaceType)
        {
            if (type == null || interfaceType == null)
                return false;

            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, interfaceType))
                return true;

            foreach (var iface in type.AllInterfaces)
                if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, interfaceType))
                    return true;

            return false;
        }

        public bool HasCompressionAttribute(ISymbol symbol, ITypeSymbol type)
        {
            if (symbol == null || type == null)
                return false;

            if (type.SpecialType == SpecialType.System_Int32 ||
                type.SpecialType == SpecialType.System_UInt32 ||
                type.SpecialType == SpecialType.System_Int64 ||
                type.SpecialType == SpecialType.System_UInt64 ||
                type.SpecialType == SpecialType.System_String ||
                type.TypeKind == TypeKind.Array ||
                Implements(type, IEnumerable))
            {
                return HasAttribute(symbol, BitCountAttribute) ||
                       HasAttribute(symbol, BitCountFromRangeAttribute) ||
                       HasAttribute(symbol, VarIntAttribute) ||
                       HasAttribute(symbol, VarIntBlocksAttribute);
            }

            if (type.SpecialType == SpecialType.System_Single ||
                type.SpecialType == SpecialType.System_Double)
                return HasAttribute(symbol, FloatPackAttribute);

            if (IsOrInherits(type, Vector2))
                return HasAttribute(symbol, Vector2PackAttribute);

            if (IsOrInherits(type, Vector3))
                return HasAttribute(symbol, Vector3PackAttribute);

            if (IsOrInherits(type, Quaternion))
                return HasAttribute(symbol, QuaternionPackAttribute);

            return false;
        }

        public bool HasSynchronizedState(INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IFieldSymbol field)
                    if (HasAttribute(field, SyncVarAttribute) || Implements(field.Type, ISyncObject))
                        return true;

                if (member is IPropertySymbol prop)
                    if (HasAttribute(prop, SyncVarAttribute) || Implements(prop.Type, ISyncObject))
                        return true;
            }

            return false;
        }

        public bool IsNetworkPlayerOrConnection(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            return IsOrInherits(typeSymbol, INetworkPlayer) ||
                   IsOrInherits(typeSymbol, NetworkPlayer) ||
                   IsOrInherits(typeSymbol, NetworkConnection) ||
                   Implements(typeSymbol, INetworkPlayer);
        }

        public static bool TryGetNamedArgument<T>(AttributeData attributeData, string name, out T value)
        {
            value = default!;
            if (attributeData == null)
                return false;

            foreach (var arg in attributeData.NamedArguments)
            {
                if (arg.Key == name)
                {
                    if (arg.Value.Value is T val)
                    {
                        value = val;
                        return true;
                    }
                    return false;
                }
            }

            return false;
        }
    }
}

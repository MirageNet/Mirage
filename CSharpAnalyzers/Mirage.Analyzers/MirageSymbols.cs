using Microsoft.CodeAnalysis;

namespace Mirage.Analyzers
{
    public static class MirageAttributes
    {
        public class AttributeInfo
        {
            public string FullName { get; }
            public string ShortName { get; }

            public AttributeInfo(string fullyQualifiedName)
            {
                FullName = fullyQualifiedName;
                var lastDot = fullyQualifiedName.LastIndexOf('.');
                ShortName = lastDot >= 0 ? fullyQualifiedName.Substring(lastDot + 1) : fullyQualifiedName;
            }

            public bool Has(ISymbol symbol)
            {
                if (symbol == null)
                    return false;

                foreach (var attr in symbol.GetAttributes())
                    if (attr.AttributeClass != null && attr.AttributeClass.Name == ShortName && attr.AttributeClass.ToDisplayString() == FullName)
                        return true;

                return false;
            }

            public bool TryGet(ISymbol symbol, out AttributeData attributeData)
            {
                attributeData = null!;
                if (symbol == null)
                    return false;

                foreach (var attr in symbol.GetAttributes())
                    if (attr.AttributeClass != null && attr.AttributeClass.Name == ShortName && attr.AttributeClass.ToDisplayString() == FullName)
                    {
                        attributeData = attr;
                        return true;
                    }

                return false;
            }

            public bool TryGetNamedArgument<T>(AttributeData attributeData, string name, out T value)
            {
                value = default!;
                if (attributeData == null)
                    return false;

                foreach (var arg in attributeData.NamedArguments)
                    if (arg.Key == name)
                    {
                        if (arg.Value.Value is T val)
                        {
                            value = val;
                            return true;
                        }
                        return false;
                    }

                return false;
            }

            public bool Matches(INamedTypeSymbol? attributeClass)
            {
                if (attributeClass == null)
                    return false;

                return attributeClass.Name == ShortName && attributeClass.ToDisplayString() == FullName;
            }
        }

        public static readonly AttributeInfo SyncVar = new AttributeInfo("Mirage.SyncVarAttribute");
        public static readonly AttributeInfo Server = new AttributeInfo("Mirage.ServerAttribute");
        public static readonly AttributeInfo Client = new AttributeInfo("Mirage.ClientAttribute");
        public static readonly AttributeInfo HasAuthority = new AttributeInfo("Mirage.HasAuthorityAttribute");
        public static readonly AttributeInfo LocalPlayer = new AttributeInfo("Mirage.LocalPlayerAttribute");
        public static readonly AttributeInfo ServerRpc = new AttributeInfo("Mirage.ServerRpcAttribute");
        public static readonly AttributeInfo ClientRpc = new AttributeInfo("Mirage.ClientRpcAttribute");
        public static readonly AttributeInfo NetworkMethod = new AttributeInfo("Mirage.NetworkMethodAttribute");
        public static readonly AttributeInfo WeaverSafeClass = new AttributeInfo("Mirage.WeaverSafeClassAttribute");
        public static readonly AttributeInfo NetworkMessage = new AttributeInfo("Mirage.NetworkMessageAttribute");
        public static readonly AttributeInfo BitCount = new AttributeInfo("Mirage.Serialization.BitCountAttribute");
        public static readonly AttributeInfo BitCountFromRange = new AttributeInfo("Mirage.Serialization.BitCountFromRangeAttribute");
        public static readonly AttributeInfo VarInt = new AttributeInfo("Mirage.Serialization.VarIntAttribute");
        public static readonly AttributeInfo VarIntBlocks = new AttributeInfo("Mirage.Serialization.VarIntBlocksAttribute");
        public static readonly AttributeInfo FloatPack = new AttributeInfo("Mirage.Serialization.FloatPackAttribute");
        public static readonly AttributeInfo Vector2Pack = new AttributeInfo("Mirage.Serialization.Vector2PackAttribute");
        public static readonly AttributeInfo Vector3Pack = new AttributeInfo("Mirage.Serialization.Vector3PackAttribute");
        public static readonly AttributeInfo QuaternionPack = new AttributeInfo("Mirage.Serialization.QuaternionPackAttribute");
        public static readonly AttributeInfo RateLimit = new AttributeInfo("Mirage.RateLimitAttribute");

        public static readonly AttributeInfo[] NetworkAttributes = new[]
        {
            SyncVar,
            Server,
            Client,
            HasAuthority,
            LocalPlayer,
            ServerRpc,
            ClientRpc,
            NetworkMethod
        };

        public static bool HasCompressionAttribute(ISymbol symbol, ITypeSymbol type)
        {
            if (symbol == null || type == null)
                return false;

            if (type.SpecialType == SpecialType.System_Int32 ||
                type.SpecialType == SpecialType.System_UInt32 ||
                type.SpecialType == SpecialType.System_Int64 ||
                type.SpecialType == SpecialType.System_UInt64 ||
                type.SpecialType == SpecialType.System_String ||
                type.TypeKind == TypeKind.Array ||
                MirageTypes.IEnumerable.Implements(type))
                return BitCount.Has(symbol) ||
                       BitCountFromRange.Has(symbol) ||
                       VarInt.Has(symbol) ||
                       VarIntBlocks.Has(symbol);

            if (type.SpecialType == SpecialType.System_Single ||
                type.SpecialType == SpecialType.System_Double)
                return FloatPack.Has(symbol);

            if (MirageTypes.Vector2.Is(type))
                return Vector2Pack.Has(symbol);

            if (MirageTypes.Vector3.Is(type))
                return Vector3Pack.Has(symbol);

            if (MirageTypes.Quaternion.Is(type))
                return QuaternionPack.Has(symbol);

            return false;
        }
    }

    public static class MirageTypes
    {
        public class TypeInfo
        {
            public string FullName { get; }
            public string ShortName { get; }

            public TypeInfo(string fullyQualifiedName)
            {
                FullName = fullyQualifiedName;
                var lastDot = fullyQualifiedName.LastIndexOf('.');
                ShortName = lastDot >= 0 ? fullyQualifiedName.Substring(lastDot + 1) : fullyQualifiedName;
            }

            public bool Is(ITypeSymbol typeSymbol)
            {
                if (typeSymbol == null)
                    return false;

                var displayString = typeSymbol.OriginalDefinition.ToDisplayString();
                var index = displayString.IndexOf('<');
                if (index >= 0)
                    displayString = displayString.Substring(0, index);

                return typeSymbol.Name == ShortName && displayString == FullName;
            }

            public bool IsOrInherits(ITypeSymbol typeSymbol)
            {
                if (typeSymbol == null)
                    return false;

                var current = typeSymbol;
                while (current != null)
                {
                    if (Is(current))
                        return true;

                    current = current.BaseType;
                }
                return false;
            }

            public bool Implements(ITypeSymbol typeSymbol)
            {
                if (typeSymbol == null)
                    return false;

                if (typeSymbol is INamedTypeSymbol namedType && namedType.TypeKind == TypeKind.Interface && Is(namedType))
                    return true;

                foreach (var iface in typeSymbol.AllInterfaces)
                    if (Is(iface))
                        return true;

                return false;
            }
        }

        public static readonly TypeInfo NetworkBehaviour = new TypeInfo("Mirage.NetworkBehaviour");
        public static readonly TypeInfo GameObject = new TypeInfo("UnityEngine.GameObject");
        public static readonly TypeInfo NetworkIdentity = new TypeInfo("Mirage.NetworkIdentity");
        public static readonly TypeInfo ISyncObject = new TypeInfo("Mirage.Collections.ISyncObject");
        public static readonly TypeInfo NetworkWriter = new TypeInfo("Mirage.Serialization.NetworkWriter");
        public static readonly TypeInfo NetworkReader = new TypeInfo("Mirage.Serialization.NetworkReader");
        public static readonly TypeInfo INetworkPlayer = new TypeInfo("Mirage.INetworkPlayer");
        public static readonly TypeInfo NetworkPlayer = new TypeInfo("Mirage.NetworkPlayer");
        public static readonly TypeInfo NetworkConnection = new TypeInfo("Mirage.NetworkConnection");
        public static readonly TypeInfo IEnumerable = new TypeInfo("System.Collections.IEnumerable");
        public static readonly TypeInfo UniTask = new TypeInfo("Cysharp.Threading.Tasks.UniTask");
        public static readonly TypeInfo SyncList = new TypeInfo("Mirage.Collections.SyncList");
        public static readonly TypeInfo SyncDictionary = new TypeInfo("Mirage.Collections.SyncDictionary");
        public static readonly TypeInfo SyncIDictionary = new TypeInfo("Mirage.Collections.SyncIDictionary");
        public static readonly TypeInfo Vector2 = new TypeInfo("UnityEngine.Vector2");
        public static readonly TypeInfo Vector3 = new TypeInfo("UnityEngine.Vector3");
        public static readonly TypeInfo Quaternion = new TypeInfo("UnityEngine.Quaternion");

        public static bool HasSynchronizedState(INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IFieldSymbol field)
                    if (MirageAttributes.SyncVar.Has(field) || MirageTypes.ISyncObject.Implements(field.Type))
                        return true;

                if (member is IPropertySymbol prop)
                    if (MirageAttributes.SyncVar.Has(prop) || MirageTypes.ISyncObject.Implements(prop.Type))
                        return true;
            }

            return false;
        }

        public static bool IsNetworkPlayerOrConnection(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            return INetworkPlayer.IsOrInherits(typeSymbol) ||
                   NetworkPlayer.IsOrInherits(typeSymbol) ||
                   NetworkConnection.IsOrInherits(typeSymbol) ||
                   INetworkPlayer.Implements(typeSymbol);
        }
    }
}

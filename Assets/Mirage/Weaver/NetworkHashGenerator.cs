using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mirage.CodeGen;
using Mono.Cecil;

namespace Mirage.Weaver
{
    public class NetworkHashGenerator
    {
        private readonly Dictionary<string, List<string>> _rpcs = new Dictionary<string, List<string>> ();
        private readonly Dictionary<string, List<string>> _syncVars = new Dictionary<string, List<string>> ();
        private readonly Dictionary<string, List<string>> _syncObjects = new Dictionary<string, List<string>> ();
        private readonly List<string> _messages = new List<string> ();
        private readonly Dictionary<TypeReference, MethodReference> _readers = new Dictionary<TypeReference, MethodReference> (new TypeReferenceComparer ());
        private readonly Dictionary<TypeReference, MethodReference> _writers = new Dictionary<TypeReference, MethodReference> (new TypeReferenceComparer ());

        private string GetMethodSignature(MethodDefinition md)
        {
            return $"{md.ReturnType.FullName} {md.Name}({string.Join (", ", md.Parameters.Select (p => $"{p.ParameterType.FullName} {p.Name}"))})";
        }

        private string GetFieldSignature(FieldDefinition fd)
        {
            return $"{fd.FieldType.FullName} {fd.Name}";
        }

        private string GetTypeSignature(TypeDefinition td)
        {
            var fields = td.Fields.Where (f => !f.IsStatic && f.IsPublic).OrderBy (f => f.Name);
            return $"{td.FullName} {{ {string.Join (", ", fields.Select (f => $"{f.FieldType.FullName} {f.Name}"))} }}";
        }

        public void AddRpc(MethodDefinition md)
        {
            var behaviour = md.DeclaringType.FullName;
            if (!_rpcs.ContainsKey (behaviour))
                _rpcs[behaviour] = new List<string> ();
            _rpcs[behaviour].Add (GetMethodSignature (md));
        }

        public void AddMessage(TypeDefinition td)
        {
            _messages.Add (GetTypeSignature (td));
        }

        public void AddSyncObject(FieldDefinition fd)
        {
            var behaviour = fd.DeclaringType.FullName;
            if (!_syncObjects.ContainsKey (behaviour))
                _syncObjects[behaviour] = new List<string> ();
            _syncObjects[behaviour].Add (GetFieldSignature (fd));
        }

        public void AddSyncVar(FieldDefinition fd)
        {
            var behaviour = fd.DeclaringType.FullName;
            if (!_syncVars.ContainsKey (behaviour))
                _syncVars[behaviour] = new List<string> ();
            _syncVars[behaviour].Add (GetFieldSignature (fd));
        }

        public void AddReader(TypeReference forType, MethodReference read) => _readers[forType] = read;
        public void AddWriter(TypeReference forType, MethodReference write) => _writers[forType] = write;

        private List<string> AllSignatures ()
        {
            var allSignatures = new List<string> ();

            foreach (var kvp in _rpcs.OrderBy (x => x.Key))
            {
                allSignatures.Add ($"\nRPCs for {kvp.Key}:");
                foreach (var rpc in kvp.Value.OrderBy (x => x))
                {
                    allSignatures.Add ($"  {rpc}");
                }
            }

            foreach (var kvp in _syncVars.OrderBy (x => x.Key))
            {
                allSignatures.Add ($"\nSyncVars for {kvp.Key}:");
                foreach (var sv in kvp.Value.OrderBy (x => x))
                {
                    allSignatures.Add ($"  {sv}");
                }
            }
            
            foreach (var kvp in _syncObjects.OrderBy (x => x.Key))
            {
                allSignatures.Add ($"\nSyncObjects for {kvp.Key}:");
                foreach (var so in kvp.Value.OrderBy (x => x))
                {
                    allSignatures.Add ($"  {so}");
                }
            }

            allSignatures.Add ("\nMessages:");
            foreach (var msg in _messages.OrderBy (x => x))
            {
                allSignatures.Add ($"  {msg}");
            }

            var allTypes = new HashSet<TypeReference> (new TypeReferenceComparer ());
            allTypes.UnionWith (_readers.Keys);
            allTypes.UnionWith (_writers.Keys);

            allSignatures.Add ("\nSerializers:");
            foreach (var type in allTypes.OrderBy (x => x.FullName))
            {
                _writers.TryGetValue (type, out var writer);
                _readers.TryGetValue (type, out var reader);
                allSignatures.Add ($"  {type.FullName} => (writer:{writer?.Name}, reader:{reader?.Name})");
            }
            
            return allSignatures;
        }

        public string GetFormattedText ()
        {
            return string.Join ("\n", AllSignatures ());
        }

        public int GenerateHash ()
        {
            var signatures = AllSignatures ();
            if (signatures.Count == 0)
                return 0;

            var fullString = string.Join ("\n", signatures);

            using (var md5 = MD5.Create ())
            {
                var hash = md5.ComputeHash (Encoding.UTF8.GetBytes (fullString));
                return BitConverter.ToInt32 (hash, 0);
            }
        }
    }
}
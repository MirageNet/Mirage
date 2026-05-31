using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace Mirage.CodeGen
{
    public class WeaverLogger : IWeaverLogger
    {
        private readonly List<DiagnosticMessage> _diagnostics = new List<DiagnosticMessage>();

        // Create Copy so private list can't be altered
        public List<DiagnosticMessage> GetDiagnostics() => _diagnostics.ToList();

        public bool EnableTrace { get; }

        public WeaverLogger(bool enableTrace)
        {
            EnableTrace = enableTrace;
        }

        public void Error(string message)
        {
            AddMessage(message, null, DiagnosticType.Error);
        }

        private string FormatMember(MemberReference mr)
        {
            if (mr == null)
                return string.Empty;

            if (mr is PropertyReference pr)
                return $"{pr.PropertyType.FullName} {pr.DeclaringType.FullName}::{pr.Name}";

            return mr.ToString();
        }

        public void Error(string message, MemberReference mr)
        {
            Error($"{message} (at {FormatMember(mr)})");
        }

        public void Error(string message, MemberReference mr, SequencePoint sequencePoint)
        {
            AddMessage($"{message} (at {FormatMember(mr)})", sequencePoint, DiagnosticType.Error);
        }

        public void Error(string message, MethodDefinition md)
        {
            Error(message, md, md.DebugInformation.SequencePoints.FirstOrDefault());
        }


        public void Warning(string message)
        {
            AddMessage($"{message}", null, DiagnosticType.Warning);
        }

        public void Warning(string message, MemberReference mr)
        {
            Warning($"{message} (at {FormatMember(mr)})");
        }

        public void Warning(string message, MemberReference mr, SequencePoint sequencePoint)
        {
            AddMessage($"{message} (at {FormatMember(mr)})", sequencePoint, DiagnosticType.Warning);
        }

        public void Warning(string message, MethodDefinition md)
        {
            Warning(message, md, md.DebugInformation.SequencePoints.FirstOrDefault());
        }


        private void AddMessage(string message, SequencePoint sequencePoint, DiagnosticType diagnosticType)
        {
            _diagnostics.Add(new DiagnosticMessage
            {
                DiagnosticType = diagnosticType,
                File = sequencePoint?.Document.Url.Replace($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}", ""),
                Line = sequencePoint?.StartLine ?? 0,
                Column = sequencePoint?.StartColumn ?? 0,
                MessageData = message
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using UnityEngine;

namespace Mirror.Weaver
{
    public class Logger : IWeaverLogger
    {
        public readonly List<DiagnosticMessage> Diagnostics = new List<DiagnosticMessage>();

        public void Error(string msg)
        {
            Debug.LogError(msg);
        }

        public void Error(string message, MemberReference mr)
        {
            Error($"{message} (at {mr})");
        }

        public void Error(string message, MethodDefinition md)
        {
            Error(message, md.DebugInformation.SequencePoints.FirstOrDefault());
        }

        public void Warning(string message, MemberReference mr)
        {
            Warning($"{message} (at {mr})");
        }

        public void Warning(string msg)
        {
            Debug.LogWarning(msg);
        }

        public void Error(string message, SequencePoint sequencePoint)
        {
            Diagnostics.Add(new DiagnosticMessage
            {
                DiagnosticType = DiagnosticType.Error,
                File = sequencePoint?.Document.Url.Replace($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}", ""),
                Line = sequencePoint?.StartLine ?? 0,
                Column = sequencePoint?.StartColumn ?? 0,
                MessageData = $" - {message}"
            });
        }
    }
}

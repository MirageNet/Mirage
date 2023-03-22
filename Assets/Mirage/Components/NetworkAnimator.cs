using System;
using System.Linq;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirage
{
    /// <summary>
    /// A component to synchronize animation states for networked objects.
    /// </summary>
    /// <remarks>
    /// <para>The animation of game objects can be networked by this component. There are two models of authority for networked movement:</para>
    /// <para>If the object has authority on the client, then it should be animated locally on the owning client. The animation state information will be sent from the owning client to the server, then broadcast to all of the other clients. This is common for player objects.</para>
    /// <para>If the object has authority on the server, then it should be animated on the server and state information will be sent to all clients. This is common for objects not related to a specific client, such as an enemy unit.</para>
    /// <para>The NetworkAnimator synchronizes all animation parameters of the selected Animator. It does not automatically synchronize triggers. The function SetTrigger can by used by an object with authority to fire an animation trigger on other clients.</para>
    /// </remarks>
    [AddComponentMenu("Network/NetworkAnimator")]
    [RequireComponent(typeof(NetworkIdentity))]
    [HelpURL("https://miragenet.github.io/Mirage/docs/components/network-animator")]
    [Obsolete("NetworkAnimator is not optimzied so should not used in production.")]
    public class NetworkAnimator : NetworkBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkAnimator));

        [Header("Authority")]
        [FormerlySerializedAs("clientAuthority")]
        [Tooltip("Set to true if animations come from owner client,  set to false if animations always come from server")]
        public bool ClientAuthority;

        /// <summary>
        /// The animator component to synchronize.
        /// </summary>
        [FormerlySerializedAs("m_Animator")]
        [FormerlySerializedAs("animator")]
        [Header("Animator")]
        [Tooltip("Animator that will have parameters synchronized")]
        public Animator Animator;

        // Note: not an object[] array because otherwise initialization is real annoying
        private int[] lastIntParameters;
        private float[] lastFloatParameters;
        private bool[] lastBoolParameters;
        private AnimatorControllerParameter[] parameters;

        // multiple layers
        private int[] animationHash;
        private int[] transitionHash;
        private float[] layerWeight;
        [System.Obsolete("Use _nextSyncTime instead", true)]
        private float nextSendTime;

        private bool SendMessagesAllowed
        {
            get
            {
                if (IsServer)
                {
                    if (!ClientAuthority)
                        return true;

                    // This is a special case where we have client authority but we have not assigned the client who has
                    // authority over it, no animator data will be sent over the network by the server.
                    //
                    // So we check here for a Owner and if it is null we will
                    // let the server send animation data until we receive an owner.
                    if (Identity != null && Identity.Owner == null)
                        return true;
                }

                return HasAuthority && ClientAuthority;
            }
        }

        private void Awake()
        {
            // store the animator parameters in a variable - the "Animator.parameters" getter allocates
            // a new parameter array every time it is accessed so we should avoid doing it in a loop
            parameters = Animator.parameters
                .Where(par => !Animator.IsParameterControlledByCurve(par.nameHash))
                .ToArray();
            lastIntParameters = new int[parameters.Length];
            lastFloatParameters = new float[parameters.Length];
            lastBoolParameters = new bool[parameters.Length];

            animationHash = new int[Animator.layerCount];
            transitionHash = new int[Animator.layerCount];
            layerWeight = new float[Animator.layerCount];
        }

        private void FixedUpdate()
        {
            if (!SendMessagesAllowed)
                return;

            if (!Animator.enabled)
                return;

            CheckSendRate();

            for (var i = 0; i < Animator.layerCount; i++)
            {
                if (!CheckAnimStateChanged(out var stateHash, out var normalizedTime, i))
                {
                    continue;
                }

                using (var writer = NetworkWriterPool.GetWriter())
                {
                    WriteParameters(writer);
                    SendAnimationMessage(stateHash, normalizedTime, i, layerWeight[i], writer.ToArraySegment());
                }
            }
        }

        private bool CheckAnimStateChanged(out int stateHash, out float normalizedTime, int layerId)
        {
            var change = false;
            stateHash = 0;
            normalizedTime = 0;

            var lw = Animator.GetLayerWeight(layerId);
            if (Mathf.Abs(lw - layerWeight[layerId]) > 0.001f)
            {
                layerWeight[layerId] = lw;
                change = true;
            }

            if (Animator.IsInTransition(layerId))
            {
                var tt = Animator.GetAnimatorTransitionInfo(layerId);
                if (tt.fullPathHash != transitionHash[layerId])
                {
                    // first time in this transition
                    transitionHash[layerId] = tt.fullPathHash;
                    animationHash[layerId] = 0;
                    return true;
                }
                return change;
            }

            var st = Animator.GetCurrentAnimatorStateInfo(layerId);
            if (st.fullPathHash != animationHash[layerId])
            {
                // first time in this animation state
                if (animationHash[layerId] != 0)
                {
                    // came from another animation directly - from Play()
                    stateHash = st.fullPathHash;
                    normalizedTime = st.normalizedTime;
                }
                transitionHash[layerId] = 0;
                animationHash[layerId] = st.fullPathHash;
                return true;
            }
            return change;
        }

        private void CheckSendRate()
        {
            if (!SendMessagesAllowed)
                return;

            var now = Time.time;
            if (!TimeToSync(now))
                return;

            SyncSettings.UpdateTime(ref _nextSyncTime, now);

            using (var writer = NetworkWriterPool.GetWriter())
            {
                if (WriteParameters(writer))
                    SendAnimationParametersMessage(writer.ToArraySegment());
            }
        }

        private void SendAnimationMessage(int stateHash, float normalizedTime, int layerId, float weight, ArraySegment<byte> parameters)
        {
            if (IsServer)
            {
                RpcOnAnimationClientMessage(stateHash, normalizedTime, layerId, weight, parameters);
            }
            else if (Client.Player != null)
            {
                CmdOnAnimationServerMessage(stateHash, normalizedTime, layerId, weight, parameters);
            }
        }

        private void SendAnimationParametersMessage(ArraySegment<byte> parameters)
        {
            if (IsServer)
            {
                RpcOnAnimationParametersClientMessage(parameters);
            }
            else if (Client.Player != null)
            {
                CmdOnAnimationParametersServerMessage(parameters);
            }
        }

        private void HandleAnimMsg(int stateHash, float normalizedTime, int layerId, float weight, NetworkReader reader)
        {
            if (HasAuthority && ClientAuthority)
                return;

            // usually transitions will be triggered by parameters, if not, play anims directly.
            // NOTE: this plays "animations", not transitions, so any transitions will be skipped.
            // NOTE: there is no API to play a transition(?)
            if (stateHash != 0 && Animator.enabled)
            {
                Animator.Play(stateHash, layerId, normalizedTime);
            }

            Animator.SetLayerWeight(layerId, weight);

            ReadParameters(reader);
        }

        private void HandleAnimParamsMsg(NetworkReader reader)
        {
            if (HasAuthority && ClientAuthority)
                return;

            ReadParameters(reader);
        }

        private void HandleAnimTriggerMsg(int hash)
        {
            if (Animator.enabled)
                Animator.SetTrigger(hash);
        }

        private void HandleAnimResetTriggerMsg(int hash)
        {
            if (Animator.enabled)
                Animator.ResetTrigger(hash);
        }

        private ulong NextDirtyBits()
        {
            ulong dirtyBits = 0;
            for (var i = 0; i < parameters.Length; i++)
            {
                var par = parameters[i];
                var changed = false;
                switch (par.type)
                {
                    case AnimatorControllerParameterType.Int:
                        {
                            var newIntValue = Animator.GetInteger(par.nameHash);
                            changed = newIntValue != lastIntParameters[i];
                            lastIntParameters[i] = newIntValue;
                            break;
                        }

                    case AnimatorControllerParameterType.Float:
                        {
                            var newFloatValue = Animator.GetFloat(par.nameHash);
                            changed = Mathf.Abs(newFloatValue - lastFloatParameters[i]) > 0.001f;
                            // only set lastValue if it was changed, otherwise value could slowly drift within the 0.001f limit each frame
                            if (changed)
                                lastFloatParameters[i] = newFloatValue;
                            break;
                        }

                    case AnimatorControllerParameterType.Bool:
                        {
                            var newBoolValue = Animator.GetBool(par.nameHash);
                            changed = newBoolValue != lastBoolParameters[i];
                            lastBoolParameters[i] = newBoolValue;
                            break;
                        }
                }
                if (changed)
                {
                    dirtyBits |= 1ul << i;
                }
            }
            return dirtyBits;
        }

        private bool WriteParameters(NetworkWriter writer, bool forceAll = false)
        {
            var dirtyBits = forceAll ? (~0ul) : NextDirtyBits();
            writer.WritePackedUInt64(dirtyBits);
            for (var i = 0; i < parameters.Length; i++)
            {
                if ((dirtyBits & (1ul << i)) == 0)
                    continue;

                var par = parameters[i];
                if (par.type == AnimatorControllerParameterType.Int)
                {
                    var newIntValue = Animator.GetInteger(par.nameHash);
                    writer.WritePackedInt32(newIntValue);
                }
                else if (par.type == AnimatorControllerParameterType.Float)
                {
                    var newFloatValue = Animator.GetFloat(par.nameHash);
                    writer.WriteSingle(newFloatValue);
                }
                else if (par.type == AnimatorControllerParameterType.Bool)
                {
                    var newBoolValue = Animator.GetBool(par.nameHash);
                    writer.WriteBoolean(newBoolValue);
                }
            }
            return dirtyBits != 0;
        }

        private void ReadParameters(NetworkReader reader)
        {
            // need to read values from NetworkReader even if animator is disabled

            var dirtyBits = reader.ReadPackedUInt64();
            for (var i = 0; i < parameters.Length; i++)
            {
                if ((dirtyBits & (1ul << i)) == 0)
                    continue;

                var par = parameters[i];
                switch (par.type)
                {
                    case AnimatorControllerParameterType.Int:
                        {
                            var newIntValue = reader.ReadPackedInt32();
                            SetInteger(par, newIntValue);
                            break;
                        }

                    case AnimatorControllerParameterType.Float:
                        {
                            var newFloatValue = reader.ReadSingle();
                            SetFloat(par, newFloatValue);
                            break;
                        }

                    case AnimatorControllerParameterType.Bool:
                        {
                            var newBoolValue = reader.ReadBoolean();
                            SetBool(par, newBoolValue);
                            break;
                        }
                }
            }
        }

        private void SetBool(AnimatorControllerParameter par, bool newBoolValue)
        {
            if (Animator.enabled)
                Animator.SetBool(par.nameHash, newBoolValue);
        }

        private void SetFloat(AnimatorControllerParameter par, float newFloatValue)
        {
            if (Animator.enabled)
                Animator.SetFloat(par.nameHash, newFloatValue);
        }

        private void SetInteger(AnimatorControllerParameter par, int newIntValue)
        {
            if (Animator.enabled)
                Animator.SetInteger(par.nameHash, newIntValue);
        }

        /// <summary>
        /// Custom Serialization
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="initialState"></param>
        /// <returns></returns>
        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                for (var i = 0; i < Animator.layerCount; i++)
                {
                    if (Animator.IsInTransition(i))
                    {
                        var st = Animator.GetNextAnimatorStateInfo(i);
                        writer.WriteInt32(st.fullPathHash);
                        writer.WriteSingle(st.normalizedTime);
                    }
                    else
                    {
                        var st = Animator.GetCurrentAnimatorStateInfo(i);
                        writer.WriteInt32(st.fullPathHash);
                        writer.WriteSingle(st.normalizedTime);
                    }
                    writer.WriteSingle(Animator.GetLayerWeight(i));
                }
                WriteParameters(writer, initialState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Custom Deserialization
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="initialState"></param>
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                for (var i = 0; i < Animator.layerCount; i++)
                {
                    var stateHash = reader.ReadInt32();
                    var normalizedTime = reader.ReadSingle();
                    Animator.SetLayerWeight(i, reader.ReadSingle());
                    Animator.Play(stateHash, i, normalizedTime);
                }

                ReadParameters(reader);
            }
        }

        /// <summary>
        /// Causes an animation trigger to be invoked for a networked object.
        /// <para>If local authority is set, and this is called from the client, then the trigger will be invoked on the server and all clients. If not, then this is called on the server, and the trigger will be called on all clients.</para>
        /// </summary>
        /// <param name="triggerName">Name of trigger.</param>
        public void SetTrigger(string triggerName)
        {
            SetTrigger(Animator.StringToHash(triggerName));
        }

        /// <summary>
        /// Causes an animation trigger to be invoked for a networked object.
        /// </summary>
        /// <param name="hash">Hash id of trigger (from the Animator).</param>
        public void SetTrigger(int hash)
        {
            if (ClientAuthority)
            {
                if (!IsClient)
                {
                    logger.LogWarning("Tried to set animation in the server for a client-controlled animator");
                    return;
                }

                if (!HasAuthority)
                {
                    logger.LogWarning("Only the client with authority can set animations");
                    return;
                }

                if (Client.Player != null)
                    CmdOnAnimationTriggerServerMessage(hash);

                // call on client right away
                HandleAnimTriggerMsg(hash);
            }
            else
            {
                if (!IsServer)
                {
                    logger.LogWarning("Tried to set animation in the client for a server-controlled animator");
                    return;
                }

                HandleAnimTriggerMsg(hash);
                RpcOnAnimationTriggerClientMessage(hash);
            }
        }

        /// <summary>
        /// Causes an animation trigger to be reset for a networked object.
        /// <para>If local authority is set, and this is called from the client, then the trigger will be reset on the server and all clients. If not, then this is called on the server, and the trigger will be reset on all clients.</para>
        /// </summary>
        /// <param name="triggerName">Name of trigger.</param>
        public void ResetTrigger(string triggerName)
        {
            ResetTrigger(Animator.StringToHash(triggerName));
        }

        /// <summary>
        /// Causes an animation trigger to be reset for a networked object.
        /// </summary>
        /// <param name="hash">Hash id of trigger (from the Animator).</param>
        public void ResetTrigger(int hash)
        {
            if (ClientAuthority)
            {
                if (!IsClient)
                {
                    logger.LogWarning("Tried to reset animation in the server for a client-controlled animator");
                    return;
                }

                if (!HasAuthority)
                {
                    logger.LogWarning("Only the client with authority can reset animations");
                    return;
                }

                if (Client.Player != null)
                    CmdOnAnimationResetTriggerServerMessage(hash);

                // call on client right away
                HandleAnimResetTriggerMsg(hash);
            }
            else
            {
                if (!IsServer)
                {
                    logger.LogWarning("Tried to reset animation in the client for a server-controlled animator");
                    return;
                }

                HandleAnimResetTriggerMsg(hash);
                RpcOnAnimationResetTriggerClientMessage(hash);
            }
        }

        #region server message handlers

        [ServerRpc]
        private void CmdOnAnimationServerMessage(int stateHash, float normalizedTime, int layerId, float weight, ArraySegment<byte> parameters)
        {
            // Ignore messages from client if not in client authority mode
            if (!ClientAuthority)
                return;

            if (logger.LogEnabled()) logger.Log("OnAnimationMessage for netId=" + NetId);

            // handle and broadcast
            using (var networkReader = NetworkReaderPool.GetReader(parameters, null))
            {
                HandleAnimMsg(stateHash, normalizedTime, layerId, weight, networkReader);
                RpcOnAnimationClientMessage(stateHash, normalizedTime, layerId, weight, parameters);
            }
        }

        [ServerRpc]
        private void CmdOnAnimationParametersServerMessage(ArraySegment<byte> parameters)
        {
            // Ignore messages from client if not in client authority mode
            if (!ClientAuthority)
                return;

            // handle and broadcast
            using (var networkReader = NetworkReaderPool.GetReader(parameters, null))
            {
                HandleAnimParamsMsg(networkReader);
                RpcOnAnimationParametersClientMessage(parameters);
            }
        }

        [ServerRpc]
        private void CmdOnAnimationTriggerServerMessage(int hash)
        {
            // Ignore messages from client if not in client authority mode
            if (!ClientAuthority)
                return;

            // handle and broadcast
            // host should have already the trigger
            var isHostOwner = IsClient && HasAuthority;
            if (!isHostOwner)
            {
                HandleAnimTriggerMsg(hash);
            }

            RpcOnAnimationTriggerClientMessage(hash);
        }

        [ServerRpc]
        private void CmdOnAnimationResetTriggerServerMessage(int hash)
        {
            // Ignore messages from client if not in client authority mode
            if (!ClientAuthority)
                return;

            // handle and broadcast
            // host should have already the trigger
            var isHostOwner = IsClient && HasAuthority;
            if (!isHostOwner)
            {
                HandleAnimResetTriggerMsg(hash);
            }

            RpcOnAnimationResetTriggerClientMessage(hash);
        }

        #endregion

        #region client message handlers

        [ClientRpc]
        private void RpcOnAnimationClientMessage(int stateHash, float normalizedTime, int layerId, float weight, ArraySegment<byte> parameters)
        {
            using (var networkReader = NetworkReaderPool.GetReader(parameters, null))
                HandleAnimMsg(stateHash, normalizedTime, layerId, weight, networkReader);
        }

        [ClientRpc]
        private void RpcOnAnimationParametersClientMessage(ArraySegment<byte> parameters)
        {
            using (var networkReader = NetworkReaderPool.GetReader(parameters, null))
                HandleAnimParamsMsg(networkReader);
        }

        [ClientRpc]
        private void RpcOnAnimationTriggerClientMessage(int hash)
        {
            // host/owner handles this before it is sent
            if (IsServer || (ClientAuthority && HasAuthority)) return;

            HandleAnimTriggerMsg(hash);
        }

        [ClientRpc]
        private void RpcOnAnimationResetTriggerClientMessage(int hash)
        {
            // host/owner handles this before it is sent
            if (IsServer || (ClientAuthority && HasAuthority)) return;

            HandleAnimResetTriggerMsg(hash);
        }

        #endregion
    }
}

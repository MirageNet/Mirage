using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Mirage
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct SyncSettings
    {
        public const string INTERVAL_TOOLTIP = "Time in seconds until next change is synchronized to the client. '0' means send immediately if changed. '0.5' means only send changes every 500ms.\n(This is for state synchronization like SyncVars, SyncLists, OnSerialize. Not for Cmds, Rpcs, etc.)";

        // FieldOffset to make sure this struct is small and can be passed around efficiently
        [FieldOffset(0)]
        public SyncFrom From;
        [FieldOffset(1)]
        public SyncTo To;
        [FieldOffset(2)]
        public SyncTiming Timing;
        [FieldOffset(4), Tooltip(INTERVAL_TOOLTIP)]
        public float Interval;

        public static readonly SyncSettings Default = new SyncSettings
        {
            From = SyncFrom.Server,
            To = SyncTo.Owner | SyncTo.ObserversOnly,
            Interval = 0.1f,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateTime(ref float nextSyncTime, float now)
        {
            UpdateTime(Interval, Timing, ref nextSyncTime, now);
        }

        public static void UpdateTime(float interval, SyncTiming timing, ref float nextSyncTime, float now)
        {
            switch (timing)
            {
                case SyncTiming.Variable:
                    // atlesat Interval before next sync 
                    nextSyncTime = now + interval;
                    break;
                case SyncTiming.Fixed:
                    // just add Interval, so that it syncs 1/Interval times per second
                    // see SyncTiming.Fixed for example
                    nextSyncTime += interval;
                    break;
                default:
                case SyncTiming.NoInterval:
                    // always sync
                    nextSyncTime = now;
                    break;
            }
        }

        public bool ShouldSyncFrom(NetworkIdentity identity)
        {
            if ((From & SyncFrom.Server) != 0 && identity.IsServer)
            {
                // if ObserversOnly, then we always want too sync
                if ((To & SyncTo.ObserversOnly) != 0)
                    return true;

                // dont need to check SyncTo.Owner, it is only case left here

                // if to.owner, only sync if not host
                return !identity.IsClient; // not host
            }

            if ((From & SyncFrom.Owner) != 0 && identity.HasAuthority)
            {
                // todo do we need to relay to observer?

                // if from owner, must be to server
                if ((To & SyncTo.Server) != 0)
                { // true if not host OR to ObserversOnly
                    return !identity.IsServer || (To & SyncTo.ObserversOnly) != 0;
                }
            }

            return false;
        }

        public bool ToObserverWriterOnly(NetworkIdentity identity)
        {
            // if not to observer, then we can return early (saves performance on IsServer check)
            if ((To & SyncTo.ObserversOnly) == 0)
                return false;

            // if not server, we are always send to server using owner Writer
            if (!identity.IsServer)
                return false;

            // if not to.Owner, then use ObserverWriter
            if ((To & (SyncTo.Owner)) == 0)
                return true;

            // if hostOwner, then use ObserverWriter
            // HasAuthority is only true on server then it most also be host mode
            if (identity.HasAuthority)
                return true;

            // no owner, then use ObserverWriter
            if (identity.Owner == null)
                return true;

            return false;
        }

        public bool CopyToObservers(NetworkIdentity identity)
        {
            // sending from server
            if ((From & SyncFrom.Server) != 0 && identity.IsServer)
            {
                // include ObserversOnly
                return (To & SyncTo.ObserversOnly) != 0;
            }

            return false;
        }

        public static bool IsValidDirection(SyncFrom from, SyncTo to)
        {
            var fromNone = from == SyncFrom.None;
            var toNone = to == SyncTo.None;

            // both true, allowed
            if (fromNone && toNone)
                return true;
            // both false, allowed, but 1 false not allowed
            if (fromNone != toNone)
                return false;


            if ((from & SyncFrom.Owner) != 0)
            {
                // if from owner,
                // server must be included in SyncTo
                // Observers is optional
                if ((to & SyncTo.Server) == 0)
                    return false;
            }

            if ((from & SyncFrom.Server) != 0)
            {
                // if from server,
                // must be to Owner or Observers
                // either is fine
                if ((to & (SyncTo.Owner | SyncTo.ObserversOnly)) == 0)
                    return false;
            }


            if ((to & SyncTo.Owner) != 0)
            {
                // if to owner, from server must be true
                // this check if different than above,
                // it is making sure From.Owner and To.Owner aren't both true without From.Server also being true
                if ((from & SyncFrom.Server) == 0)
                    return false;
            }

            if ((to & SyncTo.Server) != 0)
            {
                // if to owner, from server must be true
                // this check if different than above,
                // it is making sure From.Owner and To.Owner aren't both true without From.Server also being true
                if ((from & SyncFrom.Owner) == 0)
                    return false;
            }

            return true;
        }

        public static string InvalidReason(SyncFrom from, SyncTo to)
        {
            var fromNone = from == SyncFrom.None;
            var toNone = to == SyncTo.None;

            // both true, allowed
            if (fromNone && toNone)
                return string.Empty;
            // both false, allowed, but 1 false not allowed
            if (fromNone != toNone)
                return "Invalid sync: either both 'from' and 'to' must be 'None' or neither can be 'None'";


            if ((from & SyncFrom.Owner) != 0)
            {
                // if from owner,
                // server must be included in SyncTo
                // Observers is optional
                if ((to & SyncTo.Server) == 0)
                    return "Invalid sync: when syncing from Owner, Server must be included in SyncTo";
            }

            if ((from & SyncFrom.Server) != 0)
            {
                // if from server,
                // must be to Owner or Observers
                // either is fine
                if ((to & (SyncTo.Owner | SyncTo.ObserversOnly)) == 0)
                    return "Invalid sync: when syncing from Server, either Owner or ObserversOnly must be included in SyncTo";
            }


            if ((to & SyncTo.Owner) != 0)
            {
                // if to owner, from server must be true
                // this check if different than above,
                // it is making sure From.Owner and To.Owner aren't both true without From.Server also being true
                if ((from & SyncFrom.Server) == 0)
                    return "Invalid sync: when syncing to Owner, Server must be included in SyncFrom";
            }

            if ((to & SyncTo.Server) != 0)
            {
                // if to owner, from server must be true
                // this check if different than above,
                // it is making sure From.Owner and To.Owner aren't both true without From.Server also being true
                if ((from & SyncFrom.Owner) == 0)
                    return "Invalid sync: when syncing to Server, Owner must be included in SyncFrom";
            }

            return string.Empty;
        }
    }


    [Flags]
    public enum SyncFrom : byte
    {
        None = 0,
        /// <summary>
        /// syncs from Owner to Server or 
        /// </summary>
        Owner = 1,
        Server = 2,
    }
    [Flags]
    public enum SyncTo : byte
    {
        None = 0,
        Owner = 1,
        ObserversOnly = 2,
        Server = 4,

        OwnerAndObservers = Owner | ObserversOnly,
    }

    public enum SyncTiming : byte
    {
        /// <summary>
        /// Will wait for atleast <see cref="SyncSettings.Interval"/> after last sync before sending again.
        /// <para>
        /// Best used when values dont change often, or for non-time-critical data.
        /// </para>
        /// <para>
        /// Will send less often than <see cref="Fixed"/> for the same <see cref="SyncSettings.Interval"/>.
        /// </para>
        /// </summary>
        Variable = 0,

        /// <summary>
        /// Will ensure data is sent every <see cref="SyncSettings.Interval"/> if changed.
        /// <para>
        /// Best used for data that changes often and you want (1/<see cref="SyncSettings.Interval"/>) updates per second
        /// </para>
        /// </summary>
        /// <remarks>
        /// <b>Example of Fixed vs Variable</b>
        /// <para>
        /// if Interval = 0.1. Then Synctimes will look like: 0, 0.1, 0.2, 0.3, etc.
        /// </para>
        /// <para>
        /// Compared to <see cref="Variable"/> where the values depend more on the timedelta, for example they might look like: 0, 0.11, 0.215, 0.32, etc 
        /// </para>
        /// </remarks>
        Fixed = 1,

        /// <summary>
        /// Ignores Interval and will send changes in next update
        /// </summary>
        NoInterval = 2,
    }
}

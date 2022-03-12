using System;
using Nethermind.Core2.Types;

namespace Lantern
{
    /// <summary>
    /// Local clock for the consensus layer.
    /// </summary>
    public class Clock
    {
        private TimeParameters time = new Constants.TimeParameters();

        /// <summary>
        /// Calculates the number of slots
        /// since genesis time.
        /// </summary>
        public Slot CalculateSlot(int network)
        {
            ulong timePassed = (ulong)DateTime.UtcNow.Subtract(DateTime.MinValue.AddYears(1969)).TotalSeconds;
            ulong diffInSeconds = timePassed - time.GenesisTime[network];
            return new Slot(((ulong)Math.Floor((decimal)(diffInSeconds / time.SecondsPerSlot))));
        }

        /// <summary>
        /// Calculates the number of epochs
        /// since genesis time.
        /// </summary>
        public Epoch CalculateEpoch(int network)
        {
            return new Epoch(CalculateSlot(network) / (ulong)time.SlotsPerEpoch);
        }

        public ulong CalculateSyncPeriod(int network)
        {
            return (ulong)CalculateEpoch(network) / (ulong)time.EpochsPerSyncCommitteePeriod;
        }
    }
}

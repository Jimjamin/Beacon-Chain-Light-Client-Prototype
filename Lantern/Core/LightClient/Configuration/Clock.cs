using System;
using Nethermind.Core2.Types;

namespace Lantern
{
    /// <summary>
    /// Local clock for the consensus layer.
    /// </summary>
    public class Clock
    {
        /// <summary>
        /// Calculates the number of slots
        /// since genesis time.
        /// </summary>
        public Slot CalculateSlot(int network)
        {
            ulong timePassed = (ulong)DateTime.UtcNow.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
            ulong diffInSeconds = (timePassed / 1000) - Constants.TimeParameters.GenesisTime[network];
            return new Slot(((ulong)Math.Floor((decimal)(diffInSeconds / Constants.TimeParameters.SecondsPerSlot))));
        }

        /// <summary>
        /// Calculates the number of epochs
        /// since genesis time.
        /// </summary>
        public Epoch CalculateEpochAtSlot(ulong slot)
        {
            return new Epoch(slot / (ulong)Constants.TimeParameters.SlotsPerEpoch);
        }

        public ulong CalculateSyncPeriodAtEpoch(ulong epoch)
        {
            return epoch / (ulong)Constants.TimeParameters.EpochsPerSyncCommitteePeriod;
        }

        public Epoch CalculateRemainingEpochs(int network)
        {
            return new Epoch((ulong)Constants.TimeParameters.EpochsPerSyncCommitteePeriod);
        }
    }
}

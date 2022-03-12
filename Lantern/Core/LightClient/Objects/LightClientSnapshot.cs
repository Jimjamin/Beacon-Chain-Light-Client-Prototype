using Nethermind.Core2.Containers;
using Nethermind.Core2.Types;
using Nethermind.Core2.Crypto;

namespace Lantern
{
    public class LightClientSnapshot
    {
        private BeaconBlockHeader finalizedHeader;
        private SyncCommittee currentSyncCommittee;
        private Root[] currentSyncCommitteeBranch;

        public LightClientSnapshot()
        {
            finalizedHeader = null;
            currentSyncCommittee = new SyncCommittee();
            currentSyncCommitteeBranch = InitializeArray<Root>(4);
        }

        public LightClientSnapshot(
            BeaconBlockHeader _finalizedHeader,
            SyncCommittee _currentSyncCommittee,
            Root[] _currentSyncCommitteeBranch)
        {
            finalizedHeader = _finalizedHeader;
            currentSyncCommittee = _currentSyncCommittee;
            currentSyncCommitteeBranch = _currentSyncCommitteeBranch;
        }

        public T[] InitializeArray<T>(int length) where T : new()
        {
            T[] array = new T[length + 1];
            for (int i = 0; i < length + 1; ++i)
            {
                array[i] = new T();
            }

            return array;
        }

        public BeaconBlockHeader FinalizedHeader { get; set; }
        public SyncCommittee CurrentSyncCommittee { get; set; }
        public Root[] CurrentSyncCommitteeBranch { get; set; }

    }
}

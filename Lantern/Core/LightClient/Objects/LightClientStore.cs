using Nethermind.Core2.Containers;
using Nethermind.Core2.Crypto;

namespace Lantern
{
    public class LightClientStore
    {
        private BeaconBlockHeader finalizedHeader;
        private SyncCommittee currentSyncCommittee;
        private SyncCommittee nextSyncCommittee;
        private LightClientUpdate bestValidUpdate;
        private LightClientProofs proofs;
        private BeaconBlockHeader optimisticHeader;
        private int previousMaxActiveParticipants;
        private int currentMaxActiveParticipants;


        public LightClientStore()
        {
            finalizedHeader = new BeaconBlockHeader(Root.Zero);
            currentSyncCommittee = new SyncCommittee();
            nextSyncCommittee = null;
            bestValidUpdate = new LightClientUpdate();
            proofs = new LightClientProofs();
            optimisticHeader = new BeaconBlockHeader(Root.Zero);
            previousMaxActiveParticipants = 0;
            currentMaxActiveParticipants = 0;
        }

        public LightClientStore(
            BeaconBlockHeader _finalizedHeader,
            SyncCommittee _currentSyncCommittee,
            SyncCommittee _nextSyncCommittee,
            LightClientUpdate _bestValidUpdate,
            LightClientProofs _proofs,
            BeaconBlockHeader _optimisticHeader,
            int _previousMaxActiveParticipants,
            int _currentMaxActiveParticipants
            )
        {
            finalizedHeader = _finalizedHeader;
            currentSyncCommittee = _currentSyncCommittee;
            nextSyncCommittee = _nextSyncCommittee;
            bestValidUpdate = _bestValidUpdate;
            proofs = _proofs;
            optimisticHeader = _optimisticHeader;
            previousMaxActiveParticipants = _previousMaxActiveParticipants;
            currentMaxActiveParticipants = _currentMaxActiveParticipants;
        }

        public BeaconBlockHeader FinalizedHeader { get; set; }
        public SyncCommittee CurrentSyncCommittee { get; set; }
        public SyncCommittee NextSyncCommittee { get; set; }
        public LightClientUpdate BestValidUpdate { get; set; }
        public LightClientProofs Proofs { get; set; }
        public BeaconBlockHeader OptimisticHeader { get; set; }
        public int PreviousMaxActiveParticipants { get; set; }
        public int CurrentMaxActiveParticipants { get; set; }



    }
}

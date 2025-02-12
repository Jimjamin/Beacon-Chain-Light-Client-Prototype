﻿using System;
using System.Linq;
using Nethermind.Core2.Types;
using Nethermind.Core2.Containers;
using Nethermind.Core2.Crypto;

namespace Lantern
{
    public class CoreSpec
    {
        public LightClientUtility utility;
        public Clock clock;
        public LightClientStore storage;
        public Constants constants;
        public TimeParameters time;
        public Logging logging;

        public CoreSpec()
        {
            utility = new LightClientUtility();
            storage = new LightClientStore();
            constants = new Constants();
            time = new Constants.TimeParameters();
            logging = new Logging();
        }

        public bool ValidateCheckpoint(LightClientSnapshot snapshot)
        {
            // Verify the current sync committee branch
            var isValid = utility.IsValidMerkleBranch(
                   snapshot.CurrentSyncCommittee.HashTreeRoot(),
                   snapshot.CurrentSyncCommitteeBranch,
                   constants.CurrentSyncCommitteeDepth,
                   (ulong)constants.CurrentSyncCommitteeIndex,
                   snapshot.FinalizedHeader.StateRoot);

            if (!isValid)
            {
                logging.SelectLogsType("Error", 5, snapshot.FinalizedHeader.Slot.ToString());
                return false;
            }
            storage.CurrentSyncCommittee = snapshot.CurrentSyncCommittee;
            storage.FinalizedHeader = snapshot.FinalizedHeader;
            return true;
        }

        public bool VerifyProofs(LightClientProofs proofs, Root stateRoot)
        {
            return utility.IsValidMerkleBranch(proofs.Leaf, proofs.Proof.ToArray(), (int)(Math.Floor((double)(Math.Log2(proofs.Gindex)))), proofs.Gindex, stateRoot);            
        }

        public void ProcessSlotForLightClientStore(LightClientStore store, Slot currentSlot)
        {
            if((currentSlot % time.UpdateTimeout) == 0)
            {
                store.PreviousMaxActiveParticipants = store.CurrentMaxActiveParticipants;
                store.CurrentMaxActiveParticipants = 0;
            }
            if(currentSlot > (store.FinalizedHeader.Slot + time.UpdateTimeout) & (store.BestValidUpdate != null || store.BestValidUpdate != new LightClientUpdate()))
            {
                ApplyLightClientUpdate(store, store.BestValidUpdate);
                store.BestValidUpdate = new LightClientUpdate();
            }
        }

        public bool ValidateLightClientUpdate(LightClientStore store, LightClientUpdate update, Slot currentSlot, Root genesisValidatorsRoot)
        {
            BeaconBlockHeader activeHeader = GetActiveHeader(update);

            if(!(currentSlot >= activeHeader.Slot & currentSlot >= store.FinalizedHeader.Slot)){
                logging.SelectLogsType("Warn", 0, $"The server sent a known block (at slot {activeHeader.Slot}). Retrying...");
            }

            Epoch finalizedPeriod = new Epoch((ulong)Math.Floor((decimal)((ulong)utility.ComputeEpochAtSlot(store.FinalizedHeader.Slot) / time.EpochsPerSyncCommitteePeriod)));
            Epoch updatePeriod = new Epoch((ulong)Math.Floor((decimal)((ulong)utility.ComputeEpochAtSlot(activeHeader.Slot) / time.EpochsPerSyncCommitteePeriod)));
            Epoch newPeriod = finalizedPeriod + new Epoch(1);

            if (updatePeriod != finalizedPeriod & updatePeriod != newPeriod)
            {
                logging.SelectLogsType("Error", 0, updatePeriod.ToString());
                return false;                
            }
            if(update.FinalizedHeader == BeaconBlockHeader.Zero)
            {
                utility.assertZeroHashes(update.FinalityBranch, constants.FinalizedRootDepth, "finalityBranches");
            }
            else
            {
                var isValid = utility.IsValidMerkleBranch(
                    update.FinalizedHeader.HashTreeRoot(), 
                    update.FinalityBranch,
                    constants.FinalizedRootDepth,
                    (ulong)constants.FinalizedRootIndex,
                    update.AttestedHeader.StateRoot);

                if (!isValid)
                {
                    logging.SelectLogsType("Error", 1, update.FinalizedHeader.Slot.ToString());
                    return false;                    
                }
            }
            
            SyncCommittee syncCommittee;
            if(store.NextSyncCommittee == null)
            {
                syncCommittee = store.CurrentSyncCommittee;
                var isValid = utility.IsValidMerkleBranch(
                    update.NextSyncCommittee.HashTreeRoot(),
                    update.NextSyncCommitteeBranch,
                    constants.NextSyncCommitteeDepth,
                    (ulong)constants.NextSyncCommitteeIndex,
                    activeHeader.StateRoot);
                if (!isValid)
                {
                    logging.SelectLogsType("Error", 2, activeHeader.Slot.ToString());
                    return false;
                }
            }
            else
            {
                if (updatePeriod == finalizedPeriod)
                {
                    syncCommittee = store.CurrentSyncCommittee;
                    utility.assertZeroHashes(update.NextSyncCommitteeBranch, constants.NextSyncCommitteeDepth, "nextSyncCommitteeBranch");
                }
                else
                {
                    syncCommittee = storage.NextSyncCommittee;
                    var isValid = utility.IsValidMerkleBranch(
                        update.NextSyncCommittee.HashTreeRoot(),
                        update.NextSyncCommitteeBranch,
                        constants.NextSyncCommitteeDepth,
                        (ulong)constants.NextSyncCommitteeIndex,
                        activeHeader.StateRoot);
                    if (!isValid)
                    {
                        logging.SelectLogsType("Error", 2, activeHeader.Slot.ToString());
                        return false;
                    }
                }
            }
       
            SyncAggregate syncAggregate = update.SyncAggregate;
            int committeeParticipantsSum = syncAggregate.SyncCommitteeBits.Cast<bool>().Count(l => l);
            
            if(!(committeeParticipantsSum >= constants.MinSyncCommitteeParticipants))
            {
                logging.SelectLogsType("Error", 3, committeeParticipantsSum.ToString());
                return false;
            }

            BlsPublicKey[] publicKeys = utility.GetParticipantPubkeys(syncCommittee.PublicKeys, syncAggregate.SyncCommitteeBits);
            BlsPublicKey aggregatePublicKey = utility.Crypto.BlsAggregatePublicKeys(publicKeys);
            Domain domain = utility.ComputeDomain(new SignatureDomains().DomainSyncCommittee, update.ForkVersion, genesisValidatorsRoot);
            Root signingRoot = utility.ComputeSigningRoot(update.AttestedHeader.HashTreeRoot(), domain); 
            
            var Valid = utility.Crypto.BlsVerify(aggregatePublicKey, signingRoot, syncAggregate.SyncCommitteeSignature);

            if (!Valid)
            {
                logging.SelectLogsType("Error", 4, activeHeader.Slot.ToString());
                return false;
            }
            return true;
        }

        public void ApplyLightClientUpdate(LightClientStore store, LightClientUpdate update)
        {
            BeaconBlockHeader activeHeader = GetActiveHeader(update);
            Epoch finalizedPeriod = new Epoch((ulong)Math.Floor((decimal)((ulong)utility.ComputeEpochAtSlot(store.FinalizedHeader.Slot) / time.EpochsPerSyncCommitteePeriod)));
            Epoch updatePeriod = new Epoch((ulong)Math.Floor((decimal)((ulong)utility.ComputeEpochAtSlot(activeHeader.Slot) / time.EpochsPerSyncCommitteePeriod)));
            
            if (store.NextSyncCommittee != null & updatePeriod == (finalizedPeriod + new Epoch(1)))
            {
                store.CurrentSyncCommittee = store.NextSyncCommittee;
            }
            store.NextSyncCommittee = update.NextSyncCommittee;
            store.FinalizedHeader = activeHeader;
            if (store.FinalizedHeader.Slot > store.OptimisticHeader.Slot)
            {
                store.OptimisticHeader = store.FinalizedHeader;
            }
            storage = store;
        }

        public bool ProcessLightClientUpdate(LightClientStore store, LightClientUpdate update, Slot currentSlot, Root genesisValidatorsRoot)
        {
            if (ValidateLightClientUpdate(store, update, currentSlot, genesisValidatorsRoot))
            {
                int updateSyncCommitteeBits = update.SyncAggregate.SyncCommitteeBits.Cast<bool>().Count(l => l);
                int bestSyncCommitteeBits = store.BestValidUpdate.SyncAggregate.SyncCommitteeBits.Cast<bool>().Count(l => l);

                if ((store.BestValidUpdate == new LightClientUpdate() || store.BestValidUpdate == null) || (updateSyncCommitteeBits > bestSyncCommitteeBits))
                {
                    store.BestValidUpdate = update;
                }

                store.CurrentMaxActiveParticipants = Math.Max(store.CurrentMaxActiveParticipants, updateSyncCommitteeBits);

                if (((updateSyncCommitteeBits * 3) >= update.SyncAggregate.SyncCommitteeBits.Length * 2) & (update.FinalizedHeader != new BeaconBlockHeader(Root.Zero) || update.FinalizedHeader != null))
                {
                    ApplyLightClientUpdate(store, update);
                    store.BestValidUpdate = new LightClientUpdate();
                }
                return true;
            }
            return false;                                        
        }

        public BeaconBlockHeader GetActiveHeader(LightClientUpdate update)
        {
            if(update.FinalizedHeader != BeaconBlockHeader.Zero)
            {
                return update.FinalizedHeader;
            }
            return update.AttestedHeader;
        }

        public ulong GetSubTreeIndex(ulong generalizedIndex, ulong generalizedIndexLog2)
        {
            return generalizedIndex % ((ulong)((2 << (int)generalizedIndexLog2) / 2));
        }
    }
}

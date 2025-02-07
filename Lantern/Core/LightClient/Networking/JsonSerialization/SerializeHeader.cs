﻿using Nethermind.Core2.Crypto;
using Nethermind.Core2.Types;
using Nethermind.Core2.Containers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace Lantern
{
    public class SerializeHeader
    {
        public LightClientUtility Utility;
        public HeaderObject.Root Contents;

        public SerializeHeader()
        {
            Utility = new LightClientUtility();
        }

        public void SerializeData(string text)
        {
            Contents = JsonConvert.DeserializeObject<HeaderObject.Root>(text);
        }

        public LightClientUpdate InitializeHeader(int network)
        {
            LightClientUpdate update = new LightClientUpdate();
            update.AttestedHeader = CreateHeader(Contents.data);
            update.SyncAggregate = CreateSyncAggregate(Contents.data.sync_aggregate.sync_committee_bits, Contents.data.sync_aggregate.sync_committee_signature);
            update.ForkVersion = new Networks().ForkVersions[network];
            return update;
        }

        public BeaconBlockHeader CreateHeader(HeaderObject.Data data)
        {
            return new BeaconBlockHeader(
                new Slot(ulong.Parse(data.attested_header.slot)),
                new ValidatorIndex(ulong.Parse(data.attested_header.proposer_index)),
                Utility.ToObject(data.attested_header.parent_root, "Root"),
                Utility.ToObject(data.attested_header.state_root, "Root"),
                Utility.ToObject(data.attested_header.body_root, "Root")
                );
        }

        public SyncAggregate CreateSyncAggregate(string syncBits, string syncCommitteeSignature)
        {
            return new SyncAggregate(Utility.ToObject(syncBits, "BitArray"), Utility.ToObject(syncCommitteeSignature, "BlsSignature"));
        }
    }
}

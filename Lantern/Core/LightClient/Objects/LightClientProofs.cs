using Nethermind.Core2.Crypto;
using System.Collections.Generic;

namespace Lantern
{
    public class LightClientProofs
    {        
        private Root leaf;
        private List<Root> proof;
        private ulong gindex;
        private long balance;

        public LightClientProofs()
        {
            leaf = Root.Zero;
            proof = new List<Root>();
            gindex = 0;
            balance = 0;
        }

        public LightClientProofs(Root _leaf, List<Root> _proof, ulong _gindex, long _balance)
        {
            leaf = _leaf;
            proof = _proof;
            gindex = _gindex;
            balance = _balance;
        }
        
        public Root Leaf { get; set; }
        public List<Root> Proof { get; set; }
        public ulong Gindex { get; set; }
        public long Balance { get; set; }
    }
}

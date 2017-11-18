using System;
using System.Collections.Generic;
using System.Text;

namespace Antauri.Core
{
    public class BlockChain
    {
        private List<Block> _blocks;
        private readonly IHashProvider _hasher;

        public BlockChain(IHashProvider hasher)
        {
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));

            _blocks = new List<Block>() { Block.GenesisBlock };
        }

        public List<Block> Blocks => _blocks;

        public Block LatestBlock => _blocks[_blocks.Count - 1];

        public void Add(Block newBlock)
        {
            if (IsValidNewBlock(newBlock, LatestBlock))
            {
                _blocks.Add(newBlock);
            }
        }

        public bool IsValidNewBlock(Block newBlock, Block previousBlock)
        {
            if (previousBlock.Index + 1 != newBlock.Index)
            {
                Console.WriteLine("invalid index");
                return false;
            }
            else if (previousBlock.Hash != newBlock.PreviousHash)
            {
                Console.WriteLine("invalid previoushash");
                return false;
            }
            else
            {
                if (!_hasher.Verify(newBlock))
                {
                    Console.WriteLine("invalid hash: " + newBlock.Hash);
                    return false;
                }
            }
            return true;
        }

        public void ReplaceChain(List<Block> newBlocks)
        {
            if (IsValidBlocks(newBlocks) && newBlocks.Count > _blocks.Count)
            {
                _blocks = newBlocks;
            }
            else
            {
                Console.WriteLine("Received blockchain invalid");
            }
        }

        private bool IsValidBlocks(List<Block> newBlocks)
        {
            Block firstBlock = newBlocks[0];
            if (!firstBlock.Equals(Block.GenesisBlock))
            {
                return false;
            }

            for (int i = 1; i < newBlocks.Count; i++)
            {
                if (IsValidNewBlock(newBlocks[i], firstBlock))
                {
                    firstBlock = newBlocks[i];
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

    }
}

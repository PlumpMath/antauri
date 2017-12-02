﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antauri.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Antauri.Transactions;

namespace Antauri.Node.Controllers
{
    [Route("api/blocks")]
    public class BlocksController : Controller
    {
        private SimpleBlockChain _blockChain;
        private readonly PeerToPeerService _p2PService;
        private readonly ILogger<BlocksController> _logger;
        private readonly IBlockFactory<SimpleBlock, SimpleTransactionList> _blockFactory;

        public BlocksController(SimpleBlockChain blockChain, 
        PeerToPeerService p2pService, 
        ILogger<BlocksController> logger,
        IBlockFactory<SimpleBlock, SimpleTransactionList> blockFactory
        ){
            _blockChain = blockChain ?? throw new ArgumentNullException(nameof(blockChain));
            _p2PService = p2pService ?? throw new ArgumentNullException(nameof(p2pService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blockFactory = blockFactory;
        }
        // GET api/blocks
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_blockChain.Blocks);
        }

        // GET api/blocks/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Ok(_blockChain.Blocks.FirstOrDefault(b=>b.Header.Index==id));
        }

        // POST api/blocks/mine
        [HttpPost("mine")]
        public async void Post([FromBody]string value)
        {
            var transaction = JsonConvert.DeserializeObject<SimpleTransaction>(value);
            var transactionList = new SimpleTransactionList { transaction };
            SimpleBlock lastBlock = _blockChain.LatestBlock;
            SimpleBlock newBlock = _blockFactory.CreateBlock(lastBlock, transactionList);
            _blockChain.Add(newBlock);
            await _p2PService.Broadcast(_p2PService.ResponseLatestMessage());
            string s = JsonConvert.SerializeObject(newBlock);
            _logger.LogInformation("Block added: " + s);
        }
    }
}

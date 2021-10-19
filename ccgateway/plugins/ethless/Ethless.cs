/*
    Copyright(c) 2018 Gluwa, Inc.

    This file is part of Creditcoin.

    Creditcoin is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Lesser General Public License for more details.
    
    You should have received a copy of the GNU Lesser General Public License
    along with Creditcoin. If not, see <https://www.gnu.org/licenses/>.
*/

using CCGatewayPlugin;
using Microsoft.Extensions.Configuration;
using Nethereum.Hex.HexTypes;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace gethless
{
    class Ethless : ICCGatewayPluginAsync
    {
        private static int mConfirmationsExpected = 12;

        public async Task<Tuple<bool, string>> Run(IConfiguration cfg, string[] command)
        {
            Debug.Assert(command != null);
            Debug.Assert(command.Length > 0);
            if (command[0].Equals("verify"))
            {
                Debug.Assert(command.Length == 7);
                string sourceAddressString = command[1];
                string destinationAddressString = command[2];
                string proof = command[3];
                string destinationAmount = command[4];
                string txId = command[5];
                string unused = command[6];

#if DEBUG
                string confirmationsCount = cfg["confirmationsCount"];
                if (int.TryParse(confirmationsCount, out int parsedCount))
                {
                    mConfirmationsExpected = parsedCount;
                }
#endif

                string rpcUrl = cfg["rpc"];
                if (string.IsNullOrWhiteSpace(rpcUrl))
                {
                    return Tuple.Create(false, "ethless.rpc is not set");
                }

                var web3 = new Nethereum.Web3.Web3(rpcUrl);

                var tx = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txId);
                if (tx == null)
                {
                    return Tuple.Create(false, "Failed to retrieve transaction info");
                }
                var txReceipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
                if (txReceipt.Status.Value == 0)
                {
                    return Tuple.Create(false, "Invalid transaction: transaction status is 'failed'");
                }

                int confirmations = 0;
                if (tx.BlockNumber != null)
                {
                    var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                    confirmations = (int)(blockNumber.Value - tx.BlockNumber.Value);
                }

                if (confirmations < mConfirmationsExpected)
                {
                    return Tuple.Create(false, "Invalid transaction: not enough confirmations");
                }

                var sourceAddressStringSegments = sourceAddressString.Split('@');
                sourceAddressString = sourceAddressStringSegments[1];
                var destinationAddressStringSegment = destinationAddressString.Split('@');
                destinationAddressString = destinationAddressStringSegment[1];

                if (!sourceAddressStringSegments[0].Equals(destinationAddressStringSegment[0]))
                {
                    return Tuple.Create(false, "Invalid transaction: source and destination ethless tokens (Gluwacoins) don't match");
                }

                string ethlessContract = sourceAddressStringSegments[0];

                if (string.IsNullOrWhiteSpace(ethlessContract))
                {
                    return Tuple.Create(false, "Invalid ethless address");
                }

                if (!tx.To.Equals(ethlessContract, StringComparison.OrdinalIgnoreCase))
                {
                    return Tuple.Create(false, "transaction contract doesn't match ethlessContract");
                }

                string ethlessContractAbi = "[{\"constant\":false,\"inputs\":[{\"internalType\":\"address\",\"name\":\"_from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"_to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"_value\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_fee\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_nonce\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"_sig\",\"type\":\"bytes\"}],\"name\":\"transfer\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"success\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";

                var contract = web3.Eth.GetContract(ethlessContractAbi, ethlessContract);
                var transfer = contract.GetFunction("transfer");
                var inputs = transfer.DecodeInput(tx.Input);
                Debug.Assert(inputs.Count == 6);

                var from = inputs[0].Result.ToString();
                if (!sourceAddressString.Equals(from, StringComparison.InvariantCultureIgnoreCase))
                {
                    return Tuple.Create(false, "Invalid transaction: wrong source");
                }

                var to = inputs[1].Result.ToString();
                if (!destinationAddressString.Equals(to, StringComparison.InvariantCultureIgnoreCase))
                {
                    return Tuple.Create(false, "Invalid transaction: wrong destination");
                }

                var value = inputs[2].Result.ToString();
                if (!destinationAmount.Equals(value))
                {
                    return Tuple.Create(false, "Invalid transaction: wrong destination");
                }

                var tag = inputs[4].Result.ToString();
                var nonce = new HexBigInteger("0x" + proof.Substring(10)); //namespace length 6 plus prefix length 4
                if (!tag.Equals(nonce.Value.ToString()))
                {
                    return Tuple.Create(false, "Invalid transaction: wrong proof");
                }

                return Tuple.Create<bool, string>(true, null);
            }

            return Tuple.Create(false, "Unknown command: " + command[0]);
        }
    }
}
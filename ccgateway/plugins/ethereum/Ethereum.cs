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

using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CCGatewayPlugin;

namespace gethereum
{
    class Ethereum : ICCGatewayPluginAsync
    {
        private static int mConfirmationsExpected = 12;

        public async Task<Tuple<bool, string>> Run(IConfiguration cfg, string[] command)
        {
            Debug.Assert(command != null);
            Debug.Assert(command.Length > 0);
            string msg = null;
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
                    msg = "ethereum.rpc is not set";
                    return Tuple.Create(false, msg);
                }

                var web3 = new Nethereum.Web3.Web3(rpcUrl);

                var tx = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txId);
                if (tx == null)
                {
                    msg = "Failed to retrieve transaction info";
                    return Tuple.Create(false, msg);
                }
                var txReceipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
                if (txReceipt.Status.Value == 0)
                {
                    msg = "Invalid transaction: transaction status is 'failed'";
                    return Tuple.Create(false, msg);
                }

                int confirmations = 0;
                if (tx.BlockNumber != null)
                {
                    var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                    confirmations = (int)(blockNumber.Value - tx.BlockNumber.Value);
                }

                if (confirmations < mConfirmationsExpected)
                {
                    msg = "Invalid transaction: not enough confirmations";
                    return Tuple.Create(false, msg);
                }

                if (!sourceAddressString.Equals(tx.From, System.StringComparison.OrdinalIgnoreCase))
                {
                    msg = "Invalid transaction: wrong sourceAddressString";
                    return Tuple.Create(false, msg);
                }

                if (destinationAddressString.Equals("creditcoin"))
                {
                    string creditcoinContract = cfg["creditcoinContract"];

                    if (string.IsNullOrWhiteSpace(creditcoinContract))
                    {
                        msg = "ethereum.creditcoinContract is not set";
                        return Tuple.Create(false, msg);
                    }

                    if (!tx.To.Equals(creditcoinContract, StringComparison.OrdinalIgnoreCase))
                    {
                        msg = "transaction contract doesn't match creditcoinContract";
                        return Tuple.Create(false, msg);
                    }

                    string creditcoinContractAbi = cfg["creditcoinContractAbi"];
                    if (string.IsNullOrWhiteSpace(creditcoinContractAbi))
                    {
                        msg = "ethereum.creditcoinContractAbi is not set";
                        return Tuple.Create(false, msg);
                    }

                    var contract = web3.Eth.GetContract(creditcoinContractAbi, creditcoinContract);
                    var burn = contract.GetFunction("exchange");
                    var inputs = burn.DecodeInput(tx.Input);
                    Debug.Assert(inputs.Count == 2);
                    var value = inputs[0].Result.ToString();
                    if (destinationAmount != value)
                    {
                        msg = "Invalid transaction: wrong amount";
                        return Tuple.Create(false, msg);
                    }

                    var tag = inputs[1].Result.ToString();
                    if (!tag.Equals(proof))
                    {
                        msg = "Invalid transaction: wrong proof";
                        return Tuple.Create(false, msg);
                    }
                }
                else
                {
                    if (destinationAmount != tx.Value.Value.ToString())
                    {
                        msg = "Invalid transaction: wrong amount";
                        return Tuple.Create(false, msg);
                    }

                    if (tx.Input == null)
                    {
                        msg = "Invalid transaction: expecting data";
                        return Tuple.Create(false, msg);
                    }

                    if (!proof.StartsWith("0x"))
                    {
                        proof = "0x" + proof;
                    }
                    if (!tx.Input.Equals(proof, System.StringComparison.OrdinalIgnoreCase))
                    {
                        msg = "Invalid transaction: wrong proof";
                        return Tuple.Create(false, msg);
                    }

                    if (!tx.To.Equals(destinationAddressString, System.StringComparison.OrdinalIgnoreCase))
                    {
                        msg = "Invalid transaction: wrong destinationAddressString";
                        return Tuple.Create(false, msg);
                    }
                }

                msg = null;
                return Tuple.Create(true, msg);
            }

            msg = "Unknown command: " + command[0];
            return Tuple.Create(false, msg);
        }
    }
}
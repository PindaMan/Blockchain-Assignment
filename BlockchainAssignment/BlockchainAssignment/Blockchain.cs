using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainAssignment
{
    class Blockchain
    {
        public List<Block> Blocks = new List<Block>();
        public List<Transaction> transactionPool = new List<Transaction>();
        int transactionsPerBlock = 5;

        public Blockchain()
        {
            Blocks.Add(new Block());
        }

        public String GetBlockAsString(int index)
        {
            return Blocks[index].ToString();
        }

        public Block GetLastBlock()
        {
            return Blocks[Blocks.Count - 1];
        }

        public List<Transaction> GetPendingTransactions(String preference = "Random", String address = "")
        {
            int n;
            List<Transaction> transactions;

            if (transactionsPerBlock >= transactionPool.Count)
            {
                n = transactionPool.Count;
                transactions = transactionPool.GetRange(0, n);
                transactionPool.RemoveRange(0, n);

                return transactions;
            }
            
            n = Math.Min(transactionsPerBlock, transactionPool.Count);


            switch (preference)
            {
                case "Greedy": // Order by fee, highest first
                    transactions = transactionPool.OrderByDescending(o => o.fee).ToList();
                    transactions = transactions.GetRange(0, n);
                    break;
                
                case "Random": // Select at random
                    Random r = new Random();
                    transactions = transactionPool.OrderBy(x => r.Next()).ToList();
                    transactions = transactions.GetRange(0, n);
                    break;
                
                case "Selfish": // Select based on sender Address, otherwise just take any
                    transactions = transactionPool.Where(o => o.senderAddress == address || o.recipientAddress == address).ToList();
                    if(transactions.Count > n)
                    {
                        transactions = transactions.GetRange(0, n);
                    }
                    else if (transactions.Count < n)
                    {
                        int remaining = n - transactions.Count;
                        foreach (Transaction t in transactionPool)
                        {
                            transactionPool.Remove(t);
                        }
                        
                        transactionPool.GetRange(0, remaining);
                        
                        foreach (Transaction t in transactions)
                        {
                            transactionPool.Remove(t);
                        }

                        return transactions;
                    }
                    break;
                
                case "Altruistic": // Oldest first
                default:
                    transactions = transactionPool.GetRange(0, n);
                    break;
            }

            foreach (Transaction t in transactions)
            {
                transactionPool.Remove(t);
            }

            return transactions;
        }

        public double GetBalance(String address)
        {
            double balance = 0.0;

            foreach(Block b in Blocks)
            {
                foreach(Transaction t in b.transactionList)
                {
                    if(t.recipientAddress.Equals(address))
                    {
                        balance += t.amount;
                    }
                    if(t.senderAddress.Equals(address))
                    {
                        balance -= (t.amount + t.fee);
                    }
                }
            }

            foreach(Transaction t in transactionPool)
            {
                if (t.senderAddress.Equals(address))
                {
                    balance -= (t.amount + t.fee);
                }
            }

            return balance;
        }

        public bool validateMerkleRoot(Block b)
        {
            String reMerkle = Block.MerkleRoot(b.transactionList);
            return reMerkle.Equals(b.merkleRoot);
        }

        public override string ToString()
        {
            string output = String.Empty;
            Blocks.ForEach(b => output += (b.ToString() + "\n"));
            return output;
        }
    }
}

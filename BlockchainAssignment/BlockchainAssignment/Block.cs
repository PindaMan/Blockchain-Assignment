using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BlockchainAssignment
{
    class Block
    {
        public int index;
        DateTime timestamp;
        public String hash;
        public String prevHash;

        public List<Transaction> transactionList = new List<Transaction>();
        public String merkleRoot;

        // Proof-of-work
        public long nonce = 0;
        public ulong targetValue = 0x000ffff000000000; // Initial target Value
        public long timeToMine = 0;

        // Rewards and fees
        public double reward = 1.0;
        public double fees = 0.0;

        public String minerAddress = String.Empty;

        // Genesis block constructor
        public Block()
        {
            this.timestamp = DateTime.Now;
            this.index = 0;
            this.prevHash = String.Empty;
            reward = 0;

            merkleRoot = MerkleRoot(transactionList);

            // Set hash
            Mine();
        }

        public Block(Block lastBlock, List<Transaction> transactions, String address = "", int threads = 1, ulong target = 0x0000ffff00000000)
        {
            this.timestamp = DateTime.Now;
            this.index = lastBlock.index + 1;
            this.prevHash = lastBlock.hash;
            this.targetValue = target;
            minerAddress = address;

            transactions.Add(CreateRewardTransaction(transactions));
            transactionList = transactions;

            merkleRoot = MerkleRoot(transactionList);

            // Set hash
            Mine(threads);
        }

        public Transaction CreateRewardTransaction(List<Transaction> transactions)
        {
            // Sum the fees in the list of transactions in the mined block
            fees = transactions.Aggregate(0.0, (acc, t) => acc + t.fee);
            // Create the "Reward Transaction" being the sum of fees and reward being transferred from "Mine Rewards" (Coin Base) to miner
            return new Transaction("Mine Rewards", minerAddress, (reward + fees), 0, "");
        }

        public String CreateHash()
        {
            String hash = String.Empty;
            SHA256 hasher = SHA256Managed.Create();

            // Concatenate all Block properties for hashing
            String input = index.ToString() + timestamp.ToString() + prevHash + nonce.ToString() + reward.ToString() + merkleRoot;

            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            
            // Convert hash from byte array to string
            foreach (byte b in hashByte)
            {
                hash += String.Format("{0:x2}", b);
            }

            return hash;
        }

        public String CreateHash(long nonce)
        {
            String hash = String.Empty;
            SHA256 hasher = SHA256Managed.Create();

            // Concatenate all Block properties for hashing
            String input = index.ToString() + timestamp.ToString() + prevHash + nonce.ToString() + reward.ToString() + merkleRoot;

            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convert hash from byte array to string
            foreach (byte b in hashByte)
            {
                hash += String.Format("{0:x2}", b);
            }

            return hash;
        }

        public void Mine(int threads = 1)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long split = long.MaxValue / threads;

            List<Thread> allThreads = new List<Thread>();

            for (int i = 0; i < threads; i++)
            {
                long lowerBound = split * i;
                long upperBound = split * (i + 1);

                allThreads.Add(new Thread(() => { ThreadMine(lowerBound, upperBound); }));
            }

            foreach(Thread t in allThreads)
            {
                t.Start();
            }

            foreach (Thread t in allThreads)
            {
                t.Join();
            }

            stopwatch.Stop();
            timeToMine = stopwatch.ElapsedMilliseconds;
        }

        private void ThreadMine(long lowerBound, long upperBound)
        {
            // Difficulty criteria definition
            String h = CreateHash();
            // String re = new string('0', difficulty); // Create a string of N (difficulty, i.e. 4) 0s
            ulong hashValue = Convert.ToUInt64("0x" + h.Substring(0, 16), 16);

            if (hashValue < targetValue)
            {
                hash = h;
                return;
            }

            for(long i = lowerBound; i < upperBound; i++)
            {
                h = CreateHash(i);
                // Hash already found in another thread
                if (!String.IsNullOrEmpty(hash))
                {
                    return;
                }

                hashValue = Convert.ToUInt64("0x" + h.Substring(0, 16), 16);

                if (hashValue < targetValue)
                {
                    nonce = i;
                    hash = h;
                    return;
                }
            }
        }

        public static String MerkleRoot(List<Transaction> transactionList)
        {
            List<String> hashes = transactionList.Select(t => t.hash).ToList();
            if(hashes.Count == 0)
            {
                return String.Empty;
            }
            if(hashes.Count == 1)
            {
                return HashCode.HashTools.combineHash(hashes[0], hashes[0]);
            }
            
            while(hashes.Count != 1)
            {
                List<String> merkleLeaves = new List<String>();
                for (int i = 0; i < hashes.Count; i += 2)
                {
                    if(i == hashes.Count - 1)
                    {
                        merkleLeaves.Add(HashCode.HashTools.combineHash(hashes[i], hashes[i]));
                    }
                    else
                    {
                        merkleLeaves.Add(HashCode.HashTools.combineHash(hashes[i], hashes[i + 1]));
                    }
                }

                hashes = merkleLeaves;
            }
            
            return hashes[0];
        }

        public override string ToString()
        {
            String output = String.Empty;
            transactionList.ForEach(t => output += (t.ToString() + "\n"));

            return "Index: " + index.ToString()
                + "\nTimestamp: " + timestamp.ToString()
                + "\nPrevious hash: " + prevHash
                + "\nHash: " + hash
                + "\nNonce: " + nonce.ToString()
                + "\nHashTarget: " + targetValue.ToString()
                + "\nTime to mine: " + timeToMine.ToString() + "ms"
                + "\nReward: " + reward.ToString()
                + "\nFees: " + fees.ToString()
                + "\nMiner's Address: " + minerAddress
                + "\n\nTransactions:\n" + output + "\n";
        }
    }

}

using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainAssignment
{
    class Transaction
    {
        public String senderAddress;
        public String recipientAddress;

        public double amount;
        public double fee;

        DateTime timestamp;

        public String hash;
        String signature;
        

        public Transaction(String from, String to, double amount, double fee, String privateKey)
        {
            this.senderAddress = from;
            this.recipientAddress = to;
            this.amount = amount;
            this.fee = fee;

            this.timestamp = DateTime.Now;

            this.hash = CreateHash();
            this.signature = Wallet.Wallet.CreateSignature(from, privateKey, this.hash);
        }

        public String CreateHash()
        {
            String hash = String.Empty;

            SHA256 hasher = SHA256Managed.Create();
            // Hash all properties
            String input = timestamp.ToString() + senderAddress 
                + recipientAddress + amount.ToString() + fee.ToString();

            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            
            // Convert hash from byte array to string
            foreach (byte b in hashByte)
            {
                hash += String.Format("{0:x2}", b);
            }

            return hash;
        }

        public override string ToString()
        {
            return "Transaction hash: " + hash
                + "\nDigital signature: " + signature
                + "\nTimestamp: " + timestamp.ToString()
                + "\nTransferred: " + amount.ToString() + " AssignmentCoin"
                + "\nFees: " + fee.ToString()
                + "\nSender address: " + senderAddress
                + "\nReceiver address: " + recipientAddress + "\n";
        }
    }
}

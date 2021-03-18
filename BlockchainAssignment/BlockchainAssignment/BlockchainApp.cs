using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlockchainAssignment
{
    public partial class BlockchainApp : Form
    {
        Blockchain blockchain;

        public BlockchainApp()
        {
            InitializeComponent();
            blockchain = new Blockchain();
            richTextBox1.Text = "New Blockchain initialised!";
            preferenceCombo.SelectedIndex = 0;
            targetTime.Text = 1000.ToString();
            targetHash.Text = 0x000ffff000000000.ToString("x16");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int index = 0;
            if(Int32.TryParse(textBox1.Text, out index))
            {
                richTextBox1.Text = blockchain.GetBlockAsString(index);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String privKey;
            Wallet.Wallet myNewWallet = new Wallet.Wallet(out privKey);
            publicKey.Text = myNewWallet.publicID;
            privateKey.Text = privKey;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(Wallet.Wallet.ValidatePrivateKey(privateKey.Text, publicKey.Text))
            {
                richTextBox1.Text = "Keys are valid.";
            }
            else
            {
                richTextBox1.Text = "Keys are invalid.";
            }
        }

        private void createTransaction_Click(object sender, EventArgs e)
        {
            if(!Wallet.Wallet.ValidatePrivateKey(privateKey.Text, publicKey.Text))
            {
                richTextBox1.Text = "Transaction could not be created due to invalid public/private key pair.";
                return;
            }

            if(blockchain.GetBalance(publicKey.Text) < (Double.Parse(amount.Text) + Double.Parse(fee.Text)))
            {
                richTextBox1.Text = "Insufficient funds for transaction.";
                return;
            }

            Transaction transaction = new Transaction(publicKey.Text, receiverKey.Text, Double.Parse(amount.Text), Double.Parse(fee.Text), privateKey.Text);

            blockchain.transactionPool.Add(transaction);
            richTextBox1.Text = transaction.ToString();
        }

        private void newBlock_Click(object sender, EventArgs e)
        {
            int threads;

            if (!int.TryParse(numThreads.Text, out threads))
            {
                threads = 1;
            }

            if (threads < 1) threads = 1; // Use at least one thread
            threads = Math.Min(threads, 6); // Don't use more threads than cores.


            List<Transaction> transactions = blockchain.GetPendingTransactions(preferenceCombo.Text, publicKey.Text);
            Block newBlock = new Block(blockchain.GetLastBlock(), transactions, publicKey.Text, threads, target: 0x000ffff000000000);
            blockchain.Blocks.Add(newBlock);

            richTextBox1.Text = blockchain.ToString();
        }

        private void validateChain_Click(object sender, EventArgs e)
        {
            // Contiguity checks
            if(blockchain.Blocks.Count == 1)
            {
                if (!blockchain.validateMerkleRoot(blockchain.Blocks[0]))
                {
                    richTextBox1.Text = "Blockchain is invalid.";
                }
                else
                {
                    richTextBox1.Text = "Blockchain is valid.";
                }

                return;
            }

            for (int i = 1; i < blockchain.Blocks.Count - 1; i++)
            {
                // Compare hash to previous hash for all blocks in the chain and check cransactions via merkle root.
                if (blockchain.Blocks[i].prevHash != blockchain.Blocks[i - 1].hash || !blockchain.validateMerkleRoot(blockchain.Blocks[i]))
                {
                    richTextBox1.Text = "Blockchain is invalid.";
                    return;
                }
            }

            richTextBox1.Text = "Blockchain is valid.";
        }

        private void checkBalance_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = blockchain.GetBalance(publicKey.Text).ToString() + " AssignmentCoin";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = blockchain.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if(blockchain.transactionPool.Count() == 0)
            {
                richTextBox1.Text = "No pending transactions.";
                return;
            }
            
            String output = String.Empty;

            foreach(Transaction t in blockchain.transactionPool)
            {
                output += t.ToString() + "\n";
            }
            
            richTextBox1.Text = output;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int threads;

            if (!int.TryParse(numThreads.Text, out threads))
            {
                threads = 1;
            }

            if (threads < 1) threads = 1; // Use at least one thread
            threads = Math.Min(threads, 6); // Don't use more threads than cores.

            long elapsedMs = 0;
            ulong hashTarget = Convert.ToUInt64("0x" + targetHash.Text, 16);
            targetHash.Text = hashTarget.ToString("x16");

            for (int i = 0; i < 100; i++)
            {

                List<Transaction> transactions = blockchain.GetPendingTransactions(preferenceCombo.Text, publicKey.Text);
                Block newBlock = new Block(blockchain.GetLastBlock(), transactions, publicKey.Text, threads, target: hashTarget);
                blockchain.Blocks.Add(newBlock);

                elapsedMs += blockchain.Blocks.Last().timeToMine;
            }

            actualTime.Text = (elapsedMs/100).ToString();

            ulong Ta = (ulong)elapsedMs / 100; // Time actual
            ulong Te = ulong.Parse(targetTime.Text);
            double div = Te/(double)Ta;

            hashTarget = (ulong)(hashTarget / div);
            targetHash.Text = hashTarget.ToString("x16");

            richTextBox1.Text = blockchain.ToString();
        }
    }
}

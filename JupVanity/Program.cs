/*
 *
 * Richard Hartness (RLH), (c) 2014
 * What code I've written can be copied, modified and distributed
 * as desired.  I just request that if this is used, please provide credit, 
 * where credit is due.
 * 
 * If you found this useful, please send donations to:
 * 
 * Nxt:  1102622531
 * BTC:  1Mhk5aKnE6jN7yafQXCdDDm8T9Qoy2sTqS
 * LTC:  LKTF6AjzFj2CG81rQravs164VsoJJnEPmm
 * DOGE: DGea4Qev7eJGmohWq2iKSeDkrTsPeYXQAC
 * 
 * Special thanks to NxtSwe for providing a RS Nxt address encoder.  If you 
 * send a donation my way, please send a few Nxt to him as well.
 * 
 * Nxt: NXT-HMVV-XMBN-GYXK-22BKK
 * 
 */

//Comment out this line to not update the hash rate.
#define BENCHMARK

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace NxtRSVanity
{
    class Program
    {

        static string pfx = "";
        static string bw = "";
        static string ew = "";
        static string ct = "";
        static string regex = "";
        static Object lockOut = new Object();

        static void Main(string[] args)
        {
            SetArgs(args);
            if (pfx == "") pfx = CreateRandomString(50);

            if (bw == "" && ew == "" && ct == "" && regex == "")
            {
                Console.WriteLine("Omit dashes for all entries!");
                Console.WriteLine("============================");

                Console.Write("Enter a prefix (Optional): ");
                bw = Console.ReadLine().ToUpper();
                Console.Write("Enter a suffix (Optional): ");
                ew = Console.ReadLine().ToUpper();
                Console.Write("Enter a string to seek anywhere within the address. (Optional): ");
                ct = Console.ReadLine().ToUpper();
                Console.Write("Expert mode! Enter regex pattern to search for. (Optional): ");
                regex = Console.ReadLine().ToUpper();
            }

            Process currentProcess = Process.GetCurrentProcess();
            try
            {
                Console.Clear();
                currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal;

                //Write output
#if BENCHMARK
                DateTime lastBenchmark = DateTime.Now;
                int iterations = 0;
                Object itObject = new Object();
                Console.CursorTop = 0;
                Console.Write("Calculating address creation rate...");
                Console.CursorLeft = 0;
#endif

                Parallel.For(1, Int32.MaxValue, (i, loopState) =>
                {

                    string secret = pfx + i.ToString();
                    string  rsAddr = ReedSolomon.Encode((long)CreateAddress(secret), false);

                    //if (addr < min)
                    if ((bw != "" && rsAddr.StartsWith(bw))
                        || (ew != "" && rsAddr.EndsWith(ew))
                        || (ct != "" && rsAddr.Contains(ct))
                        || (regex != "" && Regex.Match(rsAddr, regex).Success))
                        WriteAddress(secret);

#if BENCHMARK
                    lock (itObject)
                    {
                        iterations++;
                        if (lastBenchmark.AddSeconds(1) < DateTime.Now)
                        {
                            Console.CursorTop = 0;
                            Console.Write("Executing {0:F3} kh/s!".PadRight(50), ((float)iterations) / 1000);
                            Console.CursorLeft = 0;
                            iterations = 0;
                            lastBenchmark = DateTime.Now;

                        }
                    }
#endif
                });
            }
            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        private static void WriteAddress(string secret)
        {
            lock (lockOut)
            {
                StreamWriter sw = new StreamWriter("jupvanity.txt", true);
                Console.CursorTop = 1;
                string rsAddr = "JUP-" + ReedSolomon.Encode((long)CreateAddress(secret), true);
                Console.Write("Found!: {0}", rsAddr);
                Console.CursorLeft = 0;
                sw.WriteLine("{0}: Addr: {2}, PassPhrase: {1}", DateTime.Now.ToString(), secret, rsAddr);
                sw.Flush();
                sw.Close();
            }
        }

        public static void SetArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("-"))
                {
                    Console.WriteLine("There was a problem with the your input.  Please run the application with the --help parameter for further assistance.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                switch (args[i])
                {
                    case "--prefix":
                    case "-p": pfx = args[++i]; break;
 
                    case "--beginswith":
                    case "-b":
                        bw = args[++i].ToUpper(); break;

                    case "--endswith":
                    case "-e":
                        ew = args[++i].ToUpper(); break;

                    case "--contains":
                    case "-c":
                        ew = args[++i].ToUpper(); break;

                    case "--regex":
                    case "-r":
                        regex = args[++i]; break;

                    case "--help":
                    case "-?":
                        Console.Clear();
                        Console.Write(
@"JupVanity : JUP Vanity Address Generator
This account generator, generates private keys resulting in vanity 
addresses for the Jupiter blockchain.

Once an address has been found, the application will create a file 
named jupvanity.txt in the root folder of the application, outputting the 
results.  If a jupvanity.txt file already exists, the application will append 
new results.

Thank you!

USAGE:

--prefix, -p: 
Choose your own prefix for the private key.  If not provided, JupVanity 
will randomly generate a 50 character private key.

--beginswith, -b:
Search for specific text at the beginning of the address.

--endswith, -e:
Search for specific text at the end of the address.

--contains, -c:
Search the specific string anywhere within the address.

--regex, -r:
Search for a specific regex pattern.

--help, -?:
Print this help document.

If you've found this application helpful, please send tips to 

JUP-7UJ5-YP4Z-V973-GREEN

Please also consider tipping the original author
who created this tool for NXT: 1102622531"
                        );
                        Console.ReadKey();
                        Environment.Exit(0);
                        break;
                }
            }
        }
        public static ulong CreateAddress(string privateKey)
        {
            SHA256 sha = SHA256.Create();
            byte[] sha1 = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(privateKey));

            //This needs to be done due to the Curve25519 implementation that's being used.
            sha1[0] &= 248; sha1[31] &= 127; sha1[31] |= 64;

            byte[] pubKey = Elliptic.Curve25519.GetPublicKey(sha1);
            byte[] sha2 = sha.ComputeHash(pubKey);
            return BitConverter.ToUInt64(new byte[] { sha2[0], sha2[1], sha2[2], sha2[3], sha2[4], sha2[5], sha2[6], sha2[7] }, 0);
        }
        private static string CreateRandomString(int length)
        {
            //string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789~`!@#$%^&*()_-+={[}]|\\:;\"'<,>.?/";
            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
            string result = "";
            Random r = new Random();
            while (result.Length < length) result += chars[r.Next(chars.Length - 1)];
            return result;
        }
    }
}

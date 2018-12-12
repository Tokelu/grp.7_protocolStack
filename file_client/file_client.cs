using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
    class file_client
    {
        /// <summary>
        /// The BUFSIZE.
        /// </summary>
        private const int BUFSIZE = 1000;
        private const string APP = "FILE_CLIENT.";

        /// <summary>
        /// Initializes a new instance of the <see cref="file_client"/> class.
        /// 
        /// </summary>
        /// <param name='args'>
        /// Filnavn med evtuelle sti.
        /// </param>
        private file_client(String[] args)
        {
            Transport transport = new Transport(BUFSIZE, APP);
            try
            {
                
                Console.WriteLine("Recipient file");
                string fileToReceive = (args.Length > 0) ? args[0] : "/test/test.txt";
                Console.WriteLine($"Requesting file: {fileToReceive}");
                var Filename = LIB.extractFileName(fileToReceive);
                Console.WriteLine($"File name: {Filename}");

                transport.sendText(fileToReceive);


                switch (transport.readText())
                {
                    case "FileFound":
                        Console.WriteLine($"File \"{Filename}\" exists. Commencing Transfer...");
                        receiveFile(Filename, transport);
                        break;
                    case "NoFileFound":
                        Console.WriteLine($"File  \"{Filename}\" does NOT exist on specified path. Aborting Transfer");
                        break;
                    default:
                        Console.WriteLine("unknown error");
                        break;
                }



                //if (transport.readText() == "FileFound")
                //{
                //    Console.WriteLine($"File \"{Filename}\" exists. Commencing Transfer...");
                //    receiveFile(Filename, transport);
                //}
                //else if (transport.readText() == "NoFileFound")
                //{
                //    Console.WriteLine($"File  \"{Filename}\" does NOT exist on specified path. Aborting Transfer");
                //}
                //else
                //{
                //    Console.WriteLine("unknown error");
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :(");
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Receives the file.
        /// </summary>
        /// <param name='fileName'>
        /// File name.
        /// </param>
        /// <param name='transport'>
        /// Transportlaget
        /// </param>
        private void receiveFile(String fileName, Transport transport)
        {
            // TO DO Your own code
            long fileSize = long.Parse(transport.readText());
            Console.WriteLine("Size of file: " + fileSize);
            byte[] RecData = new byte[BUFSIZE];
            int RecBytes;
            int totalrecbytes = 0;

            FileStream Fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);

            while (fileSize > totalrecbytes)
            {
                RecBytes = transport.receive(ref RecData);
                Fs.Write(RecData, 0, RecBytes);
                totalrecbytes += RecBytes;
                Console.Write("\r" + totalrecbytes + " Bytes of " + fileSize + " bytes received");
            }
            Console.WriteLine("\nTransfer completed");
            Fs.Close();
        }

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name='args'>
        /// First argument: Filname
        /// </param>
        public static void Main(string[] args)
        {
            // Transport transport=new Transport(BUFSIZE, APP);
            file_client client = new file_client(args);
        }
    }
}
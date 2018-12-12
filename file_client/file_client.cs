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
        /// file_client metoden opretter en peer-to-peer forbindelse
        /// Sender en forspÃ¸rgsel for en bestemt fil om denne findes pÃ¥ serveren
        /// Modtager filen hvis denne findes eller en besked om at den ikke findes (jvf. protokol beskrivelse)
        /// Lukker alle streams og den modtagede fil
        /// Udskriver en fejl-meddelelse hvis ikke antal argumenter er rigtige
        /// </summary>
        /// <param name='args'>
        /// Filnavn med evtuelle sti.
        /// </param>
        private file_client(String[] args)
        {
            // TO DO Your own code
            Transport transport = new Transport(BUFSIZE, APP);
            try
            {
                
                Console.WriteLine("Recipient file");
                string fileToReceive = (args.Length > 0) ? args[0] : "test.txt";
                Console.WriteLine($"Requesting file: {fileToReceive}");
                var Filename = LIB.extractFileName(fileToReceive);
                transport.sendText(fileToReceive);
                if (transport.readText() == "FileFound")
                {
                    Console.WriteLine($"File \"{Filename}\" exists. Commencing Transfer...");
                    receiveFile(Filename, transport);
                }
                else if (transport.readText() == "FileNotFound")
                {
                    Console.WriteLine($"File  \"{Filename}\" doen NOT exist on specified path. Aborting Transfer");
                }
                else
                {
                    Console.WriteLine("unknown error");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :(");
                Console.WriteLine(ex.Message);
            }
            finally
            {

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
                Console.Write("\rReceived " + totalrecbytes + " bytes from server");
            }
            Console.WriteLine("\nThe file was received - Closes the connection");
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
using System;
using System.IO;
using Transportlaget;
using Library;

namespace Application
{
    class file_server
    {
        /// <summary>
        /// The BUFSIZE
        /// </summary>
        private const int BUFSIZE = 1000;
        private const string APP = "FILE_SERVER";
        const int PORT = 9000;
        /// <summary>
        /// Initializes a new instance of the <see cref="file_server"/> class.
        /// </summary>
        private file_server()
        { 
            Transport transport = new Transport(BUFSIZE, APP);
           
            try
            {
                Console.WriteLine("Server ready - Awaiting Client");
                string fileToSend = transport.readText();
                Console.WriteLine("Client connected, want to pick up: " + fileToSend);
                long fileSize = LIB.check_File_Exists(fileToSend);
                if (fileSize != 0)
                {
                    transport.sendText("FileFound");
                    Console.WriteLine($"File {LIB.extractFileName(fileToSend)} exists. Transmitting... ");
                    sendFile(fileToSend, fileSize, transport);
                }
                else
                {
                    transport.sendText("FileNotFound");
                    Console.WriteLine($"File {LIB.extractFileName(fileToSend)} do NOT exists. Aborting Transmission... ");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //finally
            //{
            //    Console.WriteLine("Exits");
            //}
        }

        /// <summary>
        /// Sends the file.
        /// </summary>
        /// <param name='fileName'>
        /// File name.
        /// </param>
        /// <param name='fileSize'>
        /// File size.
        /// </param>
        /// <param name='tl'>
        /// Tl.
        /// </param>
        private void sendFile(String fileName, long fileSize, Transport transport)
        {
            // TO DO Your own code
            FileStream fileStream = null;

            try
            {
                Console.WriteLine("File size: " + fileSize);
                transport.sendText(fileSize.ToString());
                byte[] SendingBuffer = null;
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);//Find file
                int NoOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(fileStream.Length) / Convert.ToDouble(BUFSIZE)));
                int TotalLength = (int)fileStream.Length;
                int CurrentPacketLength, bytesSent = 0;

                for (int i = 1; i < NoOfPackets + 1; i++)
                {
                    if (TotalLength > BUFSIZE)
                    {
                        CurrentPacketLength = BUFSIZE;
                        TotalLength = TotalLength - CurrentPacketLength;
                        bytesSent += BUFSIZE;
                    }
                    else
                    {
                        CurrentPacketLength = TotalLength;
                        bytesSent += CurrentPacketLength;
                    }
                    SendingBuffer = new byte[CurrentPacketLength];
                    fileStream.Read(SendingBuffer, 0, CurrentPacketLength);
                    transport.send(SendingBuffer, (int)SendingBuffer.Length);
                    Console.Write("\r Transmitting packet" + i + " of " + NoOfPackets + " to client. - " + bytesSent + " bytes transmitted ");
                }
                Console.WriteLine("\nThe file was sent - Closes the connection");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name='args'>
        /// The command-line arguments.
        /// </param>
        public static void Main(string[] args)
        {
            while (true)
            {
                file_server server = new file_server();
            }
        }
    }
}
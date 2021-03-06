using System;
using Linklaget;

/// <summary>
/// Transport.
/// </summary>
namespace Transportlaget
{
    /// <summary>
    /// Transport.
    /// </summary>
    public class Transport
    {
        /// <summary>
        /// The link.
        /// </summary>
        private Link link;
        /// <summary>
        /// The 1' complements checksum.
        /// </summary>
        private Checksum checksum;
        /// <summary>
        /// The buffer.
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// The seq no.
        /// </summary>
        private byte seqNo;
        /// <summary>
        /// The old_seq no.
        /// </summary>
        private byte old_seqNo;
        /// <summary>
        /// The error count.
        /// </summary>
        private int errorCount;
        private int errorCount2;
        /// <summary>
        /// The DEFAULT_SEQNO.
        /// </summary>
        private const int DEFAULT_SEQNO = 2;
        /// <summary>
        /// The data received. True = received data in receiveAck, False = not received data in receiveAck
        /// </summary>
        private bool dataReceived;
        /// <summary>
        /// The number of data the recveived.
        /// </summary>
        /// 
        private int recvSize = 0;
        private const int HEADER_SIZE = 4;
        private int TRANSBUFSIZE = 1000;
        /// <summary>
        /// Initializes a new instance of the <see cref="Transport"/> class.
        /// </summary>
        public Transport(int BUFSIZE, string APP)
        {
            link = new Link(BUFSIZE + (int)TransSize.ACKSIZE, APP);
            checksum = new Checksum();
            buffer = new byte[BUFSIZE + (int)TransSize.ACKSIZE];
            seqNo = 0;
            old_seqNo = DEFAULT_SEQNO;
            errorCount = 0;
            dataReceived = false;
        }
        public void sendText(string textToSend)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            send(encoding.GetBytes(textToSend), textToSend.Length);
        }

        public string readText()
        {
            byte[] stringBuffer = new byte[TRANSBUFSIZE];
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            int bytesReceived = receive(ref stringBuffer);
            return encoding.GetString(stringBuffer, 0, bytesReceived);
        }

        /// <summary>
        /// Receives the ack.
        /// </summary>
        /// <returns>
        /// The ack.
        /// </returns>
        private bool receiveAck()
        {
            recvSize = link.receive(ref buffer);
            dataReceived = true;

            if (recvSize == (int)TransSize.ACKSIZE)
            {
                dataReceived = false;
                if (!checksum.checkChecksum(buffer, (int)TransSize.ACKSIZE) ||
                  buffer[(int)TransCHKSUM.SEQNO] != seqNo ||
                  buffer[(int)TransCHKSUM.TYPE] != (int)TransType.ACK)
                {
                    return false;
                }
                seqNo = (byte)((buffer[(int)TransCHKSUM.SEQNO] + 1) % 2);
            }

            return true;
        }

        /// <summary>
        /// Sends the ack.
        /// </summary>
        /// <param name='ackType'>
        /// Ack type.
        /// </param>
        private void sendAck(bool ackType)
        {
            byte[] ackBuf = new byte[(int)TransSize.ACKSIZE];
            ackBuf[(int)TransCHKSUM.SEQNO] = (byte)
                (ackType ? (byte)buffer[(int)TransCHKSUM.SEQNO] : (byte)(buffer[(int)TransCHKSUM.SEQNO] + 1) % 2);
            ackBuf[(int)TransCHKSUM.TYPE] = (byte)(int)TransType.ACK;
            checksum.calcChecksum(ref ackBuf, (int)TransSize.ACKSIZE);

            if (++errorCount2 == 10)
            {
                ackBuf[1]++;
                Console.WriteLine("  -   Noise introduced - byte 1 has been spoiled in ACK-message");
            }

            link.send(ackBuf, (int)TransSize.ACKSIZE);
        }

        /// <summary>
        /// Send the specified buffer and size.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        public void send(byte[] buf, int size)
        {
            var failedTransmissions = 0;
            var sumErrorCount = 0;

            do
            {
                buffer[(int)TransCHKSUM.SEQNO] = (byte)seqNo;
                buffer[(int)TransCHKSUM.TYPE] = (byte)TransType.DATA;

                Array.Copy(buf, 0, buffer, HEADER_SIZE, size);

                checksum.calcChecksum(ref buffer, size + HEADER_SIZE); // data + header

                    
                if (++errorCount == 100)
                {
                    buffer[0]++;
                    Console.WriteLine($"  -   Noise introduced - byte 0 has been spoiled in transmission ");
                    errorCount = 0;
                }
                link.send(buffer, size + HEADER_SIZE);
                failedTransmissions++;


            } while (!receiveAck() && failedTransmissions <= 5);

            old_seqNo = DEFAULT_SEQNO;
        }

        /// <summary>
        /// Receive the specified buffer.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        public int receive(ref byte[] buf)
        {
            // TO DO Your own code
            int receivedBytes = 0;
            bool receivedOK = false;
            do
            {
                receivedBytes = link.receive(ref buffer);
                receivedOK = checksum.checkChecksum(buffer, receivedBytes);
                sendAck(receivedOK);
                old_seqNo = buffer[(int)TransCHKSUM.SEQNO];
                Array.Copy(buffer, HEADER_SIZE, buf, 0, buf.Length);
            } while (!receivedOK);
            return receivedBytes - HEADER_SIZE;
        }
    }
}

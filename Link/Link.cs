using System;
using System.IO.Ports;

/// <summary>
/// Link.
/// </summary>
namespace Linklaget
{
    /// <summary>
    /// Link.
    /// </summary>
    public class Link
    {
        /// <summary>
        /// The DELIMITE for slip protocol.
        /// </summary>
        const byte DELIMITER = (byte)'A';
        /// <summary>
        /// The buffer for link.
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// The serial port.
        /// </summary>
        SerialPort serialPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="link"/> class.
        /// </summary>
        public Link(int BUFSIZE, string APP)
        {
            // Create a new SerialPort object with default settings.
#if DEBUG
            if (APP.Equals("FILE_SERVER"))
            {
                serialPort = new SerialPort("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);
            }
            else
            {
                serialPort = new SerialPort("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);
            }
#else
				serialPort = new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);
#endif
            if (!serialPort.IsOpen)
                serialPort.Open();

            buffer = new byte[(BUFSIZE * 2)];

            // Uncomment the next line to use timeout
            //serialPort.ReadTimeout = 500;

            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
        }

        /// <summary>
        /// Send the specified buf and size.
        /// </summary>
        /// <param name='buf'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        public void send(byte[] buf, int size)
        {

            var framedDataSize = Enframe(buf, size);
            serialPort.Write(buffer,0,framedDataSize);


            //// TO DO Your own code

            //buffer[0] = 65;//start A
            //int bytesToSendIndex = 1;
            //for (int i = 0; i < size; i++)
            //{
            //    if (buf[i] == 65)
            //    {
            //        buffer[bytesToSendIndex] = 66;//B
            //        bytesToSendIndex++;
            //        buffer[bytesToSendIndex] = 67;//C
            //        bytesToSendIndex++;
            //    }
            //    else if (buf[i] == 66)
            //    {
            //        buffer[bytesToSendIndex] = 66;//B

            //        bytesToSendIndex++;
            //        buffer[bytesToSendIndex] = 68;//D
            //        bytesToSendIndex++;
            //    }
            //    else
            //    {
            //        buffer[bytesToSendIndex] = buf[i];
            //        bytesToSendIndex++;
            //    }
            //}
            //buffer[bytesToSendIndex] = 65;//ending A
            //bytesToSendIndex++;
            //serialPort.Write(buffer, 0, bytesToSendIndex);
        }

        /// <summary>
        /// Receive the specified buf and size.
        /// </summary>
        /// <param name='buf'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        public int receive(ref byte[] buf)
        {

            var framedDataSize = Receive();

            var dataSize = Deframe(ref buf, framedDataSize);

            return dataSize;

            //// TO DO Your own code
            //byte[] tempBuf = new byte[1];
            //int bytesReceived = 0;
            //while (true)
            //{
            //    serialPort.Read(tempBuf, 0, 1);

            //    while (tempBuf[0] != 'A')
            //    {
            //    }
            //    bool registerStop = false;

            //    while (!registerStop)
            //    {

            //        serialPort.Read(tempBuf, 0, 1);

            //        if (tempBuf[0] == 'A')
            //            registerStop = true;
            //        else if (tempBuf[0] == 'B')
            //        {
            //            serialPort.Read(tempBuf, 0, 1);

            //            if (tempBuf[0] == 'C')
            //            {
            //                buf[bytesReceived] = 65;
            //                bytesReceived++;
            //            }
            //            else if (tempBuf[0] == 'D')
            //            {
            //                buf[bytesReceived] = 66;
            //                bytesReceived++;
            //            }
            //            else
            //                Console.WriteLine("Bad byte after B");
            //        }
            //        else
            //        {
            //            buf[bytesReceived] = tempBuf[0];
            //            bytesReceived++;
            //        }
            //    }
            //    return bytesReceived;
            //}
        }


        private int Receive()
        {
            while (!BeginReceive()) { }
            int counter = 0;
            while (counter < buffer.Length)
            {
                var received = (byte)serialPort.ReadByte();
                buffer[counter++] = received;
                if (received == DELIMITER)
                    break;
            }
            return counter;
        }

        private bool BeginReceive()
        {
            var received = (byte)serialPort.ReadByte();
            if (received == DELIMITER)
                return true;

            return false;
        }



        /// <summary>
        /// Deframes what is currently stored in buffer and returns this to target
        /// Size is the size of what is currently stored in buffer including the final delimiter
        /// </summary>
        /// <param name="">.</param>
        private int Deframe(ref byte[] target, int size)
        {
            var inserted = 0;
            for (var i = 0; i < size - 1; i++)
            {
                if (buffer[i] == (byte)'B')
                {
                    if (buffer[++i] == (byte)'C')
                        target[inserted++] = (byte)'A';
                    else
                        target[inserted++] = (byte)'B';

                    continue;
                }
                target[inserted++] = buffer[i];
            }
            return inserted;
        }


        /// <summary>
        /// Frames the given buf with the given size
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private int Enframe(byte[] buf, int size)
        {
            var counter = 0;
            var inserted = 0;

            buffer[inserted++] = DELIMITER;

            while (counter < size)
            {
                if (buf[counter] == DELIMITER)
                {
                    buffer[inserted++] = (byte)'B';
                    buffer[inserted++] = (byte)'C';
                }
                else if (buf[counter] == (byte)'B')
                {
                    buffer[inserted++] = (byte)'B';
                    buffer[inserted++] = (byte)'D';
                }
                else
                {
                    buffer[inserted++] = buf[counter];
                }
                counter++;
            }

            buffer[inserted++] = DELIMITER;
            return inserted;
        }

    }
}
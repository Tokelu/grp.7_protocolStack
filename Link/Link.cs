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
		const byte Delimiter = (byte)'A';
		/// <summary>
		/// The buffer for link.
		/// </summary>
		private readonly byte[] _buffer;

	    /// <summary>
	    /// The serial port.
	    /// </summary>
	    private static SerialPort _serialPort = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="link"/> class.
		/// </summary>
		public Link (int buffSize)
		{
			// Create a new SerialPort object with default settings.

			_serialPort = _serialPort ?? new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);

			if(!_serialPort.IsOpen)
				_serialPort.Open();

			_buffer = new byte[(buffSize*2) + 2]; // Added two extra for delimeters

			// Uncomment the next line to use timeout
			_serialPort.ReadTimeout = 500;

			_serialPort.DiscardInBuffer ();
			_serialPort.DiscardOutBuffer ();
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
		public void Send (byte[] buf, int size)
		{
			var byteCount = Frame (buf, size);
			_serialPort.Write(_buffer, 0, byteCount);
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
		public int Receive (ref byte[] buf)
		{			
			var sizeWithDelimiter = Receive ();
			var size = Deframe (ref buf, sizeWithDelimiter);
			return size;
		}

        #region Receive Utility

	    private int Receive()
	    {
	        while (!BeginReceive()) { }
	        int counter = 0;
	        while (counter < _buffer.Length)
	        {
	            var received = (byte)_serialPort.ReadByte();
	            _buffer[counter++] = received;
	            if (received == Delimiter)
	                break;
	        }
	        return counter;
	    }

	    private bool BeginReceive()
	    {
	        var received = (byte)_serialPort.ReadByte();
	        if (received == Delimiter)
	            return true;

	        return false;
	    }

        #endregion

        #region Framing

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
	            if (_buffer[i] == (byte)'B')
	            {
	                if (_buffer[++i] == (byte)'C')
	                    target[inserted++] = (byte)'A';
	                else
	                    target[inserted++] = (byte)'B';

	                continue;
	            }
	            target[inserted++] = _buffer[i];
	        }
	        return inserted;
	    }


	    /// <summary>
	    /// Frames the given buf with the given size
	    /// </summary>
	    /// <param name="buf"></param>
	    /// <param name="size"></param>
	    /// <returns></returns>
	    private int Frame(byte[] buf, int size)
	    {
	        var counter = 0;
	        var inserted = 0;

	        _buffer[inserted++] = Delimiter;

	        while (counter < size)
	        {
	            if (buf[counter] == Delimiter)
	            {
	                _buffer[inserted++] = (byte)'B';
	                _buffer[inserted++] = (byte)'C';
	            }
	            else if (buf[counter] == (byte)'B')
	            {
	                _buffer[inserted++] = (byte)'B';
	                _buffer[inserted++] = (byte)'D';
	            }
	            else
	            {
	                _buffer[inserted++] = buf[counter];
	            }
	            counter++;
	        }

	        _buffer[inserted++] = Delimiter;
	        return inserted;
	    }

        #endregion

    }
}

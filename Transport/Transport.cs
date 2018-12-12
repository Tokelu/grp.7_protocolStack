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
		/// The _link.
		/// </summary>
		private readonly Link _link;
		/// <summary>
		/// The 1' complements _checksum.
		/// </summary>
		private readonly Checksum _checksum;
		/// <summary>
		/// The _buffer.
		/// </summary>
		private byte[] _buffer;
		/// <summary>
		/// The seq no.
		/// </summary>
		private byte _seqNr;
		/// <summary>
		/// The error count.
		/// </summary>
		private int _errorCount;
		/// <summary>
		/// The DefaultSeqNr.
		/// </summary>
		private const int DefaultSeqNr = 2;
        /// <summary>
        /// For simulating noise
        /// </summary>
		private int _noiseSimulation = 0;
        /// <summary>
		/// Initializes a new instance of the <see cref="Transport"/> class.
		/// </summary>
		public Transport (int buffSize)
		{
			_link = new Link(buffSize+(int)TransSize.ACKSIZE);
			_checksum = new Checksum();
			_buffer = new byte[buffSize+(int)TransSize.ACKSIZE];
			_seqNr = 0;
			_errorCount = 0;
		}

		/// <summary>
		/// Send the specified _buffer and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public void Send(byte[] buf, int size)
		{
            // Construct the package
			var packageSize = ConstructPackage(buf, size);

		    while (_errorCount < 5)
		    {
		        try
		        {
		            do
		            {
		                _link.Send(_buffer, packageSize);
		            } while (ReceiveAck() != _seqNr);
		            NextSeqNo();
		            break;
		        }
		        catch (TimeoutException)
		        {
		            _errorCount++;
		        }
		    }

		    _errorCount = 0;
		}

	    public int Receive(ref byte[] buff)
	    {
	        var readSize = 0;

	        while (readSize == 0 && _errorCount < 5) 
	        {
	            try
	            {
	                while ((readSize = _link.Receive(ref _buffer)) > 0)
	                {
	                    if (_checksum.checkChecksum(_buffer, readSize))
	                    {
                            SendAck(true);
	                        if (_buffer[(int) TransCHKSUM.SEQNO] == _seqNr)
	                        {
	                            NextSeqNo();
	                            readSize = buff.Length < readSize - (int)TransSize.ACKSIZE ? buff.Length : readSize - (int)TransSize.ACKSIZE;
	                            Array.Copy(_buffer, (int)TransSize.ACKSIZE, buff, 0, readSize);
	                            break;
	                        }
	                            
                            continue;
	                    }

	                    SendAck(false);
	                }                   
	            }
	            catch (TimeoutException)
	            {
	                readSize = 0;
                    _errorCount++;
	            }

            }

	        _errorCount = 0;
	        return readSize;
	    }

        #region Utility

	    /// <summary>
	    /// Receives the ack.
	    /// </summary>
	    /// <returns>
	    /// The ack.
	    /// </returns>
	    private byte ReceiveAck()
	    {
	        byte[] buf = new byte[(int)TransSize.ACKSIZE];
	        int size = _link.Receive(ref buf);

	        if (size != (int)TransSize.ACKSIZE) return DefaultSeqNr;

	        if (!_checksum.checkChecksum(buf, (int)TransSize.ACKSIZE) ||
	            buf[(int)TransCHKSUM.SEQNO] != _seqNr || buf[(int)TransCHKSUM.TYPE] != (int)TransType.ACK)
	            return DefaultSeqNr;

	        return _seqNr;
	    }

	    /// <summary>
	    /// Sends the ack.
	    /// </summary>
	    /// <param name='ackType'>
	    /// Ack type.
	    /// </param>
	    private void SendAck(bool ackType)
	    {
	        byte[] ackBuf = new byte[(int)TransSize.ACKSIZE];
	        ackBuf[(int)TransCHKSUM.SEQNO] = (byte)
	            (ackType ? _buffer[(int)TransCHKSUM.SEQNO] : (byte)(_buffer[(int)TransCHKSUM.SEQNO] + 1) % 2);
	        ackBuf[(int)TransCHKSUM.TYPE] = (byte)(int)TransType.ACK;
	        _checksum.calcChecksum(ref ackBuf, (int)TransSize.ACKSIZE);

	        if (++_noiseSimulation == 2)
	        {
	            ackBuf[0]++;
	            _noiseSimulation = 0;
	        }

	        _link.Send(ackBuf, (int)TransSize.ACKSIZE);
	    }

        private void NextSeqNo()
	    {
	        _seqNr = (byte)((_seqNr + 1) % 2);
	    }

	    private int ConstructPackage(byte[] data, int size)
	    {
	        _buffer[(int)TransCHKSUM.SEQNO] = _seqNr;
	        _buffer[(int)TransCHKSUM.TYPE] = (int)TransType.DATA;
	        for (var i = 0; i < size; i++)
	        {
	            _buffer[i + (int)TransSize.ACKSIZE] = data[i];
	        }
	        size = size + (int)TransSize.ACKSIZE;
	        _checksum.calcChecksum(ref _buffer, size);
	        return size;
	    }

        #endregion


    }
}
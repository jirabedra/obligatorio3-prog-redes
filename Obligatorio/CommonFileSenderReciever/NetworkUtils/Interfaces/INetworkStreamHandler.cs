using System;
using System.Collections.Generic;
using System.Text;

namespace CommonFileSenderReceiver.NetworkUtils.Interfaces
{
    public interface INetworkStreamHandler
    {
        void Write(byte[] data);
        byte[] Read(int length);
    }
}

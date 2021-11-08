using System;
using System.Collections.Generic;
using System.Text;

namespace CommonFileSenderReceiver.Protocol
{
    public static class SpecificationHelper
    {
        public static long GetParts(long fileSize)
        {
            var parts = fileSize / Specification.MaxPacketSize;
            return parts * Specification.MaxPacketSize == fileSize ? parts : parts + 1;
        }
    }
}

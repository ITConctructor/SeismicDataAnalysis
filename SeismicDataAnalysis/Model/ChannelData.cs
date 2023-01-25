using System.Collections.Generic;

namespace SeismicDataAnalysis.Model
{
    public class ChannelData : FileData
    {
        public override string ToString()
        {
            return ChannelNumber.ToString();
        }
    }
}

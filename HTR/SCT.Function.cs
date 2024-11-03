using System;

namespace HTR
{
    public partial class SCT
    {
        public SCT()
        {

        }

        public string getTmp(int da)
        {
            float f = da / 100.0f;

            return f.ToString("f2");
        }
    }
}

//end
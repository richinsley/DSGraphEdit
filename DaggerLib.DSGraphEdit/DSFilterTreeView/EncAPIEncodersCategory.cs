using System;
using DirectShowLib;

namespace DaggerLib.DSGraphEdit
{
    public class EncAPIEncodersCategory : BaseFilterCategory
    {
        public override string CategoryName
        {
            get
            {
                return "EncAPI Encoders";
            }
        }

        protected override Guid Category
        {
            get { return FilterCategory.MediaEncoderCategory; }
        }

        public override Guid DMOCategory
        {
            get { return Guid.Empty; }
        }
    }
}
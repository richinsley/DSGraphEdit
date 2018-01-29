using System;

using DirectShowLib;

namespace DaggerLib.DSGraphEdit
{
    public class EncAPIMultiplexersCategory : BaseFilterCategory
    {
        public override string CategoryName
        {
            get
            {
                return "EncAPI Multiplexers";
            }
        }

        protected override Guid Category
        {
            get { return FilterCategory.MediaMultiplexerCategory; }
        }

        public override Guid DMOCategory
        {
            get { return Guid.Empty; }
        }
    }
}
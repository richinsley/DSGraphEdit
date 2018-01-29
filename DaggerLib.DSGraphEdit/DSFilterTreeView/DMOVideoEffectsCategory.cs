using System;

using DirectShowLib;
using DirectShowLib.DMO;

namespace DaggerLib.DSGraphEdit
{
    public class DMOVideoEffectsCategory : BaseFilterCategory
    {
        public override string CategoryName
        {
            get
            {
                return "DMO Video Effects";
            }
        }

        protected override Guid Category
        {
            get { return DirectShowLib.DMO.DMOCategory.VideoEffect; }
        }

        public override Guid DMOCategory
        {
            get { return DirectShowLib.DMO.DMOCategory.VideoEffect; }
        }
    }
}

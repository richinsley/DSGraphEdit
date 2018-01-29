using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

using DirectShowLib;
using DirectShowLib.DMO;

namespace DaggerLib.DSGraphEdit
{
    public class DMOAudioEffectsCategory : BaseFilterCategory
    {
        public override string CategoryName
        {
            get
            {
                return "DMO Audio Effects";
            }
        }

        protected override Guid Category
        {
            get { return DirectShowLib.DMO.DMOCategory.AudioEffect; }
        }

        public override Guid DMOCategory
        {
            get { return DirectShowLib.DMO.DMOCategory.AudioEffect; }
        }
    }
}

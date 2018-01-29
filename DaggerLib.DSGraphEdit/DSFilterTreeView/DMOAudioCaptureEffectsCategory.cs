using System;

using DirectShowLib;
using DirectShowLib.DMO;

namespace DaggerLib.DSGraphEdit
{
    public class DMOAudioCaptureEffectsCategory : BaseFilterCategory
    {
        public override string CategoryName
        {
            get
            {
                return "DMO Audio Capture Effects";
            }
        }

        protected override Guid Category
        {
            get { return DirectShowLib.DMO.DMOCategory.AudioCaptureEffect; }
        }

        public override Guid DMOCategory
        {
            get { return DirectShowLib.DMO.DMOCategory.AudioCaptureEffect; }
        }
    }
}

using System;

using DirectShowLib;
using DirectShowLib.DMO;

namespace DaggerLib.DSGraphEdit
{
    public class AudioEffects1Category : BaseFilterCategory
    {
        public override string CategoryName
        {
            get
            {
                return "Audio Effects 1";
            }
        }

        protected override Guid Category
        {
            get { return FilterCategory.AudioEffects1Category; }
        }

        public override Guid DMOCategory
        {
            get { return Guid.Empty; }
        }
    }

    public class AudioEffects2Category : BaseFilterCategory
    {
        public override string CategoryName
        {
            get
            {
                return "Audio Effects 2";
            }
        }

        protected override Guid Category
        {
            get { return FilterCategory.AudioEffects2Category; }
        }

        public override Guid DMOCategory
        {
            get { return Guid.Empty; }
        }
    }

    public class VideoEffects1Category : BaseFilterCategory
    {
        public override string CategoryName
        {
            get
            {
                return "Video Effects 1";
            }
        }

        protected override Guid Category
        {
            get { return FilterCategory.VideoEffects1Category; }
        }

        public override Guid DMOCategory
        {
            get { return Guid.Empty; }
        }
    }

    public class VideoEffects2Category : BaseFilterCategory
    {
        public override string CategoryName
        {
            get
            {
                return "Video Effects 2";
            }
        }

        protected override Guid Category
        {
            get { return FilterCategory.VideoEffects2Category; }
        }

        public override Guid DMOCategory
        {
            get { return Guid.Empty; }
        }
    }
}

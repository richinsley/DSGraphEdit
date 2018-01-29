using System;
using DirectShowLib;

namespace DaggerLib.DSGraphEdit
{
    public class StandardFilterCategory : BaseFilterCategory
    {
        DsDevice _category;
        Guid _clsid;

        public StandardFilterCategory(DsDevice category)
        {
            _category = category;
            _clsid = DSFilterTreeViewNode.GetMonikerGuid(category.Mon);
        }

        public override string CategoryName
        {
            get
            {
                return _category.Name;
            }
        }

        protected override Guid Category
        {
            get { return _clsid; }
        }

        public override Guid DMOCategory
        {
            get { return Guid.Empty; }
        }
    }
}
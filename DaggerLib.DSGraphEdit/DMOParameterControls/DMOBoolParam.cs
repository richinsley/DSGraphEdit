using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using DirectShowLib;
using DirectShowLib.DMO;

namespace DaggerLib.DSGraphEdit
{
    [ToolboxItem(false)]
    public class DMOBoolParam : CheckBox
    {
        IMediaParams _param;
        int _paramNum;
        ParamInfo _pInfo;

        public DMOBoolParam(IMediaParams param, int paramNum, ParamInfo pInfo)
        {
            _param = param;
            _paramNum = paramNum;
            _pInfo = pInfo;

            Text = "";
            Size = new System.Drawing.Size(15, 19);
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            
            MPData val;
            param.GetParam(_paramNum, out val);
            if (_pInfo.mopCaps == MPCaps.Jump)
            {
                Checked = val.vBool;
            }
            else
            {
                Checked = (val.vFloat == 0) ? false : true;
            }

            CheckedChanged += new EventHandler(DMOBoolParam_CheckedChanged);
        }

        void DMOBoolParam_CheckedChanged(object sender, EventArgs e)
        {
            MPData val = new MPData();
            if (_pInfo.mopCaps == MPCaps.Jump)
            {
                val.vBool = Checked;
            }
            else
            {
                val.vFloat = Checked ? 1f : 0f;
            }

            _param.SetParam(_paramNum, val);
        }
    }
}

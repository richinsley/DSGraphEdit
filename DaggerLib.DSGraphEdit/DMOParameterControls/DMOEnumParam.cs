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
    public class DMOEnumParam : ComboBox
    {
        IMediaParams _param;
        int _paramNum;
        ParamInfo _pInfo;

        public DMOEnumParam(string[] sEnum, IMediaParams param, int paramNum, ParamInfo pInfo)
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            Dock = DockStyle.Fill;
            _param = param;
            _paramNum = paramNum;
            _pInfo = pInfo;

            for (int i = 0; i < sEnum.Length; i++)
            {
                Items.Add(sEnum[i]);
            }
            MPData val;
            param.GetParam(_paramNum, out val);
            if (_pInfo.mopCaps == MPCaps.Jump)
            {
                SelectedIndex = val.vInt;
            }
            else
            {
                SelectedIndex = (int)((float)(Items.Count - 1) * val.vFloat);
            }
            SelectedIndexChanged += new EventHandler(DMOEnumParam_SelectedIndexChanged);
        }

        void DMOEnumParam_SelectedIndexChanged(object sender, EventArgs e)
        {
            MPData val = new MPData();
            if (_pInfo.mopCaps == MPCaps.Jump)
            {
                val.vInt = SelectedIndex;
            }
            else
            {
                if (SelectedIndex == 0)
                {
                    val.vFloat = 0f;
                }
                else
                {
                    val.vFloat = (float)SelectedIndex / (float)(Items.Count - 1);
                }
            }
            _param.SetParam(_paramNum, val);
        }
    }
}

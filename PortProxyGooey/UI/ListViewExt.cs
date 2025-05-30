﻿// Source: https://stackoverflow.com/a/71009137/553663

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

[ToolboxItem(false)]
public partial class ListViewExt : ListView
{
    private Color _groupHeadingBackColor = Color.Gray;

    public Color GroupHeadingBackColor
    {
        get
        {
            return _groupHeadingBackColor;
        }
        set
        {
            _groupHeadingBackColor = value;
        }
    }

    private Color _groupHeadingForeColor = Color.Black;

    public Color GroupHeadingForeColor
    {
        get
        {
            return _groupHeadingForeColor;
        }
        set
        {
            _groupHeadingForeColor = value;
        }
    }

    private Font _groupHeadingFont;

    public Font GroupHeadingFont
    {
        get
        {
            return _groupHeadingFont;
        }
        set
        {
            _groupHeadingFont = value;
        }
    }

    private Color _separatorColor;

    public Color SeparatorColor
    {
        get
        {
            return _separatorColor;
        }
        set
        {
            _separatorColor = value;
        }
    }

    public const int LVCDI_ITEM = 0x0;
    public const int LVCDI_GROUP = 0x1;
    public const int LVCDI_ITEMSLIST = 0x2;

    public const int LVM_FIRST = 0x1000;
    public const int LVM_GETGROUPRECT = LVM_FIRST + 98;
    public const int LVM_ENABLEGROUPVIEW = LVM_FIRST + 157;
    public const int LVM_SETGROUPINFO = LVM_FIRST + 147;
    public const int LVM_GETGROUPINFO = LVM_FIRST + 149;
    public const int LVM_REMOVEGROUP = LVM_FIRST + 150;
    public const int LVM_MOVEGROUP = LVM_FIRST + 151;
    public const int LVM_GETGROUPCOUNT = LVM_FIRST + 152;
    public const int LVM_GETGROUPINFOBYINDEX = LVM_FIRST + 153;
    public const int LVM_MOVEITEMTOGROUP = LVM_FIRST + 154;

    public const int WM_LBUTTONUP = 0x202;

    [StructLayout(LayoutKind.Sequential)]
    public partial struct NMHDR
    {
        public IntPtr hwndFrom;
        public IntPtr idFrom;
        public int code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct NMCUSTOMDRAW
    {
        public NMHDR hdr;
        public int dwDrawStage;
        public IntPtr hdc;
        public RECT rc;
        public IntPtr dwItemSpec;
        public uint uItemState;
        public IntPtr lItemlParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct NMLVCUSTOMDRAW
    {
        public NMCUSTOMDRAW nmcd;
        public int clrText;
        public int clrTextBk;
        public int iSubItem;
        public int dwItemType;
        public int clrFace;
        public int iIconEffect;
        public int iIconPhase;
        public int iPartId;
        public int iStateId;
        public RECT rcText;
        public uint uAlign;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public partial struct LVGROUP
    {
        public uint cbSize;
        public uint mask;
        // <MarshalAs(UnmanagedType.LPTStr)>
        // Public pszHeader As String
        public IntPtr pszHeader;
        public int cchHeader;
        // <MarshalAs(UnmanagedType.LPTStr)>
        // Public pszFooter As String
        public IntPtr pszFooter;
        public int cchFooter;
        public int iGroupId;
        public uint stateMask;
        public uint state;
        public uint uAlign;

        // <MarshalAs(UnmanagedType.LPTStr)>
        // Public pszSubtitle As String
        public IntPtr pszSubtitle;
        public uint cchSubtitle;
        // <MarshalAs(UnmanagedType.LPTStr)>
        // Public pszTask As String
        public IntPtr pszTask;
        public uint cchTask;
        // <MarshalAs(UnmanagedType.LPTStr)>
        // Public pszDescriptionTop As String
        public IntPtr pszDescriptionTop;
        public uint cchDescriptionTop;
        // <MarshalAs(UnmanagedType.LPTStr)>
        // Public pszDescriptionBottom As String
        public IntPtr pszDescriptionBottom;
        public uint cchDescriptionBottom;
        public int iTitleImage;
        public int iExtendedImage;
        public int iFirstItem;
        public uint cItems;
        // <MarshalAs(UnmanagedType.LPTStr)>
        // Public pszSubsetTitle As String
        public IntPtr pszSubsetTitle;
        public uint cchSubsetTitle;
    }

    [Flags]
    public enum CDRF
    {
        CDRF_DODEFAULTField = 0x0,
        CDRF_NEWFONTField = 0x2,
        CDRF_SKIPDEFAULTField = 0x4,
        CDRF_DOERASEField = 0x8,
        CDRF_SKIPPOSTPAINTField = 0x100,
        CDRF_NOTIFYPOSTPAINTField = 0x10,
        CDRF_NOTIFYITEMDRAWField = 0x20,
        CDRF_NOTIFYSUBITEMDRAWField = 0x20,
        CDRF_NOTIFYPOSTERASEField = 0x40
    }

    [Flags]
    public enum CDDS
    {
        CDDS_PREPAINTField = 0x1,
        CDDS_POSTPAINTField = 0x2,
        CDDS_PREERASEField = 0x3,
        CDDS_POSTERASEField = 0x4,
        CDDS_ITEMField = 0x10000,
        CDDS_ITEMPREPAINTField = CDDS_ITEMField | CDDS_PREPAINTField,
        CDDS_ITEMPOSTPAINTField = CDDS_ITEMField | CDDS_POSTPAINTField,
        CDDS_ITEMPREERASEField = CDDS_ITEMField | CDDS_PREERASEField,
        CDDS_ITEMPOSTERASEField = CDDS_ITEMField | CDDS_POSTERASEField,
        CDDS_SUBITEMField = 0x20000
    }

    public const int LVGF_NONE = 0x0;
    public const int LVGF_HEADER = 0x1;
    public const int LVGF_FOOTER = 0x2;
    public const int LVGF_STATE = 0x4;
    public const int LVGF_ALIGN = 0x8;
    public const int LVGF_GROUPID = 0x10;

    public const int LVGF_SUBTITLE = 0x100; // pszSubtitle is valid
    public const int LVGF_TASK = 0x200; // pszTask is valid
    public const int LVGF_DESCRIPTIONTOP = 0x400; // pszDescriptionTop is valid
    public const int LVGF_DESCRIPTIONBOTTOM = 0x800; // pszDescriptionBottom is valid
    public const int LVGF_TITLEIMAGE = 0x1000; // iTitleImage is valid
    public const int LVGF_EXTENDEDIMAGE = 0x2000; // iExtendedImage is valid
    public const int LVGF_ITEMS = 0x4000; // iFirstItem and cItems are valid
    public const int LVGF_SUBSET = 0x8000; // pszSubsetTitle is valid
    public const int LVGF_SUBSETITEMS = 0x10000; // readonly, cItems holds count of items in visible subset, iFirstItem is valid

    public const int LVGS_NORMAL = 0x0;
    public const int LVGS_COLLAPSED = 0x1;
    public const int LVGS_HIDDEN = 0x2;
    public const int LVGS_NOHEADER = 0x4;
    public const int LVGS_COLLAPSIBLE = 0x8;
    public const int LVGS_FOCUSED = 0x10;
    public const int LVGS_SELECTED = 0x20;
    public const int LVGS_SUBSETED = 0x40;
    public const int LVGS_SUBSETLINKFOCUSED = 0x80;

    public const int LVGA_HEADER_LEFT = 0x1;
    public const int LVGA_HEADER_CENTER = 0x2;
    public const int LVGA_HEADER_RIGHT = 0x4; // Don't forget to validate exclusivity
    public const int LVGA_FOOTER_LEFT = 0x8;
    public const int LVGA_FOOTER_CENTER = 0x10;
    public const int LVGA_FOOTER_RIGHT = 0x20; // Don't forget to validate exclusivity

    public const int LVGGR_GROUP = 0; // Entire expanded group
    public const int LVGGR_HEADER = 1;  // Header only (collapsed group)
    public const int LVGGR_LABEL = 2;  // Label only
    public const int LVGGR_SUBSETLINK = 3;  // subset link only

    public ListViewExt()
    {
        _groupHeadingFont = this.Font;
    }

    [DllImport("User32.dll", EntryPoint = "SendMessageW", SetLastError = true)]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref IntPtr lParam);

    [DllImport("User32.dll", EntryPoint = "SendMessageW", SetLastError = true)]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref LVGROUP lParam);

    [DllImport("User32.dll", EntryPoint = "SendMessageW", SetLastError = true)]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref RECT lParam);

    [DllImport("User32.dll", EntryPoint = "PostMessageW", SetLastError = true)]
    public static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, ref IntPtr lParam);

    public int SetGroupInfo(IntPtr hWnd, int nGroupID, uint nSate)
    {
        LVGROUP lvg = new();

        lvg.cbSize = (uint)Marshal.SizeOf(lvg);
        lvg.mask = LVGF_STATE | LVGF_GROUPID | LVGF_HEADER;

        // for test
        int nRet2 = SendMessage(hWnd, LVM_GETGROUPINFO, nGroupID, ref lvg);

        lvg.state = nSate;
        lvg.mask = LVGF_STATE;
        nRet2 = SendMessage(hWnd, LVM_SETGROUPINFO, nGroupID, ref lvg);
        return -1;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_REFLECT + WM_NOFITY)
        {
            NMHDR pnmhdr = (NMHDR)m.GetLParam(typeof(NMHDR));
            if (pnmhdr.code == NM_CUSTOMDRAW)
            {
                NMLVCUSTOMDRAW pnmlv = (NMLVCUSTOMDRAW)m.GetLParam(typeof(NMLVCUSTOMDRAW));
                switch (pnmlv.nmcd.dwDrawStage)
                {
                    case (int)CDDS.CDDS_PREPAINTField:
                        {
                            if (pnmlv.dwItemType == LVCDI_GROUP)
                            {
                                var rectHeader = new RECT();
                                rectHeader.top = LVGGR_HEADER;
                                int nItem = (int)pnmlv.nmcd.dwItemSpec;
                                // If (nItem = 0) Then
                                int nRet = SendMessage(m.HWnd, LVM_GETGROUPRECT, nItem, ref rectHeader);
                                using (Graphics g = Graphics.FromHdc(pnmlv.nmcd.hdc))
                                {
                                    Rectangle rect = new Rectangle(rectHeader.left, rectHeader.top, rectHeader.right - rectHeader.left, rectHeader.bottom - rectHeader.top);

                                    // Dim linGrBrush As New LinearGradientBrush(New System.Drawing.Point(0, 0), New System.Drawing.Point(rectHeader.right, rectHeader.bottom), Color.Blue, Color.LightCyan)
                                    SolidBrush BgBrush = new SolidBrush(_groupHeadingBackColor);
                                    g.FillRectangle(BgBrush, rect);

                                    LVGROUP lvg = new();
                                    lvg.cbSize = (uint)Marshal.SizeOf(lvg);
                                    lvg.mask = LVGF_STATE | LVGF_GROUPID | LVGF_HEADER;
                                    int nRet2 = SendMessage(m.HWnd, LVM_GETGROUPINFO, nItem, ref lvg);
                                    string sText = Marshal.PtrToStringUni(lvg.pszHeader);

                                    SizeF textSize = g.MeasureString(sText, _groupHeadingFont);

                                    int RectHeightMiddle = (int)Math.Round((rect.Height - textSize.Height) / 2f);

                                    rect.Offset(10, RectHeightMiddle);

                                    using (SolidBrush drawBrush = new SolidBrush(_groupHeadingForeColor))
                                    {
                                        g.DrawString(sText, _groupHeadingFont, drawBrush, rect);
                                        rect.Offset(0, -RectHeightMiddle);

                                        using (SolidBrush lineBrush = new SolidBrush(_separatorColor))
                                        {
                                            g.DrawLine(
                                                new Pen(lineBrush),
                                                rect.X + g.MeasureString(sText, _groupHeadingFont).Width + 10,
                                                rect.Y + (int)Math.Round(rect.Height / 2d),
                                                rect.X + (int)Math.Round(rect.Width * 95 / 100d),
                                                rect.Y + (int)Math.Round(rect.Height / 2d));
                                        }
                                    }
                                }
                                m.Result = new IntPtr((int)CDRF.CDRF_SKIPDEFAULTField);
                            }
                            // End If
                            else
                            {
                                m.Result = new IntPtr((int)CDRF.CDRF_NOTIFYITEMDRAWField);
                            }

                            break;
                        }
                    case (int)CDDS.CDDS_ITEMPREPAINTField:
                        {
                            m.Result = new IntPtr((int)(CDRF.CDRF_NOTIFYSUBITEMDRAWField | CDRF.CDRF_NOTIFYPOSTPAINTField));
                            break;
                        }
                    case (int)CDDS.CDDS_ITEMPOSTPAINTField:
                        {
                            break;
                        }
                }
            }
            return;
        }
        else
        {
            base.WndProc(ref m);
        }
    }

    private const int NM_FIRST = 0;
    private const int NM_CLICK = NM_FIRST - 2;
    private const int NM_CUSTOMDRAW = NM_FIRST - 12;
    private const int WM_REFLECT = 0x2000;
    private const int WM_NOFITY = 0x4E;
}
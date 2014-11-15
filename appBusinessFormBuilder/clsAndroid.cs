using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Data;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net;
using Android.Graphics.Drawables;


using Android.Util;

using Java.IO;

using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

namespace appBusinessFormBuilder
{
    public class AndroidUtils
    {
        public enum ColorType { Background = 1, Text, Gridlines, Border, RadioGroupHighlight };
        public class ProgressBar
        {
            Context m_context;
            ProgressDialog m_progress_dialog;
            int m_iStyle = 0;

            public void SetContext(Context context)
            {
                m_context = context;
            }

            public void SetStyle(int iStyle)
            {
                m_iStyle = iStyle;
            }

            public void CreateProgressBar()
            {
                ProgressDialog progress_dialog = new ProgressDialog(m_context);
                switch (m_iStyle)
                {
                    case 0:
                    default:
                        progress_dialog.SetIcon(Android.Resource.Attribute.AlertDialogStyle);
                        progress_dialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                        break;
                    case 1:
                        progress_dialog.SetIcon(Android.Resource.Attribute.AlertDialogStyle);
                        progress_dialog.SetProgressStyle(ProgressDialogStyle.Spinner);
                        break;
                }
                progress_dialog.SetCanceledOnTouchOutside(false);
                progress_dialog.SetCancelable(false);
                m_progress_dialog = progress_dialog;
                //progress_dialog.SetButton(-1, GetText(Resource.String.alert_dialog_ok), OkClicked);
                //progress_dialog.SetButton(-2, GetText(Resource.String.alert_dialog_cancel), CancelClicked);
            }

            public void SetProgressBarTitle(string sTitle)
            {
                m_progress_dialog.SetTitle(sTitle);
            }

            public void ShowProgressBar(int iMaximum)
            {
                m_progress_dialog.Max = iMaximum;
                m_progress_dialog.Progress = 0;
                m_progress_dialog.Show();
            }

            public void UpdateProgressBar(int iCounter)
            {
                int iCurrentPercentage = m_progress_dialog.Progress;
                int iIncrement = iCounter - iCurrentPercentage;
                m_progress_dialog.IncrementProgressBy(iIncrement);
            }

            public void CloseProgressBar()
            {
                m_progress_dialog.Hide();
            }

            public void RemoveProgressBar()
            {
                m_progress_dialog.Dismiss();
//                m_progress_dialog.Dispose();
            }
        }

        public class ColorClass
        {
            public Color GetColor(string sColor, int iType)
            {
                Android.Graphics.Color color = new Color();
                switch (sColor)
                {
                    case "Antique White":
                        color = Color.AntiqueWhite;
                        break;
                    case "Aqua":
                        color = Color.Aqua;
                        break;
                    case "Beige":
                        color = Color.Beige;
                        break;
                    case "Black":
                        color = Color.Black;
                        break;
                    case "Blue":
                        color = Color.Blue;
                        break;
                    case "Brown":
                        color = Color.Brown;
                        break;
                    case "Coral":
                        color = Color.Coral;
                        break;
                    case "Crimson":
                        color = Color.Crimson;
                        break;
                    case "Cyan":
                        color = Color.Cyan;
                        break;
                    case "Dark Blue":
                        color = Color.DarkBlue;
                        break;
                    case "Dark Gray":
                        color = Color.DarkGray;
                        break;
                    case "Dark Green":
                        color = Color.DarkGreen;
                        break;
                    case "Dark Orange":
                        color = Color.DarkOrange;
                        break;
                    case "Dark Red":
                        color = Color.DarkRed;
                        break;
                    case "Dark Violet":
                        color = Color.DarkViolet;
                        break;
                    case "Deep Pink":
                        color = Color.DeepPink;
                        break;
                    case "Lime":
                        color = Color.Lime;
                        break;
                    case "White":
                        color = Color.White;
                        break;
                    default:
                        switch (iType)
                        {
                            case (int)ColorType.Border:
                            case (int)ColorType.Text:
                                color = Color.Black;
                                break;
                            case (int)ColorType.Gridlines:
                                color = Color.Gray;
                                break;
                            case (int)ColorType.RadioGroupHighlight:
                                color = Color.Yellow;
                                break;
                            case (int)ColorType.Background:
                            default:
                                color = Color.White;
                                break;
                        }
                        break;
                }

                return color;
            }


        }
        public class DropdownBox
        {
            Context m_context;
            Spinner m__spinner;
            ArrayAdapter m_Adapter;

            public void SetContext(Context context)
            {
                m_context = context;
            }

            public Spinner GetSpinner()
            {
                m__spinner.Adapter = m_Adapter;
                return m__spinner;
            }

            public void SetSpinnerAdapterValues(string[] strArray)
            {
                int i;

                for (i = 0; i < strArray.Length; i++)
                {
                    m_Adapter.Add(strArray[i]);
                }
            }

            public bool SetMainResource(int iResourceId)
            {
                if (m_context == null)
                {
                    return false;
                }
                ArrayAdapter arrAdapt = new ArrayAdapter(m_context, iResourceId);
                m_Adapter = arrAdapt;
                return true;
            }

            public void SetDropdownResource(int iResourceId)
            {
                m_Adapter.SetDropDownViewResource(iResourceId);
            }

        }

        public class AlertBox
        {
            Context m_context;
            AlertDialog m_alert_dialog;
            string m_Message;

            public void SetContext(Context context)
            {
                m_context = context;
            }

            public void CreateAlertDialog()
            {
                AlertDialog ad = new AlertDialog.Builder(m_context).Create();
                ad.SetCancelable(false); // This blocks the 'BACK' button  
                ad.SetButton("OK", (s, ee) => { RemoveAlertDialog(); });
                m_alert_dialog = ad;
            }

            public void SetAlertMessage(string sMessage)
            {
                m_Message = sMessage;
                m_alert_dialog.SetMessage(sMessage);
            }

            public void ShowAlertBox()
            {
                m_alert_dialog.Show();
            }

            public void RemoveAlertDialog()
            {
            }

        }

        public class DrawCellBorder
        {
            Context m_context;
            TableRow m_row;

            public void SetContext(Context context)
            {
                m_context = context;
            }

            public void SetRow(TableRow row)
            {
                m_row = row;
            }

            public void SetRightBorder()
            {
                TableRow.LayoutParams llp = new TableRow.LayoutParams(TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent);
                llp.SetMargins(0, 0, 2, 0);//2px right-margin

                TextView lblCell = new TextView(m_context);
                lblCell.Text = "";
                lblCell.SetWidth(2);
                lblCell.SetTextColor(Android.Graphics.Color.Black);
                lblCell.SetPadding(0, 0, 0, 0);

                LinearLayout cell = new LinearLayout(m_context);
                cell.SetBackgroundColor(Android.Graphics.Color.Black);
                cell.LayoutParameters = llp;//2px border on the right for the cell
                cell.AddView(lblCell);
                m_row.AddView(cell);
            }

            public void SetLeftBorder()
            {
                TableRow.LayoutParams llp = new TableRow.LayoutParams(TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent);
                llp.SetMargins(2, 0, 0, 0);//2px right-margin

                TextView lblCell = new TextView(m_context);
                lblCell.Text = "";
                lblCell.SetWidth(2);
                lblCell.SetTextColor(Android.Graphics.Color.Black);
                lblCell.SetPadding(0, 0, 0, 0);

                LinearLayout cell = new LinearLayout(m_context);
                cell.SetBackgroundColor(Android.Graphics.Color.Black);
                cell.LayoutParameters = llp;//2px border on the right for the cell
                cell.AddView(lblCell);
                m_row.AddView(cell);
            }

            public void SetTopBorder()
            {
                TableRow.LayoutParams llp = new TableRow.LayoutParams(TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent);
                llp.SetMargins(0, 2, 0, 0);//2px right-margin

                TextView lblCell = new TextView(m_context);
                lblCell.Text = "";
                lblCell.SetWidth(2);
                lblCell.SetTextColor(Android.Graphics.Color.Black);
                lblCell.SetPadding(0, 0, 0, 0);

                LinearLayout cell = new LinearLayout(m_context);
                cell.SetBackgroundColor(Android.Graphics.Color.Black);
                cell.LayoutParameters = llp;//2px border on the right for the cell
                cell.AddView(lblCell);
                m_row.AddView(cell);
            }

            public void SetBottomBorder()
            {
                TableRow.LayoutParams llp = new TableRow.LayoutParams(TableRow.LayoutParams.WrapContent, TableRow.LayoutParams.WrapContent);
                llp.SetMargins(0, 0, 0, 2);//2px right-margin

                TextView lblCell = new TextView(m_context);
                lblCell.Text = "";
                lblCell.SetWidth(2);
                lblCell.SetTextColor(Android.Graphics.Color.Black);
                lblCell.SetPadding(0, 0, 0, 0);

                LinearLayout cell = new LinearLayout(m_context);
                cell.SetBackgroundColor(Android.Graphics.Color.Black);
                cell.LayoutParameters = llp;//2px border on the right for the cell
                cell.AddView(lblCell);
                m_row.AddView(cell);
            }
        }

        public class ComboBox
        {
            public int PopulateAdapter(ref ArrayAdapter adapter, ArrayList sSQL, string sValue, bool bIncludeSelect, ref string sRtnMsg)
            {
                string sSQLString = "";
                int i;

                for (i = 0; i < sSQL.Count; i++)
                {
                    sSQLString += sSQL[i].ToString() + ";";
                }

                return PopulateAdapter(ref adapter, sSQLString, sValue, bIncludeSelect, ref sRtnMsg);

            }

            public int PopulateAdapter(ref ArrayAdapter adapter, string sSQL, string sValue, bool bIncludeSelect, ref string sRtnMsg)
            {
                LocalDB DB = new LocalDB();
                int i;
                int iSelectedIndex = -1;
                DataSet ds;
                bool bFound = false;
                ArrayList sTableName = new ArrayList();
                ArrayList sColNames = new ArrayList();

                if (sSQL == "")
                {
                    if (bIncludeSelect)
                    {
                        adapter.Add("[select]");
                        iSelectedIndex++;
                    }

                    adapter.Add("");

                    return iSelectedIndex;


                }

                if (sSQL.Contains(";"))
                {
                    sSQL = sSQL.Trim();
                    if (sSQL.EndsWith(";"))
                    {
                        sSQL = sSQL.Substring(0, sSQL.Length - 1);
                    }
                    string[] arrValues = sSQL.Split(';');
                    if (bIncludeSelect)
                    {
                        adapter.Add("[select]");
                    }
                    for (i = 0; i < arrValues.Length; i++)
                    {
                        adapter.Add(arrValues[i]);
                        if (arrValues[i] == sValue && !bFound)
                        {
                            iSelectedIndex = i;
                            if (bIncludeSelect)
                            {
                                iSelectedIndex++;
                            }
                            bFound = true;
                        }
                    }

                    return iSelectedIndex;
                }
                else
                {
                    sColNames = DB.GetColumnNamesFromSQL(sSQL, ref sRtnMsg);
                    if(sColNames.Count > 1)
                    {
                        iSelectedIndex = -1;
                        sRtnMsg = "You cannot have more than 1 column in your drop down box. Please fix your SQL.";
                        adapter.Add(sRtnMsg);
                        return iSelectedIndex;
                    }
                    else
                    {
                        string[] sColumnNames = new string[1];
                        sColumnNames[0] = sColNames[0].ToString();
                        //Read the database
                        ds = DB.ReadSQLDataSet(sSQL, sColumnNames, ref sRtnMsg);

                        bFound = false;
                        if (bIncludeSelect)
                        {
                            adapter.Add("[select]");
                        }
                        if (ds != null)
                        {
                            if (ds.Tables.Count > 0)
                            {
                                for (i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {
                                    string sItem = ds.Tables[0].Rows[i].ItemArray[0].ToString();
                                    adapter.Add(sItem);
                                    if (sItem == sValue && !bFound)
                                    {
                                        iSelectedIndex = i;
                                        if (bIncludeSelect)
                                        {
                                            iSelectedIndex++;
                                        }

                                        bFound = true;
                                    }
                                }
                            }
                        }
                    }

                    return iSelectedIndex;
                }
            }
        }

        public class GridBuildView
        {
            //Simple constants for the main sections
            enum SectionType { Form = 1, Header, Detail, Footer, HeaderRow, HeaderColumn, DetailRow, DetailColumn, FooterRow, FooterColumn, GridItem };
            enum ItemType { Label = 1, TextBox, TextArea, DropDown, Checkbox, RadioButton, Button, DatePicker, TimePicker, ProgressBar, ColumnHeader, RowHeader, ColumnDetail, RowDetail, ColumnFooter, RowFooter };
            string sRtnMsg = "";
            Context m_context;
            Activity m_activity;
            string m_text = "";
            Button m_detailbtn;
            EditText m_txt;
            Spinner m_Spinner;
            RadioGroup m_RadGrp;
            int m_width;
            float m_density;
            int m_cellid;
            bool m_bShowGridLines = false;
            int m_iGridlineWeight = 0;
            int m_iBuildType = 0;
            int m_iRowHeight = 70;
//            int m_ControlHeight = 70;
            int m_iTopPaddingCell = 0;
            int m_iBottomPaddingCell = 0;
            int m_iRightPaddingCell = 0;
            int m_iLeftPaddingCell = 0;
            string m_sAlign = "Left";
            int m_iRecordCounter = -1;
            string m_DDSQL = "";
            bool m_bIncludeSelect = false;
            int m_iRadioGroupOrientation = 0;
            string m_sRadioGroupLabels = "";
            string m_sRadioGroupValues = "";

            //Text stuff
            Typeface m_Typeface = Typeface.SansSerif;
            TypefaceStyle m_TypefaceStyle = TypefaceStyle.Normal;

            //Some minumums required
            int iMinimumHeight = 30;
            int iMinimumBuildHeight = 70;
            int iMinimumWidthBuild = 120;
            int iMinimumWidth = 40;

            //Color defaults
            int iBackground = 1;
            int iText = 2;
            int iGridlines = 3;
            int iBorder = 4;

            Android.Graphics.Color m_BackgroundColor = Android.Graphics.Color.Wheat;
            Android.Graphics.Color m_BorderColor = Android.Graphics.Color.Black;
            Android.Graphics.Color m_GridlineColor = Android.Graphics.Color.LightGray;
            Android.Graphics.Color m_RadioGroupHighlightColor = Android.Graphics.Color.Yellow;

            public void SetMainActivity(Activity activity)
            {
                m_activity = activity;
            }

            public void SetContext(Context context)
            {
                m_context = context;
            }

            public void SetDensity(float fDensity)
            {
                m_density = fDensity;
            }

            public void SetText(string sText)
            {
                m_text = sText;
            }

            public void SetRecordCounter(int iRecordCounter)
            {
                m_iRecordCounter = iRecordCounter;
            }

            public void SetRowWidth(int iWidthinDp)
            {
                m_width = iWidthinDp;
            }

            public void SetId(int iCellId)
            {
                m_cellid = iCellId;
            }

            public void SetBuildType(int iBuildType)
            {
                m_iBuildType = iBuildType;
            }

            public void SetGridLines(bool bGridLines)
            {
                m_bShowGridLines = bGridLines;
            }

            public void SetGridLineWeight(int iWeight)
            {
                m_iGridlineWeight = iWeight;
            }

            public void SetCellPadding(int iLeft, int iTop, int iRight, int iBottom)
            {
                m_iLeftPaddingCell = iLeft;
                m_iTopPaddingCell = iTop;
                m_iRightPaddingCell = iRight;
                m_iBottomPaddingCell = iBottom;
            }

            public void SetRowHeight(int iHeight)
            {
                m_iRowHeight = iHeight;
                if (m_iRowHeight < iMinimumHeight)
                {
                    m_iRowHeight = iMinimumHeight;
                }
            }

            //public void SetControlHeight(int iHeight)
            //{
            //    m_ControlHeight = iHeight;
            //    if (m_ControlHeight < iMinimumHeight)
            //    {
            //        m_ControlHeight = iMinimumHeight;
            //    }
            //}

            public void SetTextAlignment(string sAlignment)
            {
                m_sAlign = sAlignment;
            }

            public void SetBackgroundColor(string sColor)
            {
                SetColor(sColor, (int)ColorType.Background);
            }

            public void SetBorderColor(string sColor)
            {
                SetColor(sColor, (int)ColorType.Border);
            }

            public void SetGridLinesColor(string sColor)
            {
                SetColor(sColor, (int)ColorType.Gridlines);
            }

            public void SetRadioGroupHighlightColor(string sColor)
            {
                SetColor(sColor, (int)ColorType.Gridlines);
            }

            public void SetColor(string sColor, int iType)
            {

                ColorClass clsColor = new ColorClass();
                Android.Graphics.Color color = new Color();
                color = clsColor.GetColor(sColor, iType);
                switch (iType)
                {
                    case (int)ColorType.Background:
                        m_BackgroundColor = color;
                        break;
                    case (int)ColorType.Border:
                        m_BorderColor = color;
                        break;
                    case (int)ColorType.Gridlines:
                        m_GridlineColor = color;
                        break;
                    case (int)ColorType.RadioGroupHighlight:
                        m_RadioGroupHighlightColor = color;
                        break;
                        
                }
            }

            public void SetDropdownSQL(string sSQL)
            {
                m_DDSQL = sSQL;
            }

            public void SetDropdownSelectShowing(bool bIncludeSelect)
            {
                m_bIncludeSelect = bIncludeSelect;
            }

            //0 = Horizontal, 1= Vertical
            public void SetRadioGroupOrientation(int iOrientation)
            {
                m_iRadioGroupOrientation = iOrientation;
            }

            public void SetRadioGroupLabels(string sLabelsSemiColonSeparated)
            {
                m_sRadioGroupLabels = sLabelsSemiColonSeparated;
            }

            public void SetRadioGroupValues(string sValuesSemiColonSeparated)
            {
                m_sRadioGroupValues = sValuesSemiColonSeparated;
            }

            public void SetTextFont(string sFont)
            {

                switch (sFont)
                {
                    case "Sans Serif":
                        m_Typeface = Typeface.SansSerif;
                        break;
                    case "Serif":
                        m_Typeface = Typeface.Serif;
                        break;
                    case "Monospace":
                        m_Typeface = Typeface.Monospace;
                        break;
                    default:
                        m_Typeface = Typeface.Default;
                        break;
                }
            }

            public void SetTextStyle(string sBold, string sItalic)
            {
                string sStyle = sBold + ";" + sItalic;

                switch (sStyle)
                {
                    case "Yes;":
                    case "Yes;No":
                        m_TypefaceStyle = TypefaceStyle.Bold;
                        break;
                    case ";Yes":
                    case "No;Yes":
                        m_TypefaceStyle = TypefaceStyle.Italic;
                        break;
                    case "Yes;Yes":
                        m_TypefaceStyle = TypefaceStyle.BoldItalic;
                        break;
                    default:
                        m_TypefaceStyle = TypefaceStyle.Normal;
                        break;
                }
            }

            public View GetCellView(int iType)
            {
                float fExtraPadding = 0f;

                TableLayout table = new TableLayout(m_context);
                table.SetBackgroundColor(m_GridlineColor);

                if (m_bShowGridLines)
                {
                    table.SetPadding(m_iGridlineWeight, m_iGridlineWeight, m_iGridlineWeight, m_iGridlineWeight);
                    fExtraPadding = 2 * m_iGridlineWeight;
                }
                else
                {
                    table.SetPadding(0, 0, 0, 0);
                    fExtraPadding = 0f;
                }

                if (m_iBuildType == 1)
                {
                    fExtraPadding += 64f;
                }

                TableRow.LayoutParams params1 = new TableRow.LayoutParams();
                params1.Height = ConvertPixelsToDp(m_iRowHeight);
                TableRow row = new TableRow(m_context);
                row.SetBackgroundColor(m_BorderColor);
                row.LayoutParameters = params1;
                row.SetGravity(GravityFlags.Center);

                if (m_iBuildType == 1)
                {
                    if (m_width < ConvertPixelsToDp(iMinimumWidthBuild))
                    {
                        m_width = ConvertPixelsToDp(iMinimumWidthBuild);
                    }

                    iMinimumBuildHeight = 64 + m_iTopPaddingCell + m_iBottomPaddingCell;
                    if (m_iRowHeight < ConvertPixelsToDp(iMinimumBuildHeight))
                    {
                        m_iRowHeight = ConvertPixelsToDp(iMinimumBuildHeight);
                    }

                    //if (m_ControlHeight < ConvertPixelsToDp(iMinimumHeight))
                    //{
                    //    m_ControlHeight = ConvertPixelsToDp(iMinimumHeight);
                    //}
                }
                else
                {
                    if (m_width < ConvertPixelsToDp(iMinimumWidth))
                    {
                        m_width = ConvertPixelsToDp(iMinimumWidth);
                    }

                    if (m_iRowHeight < ConvertPixelsToDp(iMinimumHeight))
                    {
                        m_iRowHeight = ConvertPixelsToDp(iMinimumHeight);
                    }
                }



                row.SetPadding(m_iLeftPaddingCell, m_iTopPaddingCell, m_iRightPaddingCell, m_iBottomPaddingCell);
                //                row.SetMinimumHeight(ConvertPixelsToDp(m_iRowHeight));

                switch (iType)
                {
                    case (int)ItemType.Label:
                    case (int)ItemType.ColumnHeader:
                    case (int)ItemType.RowHeader:
                    case (int)ItemType.ColumnDetail:
                    case (int)ItemType.RowDetail:
                    case (int)ItemType.ColumnFooter:
                    case (int)ItemType.RowFooter:
                    default:
                        TextView txt = new TextView(m_context);
                        txt.Text = m_text;
                        txt.SetWidth(m_width - ConvertPixelsToDp(fExtraPadding + m_iLeftPaddingCell + m_iRightPaddingCell));
                        txt.SetTextColor(Android.Graphics.Color.Black);
                        if (iType == (int)ItemType.ColumnHeader || iType == (int)ItemType.ColumnDetail || iType == (int)ItemType.ColumnFooter)
                        {
                            txt.SetBackgroundResource(appBusinessFormBuilder.Resource.Drawable.NShapedTextBox);
                            GradientDrawable bgShape = (GradientDrawable)txt.Background;
                            bgShape.SetColor(Android.Graphics.Color.Wheat);
                        }
                        else if (iType == (int)ItemType.RowHeader || iType == (int)ItemType.RowDetail || iType == (int)ItemType.RowFooter)
                        {
                            txt.SetBackgroundResource(appBusinessFormBuilder.Resource.Drawable.CShapedTextBox);
                            GradientDrawable bgShape = (GradientDrawable)txt.Background;
                            bgShape.SetColor(Android.Graphics.Color.Wheat);
                        }
                        else
                        {
                            txt.SetBackgroundColor(m_BackgroundColor);
                        }
                        txt.Id = m_cellid + 100;
                        txt.SetPadding(2, 2, 2, 2);
                        txt.SetHeight(m_iRowHeight - (m_iTopPaddingCell + m_iBottomPaddingCell));
                        switch(m_sAlign)
                        {
                            case "Left":
                                txt.Gravity = GravityFlags.Left;
                                break;
                            case "Center":
                                txt.Gravity = GravityFlags.Center;
                                break;
                            case "Right":
                                txt.Gravity = GravityFlags.Right;
                                break;
                        }
                        row.AddView(txt);
                        break;
                    case (int)ItemType.TextBox:
                        //TextView rect = new TextView(m_context);
                        //rect.SetHeight(m_ControlHeight - ConvertPixelsToDp(20));
                        //SetWidth(m_width - ConvertPixelsToDp(fExtraPadding));
                        //row.AddView(rect);

                        int iEditTextResource = appBusinessFormBuilder.Resource.Layout.textbox;
                        LayoutInflater li = LayoutInflater.From(m_context);                        
                        EditText txtEdit = (EditText)li.Inflate(iEditTextResource, null);
                        txtEdit.Text = m_text;
                        txtEdit.SetWidth(m_width - ConvertPixelsToDp(fExtraPadding + m_iLeftPaddingCell + m_iRightPaddingCell));
                        txtEdit.SetTextColor(Android.Graphics.Color.Blue);
                        txtEdit.SetBackgroundColor(Android.Graphics.Color.Beige);
                        txtEdit.Id = m_cellid + 100;
                        txtEdit.SetSingleLine(true);
                        txtEdit.SetPadding(2, 2, 2, 2);
                        txtEdit.SetHeight(m_iRowHeight - (m_iTopPaddingCell + m_iBottomPaddingCell)); //This has to be dynamic
                        txtEdit.Gravity = GravityFlags.Center;
                        if (m_iBuildType == 1)
                        {
                            txtEdit.Enabled = false;
                        }
                        row.AddView(txtEdit);
                        m_txt = txtEdit;
                        break;
                    case (int)ItemType.TextArea:
                        int iEditTextAreaResource = appBusinessFormBuilder.Resource.Layout.textbox;
                        LayoutInflater li2 = LayoutInflater.From(m_context);
                        EditText txtEdit2 = (EditText)li2.Inflate(iEditTextAreaResource, null);
                        txtEdit2.Text = m_text;
                        txtEdit2.SetWidth(m_width - ConvertPixelsToDp(fExtraPadding));
                        txtEdit2.SetTextColor(Android.Graphics.Color.Black);
                        txtEdit2.SetSingleLine(false);
//                        txtEdit2.SetHeight(ConvertPixelsToDp(98));
                        txtEdit2.Id = m_cellid + 100;
                        txtEdit2.SetPadding(2, 2, 2, 2);
                        if (m_iBuildType == 1)
                        {
                            txtEdit2.Enabled = false;
                        }
                        row.AddView(txtEdit2);
                        m_txt = txtEdit2;
//                        row.SetMinimumHeight(ConvertPixelsToDp(98));
                        break;
                    case (int)ItemType.DropDown:
                        TableRow.LayoutParams params2 = new TableRow.LayoutParams();
                        AndroidUtils.ComboBox cmbBox0 = new AndroidUtils.ComboBox();
                        ArrayAdapter arrCmbItems0 = new ArrayAdapter(m_context, Resource.Layout.layoutSpinner); //This is the resource for the main box
                        //Now get the info from the database for the drop down items
                        if (m_iBuildType == 1)
                        {
                            m_DDSQL = m_text + ";";
                        }
                        int iSelectedIndex = cmbBox0.PopulateAdapter(ref arrCmbItems0, m_DDSQL, m_text, m_bIncludeSelect, ref sRtnMsg);
                        arrCmbItems0.SetDropDownViewResource(Resource.Layout.layoutSpinnerBase); //This is the resource for the drop down
                        Spinner cmbEdit0 = new Spinner(m_context);
                        cmbEdit0.Adapter = arrCmbItems0;
                        cmbEdit0.Id = m_cellid + 100;
                        cmbEdit0.SetPadding(2, 2, 2, 2);
                        //cmbEdit0.SetBackgroundColor(m_BackgroundColor);
                        cmbEdit0.LayoutParameters = params2;

                        ViewGroup.LayoutParams lp = cmbEdit0.LayoutParameters;
                        lp.Width = m_width - ConvertPixelsToDp(fExtraPadding);
                        lp.Height = m_iRowHeight - (m_iTopPaddingCell + m_iBottomPaddingCell);
                        cmbEdit0.LayoutParameters = lp;
                        cmbEdit0.SetBackgroundResource(Resource.Drawable.defaultSpinner2);

                        cmbEdit0.SetSelection(iSelectedIndex);

                        //Drawable bgShapeDD = (Drawable)cmbEdit0.Background;
                        //Drawable.ConstantState bgDrawableCont = (Drawable.ConstantState)bgShapeDD.GetConstantState();
                        //LayerDrawable item1 = (LayerDrawable) bgShapeDD.Current;
                        //GradientDrawable item2 = (GradientDrawable)item1.GetDrawable(0);
                        //item2.SetColor(m_BackgroundColor);
//                        LayerDrawable item1 = bgDrawableCont.NewDrawable(;

//                        LayerDrawable selectedItem = (LayerDrawable)bgDrawableCont.;
//                        bgShapeDD.(m_BackgroundColor);
//                        Resource rsc = Resource.Layout.layoutSpinnerBase;

                        if (m_iBuildType == 1)
                        {
                            cmbEdit0.Enabled = false;
                        }

                        row.AddView(cmbEdit0);
                        m_Spinner = cmbEdit0;
                        break;
                    case (int)ItemType.RadioButton:

//                        LinearLayout.LayoutParams paramsRad = new LinearLayout.LayoutParams(m_width - ConvertPixelsToDp(2*fExtraPadding),(m_iRowHeight - (m_iTopPaddingCell + m_iBottomPaddingCell))/2);
                        TableRow.LayoutParams paramsRad = new TableRow.LayoutParams(m_width - ConvertPixelsToDp(fExtraPadding), m_iRowHeight - (m_iTopPaddingCell + m_iBottomPaddingCell));
                        RadioGroup radGrp = new RadioGroup(m_context);
                        if(m_iRadioGroupOrientation == 0)
                        {
                            radGrp.Orientation = Android.Widget.Orientation.Horizontal;
                        }
                        else
                        {
                            radGrp.Orientation = Android.Widget.Orientation.Vertical;
                        }

                        m_sRadioGroupValues = m_sRadioGroupValues.Trim();
                        if (m_sRadioGroupValues.EndsWith(";"))
                        {
                            m_sRadioGroupValues = m_sRadioGroupValues.Substring(0, m_sRadioGroupValues.Length - 1);
                        }
                        string[] sValues = m_sRadioGroupValues.Split(';');

                        m_sRadioGroupLabels = m_sRadioGroupLabels.Trim();
                        if (m_sRadioGroupLabels.EndsWith(";"))
                        {
                            m_sRadioGroupLabels = m_sRadioGroupLabels.Substring(0, m_sRadioGroupLabels.Length - 1);
                        }
                        string[] sLabels = m_sRadioGroupLabels.Split(';');

                        for (int i = 0; i < sValues.Length; i++)
                        {
                            RadioButton radBtn = new RadioButton(m_context);
                            string sLabelRad = "";
                            if (sLabels[i] != null)
                            {
                                sLabelRad = sLabels[i];
                            }
                            radBtn.Text = sLabelRad;
                            radBtn.SetTextColor(Android.Graphics.Color.Black);
                            radBtn.SetTypeface(m_Typeface, m_TypefaceStyle);
                            radBtn.SetBackgroundColor(m_BackgroundColor);
                            radBtn.SetHighlightColor(m_RadioGroupHighlightColor);
                            radBtn.Id = m_cellid + 100 + (i + 1);
                            switch (m_sAlign)
                            {
                                case "Left":
                                    radBtn.Gravity = GravityFlags.Left;
                                    break;
                                case "Center":
                                    radBtn.Gravity = GravityFlags.Center;
                                    break;
                                case "Right":
                                    radBtn.Gravity = GravityFlags.Right;
                                    break;
                            }

                            if (m_text == sValues[i])
                            {
                                radBtn.Checked = true;
                            }

                            radGrp.AddView(radBtn);
                        }

                        radGrp.Id = m_cellid + 100;
                        radGrp.SetBackgroundColor(m_BackgroundColor);
                        radGrp.SetPadding(2, 2, 2, 2);
                        radGrp.LayoutParameters = paramsRad;
                        //ViewGroup.LayoutParams lpRad = radGrp.LayoutParameters;
                        ////lpRad.Width = m_width - ConvertPixelsToDp(fExtraPadding);
                        //lpRad.Height = m_iRowHeight - (m_iTopPaddingCell + m_iBottomPaddingCell);
                        //radGrp.LayoutParameters = lpRad;
                        radGrp.SetGravity(GravityFlags.Center);
                        if (m_iBuildType == 1)
                        {
                            radGrp.Enabled = false;
                        }
                        row.AddView(radGrp);
                        m_RadGrp = radGrp;
                        break;
                }
                
                //In every case add a hidden textview that holds the original value
                if (iType != (int)ItemType.ColumnHeader && iType != (int)ItemType.RowHeader)
                {
                    TextView txtHidden = new TextView(m_context);
                    txtHidden.Text = m_text;
                    txtHidden.Id = m_cellid + 800;
                    txtHidden.Visibility = ViewStates.Gone;
                    row.AddView(txtHidden);
                }

                if (m_iRecordCounter > 0)
                {
                    TextView txtHiddenRC = new TextView(m_context);
                    txtHiddenRC.Text = m_iRecordCounter.ToString();
                    txtHiddenRC.Id = m_cellid + 700;
                    txtHiddenRC.Visibility = ViewStates.Gone;
                    row.AddView(txtHiddenRC);
                }

                //Now add a button for the attributes
                if (m_iBuildType == 1)
                {
                    Button btn = new Button(m_context);
                    btn.Text = "+";
                    btn.SetWidth(ConvertPixelsToDp(64f)); //Appears that it is a minimum of 64 pixels for a button
                    btn.SetHeight(ConvertPixelsToDp(64f));
                    btn.Id = m_cellid + 900;
                    btn.Gravity = GravityFlags.Center;
                    row.AddView(btn);
                    m_detailbtn = btn;
                }

                table.AddView(row);
                return table;

            }
            
            public Button GetDetailButton()
            {
                return m_detailbtn;
            }

            public EditText GetCellEditTextView()
            {
                return m_txt;
            }

            public Spinner GetCellDropdownView()
            {
                return m_Spinner;
            }

            private int ConvertPixelsToDp(float pixelValue)
            {
                var dp = (int)((pixelValue) * m_density);
                return dp;
            }

        }
        public class AndroidFile
        {
            //Here the folder has to be from /mnt/sdcard downwards
            public void CreatePublicDirectoryForFiles(string sFolder)
            {
                Java.IO.File sDir = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.AbsolutePath), sFolder);
                if (!sDir.Exists())
                {
                    sDir.Mkdirs();
                }
                sDir.Dispose();
            }

            public bool DeleteFile(string sFile)
            {
                string sTargetFolderAndFileName = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/" + Android.OS.Environment.DataDirectory.AbsolutePath + @"/" +sFile; //Need back slashes for the target
                Java.IO.File fle = new Java.IO.File(sTargetFolderAndFileName);
                //                    fle.SetWritable(true);
                bool bReturn = fle.Delete();
                fle.Dispose();
                return bReturn;
            }

            public void FolderDelete(string sFolder)
            {
                string sTargetFolderAndFileName = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/" + Android.OS.Environment.DataDirectory.AbsolutePath + @"/" + sFolder; //Need back slashes for the target
                Java.IO.File fle = new Java.IO.File(sTargetFolderAndFileName);
                recursiveDelete(fle);
                return;
            }

            private void recursiveDelete(Java.IO.File fileOrDirectory) 
            {
                if (fileOrDirectory.IsDirectory)
                {
                    Java.IO.File[] child = fileOrDirectory.ListFiles();
                    int iFiles = child.Length;
                    for (int i = 0; i < iFiles;i++)
                        recursiveDelete(child[i]);
                }

                fileOrDirectory.Delete();
            }

            //Note that the length of both the source and target arrays should be the same
            public bool DownloadMutlipleFiles(ArrayList sSourcePathAndFileName, ArrayList sTargetFolderAndFileName, int iSecureFlag, ref string sRtnMessage)
            {
                WebClient wc = new WebClient();
                string sTargetFolder;
                string sTargetFileWithoutRoot;
                Int32 i;
                clsLocalUtils DTUtil = new clsLocalUtils();
                DTUtil.SetSecureFlag(iSecureFlag);

                try
                {
                    //Firstly chgeck to see if the sdcard is mounted
                    if(Android.OS.Environment.ExternalStorageState.Equals(Android.OS.Environment.MediaMounted))
                    {
                        if (sSourcePathAndFileName.Count > 0)
                        {
                            wc.Credentials = new System.Net.NetworkCredential(DTUtil.sUser, DTUtil.sPassword, DTUtil.sDomain);
                            for (i = 0; i < sSourcePathAndFileName.Count; i++)
                            {
                                if (sSourcePathAndFileName[i].ToString().Contains("//silcar-sp11/"))
                                {
                                    string sSourceTemp = sSourcePathAndFileName[i].ToString();
                                    sSourceTemp = sSourceTemp.Replace("//silcar-sp11/", "//silcar-sp11.silcar.com.au/");
                                    sSourcePathAndFileName[i] = sSourceTemp;
                                }
                                sTargetFileWithoutRoot = sTargetFolderAndFileName[i].ToString();
                                sTargetFolderAndFileName[i] = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/" + Android.OS.Environment.DataDirectory.AbsolutePath + @"/" + sTargetFolderAndFileName[i].ToString(); //Need back slashes for the target
                                sSourcePathAndFileName[i] = sSourcePathAndFileName[i].ToString().Replace(@"\", @"/");
                                sSourcePathAndFileName[i] = DTUtil.StripMultiSlashes(sSourcePathAndFileName[i].ToString());
                                sTargetFolderAndFileName[i] = DTUtil.StripMultiSlashes(sTargetFolderAndFileName[i].ToString());

                                if (System.IO.File.Exists(sTargetFolderAndFileName[i].ToString()))
                                {
                                    System.IO.File.Delete(sTargetFolderAndFileName[i].ToString());
                                }

                                //sTargetFolder = sTargetFolderAndFileName[i].ToString();
                                if (sTargetFileWithoutRoot.Contains(@"/"))
                                {
                                    sTargetFolder = sTargetFileWithoutRoot.Substring(0, sTargetFileWithoutRoot.LastIndexOf(@"/"));

                                    if (!System.IO.Directory.Exists(DTUtil.StripMultiSlashes(Android.OS.Environment.DataDirectory.AbsolutePath + "/" + sTargetFolder)))
                                    {
                                        CreatePublicDirectoryForFiles(sTargetFolder);
                                    }
                                }

                                string sSource = sSourcePathAndFileName[i].ToString();
                                if (DTUtil.iEnvironment == 2)
                                {
                                    sSource = sSource.Replace("http://", "https://");
                                }
                                wc.DownloadFile(sSource, sTargetFolderAndFileName[i].ToString());
                            }

                            wc.Dispose();
                        }
                        else
                        {
                            sSourcePathAndFileName = null;
                        }

                        return true;
                    }
                    else
                    {
                        sRtnMessage = "The SD Card (disk space for the tablet) is either not mounted or is not present. The system cannot download any files at this time.";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    sRtnMessage = ex.Message.ToString();
                    return false;
                }

            }

        }

        //public class ZoomView : RelativeLayout      
        //{

        //    private ScaleGestureDetector mScaleDetector;    
        //    private static float mScaleFactor = 1.0f;   


        //    public ZoomView (Context context) :    
        //        base (context)
        //        {

        //        Initialize ();    
        //    }



        //    public ZoomView (Context context, IAttributeSet attrs) :base (context,attrs)    
        //    {
        //            Initialize ();    

        //    }



        //    public ZoomView (Context context, IAttributeSet attrs, int defStyle) :

        //    base (context, attrs, defStyle)
        //    {

        //        Initialize ();
        //    }

        //    void Initialize ()
        //    {

        //        mScaleDetector = new ScaleGestureDetector(Context, new ScaleListener(this));

        //    }

        //    public override bool OnTouchEvent (MotionEvent e)
        //    {
        //        mScaleDetector.OnTouchEvent(e);
        //        return true;
        //    }

        //    protected override void OnDraw(Android.Graphics.Canvas canvas) {

        //        base.OnDraw(canvas);    

        //        canvas.Save();    
        //        canvas.Scale(mScaleFactor, mScaleFactor);
        //        this.Draw(canvas);
        //        canvas.Restore();
        //    }



        //    private class ScaleListener :ScaleGestureDetector.SimpleOnScaleGestureListener 
        //    {

        //        private readonly ZoomView _view;

        //        public ScaleListener(ZoomView view)
        //        {
        //            _view = view;
        //        }

        //        public override bool OnScale(ScaleGestureDetector detector) {

        //            mScaleFactor *= detector.ScaleFactor;

        //            // Don't let the object get too small or too large.
        //            mScaleFactor = Math.Max(0.1f, Math.Min(mScaleFactor, 5.0f));
        //            _view.Invalidate();
        //            return true;

        //        }  
        //    }

        //}

        public class ScaleImageViewGestureDetector : GestureDetector.SimpleOnGestureListener
        {
            private readonly ScaleImageView m_ScaleImageView;
            public ScaleImageViewGestureDetector(ScaleImageView imageView)
            {
                m_ScaleImageView = imageView;
            }

            public override bool OnDown(MotionEvent e)
            {
                return true;
            }

            public override bool OnDoubleTap(MotionEvent e)
            {
                m_ScaleImageView.MaxZoomTo((int)e.GetX(), (int)e.GetY());
                m_ScaleImageView.Cutting();
                return true;
            }
        }

        public class ScaleImageView : ImageView, View.IOnTouchListener
        {
            private Context m_Context;

            private float m_MaxScale = 2.0f;

            private Matrix m_Matrix;
            private float[] m_MatrixValues = new float[9];
            private int m_Width;
            private int m_Height;
            private int m_IntrinsicWidth;
            private int m_IntrinsicHeight;
            private float m_Scale;
            private float m_MinScale;
            private float m_PreviousDistance;
            private int m_PreviousMoveX;
            private int m_PreviousMoveY;

            private bool m_IsScaling;
            private GestureDetector m_GestureDetector;

            public ScaleImageView(Context context, IAttributeSet attrs) :
                base(context, attrs)
            {
                m_Context = context;
                Initialize();
            }

            public ScaleImageView(Context context, IAttributeSet attrs, int defStyle) :
                base(context, attrs, defStyle)
            {
                m_Context = context;
                Initialize();
            }

            public override void SetImageBitmap(Bitmap bm)
            {
                base.SetImageBitmap(bm);
                this.Initialize();
            }

            public override void SetImageResource(int resId)
            {
                base.SetImageResource(resId);
                this.Initialize();
            }

            private void Initialize()
            {
                this.SetScaleType(ScaleType.Matrix);
                m_Matrix = new Matrix();

                if (Drawable != null)
                {
                    m_IntrinsicWidth = Drawable.IntrinsicWidth;
                    m_IntrinsicHeight = Drawable.IntrinsicHeight;
                    this.SetOnTouchListener(this);
                }

                m_GestureDetector = new GestureDetector(m_Context, new ScaleImageViewGestureDetector(this));
            }

            protected override bool SetFrame(int l, int t, int r, int b)
            {
                m_Width = r - l;
                m_Height = b - t;

                m_Matrix.Reset();
                var r_norm = r - l;
                m_Scale = (float)r_norm / (float)m_IntrinsicWidth;

                var paddingHeight = 0;
                var paddingWidth = 0;
                if (m_Scale * m_IntrinsicHeight > m_Height)
                {
                    m_Scale = (float)m_Height / (float)m_IntrinsicHeight;
                    m_Matrix.PostScale(m_Scale, m_Scale);
                    paddingWidth = (r - m_Width) / 2;
                }
                else
                {
                    m_Matrix.PostScale(m_Scale, m_Scale);
                    paddingHeight = (b - m_Height) / 2;
                }

                m_Matrix.PostTranslate(paddingWidth, paddingHeight);
                ImageMatrix = m_Matrix;
                m_MinScale = m_Scale;
                ZoomTo(m_Scale, m_Width / 2, m_Height / 2);
                Cutting();
                return base.SetFrame(l, t, r, b);
            }

            private float GetValue(Matrix matrix, int whichValue)
            {
                matrix.GetValues(m_MatrixValues);
                return m_MatrixValues[whichValue];
            }



            public float Scale
            {
                get { return this.GetValue(m_Matrix, Matrix.MscaleX); }
            }

            public float TranslateX
            {
                get { return this.GetValue(m_Matrix, Matrix.MtransX); }
            }

            public float TranslateY
            {
                get { return this.GetValue(m_Matrix, Matrix.MtransY); }
            }

            public void MaxZoomTo(int x, int y)
            {
                if (this.m_MinScale != this.Scale && (Scale - m_MinScale) > 0.1f)
                {
                    var scale = m_MinScale / Scale;
                    ZoomTo(scale, x, y);
                }
                else
                {
                    var scale = m_MaxScale / Scale;
                    ZoomTo(scale, x, y);
                }
            }

            public void ZoomTo(float scale, int x, int y)
            {
                if (Scale * scale < m_MinScale)
                {
                    scale = m_MinScale / Scale;
                }
                else
                {
                    if (scale >= 1 && Scale * scale > m_MaxScale)
                    {
                        scale = m_MaxScale / Scale;
                    }
                }
                m_Matrix.PostScale(scale, scale);
                //move to center
                m_Matrix.PostTranslate(-(m_Width * scale - m_Width) / 2, -(m_Height * scale - m_Height) / 2);

                //move x and y distance
                m_Matrix.PostTranslate(-(x - (m_Width / 2)) * scale, 0);
                m_Matrix.PostTranslate(0, -(y - (m_Height / 2)) * scale);
                ImageMatrix = m_Matrix;
            }

            public void Cutting()
            {
                var width = (int)(m_IntrinsicWidth * Scale);
                var height = (int)(m_IntrinsicHeight * Scale);
                if (TranslateX < -(width - m_Width))
                {
                    m_Matrix.PostTranslate(-(TranslateX + width - m_Width), 0);
                }

                if (TranslateX > 0)
                {
                    m_Matrix.PostTranslate(-TranslateX, 0);
                }

                if (TranslateY < -(height - m_Height))
                {
                    m_Matrix.PostTranslate(0, -(TranslateY + height - m_Height));
                }

                if (TranslateY > 0)
                {
                    m_Matrix.PostTranslate(0, -TranslateY);
                }

                if (width < m_Width)
                {
                    m_Matrix.PostTranslate((m_Width - width) / 2, 0);
                }

                if (height < m_Height)
                {
                    m_Matrix.PostTranslate(0, (m_Height - height) / 2);
                }

                ImageMatrix = m_Matrix;
            }

            private float Distance(float x0, float x1, float y0, float y1)
            {
                var x = x0 - x1;
                var y = y0 - y1;
                return FloatMath.Sqrt(x * x + y * y);
            }

            private float DispDistance()
            {
                return FloatMath.Sqrt(m_Width * m_Width + m_Height * m_Height);
            }

            public override bool OnTouchEvent(MotionEvent e)
            {
                if (m_GestureDetector.OnTouchEvent(e))
                {
                    m_PreviousMoveX = (int)e.GetX();
                    m_PreviousMoveY = (int)e.GetY();
                    return true;
                }

                var touchCount = e.PointerCount;
                switch (e.Action)
                {
                    case MotionEventActions.Down:
                    case MotionEventActions.Pointer1Down:
                    case MotionEventActions.Pointer2Down:
                        {
                            if (touchCount >= 2)
                            {
                                var distance = this.Distance(e.GetX(0), e.GetX(1), e.GetY(0), e.GetY(1));
                                m_PreviousDistance = distance;
                                m_IsScaling = true;
                            }
                        }
                        break;

                    case MotionEventActions.Move:
                        {
                            if (touchCount >= 2 && m_IsScaling)
                            {
                                var distance = this.Distance(e.GetX(0), e.GetX(1), e.GetY(0), e.GetY(1));
                                var scale = (distance - m_PreviousDistance) / this.DispDistance();
                                m_PreviousDistance = distance;
                                scale += 1;
                                scale = scale * scale;
                                this.ZoomTo(scale, m_Width / 2, m_Height / 2);
                                this.Cutting();
                            }
                            else if (!m_IsScaling)
                            {
                                var distanceX = m_PreviousMoveX - (int)e.GetX();
                                var distanceY = m_PreviousMoveY - (int)e.GetY();
                                m_PreviousMoveX = (int)e.GetX();
                                m_PreviousMoveY = (int)e.GetY();

                                m_Matrix.PostTranslate(-distanceX, -distanceY);
                                this.Cutting();
                            }
                        }
                        break;
                    case MotionEventActions.Up:
                    case MotionEventActions.Pointer1Up:
                    case MotionEventActions.Pointer2Up:
                        {
                            if (touchCount <= 1)
                            {
                                m_IsScaling = false;
                            }
                        }
                        break;
                }
                return true;
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                return OnTouchEvent(e);
            }
        }
    }
}

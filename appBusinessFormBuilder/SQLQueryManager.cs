using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.Remoting;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Util;

namespace appBusinessFormBuilder
{
    [Activity(Label = "SQLQueryManager")]
    public class SQLQueryManager : Activity
    {
        Context this_context;
        RelativeLayout mainView;
        HorizontalScrollView mainHSV;
        ScrollView mainSV;
        RelativeLayout llMain;
        AndroidUtils.AlertBox alert = new AndroidUtils.AlertBox();
        TextView gtxtTableHighlighted;
        TextView gtxtColumnHighlighted;
        int giRequestCodeImportFile = 100;

        int giFormId;
        int giParameterId;
        int giItemId;
        int giSectionTypeId;
        int giParameterCellId;

        //Item Ids
        int iMainOutermostTableId = 10; //This is the very outermost table.
        int iCheckSyntaxButtonId = 20;
        int iMoreButtonsRLId = 50;
        int iUnsavedChangedDialogId = 5010;
        int iSQLQueryId = 5020;
        int iButtonRowId = 100;
        int iTableNameId = 10000; //Aloow 100 columns per table and 1000 tables. So next Id starts at 1,000,000

        //Item Widths
        int giSQLQueryWidth = 200;
        int giUnsavedChangesWidth = 100;
        int giSyntaxButtonWidth = 140;
        int giTextButtonWidth = 120;
        int giTablesHdrWidth = 250;
        int giScrollerWidth = 100;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            this_context = this;
            //mainView = new AndroidUtils.ScaleImageView(this);
            //SetContentView(mainView);

            SetContentView(Resource.Layout.layoutEmpty);
            mainView = (RelativeLayout)FindViewById(Resource.Id.EmptyLayout);
            HorizontalScrollView hsv = new HorizontalScrollView(this);
            mainView.AddView(hsv);
            mainHSV = hsv;
            RelativeLayout scrSQL = new RelativeLayout(this);
            scrSQL = DrawOpeningPage(this);
            if (scrSQL != null)
            {
                hsv.AddView(scrSQL);
//                mainSV = scrSQL;
            }
        }

        public RelativeLayout DrawOpeningPage(Android.Content.Context context)
        {
            try
            {
                Android.App.ActionBar navBar = this.ActionBar;
                clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
                RelativeLayout layout = new RelativeLayout(context);
                llMain = layout;
                string sExistingSQL = Intent.GetStringExtra("ExistingSQL");
                giFormId = Intent.GetIntExtra("FormId", -1);
                giParameterId = Intent.GetIntExtra("ParameterId", -1);
                giItemId = Intent.GetIntExtra("ItemId", -1);
                giSectionTypeId = Intent.GetIntExtra("SectionTypeId", -1);
                giParameterCellId = Intent.GetIntExtra("ParameterCellId", -1);

                int iScreenHeight = ConvertDpToPixels(GetScreenHeightPixels());
                int iTopBarHeight = navBar.Height;

                giSQLQueryWidth = GetScreenWidthPixels() - ConvertPixelsToDp(giUnsavedChangesWidth + giSyntaxButtonWidth * 3 + giTablesHdrWidth);
                if (giSQLQueryWidth < ConvertPixelsToDp(120))
                {
                    giSQLQueryWidth = ConvertPixelsToDp(120);
                }
                //int iHeightPixels = GetScreenHeightPixels();
                string sFormName = "SQL Manager - Query Manager";
                navBar.Title = sFormName;

                layout.SetGravity(GravityFlags.CenterHorizontal);
                layout.Id = 1;
                int iPaddingMargin1 = ConvertPixelsToDp(5);
                int iPaddingMargin2 = ConvertPixelsToDp(1);
                int iPaddingButtonMargin2 = ConvertPixelsToDp(5);

                //Set up some default items for use anywhere
                //Create an alert dialog for use later on
                alert.SetContext(context);
                alert.CreateAlertDialog();

                //This simply sets spacing between each of the elements in the row
                TableRow.LayoutParams params2 = new TableRow.LayoutParams();
                params2.SetMargins(iPaddingMargin1, iPaddingMargin2, iPaddingMargin1, iPaddingMargin2);

                TableLayout table = new TableLayout(context);
                table.SetGravity(GravityFlags.CenterHorizontal);
                table.SetBackgroundColor(Android.Graphics.Color.WhiteSmoke);
                table.Id = iMainOutermostTableId;

                TableRow.LayoutParams params3 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
                params3.SetMargins(0, 0, 0, 0);
                params3.Span = 2;

                TableRow.LayoutParams params4 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
                params4.SetMargins(0, 0, 0, 0);
                params4.Span = 5;

                TableRow.LayoutParams params5 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
                params5.SetMargins(0, 0, 0, 0);
                params5.Span = 5;

                TableRow.LayoutParams params33 = new TableRow.LayoutParams();
                params33.SetMargins(iPaddingButtonMargin2, iPaddingButtonMargin2, iPaddingButtonMargin2, iPaddingButtonMargin2);

                //********************************************************//
                //          TOP HEADER ROW                                //
                //********************************************************//
                TableRow rowTopHdr = new TableRow(context);
                rowTopHdr.SetBackgroundColor(Android.Graphics.Color.Wheat);
                rowTopHdr.SetMinimumHeight(ConvertPixelsToDp(34));
                rowTopHdr.SetGravity(GravityFlags.CenterVertical);

                TextView lblScriptHdr = new TextView(this_context);
                lblScriptHdr.Text = "SQL Query";
                lblScriptHdr.SetTextColor(Android.Graphics.Color.Black);
                lblScriptHdr.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.BoldItalic);
                lblScriptHdr.SetTextSize(Android.Util.ComplexUnitType.Pt, 12);
                lblScriptHdr.SetWidth(giSQLQueryWidth);
                lblScriptHdr.SetPadding(10, 1, 10, 1);
                lblScriptHdr.SetHeight(ConvertPixelsToDp(34));
                rowTopHdr.AddView(lblScriptHdr);

                //Now put in the unsaved changes text view
                TextView txtChanges = new TextView(this_context);
                txtChanges.Text = "UNSAVED CHANGES";
                txtChanges.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Bold);
                txtChanges.SetWidth(ConvertPixelsToDp(giUnsavedChangesWidth));
                txtChanges.SetTextColor(Android.Graphics.Color.Black);
                txtChanges.SetBackgroundColor(Android.Graphics.Color.Rgb(255, 174, 255));
                txtChanges.Gravity = GravityFlags.Center;
                txtChanges.SetPadding(5, 5, 5, 5);
                txtChanges.Id = iUnsavedChangedDialogId;
                txtChanges.Visibility = ViewStates.Invisible;
                rowTopHdr.AddView(txtChanges);

                Button btnSyntaxCheck = new Button(context);
                btnSyntaxCheck.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btnSyntaxCheck.SetTextColor(Android.Graphics.Color.Black);
                btnSyntaxCheck.SetTextSize(ComplexUnitType.Pt, 8);
                btnSyntaxCheck.Text = "Check Syntax";
                btnSyntaxCheck.Gravity = GravityFlags.Center;
                btnSyntaxCheck.Id = iCheckSyntaxButtonId;
                btnSyntaxCheck.LayoutParameters = params33;
                btnSyntaxCheck.SetWidth(ConvertPixelsToDp(giSyntaxButtonWidth - 2 * iPaddingMargin1));
                btnSyntaxCheck.SetHeight(44);
                btnSyntaxCheck.Click += (sender, args) => { CheckSyntax(sender, args, false); };
                rowTopHdr.AddView(btnSyntaxCheck);

                Button btnApply = new Button(context);
                btnApply.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btnApply.SetTextColor(Android.Graphics.Color.Black);
                btnApply.SetTextSize(ComplexUnitType.Pt, 8);
                btnApply.Text = "Apply";
                btnApply.Gravity = GravityFlags.Center;
                btnApply.Id = iCheckSyntaxButtonId;
                btnApply.LayoutParameters = params33;
                btnApply.SetWidth(ConvertPixelsToDp(giSyntaxButtonWidth - 2 * iPaddingMargin1));
                btnApply.SetHeight(44);
                btnApply.Click += (sender, args) => { ApplyChanges(sender, args); };
                rowTopHdr.AddView(btnApply);

                Button btnImportFile = new Button(context);
                btnImportFile.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btnImportFile.SetTextColor(Android.Graphics.Color.Black);
                btnImportFile.SetTextSize(ComplexUnitType.Pt, 8);
                btnImportFile.Text = "Import File";
                btnImportFile.Gravity = GravityFlags.Center;
                btnImportFile.Id = iCheckSyntaxButtonId;
                btnImportFile.LayoutParameters = params33;
                btnImportFile.SetWidth(ConvertPixelsToDp(giSyntaxButtonWidth - 2 * iPaddingMargin1));
                btnImportFile.SetHeight(44);
                btnImportFile.Click += (sender, args) => { ImportFile(sender, args); };
                rowTopHdr.AddView(btnImportFile);

                Button btnExecuteSQL = new Button(context);
                btnExecuteSQL.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btnExecuteSQL.SetTextColor(Android.Graphics.Color.Black);
                btnExecuteSQL.SetTextSize(ComplexUnitType.Pt, 8);
                btnExecuteSQL.Text = "Execute SQL";
                btnExecuteSQL.Gravity = GravityFlags.Center;
                btnExecuteSQL.Id = iCheckSyntaxButtonId;
                btnExecuteSQL.LayoutParameters = params33;
                btnExecuteSQL.SetWidth(ConvertPixelsToDp(giSyntaxButtonWidth - 2 * iPaddingMargin1));
                btnExecuteSQL.SetHeight(44);
                btnExecuteSQL.Click += (sender, args) => { ExecuteSQL(sender, args); };
                rowTopHdr.AddView(btnExecuteSQL);

                TextView lblTablesHdr = new TextView(this_context);
                lblTablesHdr.Text = "Tables";
                lblTablesHdr.SetTextColor(Android.Graphics.Color.Black);
                lblTablesHdr.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.BoldItalic);
                lblTablesHdr.SetTextSize(Android.Util.ComplexUnitType.Pt, 12);
                lblTablesHdr.SetWidth(ConvertPixelsToDp(giTablesHdrWidth));
                lblTablesHdr.SetPadding(10, 1, 10, 1);
                lblTablesHdr.Gravity = GravityFlags.Center;
                lblTablesHdr.SetHeight(ConvertPixelsToDp(34));
                rowTopHdr.AddView(lblTablesHdr);

                table.AddView(rowTopHdr);

                //********************************************************//
                //          MAIN BODY ROW                                 //
                //********************************************************//
                if (iTopBarHeight < 40)
                {
                    iTopBarHeight = 40;
                }

//                int iAmountForButtons = 160;
                int iThisRowHeight = iScreenHeight - iTopBarHeight - 100;//Allow 3 rows of buttons at 40 height and some space around them of 5 pixels each
                //                iThisRowHeight = iThisRowHeight; //Allow 3 rows of buttons at 40 height and some space around them of 5 pixels each
                //if (iThisRowHeight > 300)
                //{
                //    iThisRowHeight = 300;
                //}
                TableRow rowBody = new TableRow(context);
                rowBody.SetBackgroundColor(Android.Graphics.Color.Wheat);
                rowBody.SetMinimumHeight(ConvertPixelsToDp(iThisRowHeight));


                EditText txtSQLQuery = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
                txtSQLQuery.Text = sExistingSQL;
                txtSQLQuery.Id = iSQLQueryId;
                txtSQLQuery.SetPadding(ConvertPixelsToDp(5), ConvertPixelsToDp(5), ConvertPixelsToDp(5), ConvertPixelsToDp(5));
                txtSQLQuery.VerticalScrollBarEnabled = true;
                params4.Height = ConvertPixelsToDp(iThisRowHeight -  10);
                txtSQLQuery.LayoutParameters = params4;
                txtSQLQuery.SetSingleLine(false);
                txtSQLQuery.SetBackgroundColor(Android.Graphics.Color.White);
                txtSQLQuery.Gravity = GravityFlags.Top | GravityFlags.Left;
                txtSQLQuery.TextChanged += (senderItem, args) => { SetAnyValueChanged(); };
                rowBody.AddView(txtSQLQuery);

                TableLayout tableButtons = GetTableButtonsVertical();
                rowBody.AddView(tableButtons);



                TableRow.LayoutParams paramsBigRow = new TableRow.LayoutParams(ConvertPixelsToDp(giTablesHdrWidth), ConvertPixelsToDp(iThisRowHeight));
                HorizontalScrollView hsvTables = GetTablesView();
                rowBody.AddView(hsvTables, paramsBigRow);

                table.AddView(rowBody);

                layout.AddView(table);

                return layout;
            }
            catch (Exception except)
            {
                Toast.MakeText(context, except.Message.ToString(), Android.Widget.ToastLength.Long);
                return null;
            }
        }

        public HorizontalScrollView GetTablesView()
        {
            HorizontalScrollView hsv = new HorizontalScrollView(this_context);
            ScrollView sv = new ScrollView(this_context);
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            string sRtnMsg = "";
            int i;

            ArrayList arrTables = grdUtils.GetUserTables(ref sRtnMsg);

            TableLayout table = new TableLayout(this_context);
            table.Id = iTableNameId;

            for (i = 0; i < arrTables.Count; i++)
            {
                TableRow rowBody = new TableRow(this_context);
                rowBody.Id = iTableNameId + (1000 * (i+1)) + 500;
                rowBody.SetBackgroundColor(Android.Graphics.Color.Wheat);
                rowBody.SetMinimumHeight(ConvertPixelsToDp(40));

                TextView txtTableName = new TextView(this_context);
                txtTableName.Text = arrTables[i].ToString();
                txtTableName.SetWidth(ConvertPixelsToDp(giTablesHdrWidth - giScrollerWidth));
                txtTableName.Id = iTableNameId + (1000 * (i+1));
                txtTableName.SetPadding(ConvertPixelsToDp(2), ConvertPixelsToDp(1), ConvertPixelsToDp(2), ConvertPixelsToDp(1));
                txtTableName.SetHeight(ConvertPixelsToDp(40));
                txtTableName.SetSingleLine(true);
                txtTableName.SetTextIsSelectable(true);
                txtTableName.SetTextColor(Android.Graphics.Color.Black);
                txtTableName.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Bold);
                txtTableName.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                txtTableName.Clickable = true;
                txtTableName.Click += (senderItem, args) => { HighlightTableItem(senderItem, args); };
                rowBody.AddView(txtTableName);

                TextView txtScroll = new TextView(this_context);
                txtScroll.Text = "";
                txtScroll.SetWidth(ConvertPixelsToDp(giScrollerWidth));
                txtScroll.SetHeight(ConvertPixelsToDp(40));
                rowBody.AddView(txtScroll);

                table.AddView(rowBody);
            }

            sv.AddView(table);
            hsv.AddView(sv);

            return hsv;
        }


        public TableLayout GetTableButtonsVertical()
        {
            TableLayout table = new TableLayout(this_context);
            int i;
            int j;
            int iScreenHeight = GetScreenHeightPixels();
            string[] sButtons = {"Select", "From", "Where", "And", "Table Name", "All Columns", "Column Name", "More"};
            int iPaddingButtonMargin2 = ConvertPixelsToDp(5);
            TableRow.LayoutParams params33 = new TableRow.LayoutParams();
            params33.SetMargins(iPaddingButtonMargin2, iPaddingButtonMargin2, iPaddingButtonMargin2, iPaddingButtonMargin2);

            for (i = 0; i < sButtons.Length / 2; i++)
            {
                TableRow row = new TableRow(this_context);
                row.Id = (iButtonRowId + (i + 1));
                row.SetGravity(GravityFlags.CenterVertical);

                Button btn = new Button(this_context);
                btn.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btn.SetTextColor(Android.Graphics.Color.Black);
                btn.SetTextSize(ComplexUnitType.Pt, 8);
                btn.Text = sButtons[i];
                btn.Gravity = GravityFlags.Center;
                btn.Id = (iButtonRowId + (i + 101));
                btn.LayoutParameters = params33;
                btn.SetWidth(ConvertPixelsToDp(giTextButtonWidth - 2 * iPaddingButtonMargin2));
                btn.SetHeight(44);
                btn.Click += (sender, args) => { InsertTextAtCursor(sender, args); };
                row.AddView(btn);

                Button btn2 = new Button(this_context);
                btn2.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btn2.SetTextColor(Android.Graphics.Color.Black);
                btn2.SetTextSize(ComplexUnitType.Pt, 8);
                btn2.Text = sButtons[i + sButtons.Length/2];
                btn2.Gravity = GravityFlags.Center;
                btn2.Id = (iButtonRowId + (i + 201));
                btn2.LayoutParameters = params33;
                btn2.SetWidth(ConvertPixelsToDp(giTextButtonWidth - 2 * iPaddingButtonMargin2));
                btn2.SetHeight(44);
                if (i == sButtons.Length/2 - 1)
                {
                    btn2.Click += (sender, args) => { OpenMoreRL(sender, args); };
                }
                else
                {
                    btn2.Click += (sender, args) => { InsertTableInfoAtCursor(sender, args); };
                }
                row.AddView(btn2);

                table.AddView(row);

            }

            //Add one extra row at the bottom
            TableRow.LayoutParams params22 = new TableRow.LayoutParams();
            params22.SetMargins(iPaddingButtonMargin2, iPaddingButtonMargin2, iPaddingButtonMargin2, iPaddingButtonMargin2);
            params22.Span = 2;

            TableRow row2 = new TableRow(this_context);
            row2.Id = (iButtonRowId + (sButtons.Length / 2 + 1));
            row2.SetGravity(GravityFlags.CenterVertical);


            Button btn3 = new Button(this_context);
            btn3.SetBackgroundColor(Android.Graphics.Color.LightGray);
            btn3.SetTextColor(Android.Graphics.Color.Black);
            btn3.SetTextSize(ComplexUnitType.Pt, 8);
            btn3.Text = "Table Manager";
            btn3.Gravity = GravityFlags.Center;
            btn3.Id = (iButtonRowId + (i + 201));
            btn3.LayoutParameters = params22;
            btn3.SetHeight(44);
            btn3.Click += (sender, args) => { OpenTableManager(sender, args); };

            row2.AddView(btn3);
            table.AddView(row2);

            return table;
        }

        public void ExecuteSQL(object sender, EventArgs e)
        {
            LocalDB DB = new LocalDB();
            string sRtnMsg = "";
            if (CheckSyntax(sender, e, true))
            {
                EditText txt = (EditText)FindViewById(iSQLQueryId);
                if (txt != null)
                {
                    string sSQL = txt.Text;
                    bool bRtn = DB.ExecuteSQL(sSQL, ref sRtnMsg);
                    if (!bRtn || sRtnMsg != "")
                    {
                        alert.SetAlertMessage("Error in executing SQL. " + sRtnMsg);
                        alert.ShowAlertBox();
                        return;
                    }
                }
            }
            return;
        }

        public void OpenMoreRL(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string sBtnText = btn.Text;
            if (sBtnText == "More")
            {
                btn.Text = "Close More";
                int[] iPosn = new int[2];
                btn.GetLocationOnScreen(iPosn);
                int iLeft = (iPosn[0]) - (btn.Width * 3) - ConvertPixelsToDp(20); // btn.Left + btn.Width;
                int iTop = ConvertPixelsToDp(60); // btn.Top + btn.Height;
                int iScreenHeight = ConvertDpToPixels(GetScreenHeightPixels());
                RelativeLayout.LayoutParams params1 = new RelativeLayout.LayoutParams(ConvertPixelsToDp(giTextButtonWidth * 2 + 20), ConvertPixelsToDp(iScreenHeight - 180));
                params1.SetMargins(iLeft, iTop, 0, 0);
                RelativeLayout RL = new RelativeLayout(this_context);
                RL.Id = iMoreButtonsRLId;
                ScrollView sv = new ScrollView(this_context);
                TableLayout table = GetTableButtons();
                table.SetBackgroundColor(Android.Graphics.Color.DarkGray);
                sv.AddView(table);
                RL.AddView(sv);
                mainView.AddView(RL, params1);
            }
            else
            {
                btn.Text = "More";
                RelativeLayout RL = (RelativeLayout)FindViewById(iMoreButtonsRLId);
                mainView.RemoveView(RL);
            }
        }

        public TableLayout GetTableButtons()
        {
            TableLayout table = new TableLayout(this_context);
            int i;
            int j;
            string[] sButtons = {"And", "Or", "As", "Inner Join", "Left Outer Join", "Right Outer Join", "Insert Into", "Update",
                                 "Delete", "Cast()", "()", "ifnull()", "Like", "=", "<>", "+", "-", "*", "/",  "%", "Union", "Union All", 
                                 "Values()", "LTrim()", "RTrim()", "Case", "When", "Then", "Else", "End"
                                 }; //This must be an even number to accomodate the 2 columns
            int iPaddingButtonMargin2 = ConvertPixelsToDp(5);
            TableRow.LayoutParams params33 = new TableRow.LayoutParams();
            params33.SetMargins(iPaddingButtonMargin2, iPaddingButtonMargin2, iPaddingButtonMargin2, iPaddingButtonMargin2);

            for (i = 0; i < sButtons.Length / 2; i++)
            {
                TableRow row = new TableRow(this_context);
                row.Id = (iButtonRowId +  (i + 50));
                row.SetGravity(GravityFlags.CenterVertical);

                Button btn = new Button(this_context);
                btn.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btn.SetTextColor(Android.Graphics.Color.Black);
                btn.SetTextSize(ComplexUnitType.Pt, 8);
                btn.Text = sButtons[i];
                btn.Gravity = GravityFlags.Center;
                btn.Id = iButtonRowId + (i + 151);
                btn.LayoutParameters = params33;
                btn.SetWidth(ConvertPixelsToDp(giTextButtonWidth - 2 * iPaddingButtonMargin2));
                btn.SetHeight(44);
                btn.Click += (sender, args) => { InsertTextAtCursor(sender, args); };
                row.AddView(btn);

                Button btn2 = new Button(this_context);
                btn2.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btn2.SetTextColor(Android.Graphics.Color.Black);
                btn2.SetTextSize(ComplexUnitType.Pt, 8);
                btn2.Text = sButtons[i + sButtons.Length / 2];
                btn2.Gravity = GravityFlags.Center;
                btn2.Id = iButtonRowId + (i + 251);
                btn2.LayoutParameters = params33;
                btn2.SetWidth(ConvertPixelsToDp(giTextButtonWidth - 2 * iPaddingButtonMargin2));
                btn2.SetHeight(44);
                btn2.Click += (sender, args) => { InsertTextAtCursor(sender, args); };
                row.AddView(btn2);

                table.AddView(row);

            }

            return table;
        }

        public bool CheckSyntax(object sender, EventArgs e, bool bSuppressSuccessMsg)
        {
            LocalDB DB = new LocalDB();
            string sRtnMsg = "";
            EditText txtSQL = (EditText)FindViewById(iSQLQueryId);
            string sSQL = txtSQL.Text;
            if (sSQL != "")
            {
                bool bRtn = DB.CheckSQLSyntax(sSQL, ref sRtnMsg);
                if (!bRtn)
                {
                    alert.SetAlertMessage(sRtnMsg);
                    alert.ShowAlertBox();
                    return false;
                }
                else
                {
                    if (!bSuppressSuccessMsg)
                    {
                        alert.SetAlertMessage("SQL syntax OK.");
                        alert.ShowAlertBox();
                    }
                    return true;
                }
            }
            else
            {
                alert.SetAlertMessage("You must have some SQL in the main SQL Query box.");
                alert.ShowAlertBox();
                return false;
            }
        }

        public void ImportFile(object sender, EventArgs e)
        {
            clsLocalUtils dtUtil = new clsLocalUtils();
            string sFolderPath = Android.OS.Environment.ExternalStorageDirectory + "/data/mnt/sdcard/data";
            sFolderPath = dtUtil.StripMultiSlashes(sFolderPath);
            Intent intent = new Intent();  
            intent.SetAction(Intent.ActionGetContent);
            Android.Net.Uri myUri = Android.Net.Uri.Parse(sFolderPath);
            intent.SetDataAndType(myUri , "*/*");
            StartActivityForResult(intent, giRequestCodeImportFile);
        }

        public void ApplyChanges(object sender, EventArgs e)
        {
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            string sRtnMsg = "";
            //First of all check the syntax
            if (CheckSyntax(sender, e, true))
            {
                //Save to the DB
                EditText txtSQL = (EditText)FindViewById(iSQLQueryId);
                string sSQL = txtSQL.Text;
                grdUtils.SaveItemAttribute(giFormId, giItemId, giSectionTypeId, giParameterId, sSQL, ref sRtnMsg);
                SetAnyValueChangedOff();
                
                Intent scrnBuild = new Intent(this, typeof(BuildScreen));
                scrnBuild.PutExtra("ExistingSQL", sSQL);
                scrnBuild.PutExtra("ParameterCellId", giParameterCellId);
                SetResult(Result.Ok, scrnBuild);

                this.Finish();
            }
        }

        public void OpenTableManager(object sender, EventArgs e)
        {
            int iEditStatus = GetEditStatus();

            if (iEditStatus == 1)
            {
                AlertDialog ad = new AlertDialog.Builder(this).Create();
                ad.SetCancelable(false); // This blocks the 'BACK' button  
                ad.SetMessage("You have unsaved chnages that have not been applied. Do you wish to apply these before leaving this page?");
                ad.SetButton("Yes", (s, ee) => { ShowTableManager(s, ee, true); });
                ad.SetButton2("No", (s, ee) => { ShowTableManager(s, ee, false); });
                ad.Show();
                return;
            }
            else
            {
                ShowTableManager(sender, e, false);
            }
        }

        public void ShowTableManager(object sender, EventArgs e, bool bApplyChanges)
        {
            if (bApplyChanges)
            {
                ApplyChanges(sender, e);
            }
            EditText txtSQL = (EditText)FindViewById(iSQLQueryId);
            string sSQLQuery = txtSQL.Text;
            var SQLScreen = new Intent(this, typeof(SQLTableManager));
            this.StartActivity(SQLScreen);
        }

        public void InsertTableInfoAtCursor(object sender, EventArgs e)
        
        {
            Button btn = (Button)sender;
            string sbtnText = btn.Text;
            EditText myEditText = (EditText)FindViewById(iSQLQueryId);
            string sEditText = myEditText.Text;
            int i;
            int start = Math.Max(myEditText.SelectionStart, 0);
            int end = Math.Max(myEditText.SelectionEnd, 0);
            string sTextToReplace = sEditText.Substring(start, end - start);
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", options);
            string sText = "";
            string sRtnMsg = "";
            LocalDB DB = new LocalDB();

            if(sbtnText == "Table Name")
            {
                if (gtxtTableHighlighted == null)
                {
                    alert.SetAlertMessage("You must have a table highlighted in the tables section.");
                    alert.ShowAlertBox();
                }
                else
                {
                    sText = gtxtTableHighlighted.Text + " ";
                }
            }

            if (sbtnText == "All Columns")
            {
                if (gtxtTableHighlighted == null)
                {
                    alert.SetAlertMessage("You must have a table highlighted in the tables section.");
                    alert.ShowAlertBox();
                }
                else
                {
                    string sTableName = gtxtTableHighlighted.Text;
                    ArrayList arrColDetails = DB.GetTableColumnDetails(sTableName, ref sRtnMsg);
                    if (arrColDetails.Count > 0)
                    {
                        ArrayList arrColNames = new ArrayList();
                        //Now for each piece of info in the array we need comma separate
                        arrColNames = (ArrayList)arrColDetails[0];

                        for (i = 0; i < arrColNames.Count; i++)
                        {
                            sText += arrColNames[i].ToString() + ", ";
                        }

                        sText = sText.Substring(0, sText.Length - 2) + " ";
                    }
                }
            }

            if (sbtnText == "Column Name")
            {
                if (gtxtColumnHighlighted == null)
                {
                    alert.SetAlertMessage("You must have a column of a table highlighted in the tables section.");
                    alert.ShowAlertBox();
                }
                else
                {
                    sText = gtxtColumnHighlighted.Text + " "; ;
                }
            }

            if (sText != "")
            {
                if (sEditText == "")
                {
                    myEditText.Text = sText;
                    myEditText.SetSelection(sText.Length);
                }
                else
                {
                    if (sTextToReplace == "")
                    {
                        sTextToReplace = sEditText.Substring(0, start) + sText + sEditText.Substring(start);
                        sTextToReplace = regex.Replace(sTextToReplace, @" ");
                        myEditText.Text = sTextToReplace;
                        myEditText.SetSelection(start + sText.Length);
                    }
                    else
                    {
                        sTextToReplace = sEditText.Substring(0, start) + sText + sEditText.Substring(end);
                        int iDiff = sText.Length - (end - start);
                        sTextToReplace = regex.Replace(sTextToReplace, @" ");
                        myEditText.Text = sTextToReplace;
                        int iPosn = end + iDiff - 1;
                        while (sTextToReplace.Substring(iPosn, 1) != " ")
                        {
                            iPosn--;
                        }

                        if (iPosn + 1 < sTextToReplace.Length)
                        {
                            myEditText.SetSelection(iPosn + 1);
                        }
                        else
                        {
                            myEditText.SetSelection(sTextToReplace.Length);
                        }
                    }
                }

            }

            return;
        }

        public void InsertTextAtCursor(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string sText = btn.Text + " ";
            EditText myEditText = (EditText)FindViewById(iSQLQueryId);
            string sEditText = myEditText.Text;

            int start = Math.Max(myEditText.SelectionStart, 0);
            int end = Math.Max(myEditText.SelectionEnd, 0);
            string sTextToReplace = sEditText.Substring(start, end - start);
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", options);
            if (sText != "")
            {
                if (sEditText == "")
                {
                    myEditText.Text = sText;
                    myEditText.SetSelection(sText.Length);
                }
                else
                {
                    if (sTextToReplace == "")
                    {
                        sTextToReplace = sEditText.Substring(0, start) + sText + sEditText.Substring(start);
                        sTextToReplace = regex.Replace(sTextToReplace, @" ");
                        myEditText.Text = sTextToReplace;
                        myEditText.SetSelection(start + sText.Length);
                    }
                    else
                    {
                        sTextToReplace = sEditText.Substring(0, start) + sText + sEditText.Substring(end);
                        int iDiff = sText.Length - (end - start);
                        sTextToReplace = regex.Replace(sTextToReplace, @" ");
                        myEditText.Text = sTextToReplace;
                        int iPosn = end + iDiff - 1;
                        while(sTextToReplace.Substring(iPosn, 1) != " ")
                        {
                            iPosn --;
                        }

                        if (iPosn + 1 < sTextToReplace.Length)
                        {
                            myEditText.SetSelection(iPosn + 1);
                        }
                        else
                        {
                            myEditText.SetSelection(sTextToReplace.Length);
                        }
                    }
                }

            }
        }

        public void HighlightTableItem(object sender, EventArgs e)
        {
            TextView txt = (TextView)sender;
            txt.SetBackgroundColor(Android.Graphics.Color.LightBlue);

            //Now show the columns if they are not showing
            ShowColumns(txt.Text, txt.Id);
            if (gtxtTableHighlighted != null && txt.Id != gtxtTableHighlighted.Id)
            {
                gtxtTableHighlighted.SetBackgroundColor(Android.Graphics.Color.Wheat);
            }
            gtxtTableHighlighted = txt;
            return;
        }

        public void HighlightColumnItem(object sender, EventArgs e)
        {
            TextView txt = (TextView)sender;
            txt.SetBackgroundColor(Android.Graphics.Color.LightPink);

            if (gtxtColumnHighlighted != null && txt.Id != gtxtColumnHighlighted.Id)
            {
                gtxtColumnHighlighted.SetBackgroundColor(Android.Graphics.Color.Wheat);
            }
            gtxtColumnHighlighted = txt;
            return;
        }

        public void ShowColumns(string sTableName, int iTableId)
        {
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            LocalDB DB = new LocalDB();
            string sRtnMsg = "";
            int i;

            //First check to see if the columns are showing
            TableRow txtTableRow = (TableRow)FindViewById(iTableId + 500);
            TextView txtColumn1 = (TextView)FindViewById(iTableId + 1); //This is the first column
            TableLayout table = (TableLayout)FindViewById(iTableNameId);
            if (txtColumn1 == null)
            {
                int iIndex = table.IndexOfChild(txtTableRow);
                ArrayList arrColDetails = DB.GetTableColumnDetails(sTableName, ref sRtnMsg);
                if (arrColDetails.Count > 0)
                {
                    ArrayList arrColNames = new ArrayList();
                    //Now for each piece of info in the array we need to build a row
                    arrColNames = (ArrayList)arrColDetails[0];

                    for (i = 0; i < arrColNames.Count; i++)
                    {
                        TableRow rowBody = new TableRow(this_context);
                        rowBody.Id = iTableId + (i + 501);
                        rowBody.SetBackgroundColor(Android.Graphics.Color.Wheat);
                        rowBody.SetMinimumHeight(ConvertPixelsToDp(40));

                        TextView txtColumnName = new TextView(this_context);
                        txtColumnName.Text = arrColNames[i].ToString();
                        txtColumnName.SetWidth(ConvertPixelsToDp(giTablesHdrWidth - giScrollerWidth));
                        txtColumnName.Id = iTableId + (i + 1);
                        txtColumnName.SetPadding(ConvertPixelsToDp(17), ConvertPixelsToDp(1), ConvertPixelsToDp(2), ConvertPixelsToDp(1));
                        txtColumnName.SetHeight(ConvertPixelsToDp(40));
                        txtColumnName.SetSingleLine(true);
                        txtColumnName.SetTextIsSelectable(true);
                        txtColumnName.SetTextColor(Android.Graphics.Color.Black);
                        txtColumnName.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Italic);
                        txtColumnName.SetTextSize(Android.Util.ComplexUnitType.Pt, 9);
                        txtColumnName.Clickable = true;
                        txtColumnName.Click += (senderItem, args) => { HighlightColumnItem(senderItem, args); };
                        rowBody.AddView(txtColumnName);

                        TextView txtScroll = new TextView(this_context);
                        txtScroll.Text = "";
                        txtScroll.SetWidth(ConvertPixelsToDp(giScrollerWidth));
                        txtScroll.SetHeight(ConvertPixelsToDp(40));
                        rowBody.AddView(txtScroll);

                        if (iIndex + (i + 1) == table.ChildCount)
                        {
                            table.AddView(rowBody);
                        }
                        else
                        {
                            table.AddView(rowBody, iIndex + (i + 1));
                        }

                    }
                }
            }
            else
            {
                //Remove all the columns
                for (i = 1; i < table.ChildCount; i++)
                {
                    TableRow row = (TableRow)FindViewById(iTableId + (i + 500));
                    if (row != null)
                    {
                        table.RemoveView(row);
                    }
                }

            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == giRequestCodeImportFile)
            {


                string sFilepath = data.Data.Path;
                if (System.IO.File.Exists(sFilepath))
                {
                    string sFileContent = System.IO.File.ReadAllText(sFilepath);
                    EditText txt = (EditText)FindViewById(iSQLQueryId);
                    txt.Text += ";\r\n" + sFileContent;
                }

                return;
            }
            //if (resultCode == Result.Ok)
            //{
            //    EditText txtSQL = (EditText)FindViewById(iSQLQueryId);
            //    if (txtSQL != null)
            //    {
            //        txtSQL.Text = data.GetStringExtra("ExistingSQL");
            //    }
            //}
        }
        
        public void SetAnyValueChanged()
        {
            TextView changes = FindViewById<TextView>(iUnsavedChangedDialogId);
            changes.Visibility = ViewStates.Visible;
            TextView txtEditStatus = FindViewById<TextView>(Resource.Id.hfEditStatus);
            txtEditStatus.Text = "1";
        }

        public void SetAnyValueChangedOff()
        {
            TextView changes = FindViewById<TextView>(iUnsavedChangedDialogId);
            changes.Visibility = ViewStates.Invisible;
            TextView txtEditStatus = FindViewById<TextView>(Resource.Id.hfEditStatus);
            txtEditStatus.Text = "0";
        }

        public int GetEditStatus()
        {
            clsLocalUtils utils = new clsLocalUtils();
            int iEditStatus = -99;
            TextView txtEditStatus = FindViewById<TextView>(Resource.Id.hfEditStatus);
            if (txtEditStatus != null)
            {
                string sEditStatus = txtEditStatus.Text;
                if (utils.IsNumeric(sEditStatus))
                {
                    iEditStatus = Convert.ToInt32(sEditStatus);
                }

                if (sEditStatus == "")
                {
                    iEditStatus = 0;
                }
            }

            return iEditStatus;
        }

        private int GetScreenWidthPixels()
        {
            return Resources.DisplayMetrics.WidthPixels;
        }

        private int GetScreenHeightPixels()
        {
            return Resources.DisplayMetrics.HeightPixels;
        }

        private int GetOrientation()
        {
            var orientation = WindowManager.DefaultDisplay.Rotation;
            if (orientation == SurfaceOrientation.Rotation0 || orientation == SurfaceOrientation.Rotation180)
            {
                return 0;
            }
            else
            {
                return 1;
            }

        }

        private int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int)((pixelValue) * Resources.DisplayMetrics.Density);
            return dp;
        }
    
        private int ConvertDpToPixels(float DpValue)
        {
            var iPixels = (int)((DpValue) / Resources.DisplayMetrics.Density);
            return iPixels;
        }
    }
}
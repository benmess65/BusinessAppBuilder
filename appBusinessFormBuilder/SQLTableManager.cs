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
    [Activity(Label = "SQLTableManager")]
    public class SQLTableManager : Activity
    {
        Context this_context;
        RelativeLayout mainView;
        HorizontalScrollView mainHSV;
        ScrollView mainSV;
        RelativeLayout llMain;
        AndroidUtils.AlertBox alert = new AndroidUtils.AlertBox();
        Dialog gDialogOpen;

        //Ids for various parts of the screen
        int iMainOutermostTableId = 10; //This is the very outermost table.
        int iTableNameLabelId = 20;
        int iTableNameId = 30;
        int iSaveTableId = 40;
        int iClearTableId = 50;
        int iColumnNameLabelId = 60;
        int iColumnAddButtonId = 70;
        int iBlankLabelId = 80;
        int iBlankLabelId1 = 90;
        int iNameLabelId = 200;
        int iTypeLabelId = 210;
        int iSizeLabelId = 220;
        int iAllowNullLabelId = 230;
        int iDeleteLabelId = 240;
        int iExistingTableNameLabelId = 520;
        int iExistingTableNameId = 530;
        int iExistingSaveTableId = 540;
        int iExistingMoreTableId = 550;
        int iExistingBlankLabelId = 570;

        int iUnsavedChangedDialogId = 1010;
        int iMoreDialogId = 2010;

        int giTableLabelNameWidth = 150;
        int giTableNameWidth = 450;
        int giSaveButtonWidth = 120;
        int giClearButtonWidth = 120;
        int giBlank1Width = 120;

        int giTableColumnNameWidth = 150;
        int giTableColumnTypeWidth = 150;

        //For the copy dialog popup
        int iSourceTableId = 5000;
        int iTargetTableId = 6000;

        //For the table builder table
        int iColumnsTable = 80000; //This is the actual table showing all the columns
        int iTableColumnRowId = 90000;
        int iTableColumnNameId = 100000;
        int iTableColumnNameHiddenId = 110000;
        int iTableColumnTypeId = 120000;
        int iTableColumnTypeHiddenId = 130000;
        int iTableColumnSizeId = 140000;
        int iTableColumnSizeHiddenId = 150000;
        int iTableColumnAllowNullId = 160000;
        int iTableColumnAllowNullHiddenId = 170000;
        int iTableColumnDeleteId = 180000;
        int iTableColumnDeleteHiddenId = 190000;

        //This is the top of the columns table
        int iTopTableHeight = 234;
        int iUnsavedItemWidth = 200;

        int iRowIdColumnWidth = 80;
        int iNameColumnWidth = 250;
        int iTypeColumnWidth = 270;
        int iSizeColumnWidth = 100;
        int iAllowNullColumnWidth = 150;
        int iDeleteColumnWidth = 150;

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
            ScrollView scrSQL = new ScrollView(this);
            scrSQL = DrawOpeningPage(this);
            if (scrSQL != null)
            {
                hsv.AddView(scrSQL);
                mainSV = scrSQL;
            }

        }

        public ScrollView DrawOpeningPage(Android.Content.Context context)
        {
            try
            {
                Android.App.ActionBar navBar = this.ActionBar;
                clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
                ScrollView sv = new ScrollView(context);
                RelativeLayout layout = new RelativeLayout(context);
                llMain = layout;
                string sRtnMsg = "";
                //int iWidthPixels = GetScreenWidthPixels();
                //int iHeightPixels = GetScreenHeightPixels();
                string sFormName = "SQL Manager - Table Manager";
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
                params4.Span = 3;

                TableRow.LayoutParams params33 = new TableRow.LayoutParams();
                params33.SetMargins(iPaddingButtonMargin2, iPaddingButtonMargin2, iPaddingButtonMargin2, iPaddingButtonMargin2);

                //********************************************************//
                //          EXISTING TABLE                                     //
                //********************************************************//
                TableRow rowExistingName = new TableRow(context);
                rowExistingName.SetBackgroundColor(Android.Graphics.Color.Wheat);
                rowExistingName.SetMinimumHeight(ConvertPixelsToDp(30));

                TextView lblExistingHdr = new TextView(this_context);
                lblExistingHdr.Text = "Existing Table Details";
                lblExistingHdr.SetTextColor(Android.Graphics.Color.Black);
                lblExistingHdr.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.BoldItalic);
                lblExistingHdr.SetTextSize(Android.Util.ComplexUnitType.Pt, 12);
                lblExistingHdr.SetWidth(ConvertPixelsToDp(giTableLabelNameWidth));
                lblExistingHdr.SetPadding(10, 1, 10, 1);
                lblExistingHdr.LayoutParameters = params3;
                lblExistingHdr.SetHeight(ConvertPixelsToDp(34));
                rowExistingName.AddView(lblExistingHdr);

                //Now put in the unsaved changes text view
                TextView txtChanges = new TextView(this_context);
                txtChanges.Text = "UNSAVED CHANGES";
                txtChanges.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Bold);
                txtChanges.SetWidth(ConvertPixelsToDp(iUnsavedItemWidth));
                txtChanges.SetTextColor(Android.Graphics.Color.Black);
                txtChanges.SetBackgroundColor(Android.Graphics.Color.Rgb(255, 174, 255));
                txtChanges.Gravity = GravityFlags.Center;
                txtChanges.SetPadding(5, 5, 5, 5);
                txtChanges.Id = iUnsavedChangedDialogId;
                txtChanges.Visibility = ViewStates.Invisible;
                rowExistingName.AddView(txtChanges, params3);

                table.AddView(rowExistingName);

                //Row for the new table
                TableRow rowExistName = new TableRow(context);
                rowExistName.SetBackgroundColor(Android.Graphics.Color.Wheat);
                rowExistName.SetMinimumHeight(ConvertPixelsToDp(30));

                TextView lblTableExistName = new TextView(this_context);
                lblTableExistName.Text = "Table Name";
                lblTableExistName.SetTextColor(Android.Graphics.Color.Black);
                lblTableExistName.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
                lblTableExistName.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                lblTableExistName.SetWidth(ConvertPixelsToDp(giTableLabelNameWidth));
                lblTableExistName.Id = iExistingTableNameLabelId;
                lblTableExistName.SetPadding(10, 1, 10, 1);
                lblTableExistName.LayoutParameters = params2;
                lblTableExistName.SetHeight(ConvertPixelsToDp(34));
                rowExistName.AddView(lblTableExistName);

                AndroidUtils.ComboBox cmbBox0 = new AndroidUtils.ComboBox();
                ArrayAdapter arrCmbItems0 = new ArrayAdapter(this_context, Resource.Layout.layoutSpinner); //This is the resource for the main box
                int iSelectedIndex = cmbBox0.PopulateAdapter(ref arrCmbItems0, "select TableName from tblUserTables", "", true, ref sRtnMsg);
                arrCmbItems0.SetDropDownViewResource(Resource.Layout.layoutSpinnerBase); //This is the resource for the drop down
                //Now add in a new table item
                arrCmbItems0.Insert("New Table", 1);

                Spinner cmbEdit0 = new Spinner(this_context);
                cmbEdit0.Adapter = arrCmbItems0;
                cmbEdit0.Id = iExistingTableNameId;
                cmbEdit0.SetPadding(10, 1, 10, 1);
                //cmbEdit0.LayoutParameters = params2;

                //ViewGroup.LayoutParams lp = cmbEdit0.LayoutParameters;
                //lp.Width = ConvertPixelsToDp(iTypeColumnWidth - 2 * iPaddingMargin1);
                //lp.Height = ConvertPixelsToDp(28);
                //cmbEdit0.LayoutParameters = lp;
                cmbEdit0.SetBackgroundResource(Resource.Drawable.defaultSpinner2);
                if (iSelectedIndex > 0)
                {
                    iSelectedIndex += 1;
                }
                cmbEdit0.SetSelection(iSelectedIndex);
                cmbEdit0.ItemSelected += (senderItem, args) => { ExistingTableSelected(senderItem, args); };

                rowExistName.AddView(cmbEdit0);

                Button btnExistSave = new Button(context);
                btnExistSave.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btnExistSave.SetTextColor(Android.Graphics.Color.Black);
                btnExistSave.Text = "Save";
                btnExistSave.Id = iExistingSaveTableId;
                btnExistSave.LayoutParameters = params33;
                btnExistSave.SetWidth(giSaveButtonWidth - 2 * iPaddingMargin1);
                btnExistSave.Click += (sender, args) => { SaveTable(sender, args, ""); };
                rowExistName.AddView(btnExistSave);

                Button btnExistMore = new Button(context);
                btnExistMore.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btnExistMore.SetTextColor(Android.Graphics.Color.AntiqueWhite);
                btnExistMore.Text = "More";
                btnExistMore.Id = iExistingMoreTableId;
                btnExistMore.LayoutParameters = params33;
                btnExistMore.SetWidth(giClearButtonWidth - 2 * iPaddingMargin1);
                btnExistMore.Click += (sender, args) => { MoreTableDetails(sender, args); };
                btnExistMore.Enabled = false;
                rowExistName.AddView(btnExistMore);

                //TextView lblExistBlank = new TextView(this_context);
                //lblExistBlank.Text = "";
                //lblExistBlank.SetTextColor(Android.Graphics.Color.Black);
                //lblExistBlank.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
                //lblExistBlank.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                //lblExistBlank.Id = iExistingBlankLabelId;
                //lblExistBlank.SetPadding(10, 1, 10, 1);
                //lblExistBlank.SetWidth(ConvertPixelsToDp(100));
                //lblExistBlank.LayoutParameters = params2;
                //lblExistBlank.SetHeight(ConvertPixelsToDp(34));
                //rowExistName.AddView(lblExistBlank);

                table.AddView(rowExistName);

                
                //********************************************************//
                //          NEW TABLE                                     //
                //********************************************************//
                TableRow rowNewName = new TableRow(context);
                rowNewName.SetBackgroundColor(Android.Graphics.Color.Wheat);
                rowNewName.SetMinimumHeight(ConvertPixelsToDp(30));

                TextView lblNewTableHdr = new TextView(this_context);
                lblNewTableHdr.Text = "New Table Details";
                lblNewTableHdr.SetTextColor(Android.Graphics.Color.Black);
                lblNewTableHdr.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.BoldItalic);
                lblNewTableHdr.SetTextSize(Android.Util.ComplexUnitType.Pt, 12);
                lblNewTableHdr.SetWidth(ConvertPixelsToDp(giTableLabelNameWidth));
                lblNewTableHdr.SetPadding(10, 1, 10, 1);
                lblNewTableHdr.LayoutParameters = params4;
                lblNewTableHdr.SetHeight(ConvertPixelsToDp(34));
                rowNewName.AddView(lblNewTableHdr);

                table.AddView(rowNewName);

                //Row for the new table
                TableRow rowName = new TableRow(context);
                rowName.SetBackgroundColor(Android.Graphics.Color.Wheat);
                rowName.SetMinimumHeight(ConvertPixelsToDp(30));

                TextView lblTableName = new TextView(this_context);
                lblTableName.Text = "Table Name";
                lblTableName.SetTextColor(Android.Graphics.Color.Black);
                lblTableName.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
                lblTableName.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                lblTableName.SetWidth(ConvertPixelsToDp(giTableLabelNameWidth));
                lblTableName.Id = iTableNameLabelId;
                lblTableName.SetPadding(10, 1, 10, 1);
                lblTableName.LayoutParameters = params2;
                lblTableName.SetHeight(ConvertPixelsToDp(34));
                rowName.AddView(lblTableName);

                EditText txtEdit1 = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
                txtEdit1.Text = "";
                txtEdit1.SetWidth(ConvertPixelsToDp(giTableNameWidth - 2 * iPaddingMargin1));
                txtEdit1.Id = iTableNameId;
                txtEdit1.SetPadding(10, 1, 10, 1);
                txtEdit1.LayoutParameters = params2;
                txtEdit1.SetHeight(ConvertPixelsToDp(34));
                txtEdit1.SetSingleLine(true);
                txtEdit1.Enabled = false;
                txtEdit1.SetBackgroundColor(Android.Graphics.Color.LightGray);

                txtEdit1.KeyPress += (sender, args) => { NewTableTextBoxFocusChanged(sender, args); };

                rowName.AddView(txtEdit1);


                //TextView lblSaveBlank = new TextView(this_context);
                //lblSaveBlank.Text = "";
                //lblSaveBlank.SetTextColor(Android.Graphics.Color.Black);
                //lblSaveBlank.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
                //lblSaveBlank.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                //lblSaveBlank.Id = iBlankLabelId;
                //lblSaveBlank.SetPadding(10, 1, 10, 1);
                //lblSaveBlank.SetWidth(ConvertPixelsToDp(giSaveButtonWidth));
                //lblSaveBlank.LayoutParameters = params2;
                //lblSaveBlank.SetHeight(ConvertPixelsToDp(34));
                //rowName.AddView(lblSaveBlank);

                //Button btnSave = new Button(context);
                //btnSave.SetBackgroundColor(Android.Graphics.Color.LightGray);
                //btnSave.SetTextColor(Android.Graphics.Color.Black);
                //btnSave.Text = "Save";
                //btnSave.Id = iSaveTableId;
                //btnSave.LayoutParameters = params33;
                //btnSave.SetWidth(giSaveButtonWidth - 2 * iPaddingMargin1);
                //btnSave.Click += (sender, args) => { SaveTable(sender, args); };
                //rowName.AddView(btnSave);

                Button btnClear = new Button(context);
                btnClear.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btnClear.SetTextColor(Android.Graphics.Color.Black);
                btnClear.Text = "Reset";
                btnClear.Id = iClearTableId;
                btnClear.LayoutParameters = params33;
                btnClear.SetWidth(giSaveButtonWidth - 2 * iPaddingMargin1);
                btnClear.Click += (sender, args) => { ResetTable(sender, args); };
                rowName.AddView(btnClear);

                TextView lblBlank = new TextView(this_context);
                lblBlank.Text = "";
                lblBlank.SetTextColor(Android.Graphics.Color.Black);
                lblBlank.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
                lblBlank.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                lblBlank.Id = iBlankLabelId;
                lblBlank.SetPadding(10, 1, 10, 1);
                lblBlank.SetWidth(ConvertPixelsToDp(100));
                lblBlank.LayoutParameters = params2;
                lblBlank.SetHeight(ConvertPixelsToDp(34));
                rowName.AddView(lblBlank);

                table.AddView(rowName);

                //The table builder column header row
                TableRow rowColumns = new TableRow(context);
                rowColumns.SetBackgroundColor(Android.Graphics.Color.Wheat);
                rowColumns.SetMinimumHeight(ConvertPixelsToDp(30));

                TableRow.LayoutParams params23 = new TableRow.LayoutParams();
                params23.SetMargins(iPaddingMargin1, iPaddingMargin2, iPaddingMargin1, iPaddingMargin2);
                params23.Span = 2;

                TextView lblColumnName = new TextView(this_context);
                lblColumnName.Text = "Columns";
                lblColumnName.SetTextColor(Android.Graphics.Color.Black);
                lblColumnName.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Bold);
                lblColumnName.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                lblColumnName.Id = iColumnNameLabelId;
                lblColumnName.SetPadding(10, 1, 10, 1);
                lblColumnName.LayoutParameters = params23;
                lblColumnName.SetHeight(ConvertPixelsToDp(34));
                rowColumns.AddView(lblColumnName);

                Button btnAdd = new Button(context);
                btnAdd.SetBackgroundColor(Android.Graphics.Color.LightGray);
                btnAdd.SetTextColor(Android.Graphics.Color.Black);
                btnAdd.Text = "Add";
                btnAdd.Id = iColumnAddButtonId;
                btnAdd.LayoutParameters = params33;
                btnAdd.Click += (sender, args) => { AddColumnRow(sender, args); };
                btnAdd.Enabled = false;
                rowColumns.AddView(btnAdd);

                TextView lblBlank11 = new TextView(this_context);
                lblBlank11.Text = "";
                lblBlank11.SetTextColor(Android.Graphics.Color.Black);
                lblBlank11.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
                lblBlank11.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                lblBlank11.Id = iBlankLabelId1;
                lblBlank11.SetPadding(10, 1, 10, 1);
                lblBlank11.SetWidth(ConvertPixelsToDp(100));
                lblBlank11.LayoutParameters = params2;
                lblBlank11.SetHeight(ConvertPixelsToDp(34));
                rowColumns.AddView(lblBlank11);

                table.AddView(rowColumns);
                layout.AddView(table);

                //Now put in the columns table
                TableLayout tableBody = BuildColumnsTable();
                layout.AddView(tableBody);

                sv.AddView(layout);

                return sv;
            }
            catch (Exception except)
            {
                Toast.MakeText(context, except.Message.ToString(), Android.Widget.ToastLength.Long);
                return null;
            }
        }

        public TableLayout BuildColumnsTable()
        {
            int iPaddingMargin1 = ConvertPixelsToDp(5);
            int iPaddingMargin2 = ConvertPixelsToDp(1);
            int i;
            int iRemainingHeight = GetScreenHeightPixels(); // -ConvertPixelsToDp(120);

            RelativeLayout.LayoutParams params1 = new RelativeLayout.LayoutParams(ConvertPixelsToDp(iRowIdColumnWidth + iNameColumnWidth + iTypeColumnWidth + iSizeColumnWidth + iAllowNullColumnWidth + iDeleteColumnWidth), iRemainingHeight);
            params1.SetMargins(0, ConvertPixelsToDp(iTopTableHeight), 0, 0);

            TableLayout table = new TableLayout(this_context);
            table.Id = iColumnsTable;
            table.LayoutParameters = params1;
            table.SetBackgroundColor(Android.Graphics.Color.Wheat);


            TableRow rowHdr = new TableRow(this_context);
            //This simply sets spacing between each of the elements in the row
            TableRow.LayoutParams params2 = new TableRow.LayoutParams();
            params2.SetMargins(iPaddingMargin1, iPaddingMargin2, iPaddingMargin1, iPaddingMargin2);

            TextView lblHdrRowId = new TextView(this_context);
            lblHdrRowId.Text = "Row";
            lblHdrRowId.SetTextColor(Android.Graphics.Color.Black);
            lblHdrRowId.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
            lblHdrRowId.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
            lblHdrRowId.Id = iNameLabelId;
            lblHdrRowId.SetPadding(10, 1, 10, 1);
            lblHdrRowId.SetWidth(ConvertPixelsToDp(iRowIdColumnWidth));
            lblHdrRowId.Gravity = GravityFlags.Center;
            //            lblHdrRowId.LayoutParameters = params2;
            lblHdrRowId.SetHeight(ConvertPixelsToDp(74));
            rowHdr.AddView(lblHdrRowId);

            TextView lblHdrName = new TextView(this_context);
            lblHdrName.Text = "Name";
            lblHdrName.SetTextColor(Android.Graphics.Color.Black);
            lblHdrName.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
            lblHdrName.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
            lblHdrName.Id = iNameLabelId;
            lblHdrName.SetPadding(10, 1, 10, 1);
            lblHdrName.SetWidth(ConvertPixelsToDp(iNameColumnWidth));
            lblHdrName.Gravity = GravityFlags.Center;
//            lblHdrName.LayoutParameters = params2;
            lblHdrName.SetHeight(ConvertPixelsToDp(74));
            rowHdr.AddView(lblHdrName);

            TextView hdnTableColumnName = new TextView(this_context);
            hdnTableColumnName.Text = "";
            hdnTableColumnName.Visibility = ViewStates.Gone;
            rowHdr.AddView(hdnTableColumnName);

            TextView lblHdrType = new TextView(this_context);
            lblHdrType.Text = "Type";
            lblHdrType.SetTextColor(Android.Graphics.Color.Black);
            lblHdrType.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
            lblHdrType.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
            lblHdrType.Id = iTypeLabelId;
            lblHdrType.SetPadding(10, 1, 10, 1);
            lblHdrType.SetWidth(ConvertPixelsToDp(iTypeColumnWidth));
            lblHdrType.Gravity = GravityFlags.Center;
            //            lblHdrType.LayoutParameters = params2;
            lblHdrType.SetHeight(ConvertPixelsToDp(74));
            rowHdr.AddView(lblHdrType);

            TextView hdnTableColumnType = new TextView(this_context);
            hdnTableColumnType.Text = "";
            hdnTableColumnType.Visibility = ViewStates.Gone;
            rowHdr.AddView(hdnTableColumnType);

            TextView lblHdrSize = new TextView(this_context);
            lblHdrSize.Text = "Size";
            lblHdrSize.SetTextColor(Android.Graphics.Color.Black);
            lblHdrSize.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
            lblHdrSize.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
            lblHdrSize.Id = iSizeLabelId;
            lblHdrSize.SetPadding(10, 1, 10, 1);
            lblHdrSize.SetWidth(ConvertPixelsToDp(iSizeColumnWidth));
            lblHdrSize.Gravity = GravityFlags.Center;
            //            lblHdrSize.LayoutParameters = params2;
            lblHdrSize.SetHeight(ConvertPixelsToDp(74));
            rowHdr.AddView(lblHdrSize);

            TextView hdnTableColumnSize = new TextView(this_context);
            hdnTableColumnSize.Text = "";
            hdnTableColumnSize.Visibility = ViewStates.Gone;
            rowHdr.AddView(hdnTableColumnSize);
            
            TableRow.LayoutParams params3 = new TableRow.LayoutParams();
            params3.SetMargins(0, 0, 0, 0);
            params3.Span = 3;
            params3.Gravity = GravityFlags.Center;

            TableRow.LayoutParams params4 = new TableRow.LayoutParams();
            params4.SetMargins(0, 0, 0, 0);
            params4.Span = 2;
            params4.Gravity = GravityFlags.Center;

            TextView lblHdrAllowNull = new TextView(this_context);
            lblHdrAllowNull.Text = "Allow Null";
            lblHdrAllowNull.SetTextColor(Android.Graphics.Color.Black);
            lblHdrAllowNull.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
            lblHdrAllowNull.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
            lblHdrAllowNull.Id = iAllowNullLabelId;
            lblHdrAllowNull.SetPadding(10, 1, 10, 1);
//            lblHdrAllowNull.SetWidth(ConvertPixelsToDp(iAllowNullColumnWidth));
            lblHdrAllowNull.Gravity = GravityFlags.Center;
            lblHdrAllowNull.LayoutParameters = params4;
            lblHdrAllowNull.SetHeight(ConvertPixelsToDp(74));
            rowHdr.AddView(lblHdrAllowNull);

            TextView hdnTableColumnAllowNull = new TextView(this_context);
            hdnTableColumnAllowNull.Text = "";
            hdnTableColumnAllowNull.Visibility = ViewStates.Gone;
            rowHdr.AddView(hdnTableColumnAllowNull);

            TextView lblHdrDelete = new TextView(this_context);
            lblHdrDelete.Text = "Delete";
            lblHdrDelete.SetTextColor(Android.Graphics.Color.Black);
            lblHdrDelete.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
            lblHdrDelete.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
            lblHdrDelete.Id = iDeleteLabelId;
            lblHdrDelete.SetPadding(10, 1, 10, 1);
//                        lblHdrDelete.SetWidth(ConvertPixelsToDp(iDeleteColumnWidth));
            lblHdrDelete.Gravity = GravityFlags.Center;
            lblHdrDelete.LayoutParameters = params4;
            lblHdrDelete.SetHeight(ConvertPixelsToDp(74));
            rowHdr.AddView(lblHdrDelete);

            TextView hdnTableColumnDelete = new TextView(this_context);
            hdnTableColumnDelete.Text = "";
            hdnTableColumnDelete.Visibility = ViewStates.Gone;
            rowHdr.AddView(hdnTableColumnDelete);

            table.AddView(rowHdr);

            //int iRowCounter = 10;
            //bool bAutoIncrementRow;

            //for (i = 0; i < iRowCounter; i++)
            //{
            //    if (i == 0)
            //    {
            //        bAutoIncrementRow = true;
            //    }
            //    else
            //    {
            //        bAutoIncrementRow = false;
            //    }
            //    TableRow rowCol = MakeTableManagerTableRow(i + 1, "", "", "", true, bAutoIncrementRow);                
            //    table.AddView(rowCol);
            //}

            return table;
        }

        //The RowId is 1 based
        public TableRow MakeTableManagerTableRow(int iRowId, string sColumnName, string sType, string sSize, bool bNull, bool bFirstRow)
        {
            int iPaddingMargin1 = ConvertPixelsToDp(5);
            int iPaddingMargin2 = ConvertPixelsToDp(1);
            string sRtnMsg = "";
            string sSelections;

            //This simply sets spacing between each of the elements in the row
            TableRow.LayoutParams params2 = new TableRow.LayoutParams();
            params2.SetMargins(iPaddingMargin1, iPaddingMargin2, iPaddingMargin1, iPaddingMargin2);

            TableRow row = new TableRow(this_context);

            TextView txtRowId = new TextView(this_context);
            txtRowId.Text = iRowId.ToString();
            txtRowId.SetWidth(ConvertPixelsToDp(iRowIdColumnWidth - 2 * iPaddingMargin1));
            txtRowId.Id = iTableColumnRowId + iRowId;
            txtRowId.SetTextColor(Android.Graphics.Color.Black);
            txtRowId.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Bold);
            txtRowId.SetTextSize(Android.Util.ComplexUnitType.Pt, 8);
            row.AddView(txtRowId);

            EditText txtEdit1 = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
            txtEdit1.Text = sColumnName;
            txtEdit1.SetWidth(ConvertPixelsToDp(iNameColumnWidth - 2 * iPaddingMargin1));
            txtEdit1.Id = iTableColumnNameId + iRowId;
            txtEdit1.SetPadding(5, 1, 5, 1);
            txtEdit1.LayoutParameters = params2;
            txtEdit1.SetHeight(ConvertPixelsToDp(34));
            txtEdit1.SetSingleLine(true);

            txtEdit1.FocusChange += (sender, args) => { ColumnNameTextBoxFocusChanged(sender, args); };


            if (bFirstRow)
            {
                txtEdit1.Text = "UniqueId";
                txtEdit1.Enabled = false;
                txtEdit1.SetBackgroundColor(Android.Graphics.Color.LightGray);
            }

            row.AddView(txtEdit1);

            TextView hdnTableColumnName = new TextView(this_context);
            hdnTableColumnName.Text = sColumnName;
            hdnTableColumnName.Id = iTableColumnNameHiddenId + iRowId;
            hdnTableColumnName.Visibility = ViewStates.Gone;
            row.AddView(hdnTableColumnName);

            AndroidUtils.ComboBox cmbBox0 = new AndroidUtils.ComboBox();
            ArrayAdapter arrCmbItems0 = new ArrayAdapter(this_context, Resource.Layout.layoutSpinner); //This is the resource for the main box
            //Now add a string semi colon separated for the drop down items
            if (bFirstRow)
            {
                sSelections = "Integer Primary Key AutoIncrement;float;nvarchar";
                sType = "Integer Primary Key AutoIncrement";
            }
            else
            {
                sSelections = "int;float;nvarchar";
            }

            int iSelectedIndex = cmbBox0.PopulateAdapter(ref arrCmbItems0, sSelections, sType, true, ref sRtnMsg);
            arrCmbItems0.SetDropDownViewResource(Resource.Layout.layoutSpinnerBase); //This is the resource for the drop down
            Spinner cmbEdit0 = new Spinner(this_context);
            cmbEdit0.Adapter = arrCmbItems0;
            cmbEdit0.Id = iTableColumnTypeId + iRowId;
            cmbEdit0.SetPadding(10, 1, 10, 1);
            //cmbEdit0.LayoutParameters = params2;

            //ViewGroup.LayoutParams lp = cmbEdit0.LayoutParameters;
            //lp.Width = ConvertPixelsToDp(iTypeColumnWidth - 2 * iPaddingMargin1);
            //lp.Height = ConvertPixelsToDp(28);
            //cmbEdit0.LayoutParameters = lp;
            cmbEdit0.SetBackgroundResource(Resource.Drawable.defaultSpinner2);
            if (iSelectedIndex < 0)
            {
                iSelectedIndex = 0;
            }
            cmbEdit0.SetSelection(iSelectedIndex);

            if (bFirstRow)
            {
                cmbEdit0.Enabled = false;
                cmbEdit0.SetBackgroundResource(Resource.Drawable.defaultSpinnerLightGray);
            }
            cmbEdit0.ItemSelected += (sender, args) => { ColumnTypeChanged(sender, args); };

            row.AddView(cmbEdit0);

            TextView hdnTableColumnType = new TextView(this_context);
            hdnTableColumnType.Text = iSelectedIndex.ToString();
            hdnTableColumnType.Id = iTableColumnTypeHiddenId + iRowId;
            hdnTableColumnType.Visibility = ViewStates.Gone;
            row.AddView(hdnTableColumnType);

            EditText txtSize = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
            txtSize.Text = sSize;
            txtSize.SetWidth(ConvertPixelsToDp(iSizeColumnWidth - 2 * iPaddingMargin1));
            txtSize.Id = iTableColumnSizeId + iRowId;
            txtSize.SetPadding(5, 1, 5, 1);
            txtSize.LayoutParameters = params2;
            txtSize.SetHeight(ConvertPixelsToDp(34));
            txtSize.SetSingleLine(true);
            if (sType == "nvarchar")
            {
                txtSize.Enabled = true;
            }
            else
            {
                txtSize.Text = "";
                txtSize.Enabled = false;
                txtSize.SetBackgroundColor(Android.Graphics.Color.LightGray);
            }

            if (bFirstRow)
            {
                txtSize.Text = "";
                txtSize.Enabled = false;
                txtSize.SetBackgroundColor(Android.Graphics.Color.LightGray);
            }

            txtSize.AfterTextChanged += (sender, args) => { ColumnSizeTextBoxChanged(sender, args); };
            row.AddView(txtSize);

            TextView hdnTableColumnSize = new TextView(this_context);
            hdnTableColumnSize.Text = sSize;
            hdnTableColumnSize.Id = iTableColumnSizeHiddenId + iRowId;
            hdnTableColumnSize.Visibility = ViewStates.Gone;
            row.AddView(hdnTableColumnSize);

            TextView hdnTableColumnSize2 = new TextView(this_context);
            hdnTableColumnSize2.Text = "";
            hdnTableColumnSize2.SetWidth(ConvertPixelsToDp(40));
            row.AddView(hdnTableColumnSize2);

            int iWidthOfCheckbox = 50;
            CheckBox chkBox = new CheckBox(this_context);
            chkBox.Text = "";
            chkBox.SetTextColor(Android.Graphics.Color.Black);
            chkBox.SetWidth(ConvertPixelsToDp(iWidthOfCheckbox));
            chkBox.Id = iTableColumnAllowNullId + iRowId; ;
            chkBox.Checked = bNull;
            chkBox.SetButtonDrawable(Resource.Drawable.chkboxDefaultStates);
            if (bFirstRow)
            {
                chkBox.Enabled = false;
            }

            row.AddView(chkBox);

            TextView hdnTableColumnAllowNull = new TextView(this_context);
            hdnTableColumnAllowNull.Text = bNull ? "1" : "0";
            hdnTableColumnAllowNull.Id = iTableColumnAllowNullHiddenId + iRowId;
            hdnTableColumnAllowNull.Visibility = ViewStates.Gone;
            row.AddView(hdnTableColumnAllowNull);

            TextView hdnTableColumnSize3 = new TextView(this_context);
            hdnTableColumnSize3.Text = "";
            hdnTableColumnSize3.SetWidth(ConvertPixelsToDp(40));
            row.AddView(hdnTableColumnSize3);

            CheckBox chkBoxDelete = new CheckBox(this_context);
            chkBoxDelete.Text = "";
            chkBoxDelete.SetTextColor(Android.Graphics.Color.Black);
            chkBoxDelete.SetWidth(ConvertPixelsToDp(iWidthOfCheckbox));
            chkBoxDelete.Id = iTableColumnDeleteId + iRowId; ;
            chkBoxDelete.Checked = false;
            chkBoxDelete.SetButtonDrawable(Resource.Drawable.chkboxDefaultStates);
            chkBoxDelete.Click += (sender, args) => { CheckBoxDeleteChanged(sender, args); };

            if (bFirstRow)
            {
                chkBoxDelete.Enabled = false;
            }

            row.AddView(chkBoxDelete);

            TextView hdnTableColumnDelete = new TextView(this_context);
            hdnTableColumnDelete.Text = "0";
            hdnTableColumnDelete.Id = iTableColumnDeleteHiddenId + iRowId;
            hdnTableColumnDelete.Visibility = ViewStates.Gone;
            row.AddView(hdnTableColumnDelete);

            return row;

        }

        public void AddColumnRow(object sender, EventArgs e)
        {
            TableLayout table = (TableLayout)FindViewById(iColumnsTable);
            TableRow row = MakeTableManagerTableRow(table.ChildCount, "", "", "", true, false);
            table.AddView(row);

            int iRemainingHeight = GetScreenHeightPixels(); // -ConvertPixelsToDp(120);

            if (iRemainingHeight < table.ChildCount * ConvertPixelsToDp(40) + ConvertPixelsToDp(iTopTableHeight))
            {
                iRemainingHeight = table.ChildCount * ConvertPixelsToDp(40) + ConvertPixelsToDp(iTopTableHeight);
            }
            RelativeLayout.LayoutParams params1 = new RelativeLayout.LayoutParams(ConvertPixelsToDp(iRowIdColumnWidth + iNameColumnWidth + iTypeColumnWidth + iSizeColumnWidth + iAllowNullColumnWidth + iDeleteColumnWidth), iRemainingHeight);
            params1.SetMargins(0, ConvertPixelsToDp(iTopTableHeight), 0, 0);
            table.LayoutParameters = params1;
        }

        public void ExistingTableSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            clsLocalUtils utils = new clsLocalUtils();
            string sRtnMsg = "";
            Spinner cmbBox = (Spinner)sender;
            string sTableName = cmbBox.SelectedItem.ToString();
            EditText txtNewName = (EditText)FindViewById(iTableNameId);
            string sNewName = "";
            if (txtNewName != null)
            {
                sNewName = txtNewName.Text;
            }
            if (GetEditStatus() == 1)
            {
                if (sNewName != "")
                {
                    sTableName = sNewName;
                }

                AlertDialog ad = new AlertDialog.Builder(this).Create();
                ad.SetCancelable(false); // This blocks the 'BACK' button  
                ad.SetMessage("You have unsaved changes. Do you wish to save these changes");
                ad.SetButton("Yes", (s, ee) => { SaveTable(s, ee, sTableName); });
                ad.SetButton2("No", (s, ee) => { ShowTable(s, ee, sTableName); });
                ad.Show();
                return;
            }
            else
            {
                ClearTable(sender, e, sTableName);
            }

            SetNewTableDetails(sTableName);

            if (sTableName != "[select]" && sTableName != "New Table")
            {
                PopulateColumnsTable(sTableName);
                EnableMoreButton(true);
            }
            return;

        }

        public void EnableMoreButton(bool bOn)
        {

            Button btn = (Button)FindViewById(iExistingMoreTableId);
            if (bOn)
            {
                btn.Enabled = true;
                btn.SetTextColor(Android.Graphics.Color.Black);
            }
            else
            {
                btn.Enabled = false;
                btn.SetTextColor(Android.Graphics.Color.AntiqueWhite);
            }

        }
        public void SetNewTableDetails(string sTableName)
        {
            EditText txtNewTable = (EditText)FindViewById(iTableNameId);
            if (sTableName == "New Table")
            {
                txtNewTable.Enabled = true;
                txtNewTable.SetBackgroundColor(Android.Graphics.Color.White);
                txtNewTable.RequestFocus();
            }
            else
            {
                txtNewTable.Text = "";
                txtNewTable.Enabled = false;
                txtNewTable.SetBackgroundColor(Android.Graphics.Color.LightGray);
            }

            return;
        }

        public void PopulateColumnsTable(string sTableName)
        {
            string sRtnMsg = "";
            int i;
            int iRows;
            LocalDB DB = new LocalDB();
            ArrayList arrColNames = new ArrayList();
            ArrayList arrColTypes = new ArrayList();
            ArrayList arrColSize = new ArrayList();
            ArrayList arrNull = new ArrayList();
            bool bNull;

            ArrayList arrColDetails = DB.GetTableColumnDetails(sTableName, ref sRtnMsg);

            //Now for each piece of info in the array we need to build a row
            arrColNames = (ArrayList)arrColDetails[0];
            arrColTypes = (ArrayList)arrColDetails[1];
            arrColSize = (ArrayList)arrColDetails[2];
            arrNull = (ArrayList)arrColDetails[3];
            iRows = arrColNames.Count;

            TableLayout table = (TableLayout)FindViewById(iColumnsTable);

            for (i = 0; i < iRows; i++)
            {
                if(arrNull[i].ToString() == "0")
                {
                    bNull = true;
                }
                else
                {
                    bNull = false;
                }
                TableRow row = MakeTableManagerTableRow(i + 1, arrColNames[i].ToString(), arrColTypes[i].ToString(), arrColSize[i].ToString(), bNull, i == 0 ? true : false);
                if (row != null)
                {
                    table.AddView(row);
                }
            }

            Button btnAdd = (Button)FindViewById(iColumnAddButtonId);
            btnAdd.Enabled = true;

            return;
        }

        public void SaveTable(object sender, EventArgs e, string sTableName)
        {
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            clsLocalUtils utils = new clsLocalUtils();
            string sRtnMsg = "";
            LocalDB DB = new LocalDB();
            int i, j;
            bool bExisting = false;

            Spinner cmbExisting = (Spinner)FindViewById(iExistingTableNameId);
            string sExistingTableName = cmbExisting.SelectedItem.ToString();

            EditText txtName = (EditText)FindViewById(iTableNameId);
            string sNewTableName = txtName.Text;

            if (sExistingTableName == "New Table")
            {
                sTableName = txtName.Text;
            }
            else if(sTableName == "")
            {
                sTableName = sExistingTableName;
                bExisting = true;
            }

            if (sTableName == "" || sTableName == "[select]")
            {
                //Get the modify table name and if that is also empty then stop
                alert.SetAlertMessage("You must provide a table name in either the new table field or you must have selected an existing table to modify.");
                alert.ShowAlertBox();
                return;
            }

            if (grdUtils.UserTableExists(sTableName, ref sRtnMsg) && !bExisting)
            {
                alert.SetAlertMessage("The table " + sTableName + " already exists. Please choose a new name.");
                alert.ShowAlertBox();
                return;
            }

            if (grdUtils.ReservedTableExists(sTableName, ref sRtnMsg))
            {
                alert.SetAlertMessage("The table " + sTableName + " is a reserved table name. Please choose a new name.");
                alert.ShowAlertBox();
                return;
            }

            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                alert.ShowAlertBox();
                return;
            }

            //Check for any matchingcolumns
            int iCounter = 0;
            TableLayout table = (TableLayout)FindViewById(iColumnsTable);
            string sValue1;
            string sValue2;
            string sValue4;
            string sValue5;
            string sValue6;
            string sValue7;
            string sValue8;
            string sCombo1;
            int iValue4 = 0;
            int iValue7 = 0;
            bool bShowAlert = false;
            bool bShowAlert2 = false;
            bool bShowAlert3 = false;
            bool bShowAlert4 = false;
            bool bShowAlert5 = false;
            string sAlertMsg = "You cannot have 2 columns with the same name. The duplcate columns are row Ids ";
            string sAlertMsg2 = "You cannot have any blank columns. The columns that are blank are row Ids ";
            string sAlertMsg3 = "You cannot have any columns with type that is '[select]'. Please fix these columns which are row Ids ";
            string sAlertMsg4 = "All coumns of type 'nvarchar' require a size. Please fix these columns which are row Ids ";
            string sAlertMsg5 = "There could be some loss of data because you are changing column types or changing column sizing.";

            for (j = 1; j < table.ChildCount; j++)
            {
                CheckBox chkDel1 = (CheckBox)FindViewById(iTableColumnDeleteId + j);

                EditText txt1 = (EditText)FindViewById(iTableColumnNameId + j);
                sValue1 = txt1.Text;
                if (sValue1 == "" && !chkDel1.Checked)
                {
                    sAlertMsg2 = sAlertMsg2 +  j.ToString() + ", ";
                    bShowAlert2 = true;
                }

                Spinner cmb1 = (Spinner)FindViewById(iTableColumnTypeId + j);
                sCombo1 = cmb1.SelectedItem.ToString();
                if (sCombo1 == "[select]" && !chkDel1.Checked)
                {
                    sAlertMsg3 = sAlertMsg3 + j.ToString() + ", ";
                    bShowAlert3 = true;
                }

                EditText txt4 = (EditText)FindViewById(iTableColumnSizeId + j);
                sValue4 = txt4.Text;
                if (sValue4 == "" && sCombo1 == "nvarchar" && !chkDel1.Checked)
                {
                    sAlertMsg4 = sAlertMsg2 + j.ToString() + ", ";
                    bShowAlert4 = true;
                }

                if (utils.IsNumeric(sValue4))
                {
                    iValue4 = Convert.ToInt32(sValue4);
                }

                //We are updating an existing table
                if (sNewTableName == "")
                {
                    TextView txt5 = (TextView)FindViewById(iTableColumnNameHiddenId + j);
                    sValue5 = txt5.Text;

                    TextView txt6 = (TextView)FindViewById(iTableColumnTypeHiddenId + j);
                    sValue6 = txt6.Text;

                    TextView txt7 = (TextView)FindViewById(iTableColumnSizeHiddenId + j);
                    sValue7 = txt7.Text;
                    if (utils.IsNumeric(sValue7))
                    {
                        iValue7 = Convert.ToInt32(sValue7);
                    }

                    TextView txt8 = (TextView)FindViewById(iTableColumnAllowNullHiddenId + j);
                    sValue8 = txt8.Text;

                    if (((sValue6 == "nvarchar" && sCombo1 != "nvarchar") || (sValue6 == "float" && sCombo1 == "int")) ||
                        (iValue7 > iValue4))
                    {
                        bShowAlert5 = true;
                    }
                }

                for (i = j + 1; i < table.ChildCount; i++)
                {
                    EditText txt2 = (EditText)FindViewById(iTableColumnNameId + i);
                    sValue2 = txt2.Text;
                    CheckBox chkDel2 = (CheckBox)FindViewById(iTableColumnDeleteId + i);
                    if (i != j && sValue1.ToUpper() == sValue2.ToUpper() && !chkDel1.Checked && !chkDel2.Checked)
                    {
                        sAlertMsg = sAlertMsg + j.ToString() + " and " + i.ToString() + ", ";
                        bShowAlert = true;
                    }
                }
            }

            if (bShowAlert)
            {
                alert.SetAlertMessage(sAlertMsg.Substring(0, sAlertMsg.Length - 2));
                alert.ShowAlertBox();
                return;
            }

            if (bShowAlert2)
            {
                sAlertMsg2 = sAlertMsg2.Substring(0, sAlertMsg2.Length - 2) + ". These columns will be ignored.";
                alert.SetAlertMessage(sAlertMsg2);
                alert.ShowAlertBox();
            }

            if (bShowAlert3)
            {
                alert.SetAlertMessage(sAlertMsg3.Substring(0, sAlertMsg3.Length - 2));
                alert.ShowAlertBox();
                return;
            }

            if (bShowAlert4)
            {
                alert.SetAlertMessage(sAlertMsg4.Substring(0, sAlertMsg4.Length - 2));
                alert.ShowAlertBox();
                return;
            }

            if (bShowAlert5)
            {
                AlertDialog ad = new AlertDialog.Builder(this).Create();
                ad.SetCancelable(false); // This blocks the 'BACK' button  
                ad.SetMessage(sAlertMsg5);
                ad.SetButton("Yes", (s, ee) => { SaveExistingTable(s, ee, true); });
                ad.SetButton2("No", (s, ee) => { SaveExistingTable(s, ee, false); });
                ad.Show();
                return;
            }


            //If we get to here then all the info is correct and we can create the table
            string[] sTypes = new string[table.ChildCount - 1];  
            string[] sColumns = new string[table.ChildCount - 1];
            string sSize = "";
            string sNull = "";

            if (bExisting)
            {
                SaveExistingTable(sender, e, true);
            }
            else
            {
                for (j = 1; j < table.ChildCount; j++)
                {
                    EditText txt11 = (EditText)FindViewById(iTableColumnNameId + j);
                    sValue1 = txt11.Text;
                    Spinner cmb11 = (Spinner)FindViewById(iTableColumnTypeId + j);
                    sCombo1 = cmb11.SelectedItem.ToString();
                    EditText txt4 = (EditText)FindViewById(iTableColumnSizeId + j);
                    sValue4 = txt4.Text;
                    CheckBox chk = (CheckBox)FindViewById(iTableColumnAllowNullId + j);
                    if (chk.Checked)
                    {
                        sNull = " NULL";
                    }
                    else
                    {
                        sNull = " NOT NULL";
                    }

                    if (sCombo1 == "nvarchar")
                    {
                        sSize = " (" + sValue4.ToString() + ")";
                    }
                    else
                    {
                        sSize = "";
                    }

                    CheckBox chkDel = (CheckBox)FindViewById(iTableColumnDeleteId + j);
                    if (!chkDel.Checked)
                    {
                        sColumns[iCounter] = sValue1;
                        sTypes[iCounter] = "[" + sCombo1 + "]" + sSize + sNull;
                        iCounter++;
                    }

                }

                string[] sColumnsToAdd = new string[iCounter];
                string[] sTypesToAdd = new string[iCounter];

                for (i = 0; i < iCounter; i++)
                {
                    sColumnsToAdd[i] = sColumns[i];
                    sTypesToAdd[i] = sTypes[i];
                }

                if (!DB.TableExists(sTableName))
                {
                    DB.CreateTable(sTableName, sColumnsToAdd, sTypesToAdd);
                    grdUtils.SetUserTableRecord(sTableName, ref sRtnMsg);
                }


                SetAnyValueChangedOff();
                RefreshExistingTablesCombo(sTableName);
            }
            return;
        }

        public void SaveExistingTable(object sender, EventArgs e, bool bActuallySave)
        {
            LocalDB DB = new LocalDB();
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            TableLayout table = (TableLayout)FindViewById(iColumnsTable);
            Spinner cmb = (Spinner)FindViewById(iExistingTableNameId);
            string sTableName = cmb.SelectedItem.ToString();

            if (bActuallySave)
            {
                //If we get to here then all the info is correct andwe can create the table
                int i,j;
                string[] sTypes = new string[table.ChildCount - 1];
                string[] sColumns = new string[table.ChildCount - 1];
                string sSize = "";
                string sNull = "";
                string sValue1;
                string sValue1Old;
                string sValue2Old;
                string sValue4;
                string sValue4Old;
                string sValue5Old;
                string sComboIndex;
                string sCombo1;
                string sSQL;
                string sRtnMsg = "";
                string sOverallRtnMsg = "";
                bool bAnyBadRtn = false;
                bool bRtn;
                bool bColumnRenameRqd = false;
                bool bProceed = false;
                bool bChange;
                int iRenameCounter = 0;
                string[] sOldColNames = new string[table.ChildCount - 1];
                string[] sNewColNames = new string[table.ChildCount - 1];
                string[] sNewColTypes = new string[table.ChildCount - 1];

                for (j = 1; j < table.ChildCount; j++)
                {
                    EditText txt1 = (EditText)FindViewById(iTableColumnNameId + j);
                    sValue1 = txt1.Text;
                    TextView txtOld1 = (TextView)FindViewById(iTableColumnNameHiddenId + j);
                    sValue1Old = txtOld1.Text;
                    Spinner cmb11 = (Spinner)FindViewById(iTableColumnTypeId + j);
                    sCombo1 = cmb11.SelectedItem.ToString();
                    sComboIndex = cmb11.SelectedItemPosition.ToString();
                    TextView txtOld2 = (TextView)FindViewById(iTableColumnTypeHiddenId + j);
                    sValue2Old = txtOld2.Text;
                    EditText txt4 = (EditText)FindViewById(iTableColumnSizeId + j);
                    sValue4 = txt4.Text;
                    TextView txtOld4 = (TextView)FindViewById(iTableColumnSizeHiddenId + j);
                    sValue4Old = txtOld4.Text;
                    CheckBox chk = (CheckBox)FindViewById(iTableColumnAllowNullId + j);
                    if (chk.Checked)
                    {
                        sNull = " NULL";
                    }
                    else
                    {
                        sNull = " NOT NULL";
                    }

                    TextView txtOld5 = (TextView)FindViewById(iTableColumnAllowNullHiddenId + j);
                    sValue5Old = txtOld5.Text;
                    bChange = false;
                    if ((sValue5Old == "1" && sNull == " NOT NULL") || (sValue5Old == "0" && sNull == " NULL"))
                    {
                        bChange = true;
                    }

                    if (sCombo1 == "nvarchar")
                    {
                        sSize = " (" + sValue4.ToString() + ")";
                    }
                    else
                    {
                        sSize = "";
                    }

                    bProceed = true;
                    CheckBox chkDel = (CheckBox)FindViewById(iTableColumnDeleteId + j);
                    if (chkDel.Checked && sValue1Old == "")
                    {
                        bProceed = false;
                    }

                    if (bProceed && (bChange || sValue1Old != sValue1 || sComboIndex != sValue2Old || sValue4Old != sValue4))
                    {
                        if (sValue1Old == "")
                        {
                            sSQL = "ALTER TABLE " + sTableName + " ADD COLUMN " + sValue1 + " " + sCombo1 + " " + sSize + " " + sNull;
                            bRtn = DB.ExecuteSQL(sSQL, ref sRtnMsg);

                            if (!bRtn)
                            {
                                bAnyBadRtn = true;
                                sOverallRtnMsg += sRtnMsg + " ";
                            }
                        }
                        else if(sValue1Old == sValue1) //This we hav to do by dropping the table (Arghhhhhh!!!!!)
                        {
                            //sSQL = "ALTER TABLE " + sTableName + " ALTER COLUMN " + sValue1 + " " + sCombo1 + " " + sSize + " " + sNull;
                            //bRtn = DB.ExecuteSQL(sSQL, ref sRtnMsg);

                            //if (!bRtn)
                            //{
                            //    bAnyBadRtn = true;
                            //    sOverallRtnMsg += sRtnMsg + " ";
                            //}
                            sOldColNames[iRenameCounter] = sValue1Old;

                            sNewColTypes[iRenameCounter] = "[" + sCombo1 + "] " + sSize + " " + sNull;
//                            sOldType = sCombo1 + " " + sSize + " " + sNull
                            if (chkDel.Checked)
                            {
                                sNewColNames[iRenameCounter] = "";
                            }
                            else
                            {
                                sNewColNames[iRenameCounter] = sValue1;
                            }
                            bColumnRenameRqd = true;
                            iRenameCounter++;
                        }
                    }

                }

                //Now if columns are renamed we need to change tables
                if (bColumnRenameRqd)
                {
                    //Trim the arrays down
                    string[] sOldReplaceNames = new string[iRenameCounter];
                    string[] sNewReplaceNames = new string[iRenameCounter];
                    string[] sNewReplaceTypes = new string[iRenameCounter];
                    for (i = 0; i < iRenameCounter; i++)
                    {
                        sOldReplaceNames[i] = sOldColNames[i];
                        sNewReplaceNames[i] = sNewColNames[i];
                        sNewReplaceTypes[i] = sNewColTypes[i];
                    }
                    //Create a new table
                    string sTableNameTemp = sTableName + "TempZZZZZZZZZ";
                    do
                    {
                        sTableNameTemp += "Z";
                    }
                    while(DB.TableExists(sTableNameTemp));

                    bRtn = DB.CopyTableStructureAndData(sTableName, sTableNameTemp, sOldReplaceNames, sNewReplaceNames, sNewReplaceTypes, true, ref sRtnMsg);
                    if (sRtnMsg != "" || !bRtn)
                    {
                        bAnyBadRtn = true;
                        sOverallRtnMsg += sRtnMsg;
                    }
                    else
                    {
                        bRtn = DB.DropTable(sTableName, ref sRtnMsg);
                        if (sRtnMsg != "" || !bRtn)
                        {
                            bAnyBadRtn = true;
                            sOverallRtnMsg += sRtnMsg;
                        }
                        else
                        {
                            bRtn = grdUtils.DeleteUserTableRecord(sTableName, ref sRtnMsg);
                            if (sRtnMsg != "" || !bRtn)
                            {
                                bAnyBadRtn = true;
                                sOverallRtnMsg += sRtnMsg;
                            }
                            else
                            {
                                bRtn = DB.CopyTableStructureAndData(sTableNameTemp, sTableName, null, null, null, true, ref sRtnMsg);
                                if (sRtnMsg != "" || !bRtn)
                                {
                                    bAnyBadRtn = true;
                                    sOverallRtnMsg += sRtnMsg;
                                }
                                else
                                {
                                    bRtn = DB.DropTable(sTableNameTemp, ref sRtnMsg);
                                    if (sRtnMsg != "" || !bRtn)
                                    {
                                        bAnyBadRtn = true;
                                        sOverallRtnMsg += sRtnMsg;
                                    }
                                    else
                                    {
                                        bRtn = grdUtils.SetUserTableRecord(sTableName, ref sRtnMsg);
                                        if (sRtnMsg != "" || !bRtn)
                                        {
                                            bAnyBadRtn = true;
                                            sOverallRtnMsg += sRtnMsg;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (bAnyBadRtn)
                {
                    alert.SetAlertMessage(sOverallRtnMsg);
                    alert.ShowAlertBox();
                    return;
                }
            }

            SetAnyValueChangedOff();
            return;
        }

        public void ResetTable(object sender, EventArgs e)
        {
            AlertDialog ad = new AlertDialog.Builder(this).Create();
            ad.SetCancelable(false); // This blocks the 'BACK' button  
            ad.SetMessage("You have unsaved changes. Do you wish to save these changes");
            ad.SetButton("Yes", (s, ee) => { PerformReset(s, ee, true); });
            ad.SetButton2("No", (s, ee) => { PerformReset(s, ee, false); });
            ad.Show();
            return;

        }

        public void PerformReset(object sender, EventArgs e, bool bReset)
        {
            if (bReset)
            {
                Spinner cmb = (Spinner)FindViewById(iExistingTableNameId);
                //string sTableName = cmb.SelectedItem.ToString();
                cmb.SetSelection(0);
                ClearTable(sender, e, "");
                SetAnyValueChangedOff();
            }

            return;
        }

        public void ShowTable(object sender, EventArgs e, string sTableName)
        {
            ClearTable(sender, e, sTableName);
            SetAnyValueChangedOff();
            if (sTableName != "New Table")
            {
                PopulateColumnsTable(sTableName);
            }
            else
            {
                SetNewTableDetails(sTableName);
            }
        }

        public void ClearTable(object sender, EventArgs e, string sTableName)
        {
            TableLayout table = (TableLayout)FindViewById(iColumnsTable);
            int i;

            for (i = table.ChildCount - 1; i > 0; i--)
            {
//                TableRow row = (TableRow)table.GetChildAt(i);
                table.RemoveViewAt(i);
            }

            //Populate either the drop down or the new table info
            SetNewTableDetails(sTableName);
        }

        public void RefreshExistingTablesCombo(string sTableName)
        {
            string sRtnMsg = "";
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            Spinner cmb = (Spinner)FindViewById(iExistingTableNameId);
            AndroidUtils.ComboBox cmbBox0 = new AndroidUtils.ComboBox();
            ArrayAdapter arrCmbItems0 = new ArrayAdapter(this_context, Resource.Layout.layoutSpinner); //This is the resource for the main box
            int iSelectedIndex = cmbBox0.PopulateAdapter(ref arrCmbItems0, "select TableName from tblUserTables", sTableName, true, ref sRtnMsg);
            arrCmbItems0.SetDropDownViewResource(Resource.Layout.layoutSpinnerBase); //This is the resource for the drop down
            arrCmbItems0.Insert("New Table", 1);
            cmb.Adapter = arrCmbItems0;
            if (iSelectedIndex > 0)
            {
                iSelectedIndex += 1;
            }

            if (sTableName == "New Table")
            {
                iSelectedIndex = 1;
            }
            cmb.SetSelection(iSelectedIndex);
            return;
        }


        public void ColumnTypeChanged(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner cmbBox = (Spinner)sender;
            string sColumnType = cmbBox.SelectedItem.ToString();
            string sColumnTypeIndex = cmbBox.SelectedItemPosition.ToString();
            int iTypeId = cmbBox.Id;
            int iTypeOldId = iTypeId - iTableColumnTypeId + iTableColumnTypeHiddenId;
            int iSizeId = iTypeId - iTableColumnTypeId + iTableColumnSizeId;
            int iSizeOldId = iTypeId - iTableColumnTypeId + iTableColumnSizeHiddenId;
            EditText txtSize = (EditText)FindViewById(iSizeId);

            if (txtSize != null)
            {
                if (sColumnType == "nvarchar")
                {
                    TextView txtOldSize = (TextView)FindViewById(iSizeOldId);
                    string sOldSize = txtOldSize.Text;
                    txtSize.Enabled = true;
                    txtSize.SetBackgroundColor(Android.Graphics.Color.White);
                    if (sOldSize == "")
                    {
                        txtSize.Text = "50";
                    }
                    else
                    {
                        txtSize.Text = sOldSize;
                    }
                }
                else
                {
                    txtSize.Enabled = false;
                    txtSize.SetBackgroundColor(Android.Graphics.Color.LightGray);
                    txtSize.Text = "";
                }
            }
            TextView cmbBoxOld = (TextView)FindViewById(iTypeOldId);
            string sColumnTypeOld = cmbBoxOld.Text;
            if (sColumnTypeOld != sColumnTypeIndex)
            {
                SetAnyValueChanged();
            }
            return;
        }

        public void MoreTableDetails(object sender, EventArgs e)
        {
            ScrollView sv = new ScrollView(this_context);
            Android.App.ActionBar navBar = this.ActionBar;
            int iTopBarHeight = navBar.Height;
            //Get the position of the button
            Button btn = (Button)sender;
            int iId = btn.Id + 100; //Just add 100 more to create the new base
            int[] iPosn = new int[2];
            btn.GetLocationOnScreen(iPosn);
            int iLeft = (iPosn[0]) + (btn.Width); // btn.Left + btn.Width;
            int iTop = (iPosn[1]) + (btn.Height) - iTopBarHeight; // btn.Top + btn.Height;

            if (iLeft + ConvertPixelsToDp(350) > GetScreenWidthPixels())
            {
                iLeft = iLeft - ConvertPixelsToDp(250);
            }

            if (iTop + ConvertPixelsToDp(200) > GetScreenHeightPixels())
            {
                iTop = iTop - ConvertPixelsToDp(200);
            }

            //Create a new RelativeLayout
            RelativeLayout rl = new RelativeLayout(this_context);
            rl.Id = btn.Id + 101;

            RelativeLayout.LayoutParams params1 = new RelativeLayout.LayoutParams(ConvertPixelsToDp(350), ConvertPixelsToDp(280));
            params1.SetMargins(iLeft, iTop, 0, 0);
            TableLayout table = new TableLayout(this_context);
            table.SetGravity(GravityFlags.CenterHorizontal);
            table.SetBackgroundColor(Android.Graphics.Color.Gray);
            table.Id = btn.Id + 102;

            //Put in a header so the user knows whar button they have selected
            TableRow rowHdr = new TableRow(this_context);
            rowHdr.SetBackgroundColor(Android.Graphics.Color.Gray);
            rowHdr.SetMinimumHeight(ConvertPixelsToDp(30));

            TableRow.LayoutParams params3 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
            params3.SetMargins(0, 0, 0, 0);

            TextView txtHdr = new TextView(this_context);
            txtHdr.Text = "More Selections";
            txtHdr.SetWidth(ConvertPixelsToDp(250));
            txtHdr.Gravity = GravityFlags.CenterHorizontal;
            txtHdr.SetTextColor(Android.Graphics.Color.Black);
            txtHdr.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.BoldItalic);
            txtHdr.SetTextSize(Android.Util.ComplexUnitType.Pt, 10); //Don't convert this. It gets too big
            rowHdr.AddView(txtHdr);
            table.AddView(rowHdr);

            TableRow.LayoutParams params4 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
            params4.SetMargins(50, 5, 50, 5);

            TableRow row1 = new TableRow(this_context);
            row1.SetBackgroundColor(Android.Graphics.Color.Gray);
            row1.SetMinimumHeight(ConvertPixelsToDp(30));

            Button txtButtonModify = new Button(this_context);
            txtButtonModify.Text = "Copy Table Structure";
            txtButtonModify.SetWidth(ConvertPixelsToDp(100));
            txtButtonModify.Id = iId; //This is +100 from the base
            txtButtonModify.SetPadding(5, 1, 5, 1);
            txtButtonModify.LayoutParameters = params4;
            //txtButtonModify.SetTextColor(Android.Graphics.Color.Black);
            //txtButtonModify.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
            txtButtonModify.SetHeight(ConvertPixelsToDp(38));
            txtButtonModify.Click += (send, args) => { OpenCopyTable(send, args, 1); }; ;
            row1.AddView(txtButtonModify);
            table.AddView(row1);

            TableRow row2 = new TableRow(this_context);
            row2.SetBackgroundColor(Android.Graphics.Color.Gray);
            row2.SetMinimumHeight(ConvertPixelsToDp(30));

            Button txtxButtonRemove = new Button(this_context);
            txtxButtonRemove.Text = "Copy Table Structure and Data";
            txtxButtonRemove.SetWidth(ConvertPixelsToDp(100));
            txtxButtonRemove.Id = iId + 200; //This is +200 from the base
            txtxButtonRemove.SetPadding(5, 1, 5, 1);
            txtxButtonRemove.LayoutParameters = params4;
            txtxButtonRemove.SetHeight(ConvertPixelsToDp(68));
            txtxButtonRemove.Click += (send, args) => { OpenCopyTable(send, args, 2); }; ;
            row2.AddView(txtxButtonRemove);
            table.AddView(row2);

            TableRow row3 = new TableRow(this_context);
            row3.SetBackgroundColor(Android.Graphics.Color.Gray);
            row3.SetMinimumHeight(ConvertPixelsToDp(30));

            Button btnRename = new Button(this_context);
            btnRename.Text = "Rename Table";
            btnRename.SetWidth(ConvertPixelsToDp(100));
            btnRename.Id = iId + 300; //This is 300 from the base
            btnRename.SetPadding(5, 1, 5, 1);
            btnRename.LayoutParameters = params4;
            btnRename.SetHeight(ConvertPixelsToDp(38));
            btnRename.Click += (send, args) => { OpenCopyTable(send, args, 3); }; ;
            row3.AddView(btnRename);
            table.AddView(row3);

            TableRow row4 = new TableRow(this_context);
            row4.SetBackgroundColor(Android.Graphics.Color.Gray);
            row4.SetMinimumHeight(ConvertPixelsToDp(30));

            Button btnDelete = new Button(this_context);
            btnDelete.Text = "Delete Table";
            btnDelete.SetWidth(ConvertPixelsToDp(100));
            btnDelete.Id = iId + 300; //This is 300 from the base
            btnDelete.SetPadding(5, 1, 5, 1);
            btnDelete.LayoutParameters = params4;
            btnDelete.SetHeight(ConvertPixelsToDp(38));
            btnDelete.Click += (send, args) => { OpenCopyTable(send, args, 4); }; ;
            row4.AddView(btnDelete);
            table.AddView(row4);

            TableRow row5 = new TableRow(this_context);
            row5.SetBackgroundColor(Android.Graphics.Color.Gray);
            row5.SetMinimumHeight(ConvertPixelsToDp(30));

            Button btnClose = new Button(this_context);
            btnClose.Text = "Close";
            btnClose.SetWidth(ConvertPixelsToDp(100));
            btnClose.Id = iId + 300; //This is 300 from the base
            btnClose.SetPadding(5, 1, 5, 1);
            btnClose.LayoutParameters = params4;
            btnClose.SetHeight(ConvertPixelsToDp(38));
            btnClose.Click += (send, args) => { ClosePopup(send, args); }; ;
            row5.AddView(btnClose);
            table.AddView(row5);

            sv.AddView(table);
            rl.AddView(sv);
            mainView.AddView(rl, params1);

//            DisableAllButtons();

        }

        public void OpenCopyTable(object sender, EventArgs e, int iTypeOfCopy)
        {
            Dialog dlgCopy = new Dialog(this_context);
            string sTitle = "";
            string sLabelSource = "";
            string sLabelTarget = "";
            string sButtonText = "";

            switch(iTypeOfCopy)
            {
                case 1:
                    sTitle = "Copy Table Data and Structure";
                    sLabelSource = "Copy From";
                    sLabelTarget = "Copy To";
                    sButtonText = "Copy";
                    break;
                case 2:
                    sTitle = "Copy Table Structure Only";
                    sLabelSource = "Copy From";
                    sLabelTarget = "Copy To";
                    sButtonText = "Copy";
                    break;
                case 3:
                    sTitle = "Rename Table Data and Structure";
                    sLabelSource = "Rename From";
                    sLabelTarget = "Rename To";
                    sButtonText = "Rename";
                    break;
                case 4:
                    sTitle = "Delete Table Data and Structure";
                    sLabelSource = "Delete Table";
                    sLabelTarget = "";
                    sButtonText = "Delete";
                    break;
            }
            dlgCopy.SetTitle(sTitle);
            RelativeLayout.LayoutParams params1 = new RelativeLayout.LayoutParams(320, 150);
            params1.SetMargins(0, 0, 0, 0);

            //Create a table to hold all the info
            TableRow.LayoutParams params2 = new TableRow.LayoutParams();
            params2.SetMargins(ConvertPixelsToDp(5), ConvertPixelsToDp(2), ConvertPixelsToDp(5), ConvertPixelsToDp(2));
            TableLayout table = new TableLayout(this_context);
            TableRow rowSource = new TableRow(this_context);
            TextView lblSource = new TextView(this_context);
            lblSource.SetWidth(ConvertPixelsToDp(100));
            lblSource.LayoutParameters = params2;
            lblSource.Text = sLabelSource;
            rowSource.AddView(lblSource);

            Spinner cmbSource = (Spinner)FindViewById(iExistingTableNameId);
            string sSource = cmbSource.SelectedItem.ToString();

            TextView txtSource = new TextView(this_context);
            txtSource.Text = sSource;
            txtSource.Id = iSourceTableId;
            txtSource.SetWidth(ConvertPixelsToDp(200));
            lblSource.LayoutParameters = params2;
            rowSource.AddView(txtSource);
            table.AddView(rowSource);

            if (iTypeOfCopy != 4)
            {
                TableRow rowTarget = new TableRow(this_context);
                TextView lblTarget = new TextView(this_context);
                lblTarget.SetWidth(ConvertPixelsToDp(100));
                lblTarget.LayoutParameters = params2;
                lblTarget.Text = sLabelTarget;
                rowTarget.AddView(lblTarget);

                EditText txtTarget = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
                txtTarget.Text = "";
                txtTarget.Id = iTargetTableId;
                txtTarget.LayoutParameters = params2;
                rowTarget.AddView(txtTarget);
                table.AddView(rowTarget);
            }

            TableRow rowBtns = new TableRow(this_context);
            Button btnCopy = new Button(this_context);
            btnCopy.SetWidth(ConvertPixelsToDp(100));
            btnCopy.Text = sButtonText;
            btnCopy.Click += (senderCls, args) => { CopyTable(sender, args, iTypeOfCopy); };
            rowBtns.AddView(btnCopy);

            Button btnClose = new Button(this_context);
            btnClose.SetWidth(ConvertPixelsToDp(100));
            btnClose.Text = "Close";
            btnClose.Click += (senderClose, args) => dlgCopy.Dismiss();
            rowBtns.AddView(btnClose);

            table.AddView(rowBtns);

            dlgCopy.AddContentView(table, params1);
            gDialogOpen = dlgCopy;
            dlgCopy.Show();
        }

        public void CopyTable(object sender, EventArgs e, int iType)
        {
            LocalDB DB = new LocalDB();
            string sRtnMsg = "";

            //Perform the actual copy
            TextView txtSource = (TextView)FindViewById(iSourceTableId);
            string sSourceTable = txtSource.Text;
            EditText txtTarget = (EditText)FindViewById(iTargetTableId);
            string sTargetTable = txtTarget.Text;
            bool bCopy = false;
            string sSQL;

            switch(iType)
            {
                case 1:
                default:
                    bCopy = DB.CopyTableStructureAndData(sSourceTable, sTargetTable, null, null, null, true, ref sRtnMsg);
                    break;
                case 2:
                    bCopy = DB.CopyTableStructureAndData(sSourceTable, sTargetTable, null, null, null, true, ref sRtnMsg);
                    break;
                case 3:
                    sSQL = "";
                    bCopy = DB.RenameTable(sSourceTable, sTargetTable, ref sRtnMsg);
                    break;
                case 4:
                    bCopy = DB.DropTable(sSourceTable, ref sRtnMsg);
                    break;
            }

            if (!bCopy)
            {
                if (iType == 1 || iType == 2)
                {
                    alert.SetAlertMessage("Copy failure from table " + sSourceTable + " to table " + sTargetTable + ". " + sRtnMsg);
                }
                else if (iType == 3)
                {
                    alert.SetAlertMessage("Rename failure from table " + sSourceTable + " to table " + sTargetTable + ". " + sRtnMsg);
                }
                else
                {
                    alert.SetAlertMessage("Delete failure of table " + sSourceTable + ". " + sRtnMsg);
                }
                alert.ShowAlertBox();
                gDialogOpen.Dismiss();
                Button btn = (Button)FindViewById(iExistingMoreTableId + 400);
                ClosePopup(btn, e);
                return;
            }

            gDialogOpen.Dismiss();
            Button btnClose = (Button)FindViewById(  iExistingMoreTableId + 400);
            ClosePopup(btnClose, e);
            RefreshExistingTablesCombo(sTargetTable);
            return;
        }

        public void ClosePopup(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iId = btn.Id + 101 - 400; //This is the id of the relative layout
            RelativeLayout rl = (RelativeLayout)FindViewById(iId);
            mainView.RemoveView(rl);
            //EnableAllButtons();
        }

        public void CheckBoxDeleteChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            int iRow = chk.Id - iTableColumnDeleteId;
            EditText txtName = (EditText)FindViewById(iTableColumnNameId + iRow);
            Spinner cmbType = (Spinner)FindViewById(iTableColumnTypeId + iRow);
            EditText txtSize = (EditText)FindViewById(iTableColumnSizeId + iRow);
            CheckBox chkNull = (CheckBox)FindViewById(iTableColumnAllowNullId);
            string sComboValue  = "";

            if (chk.Checked)
            {
                //Disable all the other controls
                if (txtName != null)
                {
                    txtName.Enabled = false;
                    txtName.SetBackgroundColor(Android.Graphics.Color.LightGray);
                }

                if (cmbType != null)
                {
                    cmbType.Enabled = false;
                    cmbType.SetBackgroundResource(Resource.Drawable.defaultSpinnerLightGray);
                }

                if (txtSize != null)
                {
                    txtSize.Enabled = false;
                    txtSize.SetBackgroundColor(Android.Graphics.Color.LightGray);
                }

                if (chkNull != null)
                {
                    chkNull.Enabled = false;
                }

            }
            else
            {
                //Enable all the other controls
                if (txtName != null)
                {
                    txtName.Enabled = true;
                    txtName.SetBackgroundColor(Android.Graphics.Color.White);
                }

                if (cmbType != null)
                {
                    cmbType.Enabled = true;
                    cmbType.SetBackgroundResource(Resource.Drawable.defaultSpinnerWhite);
                    sComboValue = cmbType.SelectedItem.ToString();
                }

                if (txtSize != null)
                {
                    if(sComboValue == "nvarchar")
                    {
                        txtSize.Enabled = true;
                        txtSize.SetBackgroundColor(Android.Graphics.Color.White);
                    }
                    else
                    {
                        txtSize.Enabled = false;
                        txtSize.SetBackgroundColor(Android.Graphics.Color.LightGray);
                    }
                }

                if (chkNull != null)
                {
                    chkNull.Enabled = true;
                }

            }
            SetAnyValueChanged();
            return;
        }

        public void ColumnSizeTextBoxChanged(object sender, EventArgs e)
        {
            EditText txt = (EditText)sender;
            if (txt.HasFocus)
            {
                SetAnyValueChanged();
            }
        }

        public void NewTableTextBoxFocusChanged(object sender, View.KeyEventArgs e)
        {
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            clsLocalUtils utils = new clsLocalUtils();
            EditText vw = (EditText)sender;
            string sNewText = vw.Text;
            string sRtnMsg = "";

            if (!vw.HasFocus || (e.Event.Action == KeyEventActions.Down) && (e.KeyCode == Keycode.Enter))
            {
                if (sNewText.Contains(" "))
                {
                    sNewText = sNewText.Replace(" ", "_");
                    vw.Text = sNewText;
                }

                if (grdUtils.UserTableExists(sNewText, ref sRtnMsg))
                {
                    alert.SetAlertMessage("The table " + sNewText + " already exists. Please choose a new name.");
                    alert.ShowAlertBox();
                    //                    vw.RequestFocus();
                    return;
                }

                if (grdUtils.ReservedTableExists(sNewText, ref sRtnMsg))
                {
                    alert.SetAlertMessage("The table " + sNewText + " is a reserved table name. Please choose a new name.");
                    alert.ShowAlertBox();
                    //                    vw.RequestFocus();
                    return;
                }

                if (sRtnMsg != "")
                {
                    alert.SetAlertMessage(sRtnMsg);
                    alert.ShowAlertBox();
                    //                    vw.RequestFocus();
                    return;
                }

                //Now just add one blank row and the UniqueId row
                TableLayout table = (TableLayout)FindViewById(iColumnsTable);
                TableRow rowUnique = MakeTableManagerTableRow(1, "", "", "", false, true);
                table.AddView(rowUnique);
                TableRow row1 = MakeTableManagerTableRow(2, "", "", "", true, false);
                table.AddView(row1);

                if (sNewText != "[select]")
                {
                    Button btn = (Button)FindViewById(iColumnAddButtonId);
                    btn.Enabled = true;
                }

                SetAnyValueChanged();
            }
            else
            {
                e.Handled = false;
            }

            return;
        }

        public void ColumnNameTextBoxFocusChanged(object sender, EventArgs e)
        {
            EditText vw = (EditText)sender;
            int iViewId = vw.Id - iTableColumnNameId;
            string sNewText = vw.Text;

            if (!vw.HasFocus)
            {
                //Check to see that no other columns match
                if (!HaveMatchingColumn(iViewId, sNewText) || sNewText == "")
                {
                    if (sNewText == "")
                    {
                        alert.SetAlertMessage("You cannot have a blank column name");
                        alert.ShowAlertBox();
                        return;
                    }
//                    vw.RequestFocus();
                }
                SetAnyValueChanged();
            }

            return;
        }

        public bool HaveMatchingColumn(int iCallingRowId, string sCallingName)
        {
            int i;
            TableLayout table = (TableLayout)FindViewById(iColumnsTable);
            string sValue;
            CheckBox chkDel1 = (CheckBox)FindViewById(iTableColumnDeleteId + iCallingRowId);
            for (i = 1; i < table.ChildCount; i++)
            {
                EditText txt = (EditText)FindViewById(iTableColumnNameId + i);
                sValue = txt.Text;
                CheckBox chkDel2 = (CheckBox)FindViewById(iTableColumnDeleteId + i);
                if (i != iCallingRowId && sValue.ToUpper() == sCallingName.ToUpper() && !chkDel1.Checked && !chkDel2.Checked)
                {
                    alert.SetAlertMessage("You cannot have 2 columns with the same name. The first duplicate columns are row ids " + i + " and " + iCallingRowId);
                    alert.ShowAlertBox();
                    return false;
                }
            }

            return true;
        }

        public void SetAnyValueChanged()
        {
            TextView changes = FindViewById<TextView>(iUnsavedChangedDialogId);
            changes.Visibility = ViewStates.Visible;
            TextView txtEditStatus = FindViewById<TextView>(Resource.Id.hfEditStatus);
            txtEditStatus.Text = "1";
            EnableMoreButton(false);
        }

        public void SetAnyValueChangedOff()
        {
            TextView changes = FindViewById<TextView>(iUnsavedChangedDialogId);
            changes.Visibility = ViewStates.Invisible;
            TextView txtEditStatus = FindViewById<TextView>(Resource.Id.hfEditStatus);
            txtEditStatus.Text = "0";
            EnableMoreButton(true);
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
    }
}
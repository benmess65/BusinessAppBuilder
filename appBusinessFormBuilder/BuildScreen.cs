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
    //Simple constants for the main sections
    enum SectionType { Form = 1, Header, Detail, Footer, HeaderRow, HeaderColumn, DetailRow, DetailColumn, FooterRow, FooterColumn, GridItem };
    enum ItemType { Label = 1, TextBox, TextArea, DropDown, Checkbox, RadioButton, Button, DatePicker, TimePicker, ProgressBar, ColumnHeader, RowHeader, ColumnDetail, RowDetail, ColumnFooter, RowFooter, SQLColumnDropdown = 400 };
    enum VersionType { Free = 0, Base, Pro, Premium };

    [Activity(Label = "BuildScreen")]
    public class BuildScreen : Activity //, View.IOnTouchListener
    {
        //Globals for use through out this activity
        Context this_context;
        RelativeLayout mainView;
        HorizontalScrollView mainHSV;
        ScrollView mainSV;
        LinearLayout llMain;
        int giDialogOpen = 0;
        bool gbCloseDialog = true;
        AndroidUtils.AlertBox alert = new AndroidUtils.AlertBox();
        int giFormId = -1;
        int giBuild = -1;
        ArrayList garrDBRecords;
        ArrayList garrDBColumns;
        ArrayList garrDBValues;
        int giTotalRecords = 0;
        int giRecordsPerPage = 0;
        int giDetailRows = 0;
        int giDetailColumns = 0;
        int giTopLeftGridItemWidth = -1;
        int giDefaultColWidth = 150;
        int giSectionButtonWidth = 50;
        int giNavBarsWidth = 0;
        EditText gtxtFocused;

        //Tasks
        Task taskA;
        AndroidUtils.ProgressBar progBarDetail = new AndroidUtils.ProgressBar();
//        ProgressDialog prog;

        //Constants for widths and heights
        int iDetailDialogLabelWidth = 150;
        int iDetailDialogItemWidth = 200;
        int iDetailDialogHeight = 200;
        int iOpenTestButtonWidth = 30;

        //Ids for various parts of the screen
        int iMainOutermostTableId = 10; //This is the very outermost table.
        int iFormDetailButtonId = 100;
        int iFormLabelId = 110;

        int iHeaderDetailButtonId = 200;
        int iHeaderLabelId = 210;

        int iDetailDetailButtonId = 300;
        int iDetailLabelId = 310;

        int iFooterDetailButtonId = 400;
        int iFooterLabelId = 410;

        int iUnsavedChangedDialogId = 1010;
        int iDetailSectionPageNavigationRowId = 2010;
        int iDetailSectionRecordNavigationRowId = 2011;

        //Navigation Bar Ids
        int iNavPageFirstPageButtonId = 3001;
        int iNavPagePrevPageButtonId = 3002;
        int iNavPageLabelId = 3003;
        int iNavPageNoEditId = 3005;
        int iNavPageLabelTotalPagesId = 3006;
        int iNavPageGoToPageButtonId = 3007;
        int iNavPageNextPageButtonId = 3008;
        int iNavPageLastPageButtonId = 3009;

        int iNavRecordFirstRecordButtonId = 4001;
        int iNavRecordPrevRecordButtonId = 4002;
        int iNavRecordLabelId = 4003;
        int iNavRecordLabelHiddenId = 4004;
        int iNavRecordNoEditId = 4005;
        int iNavRecordLabelTotalRecordsId = 4006;
        int iNavRecordGoToRecordButtonId = 4007;
        int iNavRecordNextRecordButtonId = 4008;
        int iNavRecordLastRecordButtonId = 4009;

        int iFormSectionId = 1000100; //Allow 100 possible items in the settings for the header, detail and footer sections
        int iHeaderSectionId = 1000300; //Allow 100 possible items in the settings for the header, detail and footer sections
        int iHeaderSectionContainerId = 1000400; //This is the ID for a container to hold the grid
        int iDetailSectionId = 1000500; //Allow 100 possible items in the settings for the header, detail and footer sections
        int iDetailSectionContainerId = 1000600; //This is the ID for a container to hold the grid
        int iDetailSectionNavBarContainerId = 1000700; //This is the ID for a container to hold the page nav bar
        int iDetailSectionNavBarContainerId2 = 1000701; //This is the ID for a container to hold the recordnav bar
        int iFooterSectionId = 1000800; //Allow 100 possible items in the settings for the header, detail and footer sections
        int iFooterSectionContainerId = 1000900; //This is the ID for a container to hold the grid

        int iHeaderRowSectionId = 1001000; //Allow 100 possible items in the settings for the header row
        int iHeaderColumnSectionId = 1001100; //Allow 100 possible items in the settings for the header column
        int iDetailRowSectionId = 1001200; //Allow 100 possible items in the settings for the detail row
        int iDetailColumnSectionId = 1001300; //Allow 100 possible items in the settings for the detail column
        int iFooterRowSectionId = 1001400; //Allow 100 possible items in the settings for the footer row
        int iFooterColumnSectionId = 1001500; //Allow 100 possible items in the settings for the footer column


        //Each row add 100,000 and each column add 1,000 and each record add 1, using a base of 1 (NOT zero). The zero base is the table or the row or the column, with columns being a dummy row 100.
        //For example row 6 column 8 record 5 in section detail would be an Id of 20608005.
        //The table would be 20000000 for the detail section
        //The 6th row would be 20600000 in the detail section
        //The 8th column would be 20608000 in the detail section
        //The 5th record would be 20608005 in the detail section
        //That is the cell id. The control in the cell would be 20608105 (+100 for say the drop down box or textbox or radio button group)
        //The hidden field holding the old value before saving would be 20608805 (+800)
        //The button for adding the details would be 20608905 (+900) but of course this is only used in the build version of this screen
        int iHeaderSectionTableId = 20000000; //Allow 99 possible columns and 99 possible rows per section and 99 records. There are up to 10 items in each cell though. The cell, the underlying control and the button for the details/parameters
        int iDetailSectionTableId = 30000000; //Allow 99 possible columns and 99 possible rows per section. There are 3 items in each cell though. The cell, the underlying control and the button for the details/parameters
        int iFooterSectionTableId = 40000000; //Allow 99 possible columns and 99 possible rows per section. There are 3 items in each cell though. The cell, the underlying control and the button for the details/parameters

        int iGridItemDetailDialogId = 13100000; //This is for the detail dialog popup for a grid item

        int iHeaderRowBaseId = 14100000; //Allow 99 rows but because the row Id is manufactured in the same way as a grid item then we have to allow for 100000 id's
        int iHeaderColumnBaseId = 15100000; //Allow 99 columns but because the row Id is manufactured in the same way as a grid item then we have to allow for 100000 id's
        int iDetailRowBaseId = 16100000; //Allow 99 rows but because the row Id is manufactured in the same way as a grid item then we have to allow for 100000 id's
        int iDetailColumnBaseId = 17100000; //Allow 99 columns but because the row Id is manufactured in the same way as a grid item then we have to allow for 100000 id's
        int iFooterRowBaseId = 18100000; //Allow 99 rows but because the row Id is manufactured in the same way as a grid item then we have to allow for 100000 id's
        int iFooterColumnBaseId = 19100000; //Allow 99 columns but because the row Id is manufactured in the same way as a grid item then we have to allow for 100000 id's

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);


            giFormId = Intent.GetIntExtra("FormId", -1);
            giBuild = Intent.GetIntExtra("BuildNew", -1);
            giTopLeftGridItemWidth = ConvertPixelsToDp(120);

            this_context = this;
            //mainView = new AndroidUtils.ScaleImageView(this);
            //SetContentView(mainView);

            SetContentView(Resource.Layout.layoutEmpty);
            mainView = (RelativeLayout)FindViewById(Resource.Id.EmptyLayout);
            
            //            FrameLayout v = new AndroidUtils.ZoomView(this);
//            SetContentView(v);

            HorizontalScrollView hsv = new HorizontalScrollView(this);
            mainView.AddView(hsv);
            mainHSV = hsv;
            ScrollView availdownloads = new ScrollView(this);
            availdownloads = DrawOpeningPage(this);
            if (availdownloads != null)
            {
                hsv.AddView(availdownloads);
                mainSV = availdownloads;
                //progBarDetail.SetContext(this_context);
                //progBarDetail.CreateProgressBar();
                //taskA = new Task(() => PopulateTables());
                //taskA.Start();

                PopulateTables();
            }

            //// Create your application here
            //View vw = new GestureRecognizerView(this);
            //SetContentView(vw);

            //Android.Util.IAttributeSet attrib;

            //ImageView testImg = new AndroidUtils.ScaleImageView(this, null);
            //testImg.SetImageResource(Resource.Drawable.Icon);
            //llMain.AddView(testImg);

        }

        public ScrollView DrawOpeningPage(Android.Content.Context context)
        {
            try
            {
                clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
                ScrollView sv = new ScrollView(context);
                LinearLayout layout = new LinearLayout(context);
                llMain = layout;
                int iWidthPixels = GetScreenWidthPixels();
                int iHeightPixels = GetScreenHeightPixels();
                int iButtonWidth = giSectionButtonWidth;
                Android.App.ActionBar navBar = this.ActionBar;
                string sFormName;

                layout.SetGravity(GravityFlags.CenterHorizontal);
                layout.Id = 1;
                int iPaddingMargin1 = ConvertPixelsToDp(5);
                int iPaddingMargin2 = ConvertPixelsToDp(1);

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
                
                //The top FORM row
                    TableRow rowForm = new TableRow(context);
                    rowForm.SetBackgroundColor(Android.Graphics.Color.DarkSlateGray);
                    rowForm.SetMinimumHeight(ConvertPixelsToDp(30));


                    if (giBuild == 1)
                    {
                        Button btnForm = new Button(context);
                        btnForm.SetBackgroundResource(Resource.Drawable.DetailButton);
                        btnForm.SetWidth(ConvertPixelsToDp(iButtonWidth));
                        btnForm.SetHeight(ConvertPixelsToDp(30));
                        btnForm.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.Form); };

                        rowForm.AddView(btnForm);
                    }
                    else
                    {
                        iButtonWidth = 0;
                    }

                    ArrayList arrFormInfo = grdUtils.GetFormDetails(giFormId);
                    sFormName = arrFormInfo[1].ToString();

                    navBar.Title = sFormName;
                    sFormName = arrFormInfo[1].ToString() + " - " + arrFormInfo[2].ToString();
                    TextView txtForm = new TextView(context);
                    txtForm.SetPadding(ConvertPixelsToDp(5), ConvertPixelsToDp(2), ConvertPixelsToDp(5), ConvertPixelsToDp(2));
                    txtForm.Text = sFormName;
                    txtForm.Id = iFormLabelId;
                    if (giBuild == 0)
                    {
                        iOpenTestButtonWidth = 0;
                    }
                    txtForm.SetWidth(ConvertPixelsToDp(iWidthPixels - iButtonWidth - iOpenTestButtonWidth));
                    txtForm.SetTextColor(Android.Graphics.Color.White);
                    rowForm.AddView(txtForm);

                    if (giBuild == 1)
                    {
                        Button btnLaunchOpen = new Button(context);
                        btnLaunchOpen.Text = "Test";
                        btnLaunchOpen.SetWidth(ConvertPixelsToDp(iOpenTestButtonWidth));
                        btnLaunchOpen.SetHeight(ConvertPixelsToDp(30));
                        btnLaunchOpen.Click += (sender, args) => { OpenTestPage(sender, args); };

                        rowForm.AddView(btnLaunchOpen);
                    }

                    table.AddView(rowForm);

                /*************************************************************/
                /*                      HEADER SECTION                       */
                /*************************************************************/
                if (giBuild == 1)
                {
                    TableRow rowHdr = new TableRow(context);
                    rowHdr.SetBackgroundColor(Android.Graphics.Color.DarkSlateGray);
                    rowHdr.SetMinimumHeight(ConvertPixelsToDp(30));


                    Button btnHdr = new Button(context);
                    btnHdr.SetBackgroundResource(Resource.Drawable.DetailButton);
                    btnHdr.SetWidth(ConvertPixelsToDp(30));
                    btnHdr.SetHeight(ConvertPixelsToDp(30));
                    btnHdr.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.Header); }; ;

                    rowHdr.AddView(btnHdr);

                    TextView txtHdr = new TextView(context);
                    txtHdr.Text = "Header";
                    txtHdr.SetPadding(ConvertPixelsToDp(5), ConvertPixelsToDp(2), ConvertPixelsToDp(5), ConvertPixelsToDp(2));
                    txtHdr.SetWidth(ConvertPixelsToDp(iWidthPixels - iButtonWidth));
                    txtHdr.SetTextColor(Android.Graphics.Color.White);
                    txtHdr.Id = iHeaderLabelId;
                    rowHdr.AddView(txtHdr, params3);

                    table.AddView(rowHdr);
                }
                TableRow rowContainerHdr = new TableRow(context);
                rowContainerHdr.Id = iHeaderSectionContainerId;
                //                rowContainer.SetBackgroundColor(Android.Graphics.Color.DarkSlateGray);
                rowContainerHdr.SetMinimumHeight(ConvertPixelsToDp(30));
                table.AddView(rowContainerHdr);

                /*************************************************************/
                /*                      DETAIL SECTION                       */
                /*************************************************************/
                if (giBuild == 1)
                {
                    TableRow rowDetail = new TableRow(context);
                    rowDetail.SetBackgroundColor(Android.Graphics.Color.DarkSlateGray);
                    rowDetail.SetMinimumHeight(ConvertPixelsToDp(30));


                    Button btnDetail = new Button(context);
                    btnDetail.SetBackgroundResource(Resource.Drawable.DetailButton);
                    btnDetail.SetWidth(ConvertPixelsToDp(30));
                    btnDetail.SetHeight(ConvertPixelsToDp(30));
                    btnDetail.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.Detail); }; ;

                    rowDetail.AddView(btnDetail);

                    TextView txtDetail = new TextView(context);
                    txtDetail.Text = "Detail";
                    txtDetail.SetPadding(ConvertPixelsToDp(5), ConvertPixelsToDp(2), ConvertPixelsToDp(5), ConvertPixelsToDp(2));
                    txtDetail.SetWidth(ConvertPixelsToDp(iWidthPixels - iButtonWidth));
                    txtDetail.SetTextColor(Android.Graphics.Color.White);
                    txtDetail.Id = iDetailLabelId;
                    rowDetail.AddView(txtDetail, params3);

                    table.AddView(rowDetail);
                }

                TableRow rowContainerDetail = new TableRow(context);
                rowContainerDetail.Id = iDetailSectionContainerId;
//                rowContainer.SetBackgroundColor(Android.Graphics.Color.DarkSlateGray);
                rowContainerDetail.SetMinimumHeight(ConvertPixelsToDp(30));
                table.AddView(rowContainerDetail);

                TableRow rowNavBarDetail = new TableRow(context);
                rowNavBarDetail.Id = iDetailSectionNavBarContainerId;
                rowNavBarDetail.SetMinimumHeight(ConvertPixelsToDp(30));
                table.AddView(rowNavBarDetail);

                TableRow rowNavBarDetail2 = new TableRow(context);
                rowNavBarDetail2.Id = iDetailSectionNavBarContainerId2;
                rowNavBarDetail2.SetMinimumHeight(ConvertPixelsToDp(30));
                table.AddView(rowNavBarDetail2);

                /*************************************************************/
                /*                      FOOTER SECTION                       */
                /*************************************************************/
                if (giBuild == 1)
                {
                    TableRow rowFooter = new TableRow(context);
                    rowFooter.SetBackgroundColor(Android.Graphics.Color.DarkSlateGray);
                    rowFooter.SetMinimumHeight(ConvertPixelsToDp(30));


                    Button btnFooter = new Button(context);
                    btnFooter.SetBackgroundResource(Resource.Drawable.DetailButton);
                    btnFooter.SetWidth(ConvertPixelsToDp(30));
                    btnFooter.SetHeight(ConvertPixelsToDp(30));
                    btnFooter.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.Footer); }; ;

                    rowFooter.AddView(btnFooter);

                    TextView txtFooter = new TextView(context);
                    txtFooter.Text = "Footer";
                    txtFooter.SetPadding(ConvertPixelsToDp(5), ConvertPixelsToDp(2), ConvertPixelsToDp(5), ConvertPixelsToDp(2));
                    txtFooter.SetWidth(ConvertPixelsToDp(iWidthPixels - iButtonWidth));
                    txtFooter.SetTextColor(Android.Graphics.Color.White);
                    txtFooter.Id = iFooterLabelId;
                    rowFooter.AddView(txtFooter);

                    table.AddView(rowFooter);
                }
                TableRow rowContainerFooter = new TableRow(context);
                rowContainerFooter.Id = iFooterSectionContainerId;
                //                rowContainer.SetBackgroundColor(Android.Graphics.Color.DarkSlateGray);
                rowContainerFooter.SetMinimumHeight(ConvertPixelsToDp(30));
                table.AddView(rowContainerFooter);

                layout.AddView(table);

//                sv.SetPadding(0, 0, 0, 200);
                sv.AddView(layout);
                return sv;
            }
            catch (Exception except)
            {
                Toast.MakeText(context, except.Message.ToString(), Android.Widget.ToastLength.Long);
                return null;
            }
        }

        private void PopulateTables()
        {
            LocalDB DB = new LocalDB();
            string sRtnMsg = "";
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();

            //Get the array of values from the DB for the SQL in the form dialog
            string sFormSQL = grdUtils.GetItemAttribute(giFormId, (int)SectionType.Form, -1, "FormSQL", ref sRtnMsg);
            garrDBRecords = DB.ReadSQLArray(sFormSQL, ref sRtnMsg);

            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
            }
            else
            {
                if (garrDBRecords.Count == 2)
                {
                    garrDBColumns = (ArrayList)garrDBRecords[0];
                    garrDBValues = (ArrayList)garrDBRecords[1];
                }
            }

                //If some rows and columns exist in the DB then populate the tables for each section
            //this.RunOnUiThread(() =>
            //{
                InsertTable((int)SectionType.Header, 0, 0);
                InsertTable((int)SectionType.Detail, 1, 1);
                FocusFirstEditItemInRecord(1, true);
                InsertTable((int)SectionType.Footer, 0, 0);
                SetHorizontalScrollWidth();
            //            });


        }

        private void OpenDetailDialog(object sender, EventArgs e, int iType)
        {
            int iId = 0;
            int iLabelWidth = iDetailDialogLabelWidth;
            int iItemWidth = iDetailDialogItemWidth;
            ArrayList arrDialogItems = new ArrayList();
            List<String> arrString = new List<string>();
            List<int> arrTypeOfControl = new List<int>();
            Android.App.ActionBar navBar = this.ActionBar;
            int iTopBarHeight = navBar.Height;
            clsTabletDB.GridUtils gridUtils = new clsTabletDB.GridUtils();
            int iItemId = -1;
            View vwSender = (View)sender;
            int iInnerViewType = -1;
            ScrollView sv = new ScrollView(this_context);
            sv.Id = 20;
            string sHeaderText = "";
            int iRow = -1;
            int iCol = -1;
            string sRtnMsg = "";
            int iCellId = -1;
            int iSectionId = -1;

            //First check that another dialog is not open
            if (giDialogOpen == 1)
            {
                alert.SetAlertMessage("You already have a dialog open. You can only have 1 detailed dialog open at once. Please save or cancel it before proceeding.");
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            if (giFormId < 0 && iType != (int)SectionType.Form)
            {
                alert.SetAlertMessage("You cannot set up anything until the form details have been created. Please select the form details button and complete the mandatory fields.");
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;

            }

            switch (iType)
            {
                case (int)SectionType.Form:
                    iId = iFormSectionId;
                    sHeaderText = "Form Settings";
                    iSectionId = iType;
                    break;
                case (int)SectionType.Header:
                    iId = iHeaderSectionId;
                    sHeaderText = "Header Settings";
                    iSectionId = iType;
                    break;
                case (int)SectionType.Detail:
                    iId = iDetailSectionId;
                    sHeaderText = "Detail Settings";
                    iSectionId = iType;
                    break;
                case (int)SectionType.Footer:
                    iId = iFooterSectionId;
                    sHeaderText = "Footer Settings";
                    iSectionId = iType;
                    break;
                case (int)SectionType.HeaderRow:
                    iId = iHeaderRowSectionId;
                    iCellId = vwSender.Id - 900;
                    iSectionId = iType;
                    iRow = GetRowNoFromCellId(iCellId, iSectionId, 1);
                    iCol = GetColumnNoFromCellId(iCellId, iSectionId, 1);
                    sHeaderText = "Row R" + iRow + " Settings";
                    break;
                case (int)SectionType.HeaderColumn:
                    iId = iHeaderColumnSectionId;
                    iSectionId = iType;
                    iCellId = vwSender.Id - 900;
                    iSectionId = iType;
                    iRow = GetRowNoFromCellId(iCellId, iSectionId, 1);
                    iCol = GetColumnNoFromCellId(iCellId, iSectionId, 1);
                    sHeaderText = "Column C" + iCol + " Settings";
                    break;
                case (int)SectionType.DetailRow:
                    iId = iDetailRowSectionId;
                    iCellId = vwSender.Id - 900;
                    iSectionId = iType;
                    iRow = GetRowNoFromCellId(iCellId, iSectionId, 1);
                    iCol = GetColumnNoFromCellId(iCellId, iSectionId, 1);
                    sHeaderText = "Row R" + iRow + " Settings";
                    break;
                case (int)SectionType.DetailColumn:
                    iId = iDetailColumnSectionId;
                    iSectionId = iType;
                    iCellId = vwSender.Id - 900;
                    iSectionId = iType;
                    iRow = GetRowNoFromCellId(iCellId, iSectionId, 1);
                    iCol = GetColumnNoFromCellId(iCellId, iSectionId, 1);
                    sHeaderText = "Column C" + iCol + " Settings";
                    break;
                case (int)SectionType.FooterRow:
                    iId = iFooterRowSectionId;
                    iCellId = vwSender.Id - 900;
                    iSectionId = iType;
                    iRow = GetRowNoFromCellId(iCellId, iSectionId, 1);
                    iCol = GetColumnNoFromCellId(iCellId, iSectionId, 1);
                    sHeaderText = "Row R" + iRow + " Settings";
                    break;
                case (int)SectionType.FooterColumn:
                    iId = iFooterColumnSectionId;
                    iSectionId = iType;
                    iCellId = vwSender.Id - 900;
                    iSectionId = iType;
                    iRow = GetRowNoFromCellId(iCellId, iSectionId, 1);
                    iCol = GetColumnNoFromCellId(iCellId, iSectionId, 1);
                    sHeaderText = "Column C" + iCol + " Settings";
                    break;
                case (int)SectionType.GridItem:
                    iId = iGridItemDetailDialogId;
                    //Get the actual cell
                    iCellId = vwSender.Id - 900;
                    View vwCell = (View)FindViewById(iCellId);
                    Java.Lang.Object tag = vwCell.GetTag(Resource.Integer.CellType);
                    iInnerViewType = Convert.ToInt32(tag);
                    Java.Lang.Object tag2 = vwCell.GetTag(Resource.Integer.CellSectionId);
                    iSectionId = Convert.ToInt32(tag2);
                    iRow = GetRowNoFromCellId(iCellId, iSectionId, 1);
                    iCol = GetColumnNoFromCellId(iCellId, iSectionId, 1);
                    string sSection = "";
                    switch (iSectionId)
                    {
                        case (int)SectionType.Header:
                            sSection = "Header ";
                            break;
                        case (int)SectionType.Detail:
                            sSection = "Detail ";
                            break;
                        case (int)SectionType.Footer:
                            sSection = "Footer ";
                            break;
                    }
                    sHeaderText = sSection + "Item R" + iRow + "C" + iCol + " Settings";
                    break;
            }

            if (iType == (int)SectionType.HeaderRow)
            {
                iItemId = gridUtils.GetGridItemId(giFormId,iType, iCellId,ref sRtnMsg);
            }

            if (iType == (int)SectionType.GridItem)
            {
                iItemId = iInnerViewType;
            }

            //Get the position of the button
            Button btn = (Button)sender;
            int[] iPosn = new int[2];
            btn.GetLocationOnScreen(iPosn);
            int iLeft = (iPosn[0]) + (btn.Width); // btn.Left + btn.Width;
            int iTop = (iPosn[1]) + (btn.Height) - iTopBarHeight; // btn.Top + btn.Height;
            if (iLeft + ConvertPixelsToDp(iDetailDialogLabelWidth + iDetailDialogItemWidth) > GetScreenWidthPixels())
            {
                iLeft = iLeft - ConvertPixelsToDp(iDetailDialogLabelWidth + iDetailDialogItemWidth);
            }

            if (iTop + ConvertPixelsToDp(iDetailDialogHeight) > GetScreenHeightPixels() / 2) //So more than half way down we need to put it up because of the soft keyboard
            {
                iTop = iTopBarHeight;
            }


            //Create a new RelativeLayout
            RelativeLayout rl = new RelativeLayout(this_context);
            rl.Id = iId;
            RelativeLayout.LayoutParams params1 = new RelativeLayout.LayoutParams(ConvertPixelsToDp(iDetailDialogLabelWidth + iDetailDialogItemWidth), ConvertPixelsToDp(iDetailDialogHeight));
            params1.SetMargins(iLeft, iTop, 0, 0);
            int iPaddingMargin1 = ConvertPixelsToDp(5);
            int iPaddingMargin2 = ConvertPixelsToDp(1);

            //This simply sets spacing between each of the elements in the row
            TableRow.LayoutParams params2 = new TableRow.LayoutParams();
            params2.SetMargins(iPaddingMargin1, iPaddingMargin2, iPaddingMargin1, iPaddingMargin2);

            TableLayout table = new TableLayout(this_context);
            table.SetGravity(GravityFlags.CenterHorizontal);
            table.SetBackgroundColor(Android.Graphics.Color.Gray);
            table.Id = iId - 2;

            //Put in a header so the user knows whar button they have selected
            TableRow rowHdr0 = new TableRow(this_context);
            rowHdr0.SetBackgroundColor(Android.Graphics.Color.Gray);
            rowHdr0.SetMinimumHeight(ConvertPixelsToDp(30));

            TableRow.LayoutParams params3 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
            params3.SetMargins(0, 0, 0, 0);
            params3.Span = 2;

            //Now put in the unsaved changes text view
            TextView txtChanges = new TextView(this_context);
            txtChanges.Text = "UNSAVED CHANGES";
            txtChanges.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Bold);
            txtChanges.SetWidth(ConvertPixelsToDp(iItemWidth));
            txtChanges.SetTextColor(Android.Graphics.Color.Black);
            txtChanges.SetBackgroundColor(Android.Graphics.Color.Rgb(255, 174, 255));
            txtChanges.Gravity = GravityFlags.Center;
            txtChanges.SetPadding(5, 5, 5, 5);
            txtChanges.Id = iUnsavedChangedDialogId;
            txtChanges.Visibility = ViewStates.Invisible;
            rowHdr0.AddView(txtChanges,params3);

            table.AddView(rowHdr0);

            //Put in a header so the user knows whar button they have selected
            TableRow rowHdr = new TableRow(this_context);
            rowHdr.SetBackgroundColor(Android.Graphics.Color.Gray);
            rowHdr.SetMinimumHeight(ConvertPixelsToDp(30));


            TextView txtHdr = new TextView(this_context);
            txtHdr.Text = sHeaderText;
            txtHdr.SetWidth(ConvertPixelsToDp(iLabelWidth));
            txtHdr.SetTextColor(Android.Graphics.Color.Black);
            txtHdr.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.BoldItalic);
            txtHdr.SetTextSize(Android.Util.ComplexUnitType.Pt, 12); //Don't convert this. It gets too big
            rowHdr.AddView(txtHdr,params3);

            table.AddView(rowHdr);

            //If we have a grid item we have to put in the top row which is a dropdown of the types
            if (iType == (int)SectionType.GridItem)
            {
                TableRow row0 = new TableRow(this_context);
                row0.SetBackgroundColor(Android.Graphics.Color.Gray);
                //row1.SetMinimumHeight(ConvertPixelsToDp(100));

                TextView txt0 = new TextView(this_context);
                txt0.Text = "Item Type";
                txt0.SetWidth(ConvertPixelsToDp(iLabelWidth));
                txt0.SetTextColor(Android.Graphics.Color.Black);
                row0.AddView(txt0);

                AndroidUtils.ComboBox cmbBox0 = new AndroidUtils.ComboBox();
                ArrayAdapter arrCmbItems0 = new ArrayAdapter(this_context, Resource.Layout.layoutSpinner); //This is the resource for the main box
                //Now get the info from the database for the drop down items
                int iSelectedIndex = cmbBox0.PopulateAdapter(ref arrCmbItems0, "Select Description from tblItemType", gridUtils.sItemTypeDescriptions[iInnerViewType - 1], false, ref sRtnMsg);
                arrCmbItems0.SetDropDownViewResource(Resource.Layout.layoutSpinnerBase); //This is the resource for the drop down
                Spinner cmbEdit0 = new Spinner(this_context);
                cmbEdit0.Adapter = arrCmbItems0;
                cmbEdit0.Id = iId - 1;
                cmbEdit0.SetPadding(10, 1, 10, 1);
                cmbEdit0.LayoutParameters = params2;

                ViewGroup.LayoutParams lp = cmbEdit0.LayoutParameters;
                lp.Width = ConvertPixelsToDp(iItemWidth - 2 * iPaddingMargin1);
                lp.Height = ConvertPixelsToDp(28);
                cmbEdit0.LayoutParameters = lp;
                cmbEdit0.SetBackgroundResource(Resource.Drawable.defaultSpinner2);

                cmbEdit0.SetSelection(iSelectedIndex);
                cmbEdit0.SetTag(Resource.Integer.CellType, (int)ItemType.DropDown);
                cmbEdit0.SetTag(Resource.Integer.ParameterId, -99);
                cmbEdit0.ItemSelected += (senderItem, args) => { GridItemTypeChanged(senderItem, args, iId); };
                row0.AddView(cmbEdit0);

                TextView txt98 = new TextView(this_context);
                txt98.Text = iSelectedIndex.ToString();
                txt98.Id = iId - 4;
                txt98.Visibility = ViewStates.Gone;
                row0.AddView(txt98);

                table.AddView(row0);

            }


            TableLayout table2 = MakeDetailDialogMainTable(iId, iType, iItemId, iCellId, iSectionId, false);
            TableRow rowMainDialog = new TableRow(this_context);
            table2.SetTag(Resource.Integer.CellSectionId, iSectionId);
            table2.SetTag(Resource.Integer.CellRowId, iRow);
            table2.SetTag(Resource.Integer.CellColumnId, iCol);
            table2.SetTag(Resource.Integer.CellId, iCellId);

            if (iType == (int)SectionType.GridItem)
            {
                rowMainDialog.AddView(table2,params3);
            }
            else
            {
                rowMainDialog.AddView(table2);
            }
            rowMainDialog.Id = iId - 3;
            table.AddView(rowMainDialog);
            sv.AddView(table);
            rl.AddView(sv);
            mainView.AddView(rl, params1);

            //Now also disable the use of any other dialogs so that no other buttons are pressed whilst the dialog is open
            giDialogOpen = 1;
        }

        private TableLayout MakeDetailDialogMainTable(int iId, int iType, int iItemId, int iCellId, int iSectionId, bool bTypeChanged)
        {
            int i = 0;
            int iLabelWidth = iDetailDialogLabelWidth;
            int iItemWidth = iDetailDialogItemWidth;
            ArrayList arrDialogItems = new ArrayList();
            clsTabletDB.GridUtils gridUtils = new clsTabletDB.GridUtils();
            LocalDB DB = new LocalDB();
            List<String> arrString = new List<string>();
            List<int> arrTypeOfControl = new List<int>();
            string sRtnMsg = "";
            int iRow = GetRowNoFromCellId(iCellId, iSectionId, 1);
            int iCol = GetColumnNoFromCellId(iCellId, iSectionId, 1);

            int iPaddingMargin1 = ConvertPixelsToDp(5);
            int iPaddingMargin2 = ConvertPixelsToDp(1);

            //This simply sets spacing between each of the elements in the row
            TableRow.LayoutParams params2 = new TableRow.LayoutParams();
            params2.SetMargins(iPaddingMargin1, iPaddingMargin2, iPaddingMargin1, iPaddingMargin2);

            TableLayout table = new TableLayout(this_context);
            table.SetGravity(GravityFlags.CenterHorizontal);
            table.SetBackgroundColor(Android.Graphics.Color.Gray);
            table.Id = iId + 1;

            arrDialogItems = gridUtils.GetDetailDialogItems(giFormId, iType, iItemId, iCellId, iSectionId);
            if (arrDialogItems[0].ToString() == "Failure")
            {
                alert.SetAlertMessage(arrDialogItems[1].ToString());
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return null;
            }

            ArrayList arrId = (ArrayList)arrDialogItems[0];
            ArrayList arrNames = (ArrayList)arrDialogItems[1];
            ArrayList arrDesc = (ArrayList)arrDialogItems[2];
            ArrayList arrTypes = (ArrayList)arrDialogItems[3];
            ArrayList arrValues = (ArrayList)arrDialogItems[4];
            ArrayList arrSQL = (ArrayList)arrDialogItems[5];
            ArrayList arrOnBlur = (ArrayList)arrDialogItems[6];

            for (i = 0; i < arrDesc.Count; i++)
            {
                arrString.Add(arrDesc[i].ToString());
                arrTypeOfControl.Add(Convert.ToInt32(arrTypes[i]));
            }

            for (i = 0; i < arrString.Count; i++)
            {
                TableRow row1 = new TableRow(this_context);
                row1.SetBackgroundColor(Android.Graphics.Color.Gray);
                //row1.SetMinimumHeight(ConvertPixelsToDp(100));

                TextView txt1 = new TextView(this_context);
                txt1.Text = arrString[i];
                txt1.SetWidth(ConvertPixelsToDp(iLabelWidth));
                txt1.SetTextColor(Android.Graphics.Color.Black);
                row1.AddView(txt1);

                TextView txtOld = new TextView(this_context);
                txtOld.Text = arrValues[i].ToString();
                txtOld.Visibility = ViewStates.Gone;
                txtOld.Id = iId + i + 102;
                row1.AddView(txtOld);
                table.AddView(row1);

                switch (arrTypeOfControl[i])
                {
                    case (int)ItemType.Label:
                        TextView lblEdit1 = new TextView(this_context);
                        lblEdit1.Text = arrValues[i].ToString();
                        lblEdit1.SetTextColor(Android.Graphics.Color.Black);
                        lblEdit1.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Normal);
                        lblEdit1.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                        lblEdit1.SetWidth(ConvertPixelsToDp(iItemWidth - 2 * iPaddingMargin1));
                        lblEdit1.Id = iId + i + 2;
                        lblEdit1.SetPadding(10, 1, 10, 1);
                        lblEdit1.LayoutParameters = params2;
                        lblEdit1.SetHeight(ConvertPixelsToDp(34));
                        lblEdit1.SetTag(Resource.Integer.CellType, arrTypeOfControl[i]);
                        lblEdit1.SetTag(Resource.Integer.ParameterId, Convert.ToInt32(arrId[i]));
                        lblEdit1.SetTag(Resource.String.OnBlurMethodName, arrOnBlur[i].ToString());
                        row1.AddView(lblEdit1);
                        //row1.SetMinimumHeight(ConvertPixelsToDp(30));
                        break;
                    case (int)ItemType.TextBox:
                        EditText txtEdit1 = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
                        txtEdit1.Text = arrValues[i].ToString();
                        txtEdit1.SetWidth(ConvertPixelsToDp(iItemWidth - 2 * iPaddingMargin1));
                        txtEdit1.Id = iId + i + 2;
                        txtEdit1.SetPadding(10, 1, 10, 1);
                        txtEdit1.LayoutParameters = params2;
                        txtEdit1.SetHeight(ConvertPixelsToDp(28));
                        txtEdit1.SetSingleLine(true);

                        //if (arrOnBlur[i].ToString() != "")
                        //{
                        //    string sMethod = arrOnBlur[i].ToString();
                        //    txtEdit1.FocusChange += (senderText, args) => { DialogTextOnBlur(senderText, args, sMethod); }; 
                        //}
                        txtEdit1.SetTag(Resource.Integer.CellType, arrTypeOfControl[i]);
                        txtEdit1.SetTag(Resource.Integer.ParameterId, Convert.ToInt32(arrId[i]));
                        txtEdit1.SetTag(Resource.Integer.CellRowId, iRow);
                        txtEdit1.SetTag(Resource.Integer.CellColumnId, iCol);
                        txtEdit1.SetTag(Resource.Integer.CellId, iCellId);
                        txtEdit1.SetTag(Resource.Integer.CellSectionId, iSectionId);
                        txtEdit1.SetTag(Resource.Integer.FormId, giFormId);
                        string sMethod = arrOnBlur[i].ToString();
                        txtEdit1.SetTag(Resource.String.OnBlurMethodName, sMethod);
                        row1.AddView(txtEdit1);
                        //row1.SetMinimumHeight(ConvertPixelsToDp(30));

                        break;

                    case (int)ItemType.TextArea:
                        //This simply sets spacing between each of the elements in the row
                        TableRow.LayoutParams params4 = new TableRow.LayoutParams();
                        params4.SetMargins(iPaddingMargin1, iPaddingMargin2, iPaddingMargin1, iPaddingMargin2);
                        params4.Height = ConvertPixelsToDp(98);

                        EditText txtEdit2 = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
                        //EditText txtEdit2 = new EditText(this_context);
                        txtEdit2.Text = arrValues[i].ToString();
                        txtEdit2.SetWidth(ConvertPixelsToDp(iItemWidth - 2 * iPaddingMargin1));
                        txtEdit2.Id = iId + i + 2;
                        txtEdit2.SetPadding(10, 1, 10, 1);
                        txtEdit2.LayoutParameters = params4;
                        txtEdit2.SetHeight(ConvertPixelsToDp(98));
                        txtEdit2.SetSingleLine(false);

                        //txtEdit1.AfterTextChanged += (sender, args) => { SetExtraValueTextChanged(sender, args, iQuestionExtraRowValue2TagId, 2); };
                        //if (iReadOnly == -1)
                        //{
                        //    DisableTextbox(txtValue2);
                        //}
                        txtEdit2.SetTag(Resource.Integer.CellType, arrTypeOfControl[i]);
                        txtEdit2.SetTag(Resource.Integer.ParameterId, Convert.ToInt32(arrId[i]));
                        row1.AddView(txtEdit2);
                        //row1.SetMinimumHeight(ConvertPixelsToDp(100));

                        break;
                    case (int)ItemType.DropDown:
                        AndroidUtils.ComboBox cmbBox = new AndroidUtils.ComboBox();
                        ArrayAdapter arrCmbItems = new ArrayAdapter(this_context, Resource.Layout.layoutSpinner); //This is the resource for the main box
                        //Now get the info from the database for the drop down items
                        //arrCmbItems.Add("Yes");
                        //arrCmbItems.Add("No");
                        int iSelectedIndex = cmbBox.PopulateAdapter(ref arrCmbItems, arrSQL[i].ToString(), arrValues[i].ToString(), false, ref sRtnMsg);
                        arrCmbItems.SetDropDownViewResource(Resource.Layout.layoutSpinnerBase); //This is the resource for the drop down
                        Spinner cmbEdit1 = new Spinner(this_context);
                        cmbEdit1.Adapter = arrCmbItems;
                        cmbEdit1.Id = iId + i + 2;
                        cmbEdit1.SetPadding(10, 1, 10, 1);
                        cmbEdit1.LayoutParameters = params2;

                        ViewGroup.LayoutParams lp = cmbEdit1.LayoutParameters;
                        lp.Width = ConvertPixelsToDp(iItemWidth - 2 * iPaddingMargin1);
                        lp.Height = ConvertPixelsToDp(28);
                        cmbEdit1.LayoutParameters = lp;
                        cmbEdit1.SetBackgroundResource(Resource.Drawable.defaultSpinner2);

                        cmbEdit1.SetSelection(iSelectedIndex);
                        cmbEdit1.SetTag(Resource.Integer.CellType, arrTypeOfControl[i]);
                        cmbEdit1.SetTag(Resource.Integer.ParameterId, Convert.ToInt32(arrId[i]));

                        //cmbEdit1.Focusable = true;
                        //cmbEdit1.FocusableInTouchMode = true;
                        //cmbEdit1.FocusChange += (senderText, args) => { DropDownOnBlur(senderText, args, ""); }; 

                        row1.AddView(cmbEdit1);
                        //row1.SetMinimumHeight(ConvertPixelsToDp(30));
                        break;

                    case (int)ItemType.SQLColumnDropdown:
                        AndroidUtils.ComboBox cmbBox400 = new AndroidUtils.ComboBox();
                        ArrayAdapter arrCmbItems400 = new ArrayAdapter(this_context, Resource.Layout.layoutSpinner); //This is the resource for the main box
                        //Now get the info from the database for the drop down items
                        string sFormSQL = gridUtils.GetItemAttribute(giFormId, (int)SectionType.Form, -1, "FormSQL", ref sRtnMsg);
                        ArrayList arrSQLCols = DB.GetColumnNamesFromSQL(sFormSQL, ref sRtnMsg);
                        int iSelectedIndex400 = cmbBox400.PopulateAdapter(ref arrCmbItems400, arrSQLCols, arrValues[i].ToString(), true, ref sRtnMsg);
                        arrCmbItems400.SetDropDownViewResource(Resource.Layout.layoutSpinnerBase); //This is the resource for the drop down
                        Spinner cmbEdit400 = new Spinner(this_context);
                        cmbEdit400.Adapter = arrCmbItems400;
                        cmbEdit400.Id = iId + i + 2;
                        cmbEdit400.SetPadding(10, 1, 10, 1);
                        cmbEdit400.LayoutParameters = params2;

                        ViewGroup.LayoutParams lp400 = cmbEdit400.LayoutParameters;
                        lp400.Width = ConvertPixelsToDp(iItemWidth - 2 * iPaddingMargin1);
                        lp400.Height = ConvertPixelsToDp(28);
                        cmbEdit400.LayoutParameters = lp400;
                        cmbEdit400.SetBackgroundResource(Resource.Drawable.defaultSpinner2);

                        cmbEdit400.SetSelection(iSelectedIndex400);
                        cmbEdit400.SetTag(Resource.Integer.CellType, arrTypeOfControl[i]);
                        cmbEdit400.SetTag(Resource.Integer.ParameterId, Convert.ToInt32(arrId[i]));

                        row1.AddView(cmbEdit400);
                        //row1.SetMinimumHeight(ConvertPixelsToDp(30));
                        break;
                }
            }

            //Add in the old value textview

            //Now add a row with a save and close button
            TableRow row2 = new TableRow(this_context);
            row2.SetBackgroundColor(Android.Graphics.Color.Gray);
            row2.SetMinimumHeight(ConvertPixelsToDp(30));

            Button btnSave = new Button(this_context);
            btnSave.Text = "Save";
            btnSave.SetWidth(ConvertPixelsToDp(30));
            btnSave.SetHeight(ConvertPixelsToDp(20));
            btnSave.Click += (senderSave, args) => { SaveDetailDialog(senderSave, args, iId, iType); };

            row2.AddView(btnSave);

            TableRow.LayoutParams params3 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
            params3.SetMargins(0, 0, 0, 0);
            params3.Span = 2;

            Button btnClose = new Button(this_context);
            btnClose.Text = "Close";
            btnClose.SetWidth(ConvertPixelsToDp(30));
            btnClose.SetHeight(ConvertPixelsToDp(20));
            btnClose.Click += (senderClose, args) => { CloseDetailDialog(senderClose, args, iId, iType); };

            row2.AddView(btnClose, params3);

            table.AddView(row2);

            return table;
        }

        public bool DialogTextOnBlur(object sender, EventArgs e, string sMethodName)
        {

            EditText txt = (EditText)sender;

            if (!txt.HasFocus)
            {
                return Evaluate(sender, e, sMethodName);
                //                InvokeStringMethod(sMethodName, objTextEdit);
            }
            else
            {
                gtxtFocused = (EditText)sender;
                return true;
            }
        }


        public void DropDownOnBlur(object sender, EventArgs e, string sMethodName)
        {

            Spinner txt = (Spinner)sender;
            txt.PerformClick();
            //txt.RequestFocus();

            if (gtxtFocused != null)
            {
                gtxtFocused.ClearFocus();
            }
//            View vw = mainSV.FindFocus();

            //if (!txt.HasFocus)
            //{
            //    Evaluate(sender, e, sMethodName);
            //    //                InvokeStringMethod(sMethodName, objTextEdit);
            //}
            //else
            //{
            //}
        }
        
        private void SaveDetailDialog(object sender, EventArgs e, int iRLViewId, int iType)
        {
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            Android.App.ActionBar navBar = this.ActionBar;
            int i;
            int iInnerViewType;
            bool bProceed;
            string sParameterValue;
            string sFormName = "";
            int iItemType;
            int iParameterId;
            string sRtnMsg = "";
            int iUniqueItemId = -1;
            int iSelectionType = -1;
            int iRow = -1, iCol = -1, iCellSectionId = -1, iCellId= -1;
            int iContainerId;
            int iBaseId;
            string sMethod = "";

            //Loop through all the attributes in the dialog
            TableLayout table = (TableLayout)FindViewById(iRLViewId + 1);
            for (i = 0; i < table.ChildCount -1; i++) //Take 1 rows out because there is a button row at the end of the table. The header row is actually in a different table.
            {
                View vw = (View)FindViewById(iRLViewId + 1 + i + 1);
                //Now find the typeof view
                Java.Lang.Object tag = vw.GetTag(Resource.Integer.CellType);
                iInnerViewType = Convert.ToInt32(tag);
                Java.Lang.Object tag2 = vw.GetTag(Resource.Integer.ParameterId);
                iParameterId = Convert.ToInt32(tag2);
                Java.Lang.Object tag1 = vw.GetTag(Resource.String.OnBlurMethodName);
                if (tag1 != null)
                {
                    sMethod = tag1.ToString();
                }
                switch (iInnerViewType)
                {
                    case (int)ItemType.Label:
                    default:
                        bProceed = false;
                        TextView lbl = (TextView)vw;
                        sParameterValue = lbl.Text;
                        break;
                    case (int)ItemType.TextBox:
                    case (int)ItemType.TextArea:
                        EditText txt = (EditText)vw;
                        sParameterValue = txt.Text;
                        bProceed = true;
                        Evaluate(txt, e, sMethod);
//                        DialogTextOnBlur(txt, null, sMethod);
                        break;
                    case (int)ItemType.DropDown:
                    case (int)ItemType.SQLColumnDropdown:
                        Spinner cmbDD = (Spinner)vw;
                        int iSelection = cmbDD.SelectedItemPosition;
                        sParameterValue =  cmbDD.Adapter.GetItem(iSelection).ToString();
                        bProceed = true;
                        break;
                }
                switch (iType)
                {
                    case (int)SectionType.Form:
                        iItemType = -1;
                        if (iParameterId == -99) //This is the form name and cannot be changed
                        {
                            sFormName = sParameterValue;
                            bProceed = false;
                        }

                        if (iParameterId == -98) //This is the form name and cannot be changed
                        {
                            if (!grdUtils.SaveFormDetails(sFormName, sParameterValue, ref giFormId, ref sRtnMsg))
                            {
                                alert.SetAlertMessage(sRtnMsg);
                                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                                return;
                            }

                            navBar.Title = sFormName;
                            sFormName = sFormName + " - " + sParameterValue;
                            TextView txtForm = (TextView)FindViewById(iFormLabelId);
                            txtForm.Text = sFormName;

                            bProceed = false;
                        }
                        break;
                    case (int)SectionType.Detail:
                    case (int)SectionType.Header:
                    case (int)SectionType.Footer:
                        iItemType = -1;
                        break;
                    case (int)SectionType.HeaderRow:
                    case (int)SectionType.HeaderColumn:
                    case (int)SectionType.DetailRow:
                    case (int)SectionType.DetailColumn:
                    case (int)SectionType.FooterRow:
                    case (int)SectionType.FooterColumn:
                        //Save to the tblGridItems table and return the unique id
                        iSelectionType = iType;
                        int iInnerTableId = iRLViewId + 1;
                        TableLayout tableVW = (TableLayout)FindViewById(iInnerTableId);
                        Java.Lang.Object tag3 = tableVW.GetTag(Resource.Integer.CellSectionId);
                        iCellSectionId = Convert.ToInt32(tag3);
                        Java.Lang.Object tag4 = tableVW.GetTag(Resource.Integer.CellRowId);
                        iRow = Convert.ToInt32(tag4);
                        Java.Lang.Object tag5 = tableVW.GetTag(Resource.Integer.CellColumnId);
                        iCol = Convert.ToInt32(tag5);
                        Java.Lang.Object tag6 = tableVW.GetTag(Resource.Integer.CellId);
                        iCellId = Convert.ToInt32(tag6);
                        grdUtils.SaveGridItemDetails(giFormId, iCellSectionId, iSelectionType, iRow, iCol, iCellId, ref iUniqueItemId, ref sRtnMsg);
                        iItemType = iUniqueItemId;
                        break;
                    case (int)SectionType.GridItem:
                        if (i == 0) //This onlyhas to be done once
                        {
                            //Save to the tblGridItems table and return the unique id
                            Spinner cmbDDType = (Spinner)FindViewById(iRLViewId - 1);
                            iSelectionType = cmbDDType.SelectedItemPosition + 1;
                            int iInnerTableIdGI = iRLViewId + 1;
                            TableLayout tableVWGI = (TableLayout)FindViewById(iInnerTableIdGI);
                            Java.Lang.Object tag3GI = tableVWGI.GetTag(Resource.Integer.CellSectionId);
                            iCellSectionId = Convert.ToInt32(tag3GI);
                            Java.Lang.Object tag4GI = tableVWGI.GetTag(Resource.Integer.CellRowId);
                            iRow = Convert.ToInt32(tag4GI);
                            Java.Lang.Object tag5GI = tableVWGI.GetTag(Resource.Integer.CellColumnId);
                            iCol = Convert.ToInt32(tag5GI);
                            Java.Lang.Object tag6GI = tableVWGI.GetTag(Resource.Integer.CellId);
                            iCellId = Convert.ToInt32(tag6GI);
                            grdUtils.SaveGridItemDetails(giFormId, iCellSectionId, iSelectionType, iRow, iCol, iCellId, ref iUniqueItemId, ref sRtnMsg);
                            iItemType = iUniqueItemId;
                            grdUtils.DeleteAllItemAttributesNotOfType(giFormId, iCellSectionId, iType, iRow, iCol, iSelectionType);
                        }
                        else
                        {
                            iItemType = iUniqueItemId;
                        }
                        break;
                    default:
                        iItemType = -1;
                        break;
                }

                if (bProceed)
                {
                    if (!grdUtils.SaveItemAttribute(giFormId, iItemType, iType, iParameterId, sParameterValue, ref sRtnMsg))
                    {
                        alert.SetAlertMessage(sRtnMsg);
                        this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                        return;
                    }
                }
            }

            if (gbCloseDialog)
            {
                RelativeLayout rl = (RelativeLayout)FindViewById(iRLViewId);
                ScrollView sv = (ScrollView)FindViewById(20);
                mainView.RemoveView(rl);

                //Now also reenable any buttons etc
                giDialogOpen = 0;

                //And now build the Table
                if (iType == (int)SectionType.Header || iType == (int)SectionType.Detail || iType == (int)SectionType.Footer)
                {
                    switch (iType)
                    {
                        case (int)SectionType.Header:
                            iContainerId = iHeaderSectionContainerId;
                            iBaseId = iHeaderSectionTableId;
                            break;
                        case (int)SectionType.Detail:
                            iContainerId = iDetailSectionContainerId;
                            iBaseId = iDetailSectionTableId;
                            break;
                        case (int)SectionType.Footer:
                            iContainerId = iFooterSectionContainerId;
                            iBaseId = iFooterSectionTableId;
                            break;
                        default:
                            iContainerId = -1;
                            iBaseId = -1;
                            break;
                    }

                    TableRow tableRowContainer = (TableRow)FindViewById(iContainerId);
                    TableLayout tableRemove = (TableLayout)FindViewById(iBaseId);
                    tableRowContainer.RemoveView(tableRemove);

                    InsertTable(iType, 1, 1);
                }

                //If a grid item place the item into the grid itself
                if (iType == (int)SectionType.GridItem)
                {
                    InsertGridItem(iSelectionType, iRow, iCol, iCellSectionId, iCellId);
                }

                if (iType == (int)SectionType.HeaderRow)
                {
                    RebuildRow((int)SectionType.Header, iRow);
                }

                if (iType == (int)SectionType.DetailRow)
                {
                    RebuildRow((int)SectionType.Detail, iRow);
                }

                if (iType == (int)SectionType.FooterRow)
                {
                    RebuildRow((int)SectionType.Footer, iRow);
                }

                if (iType == (int)SectionType.HeaderColumn)
                {
                    RebuildColumn((int)SectionType.Header, iCol);
                }

                if (iType == (int)SectionType.DetailColumn)
                {
                    RebuildColumn((int)SectionType.Detail, iCol);
                }

                if (iType == (int)SectionType.FooterColumn)
                {
                    RebuildColumn((int)SectionType.Footer, iCol);
                }
            }
        }

        public void InsertGridItem(int iCellType, int iRow, int iCol, int iCellSectionId, int iCellId)
        {
            bool bSetGridLines = true;
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            string sRtnMsg = "";
            int i;

            int iItemId = grdUtils.GetGridItemId(giFormId, iCellSectionId, iCellId, ref sRtnMsg);
            if(sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            //The gridlines are set at the header, detail or footer level, hence the -1 for the itemid
            string sSetGridLines = grdUtils.GetItemAttribute(giFormId, iCellSectionId, -1, "Gridlines", ref sRtnMsg);
            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            if (sSetGridLines.ToUpper() == "YES")
            {
                bSetGridLines = true;
            }
            else
            {
                bSetGridLines = false;
            }

            //The columns are set at the header, detail or footer level, hence the -1 for the itemid
            string sCols = grdUtils.GetItemAttribute(giFormId, iCellSectionId, -1, "Columns", ref sRtnMsg);
            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            int iTotalCols = Convert.ToInt32(sCols);

            //The columns are set at the header, detail or footer level, hence the -1 for the itemid
            string sColSpan = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "ColumnSpan", ref sRtnMsg);
            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            int iColSpan = Convert.ToInt32(sColSpan);

            //Find the existing item, remove it and replace it with this
            View vwToRemove = (View)FindViewById(iCellId);
            TableRow parentVw = (TableRow)vwToRemove.Parent;
            int iChildIndex = parentVw.IndexOfChild(vwToRemove);

            View vw = GetCellView(iCellType, iRow - 1, iCol - 1, iCellSectionId, bSetGridLines, iTotalCols, (int)SectionType.GridItem, 1, null);
            parentVw.RemoveView(vwToRemove);

            //Now there also might be a reduction in the column span
            int iCounter = 0;
            for (i = iCol; i < iTotalCols; i++)
            {
                int iCellIdSpanExtra = iCellId + (i - iCol + 1) * 1000;
                View vwToAddSpan = (View)FindViewById(iCellIdSpanExtra);
                if (vwToAddSpan == null)
                {
                    View vwAdd = GetCellView((int)ItemType.Label, iRow - 1, iCol + iCounter, iCellSectionId, bSetGridLines, iTotalCols, (int)SectionType.GridItem, 1, null);
                    if (vwAdd != null)
                    {
                        parentVw.AddView(vwAdd, i);
                        iCounter++;
                    }
                }

            }

            //Now also remove any other cells from the spanning
            for (i = 1; i < iColSpan; i++)
            {
                int iCellIdSpan = iCellId + i * 1000;
                View vwToRemoveSpan = (View)FindViewById(iCellIdSpan);
//                int iChildIndexSpan = parentVw.IndexOfChild(vwToRemoveSpan);
                parentVw.RemoveView(vwToRemoveSpan);

            }

            if (vw != null)
            {
                parentVw.AddView(vw, iChildIndex);
            }


        }

        public void RebuildRow(int iSectionId, int iRow)
        {
            int i;
            int iCellId;
            int iBaseId = -1;
            int iContainerId;
            int iRowType = -1;
            int iColCount;
            int iCol;
            int iCellSectionId;
            int iCellType;
            int iSectionCellId = -1;
            int iItemType = -1;
            bool bSetGridLines = true;
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            string sRtnMsg = "";
            int iRowHeaderBaseId = -1;
            int iRecordCounter = -1;

            switch (iSectionId)
            {
                case (int)SectionType.Header:
                    iContainerId = iHeaderSectionContainerId;
                    iBaseId = iHeaderSectionTableId;
                    iRowType = (int)ItemType.RowHeader;
                    iRowHeaderBaseId = iHeaderRowBaseId;
                    break;
                case (int)SectionType.Detail:
                    iContainerId = iDetailSectionContainerId;
                    iBaseId = iDetailSectionTableId;
                    iRowType = (int)ItemType.RowDetail;
                    iRowHeaderBaseId = iDetailRowBaseId;
                    break;
                case (int)SectionType.Footer:
                    iContainerId = iFooterSectionContainerId;
                    iBaseId = iFooterSectionTableId;
                    iRowType = (int)ItemType.RowFooter;
                    iRowHeaderBaseId = iFooterRowBaseId;
                    break;
            }

            TableLayout table = (TableLayout)FindViewById(iBaseId);
            TableRow row = (TableRow)table.GetChildAt(0);
            iColCount = row.ChildCount;

            //The gridlines are set at the header, detail or footer level, hence the -1 for the itemid
            string sSetGridLines = grdUtils.GetItemAttribute(giFormId, iSectionId, -1, "Gridlines", ref sRtnMsg);
            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            if (sSetGridLines.ToUpper() == "YES")
            {
                bSetGridLines = true;
            }
            else
            {
                bSetGridLines = false;
            }

            //The columns are set at the header, detail or footer level, hence the -1 for the itemid
            string sCols = grdUtils.GetItemAttribute(giFormId, iSectionId, -1, "Columns", ref sRtnMsg);
            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            int iTotalCols = Convert.ToInt32(sCols);

            for (i = 0; i < iColCount; i++)
            {
                //In the first position it should be a row header. Note the row coming across is the counter not the table row (ie it is one based not zero based)
                if (i == 0)
                {
                    iCellId = iRowHeaderBaseId + ((iRow) * 100000) + 1000;
                    iCellType = iRowType;
                    iCol = 0;
                    iItemType = -1;
                    iRecordCounter = 0;
                    switch (iSectionId)
                    {
                        case (int)SectionType.Header:
                            iSectionCellId = (int)SectionType.HeaderRow;
                            break;
                        case (int)SectionType.Detail:
                            iSectionCellId = (int)SectionType.DetailRow;
                            break;
                        case (int)SectionType.Footer:
                            iSectionCellId = (int)SectionType.FooterRow;
                            break;
                    }

                }
                else
                {
                    iCellId = iBaseId + ((iRow) * 100000) + (i*1000) + 1; //Only require i because the cell in column 1 (not the row header column) will be when i = 1 NOT i = 0. 
                                                                          //We do not have to worry about records here because this is only ever used in the build screen, hence the + 1 at the end for the record counter

                    iRecordCounter = 1;
                    //if (i == 1 && iSectionId == (int)SectionType.Detail)
                    //{
                    //    TextView txtRC = (TextView)FindViewById(iCellId +700);
                    //    iRecordCounter = Convert.ToInt32(txtRC.Text);
                    //}
                    TableLayout tableVWGI = (TableLayout)FindViewById(iCellId);
                    if (tableVWGI != null)
                    {
                        Java.Lang.Object tag2GI = tableVWGI.GetTag(Resource.Integer.CellType);
                        iCellType = Convert.ToInt32(tag2GI);
                        Java.Lang.Object tag3GI = tableVWGI.GetTag(Resource.Integer.CellSectionId);
                        iCellSectionId = Convert.ToInt32(tag3GI);
                        iCol = i - 1;
                        iSectionCellId = iSectionId;
                        iItemType = (int)SectionType.GridItem;
                    }
                    else
                    {
                        iCellType = -1; //Just to satisfy the compiler
                        iCol = -1; //Just to satisfy the compiler
                    }
                    //Java.Lang.Object tag4GI = tableVWGI.GetTag(Resource.Integer.CellRowId);
                    //iRow = Convert.ToInt32(tag4GI);
                    //Java.Lang.Object tag5GI = tableVWGI.GetTag(Resource.Integer.CellColumnId);
                    //Java.Lang.Object tag6GI = tableVWGI.GetTag(Resource.Integer.CellId);
                    //iCellId = Convert.ToInt32(tag6GI);
                }

                //Find the existing item, remove it and replace it with this
                View vwToRemove = (View)FindViewById(iCellId);
                if (vwToRemove != null) //Could be null due to column spanning
                {
                    TableRow parentVw = (TableRow)vwToRemove.Parent;
                    int iChildIndex = parentVw.IndexOfChild(vwToRemove);

                    View vw = GetCellView(iCellType, iRow - 1, iCol, iSectionCellId, bSetGridLines, iTotalCols, iItemType, iRecordCounter, null);
                    parentVw.RemoveView(vwToRemove);
                    if (vw != null)
                    {
                        parentVw.AddView(vw, iChildIndex);
                    }
                }
            }

        }

        public void RebuildColumn(int iSectionId, int iColumn)
        {
            int i;
            int j;
            int iCellId;
            int iBaseId = -1;
            int iContainerId;
            int iColType = -1;
            int iRowCount;
            int iRow = -1;
            int iCellSectionId;
            int iCellType;
            int iSectionCellId = -1;
            int iItemType = -1;
            bool bSetGridLines = true;
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            string sRtnMsg = "";
            int iColHeaderBaseId = -1;
            int iRecordCounter = -1;

            switch (iSectionId)
            {
                case (int)SectionType.Header:
                    iContainerId = iHeaderSectionContainerId;
                    iBaseId = iHeaderSectionTableId;
                    iColType = (int)ItemType.ColumnHeader;
                    iColHeaderBaseId = iHeaderColumnBaseId;
                    break;
                case (int)SectionType.Detail:
                    iContainerId = iDetailSectionContainerId;
                    iBaseId = iDetailSectionTableId;
                    iColType = (int)ItemType.ColumnDetail;
                    iColHeaderBaseId = iDetailColumnBaseId;
                    break;
                case (int)SectionType.Footer:
                    iContainerId = iFooterSectionContainerId;
                    iBaseId = iFooterSectionTableId;
                    iColType = (int)ItemType.ColumnFooter;
                    iColHeaderBaseId = iDetailColumnBaseId;
                    break;
            }

            TableLayout table = (TableLayout)FindViewById(iBaseId);
            TableRow row = (TableRow)table.GetChildAt(0);
            iRowCount = table.ChildCount;

            //The gridlines are set at the header, detail or footer level, hence the -1 for the itemid
            string sSetGridLines = grdUtils.GetItemAttribute(giFormId, iSectionId, -1, "Gridlines", ref sRtnMsg);
            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            if (sSetGridLines.ToUpper() == "YES")
            {
                bSetGridLines = true;
            }
            else
            {
                bSetGridLines = false;
            }

            //The rows are set at the header, detail or footer level, hence the -1 for the itemid
            string sRows = grdUtils.GetItemAttribute(giFormId, iSectionId, -1, "Rows", ref sRtnMsg);
            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            int iTotalRows = Convert.ToInt32(sRows) + 1; //The extra row for the column header because this can only happen in the build

            //The columns are set at the header, detail or footer level, hence the -1 for the itemid
            string sCols = grdUtils.GetItemAttribute(giFormId, iSectionId, -1, "Columns", ref sRtnMsg);
            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            int iTotalCols = Convert.ToInt32(sCols);

            for (i = 0; i < iTotalRows; i++)
            {
                //In the first position it should be a row header. Note the row coming across is the counter not the table row (ie it is one based not zero based)
                if (i == 0)
                {
                    iCellId = iColHeaderBaseId + ((i + 1) * 100000) + (iColumn * 1000);
                    iCellType = iColType;
                    iRow = 0;
                    iItemType = -1;
                    iRecordCounter = 0;
                    switch (iSectionId)
                    {
                        case (int)SectionType.Header:
                            iSectionCellId = (int)SectionType.HeaderColumn;
                            break;
                        case (int)SectionType.Detail:
                            iSectionCellId = (int)SectionType.DetailColumn;
                            break;
                        case (int)SectionType.Footer:
                            iSectionCellId = (int)SectionType.FooterColumn;
                            break;
                    }

                }
                else
                {
                    iCellId = iBaseId + ((i) * 100000) + (iColumn * 1000) + 1; //Only require i becausae the cell in column 1 (not the row header column) will be when i = 1 NOT i = 0
                                                                           //We do not have to worry about the records here because this can only ever be called from the build screen, so the record counter will ALWAYS be 1 (hence the last +1)
                    iRecordCounter = 1; //In the grid the record counter is always 1 when it is in build mode
                    //if (iColumn == 1 && iSectionId == (int)SectionType.Detail)
                    //{
                    //    TextView txtRC = (TextView)FindViewById(iCellId + 700);
                    //    iRecordCounter = Convert.ToInt32(txtRC.Text);
                    //}
                    TableLayout tableVWGI = (TableLayout)FindViewById(iCellId);
                    if (tableVWGI != null)    //The cell could not exist because it could be spanned
                    {
                        Java.Lang.Object tag2GI = tableVWGI.GetTag(Resource.Integer.CellType);
                        iCellType = Convert.ToInt32(tag2GI);
                        Java.Lang.Object tag3GI = tableVWGI.GetTag(Resource.Integer.CellSectionId);
                        iCellSectionId = Convert.ToInt32(tag3GI);
                        iRow = i - 1;
                        iSectionCellId = iSectionId;
                        iItemType = (int)SectionType.GridItem;
                    }
                    else
                    {
                        iCellType = -1; //Just to satisfy the compiler
                    }
                    //Java.Lang.Object tag4GI = tableVWGI.GetTag(Resource.Integer.CellRowId);
                    //iRow = Convert.ToInt32(tag4GI);
                    //Java.Lang.Object tag5GI = tableVWGI.GetTag(Resource.Integer.CellColumnId);
                    //Java.Lang.Object tag6GI = tableVWGI.GetTag(Resource.Integer.CellId);
                    //iCellId = Convert.ToInt32(tag6GI);
                }

                //Find the existing item, remove it and replace it with this
                View vwToRemove = (View)FindViewById(iCellId);
                if (vwToRemove != null)
                {
                    TableRow parentVw = (TableRow)vwToRemove.Parent;
                    int iChildIndex = parentVw.IndexOfChild(vwToRemove);

                    View vw = GetCellView(iCellType, iRow, iColumn - 1, iSectionCellId, bSetGridLines, iTotalCols, iItemType, iRecordCounter, null);
                    parentVw.RemoveView(vwToRemove);
                    if (vw != null)
                    {
                        parentVw.AddView(vw, iChildIndex);
                    }
                }
                else
                {
                    //Then this is a column that is spanned by an earlier one.
                    //Find the previous column
                    for (j = iColumn - 1; j >= 0; j--)
                    {
                        iCellId = iBaseId + ((i) * 100000) + (j * 1000) + 1; //Only require i becausae the cell in column 1 (not the row header column) will be when i = 1 NOT i = 0
                                                                                   //We do not have to worry about the records here because this can only ever be called from the build screen, so the record counter will ALWAYS be 1 (hence the last +1)
                        iRecordCounter = 1; //In the grid the record counter is always 1 when it is in build mode                    
                        View vwSpan = (View)FindViewById(iCellId);
                        if (vwSpan != null)
                        {
                            TableLayout tableVWGI = (TableLayout)FindViewById(iCellId);
                            if (tableVWGI != null)    //The cell could not exist because it could be spanned
                            {
                                Java.Lang.Object tag2GI = tableVWGI.GetTag(Resource.Integer.CellType);
                                iCellType = Convert.ToInt32(tag2GI);
                                Java.Lang.Object tag3GI = tableVWGI.GetTag(Resource.Integer.CellSectionId);
                                iCellSectionId = Convert.ToInt32(tag3GI);
                                iRow = i - 1;
                                iSectionCellId = iSectionId;
                                iItemType = (int)SectionType.GridItem;
                            }
                            else
                            {
                                iCellType = -1; //Just to satisfy the compiler
                            }
                            //Rebuild this view
                            TableRow parentVw = (TableRow)vwSpan.Parent;
                            int iChildIndex = parentVw.IndexOfChild(vwSpan);

                            View vw = GetCellView(iCellType, iRow, j -1, iSectionCellId, bSetGridLines, iTotalCols, iItemType, iRecordCounter, null);
                            parentVw.RemoveView(vwSpan);
                            if (vw != null)
                            {
                                parentVw.AddView(vw, iChildIndex);
                            }
                            break;

                        }
                    }
                }
            }

            //Now also fix up the horizontal scroll width
            SetHorizontalScrollWidth();
        }

        public void GridItemTypeChanged(object sender, AdapterView.ItemSelectedEventArgs e, int iRLViewId)
        {
            Spinner spin = (Spinner)sender;
            string sNewType = spin.GetItemAtPosition(e.Position).ToString();
            TextView txt = (TextView)FindViewById(iRLViewId - 4);
            int iExistingSelection = Convert.ToInt32(txt.Text);
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            int iNewType = e.Position + 1;
            int iOldType = iExistingSelection + 1;

            if (e.Position != iExistingSelection)
            {
                //Now get rid of the old table from the relative layout and put in the new one
                TableLayout tableVW = (TableLayout)FindViewById(iRLViewId + 1);
                if (tableVW != null)
                {
                    Java.Lang.Object tag3 = tableVW.GetTag(Resource.Integer.CellSectionId);
                    int iCellSectionId = Convert.ToInt32(tag3);
                    Java.Lang.Object tag4 = tableVW.GetTag(Resource.Integer.CellRowId);
                    int iRow = Convert.ToInt32(tag4);
                    Java.Lang.Object tag5 = tableVW.GetTag(Resource.Integer.CellColumnId);
                    int iCol = Convert.ToInt32(tag5);
                    Java.Lang.Object tag6 = tableVW.GetTag(Resource.Integer.CellId);
                    int iCellId = Convert.ToInt32(tag6);
                    TableRow tabrow = (TableRow)FindViewById(iRLViewId - 3);

                    //Also remove all existing attributes for what we are changin to
                    grdUtils.DeleteAllItemAttributesOfType(giFormId, (int)SectionType.GridItem, iRow, iCol, iNewType);

                    //Copy across the existing attributes that match
                    grdUtils.CopyMatchingItemAttributes(giFormId, iCellSectionId, iRow, iCol, iOldType, iNewType);

                    //Add in the new table
                    TableRow.LayoutParams params3 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
                    params3.SetMargins(0, 0, 0, 0);
                    params3.Span = 2;
                    TableLayout table2 = MakeDetailDialogMainTable(iRLViewId, (int)SectionType.GridItem, e.Position + 1, iCellId, iCellSectionId, true);

                    //Must remove here becasue you need the table in the Make main table routine.
                    tabrow.RemoveView(tableVW);
                    tabrow.AddView(table2, params3);

                    table2.SetTag(Resource.Integer.CellSectionId, iCellSectionId);
                    table2.SetTag(Resource.Integer.CellRowId, iRow);
                    table2.SetTag(Resource.Integer.CellColumnId, iCol);
                    table2.SetTag(Resource.Integer.CellId, iCellId);

                    txt.Text = e.Position.ToString();

                    SetAnyValueChanged();
                }
            }

            return;
        }

        private void CloseDetailDialog(object sender, EventArgs e, int iRLViewId, int iType)
        {
            RelativeLayout rl = (RelativeLayout)FindViewById(iRLViewId);
            ScrollView sv = (ScrollView)FindViewById(20);
            mainView.RemoveView(rl);

            //Now also reenable any buttons etc
            giDialogOpen = 0;
        }

        private void InsertTable(int iSectionTypeId, int iPageNo, int iRecordNo)
        {
            int iContainerId = 0;
            int iBaseId = 0;
            int iRowId = 0;
            int iCellId = 0;
            int j;
            int i;
            int k;
            int iColWidth = 100;
            TableRow trow;
            TableLayout table1;
            int iCellType = 1;
            int iInnerViewType = -1;
            float iButtonWidthDp = 64f;
            bool bSetGridLines = true;
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            clsLocalUtils utils = new clsLocalUtils();
            int iRows = 0;
            int iCols = 0;
            int iGridlineWeight = 0;
            int iRowSection = -1;
            int iColumnSection = -1;
            int iRepeatableRecords = 1;
            string sRtnMsg = "";
            int iLowerRecord = 0;
            int iUpperRecord = 0;
            int iTotalPages = -1;
            bool bNavBarRecord = false;
            bool bNavBarPage = false;
            int iColumnHeaderItemType = 0;
            int iRowHeaderItemType = 0;
            int iTotalRecords = 0;

            try
            {
                //Get rows and columns and gridline width, color and background color from the DB
                ArrayList arrGridInfo = grdUtils.GetGridInfo(giFormId, iSectionTypeId);

                if (arrGridInfo.Count >= 3)
                {
                    ArrayList arrId = (ArrayList)arrGridInfo[0];
                    ArrayList arrValues = (ArrayList)arrGridInfo[1];
                    ArrayList arrNames = (ArrayList)arrGridInfo[2];

                    for (i = 0; i < arrId.Count; i++)
                    {
                        if (arrNames[i].ToString() == "Rows")
                        {
                            iRows = Convert.ToInt32(arrValues[i]);
                        }

                        if (arrNames[i].ToString() == "Columns")
                        {
                            iCols = Convert.ToInt32(arrValues[i]);
                        }

                        if (arrNames[i].ToString() == "Gridlines")
                        {
                            string sGridlines = arrValues[i].ToString();
                            if (sGridlines == "Yes")
                            {
                                bSetGridLines = true;
                            }
                            else
                            {
                                bSetGridLines = false;
                            }
                        }

                        if (arrNames[i].ToString() == "GridlineWeight")
                        {
                            string sGridlines = arrValues[i].ToString();
                            iGridlineWeight = Convert.ToInt32(sGridlines.Replace("px", ""));
                        }
                    }

                    switch (iSectionTypeId)
                    {
                        case (int)SectionType.Header:
                            iContainerId = iHeaderSectionContainerId;
                            iBaseId = iHeaderSectionTableId;
                            iColumnSection = (int)SectionType.HeaderColumn;
                            iRowSection = (int)SectionType.HeaderRow;
                            iColumnHeaderItemType = (int)ItemType.ColumnHeader;
                            iRowHeaderItemType = (int)ItemType.RowHeader;
                            break;
                        case (int)SectionType.Detail:
                            iContainerId = iDetailSectionContainerId;
                            iBaseId = iDetailSectionTableId;
                            iColumnSection = (int)SectionType.DetailColumn;
                            iRowSection = (int)SectionType.DetailRow;
                            iColumnHeaderItemType = (int)ItemType.ColumnDetail;
                            iRowHeaderItemType = (int)ItemType.RowDetail;
                            break;
                        case (int)SectionType.Footer:
                            iContainerId = iFooterSectionContainerId;
                            iBaseId = iFooterSectionTableId;
                            iColumnSection = (int)SectionType.FooterColumn;
                            iRowSection = (int)SectionType.FooterRow;
                            iColumnHeaderItemType = (int)ItemType.ColumnFooter;
                            iRowHeaderItemType = (int)ItemType.RowFooter;
                            break;
                    }

                    iColWidth = GetScreenWidthPixels() / iCols;

                    //Find the detail section table container
                    TableRow tableContainer = (TableRow)FindViewById(iContainerId);

                    TableLayout tableExists = (TableLayout)FindViewById(iBaseId);

                    if (tableExists == null)
                    {
                        //Create a new table
                        TableLayout table = new TableLayout(this_context);
                        //                table.SetGravity(GravityFlags.CenterHorizontal);
                        table.SetBackgroundColor(Android.Graphics.Color.White);
                        //                table.SetPadding(2, 2, 2, 2);
                        table.Id = iBaseId;
                        table1 = table;

                        //Now create a row at the top for the column headers
                        if (giBuild == 1)
                        {
                            TableRow rowColHdr = new TableRow(this_context);

                            //And put a blank cell in the top left. THis cell has no detail dialog button available
                            View vwColTL = GetCellView((int)ItemType.Label, -1, -1, iSectionTypeId * -1, bSetGridLines, iCols, -1, 0, null);
                            if (vwColTL != null)
                            {
                                rowColHdr.AddView(vwColTL);
                            }

                            for (i = 0; i < iCols; i++)
                            {

                                View vwCol = GetCellView(iColumnHeaderItemType, 0, i, iColumnSection, bSetGridLines, iCols, -1, 0, null);
                                if (vwCol != null)
                                {
                                    rowColHdr.AddView(vwCol);
                                }
                            }

                            table1.AddView(rowColHdr);
                        }
                    }
                    else
                    {
                        table1 = tableExists;
                    }

                    TableRow.LayoutParams params1 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
                    params1.SetMargins(0, 0, 0, 0);
                    if (giBuild == 1)
                    {
                        params1.Span = 3;
                    }

                    //Put in the repeatable records. This is onyl applicable to the detail section.
                    if (iSectionTypeId == (int)SectionType.Detail && giBuild == 0)
                    {
                        string sNavBarPage = grdUtils.GetItemAttribute(giFormId, iSectionTypeId, -1, "PageNavBar", ref sRtnMsg);
                        string sNavBarRecord = grdUtils.GetItemAttribute(giFormId, iSectionTypeId, -1, "RecordNavBar", ref sRtnMsg);
                        string sRepeatable = grdUtils.GetItemAttribute(giFormId, iSectionTypeId, -1, "Repeatable", ref sRtnMsg);
                        string sRecordsInSection = grdUtils.GetItemAttribute(giFormId, iSectionTypeId, -1, "RowsPerPage", ref sRtnMsg);
                        int iRecordsInSection = 1;
                        if (utils.IsNumeric(sRecordsInSection))
                        {
                            iRecordsInSection = Convert.ToInt32(sRecordsInSection);
                        }
                        
                        if (sRepeatable.ToUpper() == "YES")
                        {
                            if (garrDBValues != null)
                            {
                                iRepeatableRecords = garrDBValues.Count;
                                iTotalRecords = garrDBValues.Count;
                            }
                            else
                            {
                                iRepeatableRecords = 1;
                                iTotalRecords = 1;
                            }

                            giTotalRecords = iTotalRecords;
                            giRecordsPerPage = iRecordsInSection/iRows;
                            giDetailRows = iRows;
                            giDetailColumns = iCols;

                            if (iRepeatableRecords % (iRecordsInSection/iRows) > 0.000001)
                            {
                                iTotalPages = iRepeatableRecords / (iRecordsInSection/iRows) + 1;
                            }
                            else
                            {
                                iTotalPages = iRepeatableRecords / (iRecordsInSection / iRows);
                            }

                            iLowerRecord = (iPageNo - 1) * (iRecordsInSection / iRows);
                            iUpperRecord = (iPageNo) * (iRecordsInSection / iRows);
                            if (iUpperRecord > iTotalRecords)
                            {
                                iUpperRecord = iTotalRecords;
                            }

                            if (sNavBarPage == "" || sNavBarPage.ToUpper() == "YES")
                            {
                                bNavBarPage = true;
                            }

                            if (sNavBarRecord == "" || sNavBarRecord.ToUpper() == "YES")
                            {
                                bNavBarRecord = true;
                            }
                        }
                        else
                        {
                            iRepeatableRecords = 1;
                            iLowerRecord = 0;
                            iUpperRecord = 1;
                        }
                    }
                    else
                    {
                        iRepeatableRecords = 1;
                        iLowerRecord = 0;
                        iUpperRecord = 1;
                    }

                    //this.RunOnUiThread(() =>
                    //{
                    //    progBarDetail.SetProgressBarTitle("Building section " + ((SectionType)iSectionTypeId).ToString());
                    //    progBarDetail.ShowProgressBar(iRepeatableRecords);
                    //});

                    for (k = iLowerRecord; k < iUpperRecord; k++)
                    {
                        //this.RunOnUiThread(() =>
                        //{
                        //    progBarDetail.UpdateProgressBar(k);
                        //});
                        ArrayList arrThisRecord;
                        if (giBuild == 0 && garrDBValues!= null)
                        {
                            if (garrDBValues.Count > 0)
                            {
                                arrThisRecord = (ArrayList)garrDBValues[k];
                            }
                            else
                            {
                                arrThisRecord = null;
                            }
                        }
                        else
                        {
                            arrThisRecord = null;
                        }

                        for (j = 0; j < iRows; j++)
                        {
                            //First see if the row exists
                            iRowId = iBaseId + ((j + 1) * 100000);
                            TableRow rowExists = (TableRow)FindViewById(iRowId);

                            if (rowExists == null)
                            {
                                TableRow row = new TableRow(this_context);
                                row.SetBackgroundColor(Android.Graphics.Color.White);
                                row.SetMinimumHeight(ConvertPixelsToDp(30f));
                                row.Id = iRowId;
                                trow = row;

                                //Put in the row bits
                                if (giBuild == 1)
                                {
                                    View vwRow = GetCellView(iRowHeaderItemType, j, 0, iRowSection, bSetGridLines, iCols, -1, 0, null);
                                    if (vwRow != null)
                                    {
                                        trow.AddView(vwRow);
                                    }
                                }

                                for (i = 0; i < iCols; i++)
                                {
                                    View vw = GetCellView(iCellType, j, i, iSectionTypeId, bSetGridLines, iCols, (int)SectionType.GridItem, k+1, arrThisRecord);
                                    if (vw != null)
                                    {
                                        trow.AddView(vw);
                                    }
                                }

                                table1.AddView(trow);
                            }
                            else
                            {
                                trow = rowExists;

                                for (i = 0; i < iCols; i++)
                                {
                                    iCellType = 1; //This is dynamic from the database but default should be 1
                                    iCellId = iRowId + (i + 1);
                                    View vwExists = (View)FindViewById(iCellId);
                                    if (vwExists == null)
                                    {
                                        if (j == 0)
                                        {
                                            //Find the top row and add in the coumn header
                                        }

                                        View vw = GetCellView(iCellType, j, i, iSectionTypeId, bSetGridLines, iCols, (int)SectionType.GridItem, 0, null);
                                        if (vw != null)
                                        {
                                            trow.AddView(vw);
                                        }
                                    }
                                    else
                                    {
                                        View innerview = (View)FindViewById(iCellId + 100);
                                        Java.Lang.Object tag = vwExists.GetTag(Resource.Integer.CellType);
                                        iInnerViewType = Convert.ToInt32(tag);
                                        switch (iInnerViewType)
                                        {
                                            case (int)ItemType.Label:
                                                TextView txtvw = (TextView)innerview;
                                                txtvw.SetWidth((iColWidth - ConvertPixelsToDp(iButtonWidthDp)));
                                                break;
                                        }
                                    }
                                }

                                //Now check to see if we need to remove columns
                                for (i = iCols; i < 99; i++)
                                {
                                    iCellId = iRowId + (i + 1);
                                    View vwExists = (View)FindViewById(iCellId);
                                    if (vwExists != null)
                                    {
                                        trow.RemoveView(vwExists);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                }

                            }
                        }//End loop over j
                    }//End loop over k

                    //this.RunOnUiThread(() =>
                    //{
                    //    progBarDetail.CloseProgressBar();
                    //});

                    //Now see if we have to remove rows
                    for (j = iRows; j < 99; j++)
                    {
                        //First see if the row exists
                        iRowId = iBaseId + ((j + 1) * 100000);
                        TableRow rowExists2 = (TableRow)FindViewById(iRowId);

                        if (rowExists2 == null)
                        {
                            break;
                        }
                        else
                        {
                            table1.RemoveView(rowExists2);
                        }

                    }

                    if (tableExists == null)
                    {
                        tableContainer.AddView(table1, params1);
                    }

                    //Now build the page navigation bar if required
                    TableRow tableNavBarContainer = (TableRow)FindViewById(iDetailSectionNavBarContainerId);
                    if (bNavBarPage)
                    {
                        giNavBarsWidth = 0;

                        //TableRow rowNav = new TableRow(this_context);
                        //rowNav.SetBackgroundColor(Android.Graphics.Color.Wheat);
                        //rowNav.SetMinimumHeight(ConvertPixelsToDp(30f));
                        //rowNav.Id = iDetailSectionPageNavigationRowId;

                        //Put in another table so the widths can be different
                        TableLayout tableNav = new TableLayout(this_context);
                        tableNav.Id = iDetailSectionPageNavigationRowId;
                        TableRow.LayoutParams params2 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
                        params2.SetMargins(0, 0, 0, 0);

//                        params2.Span = iCols;

                        TableRow.LayoutParams params3 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
                        params3.SetMargins(ConvertPixelsToDp(2), ConvertPixelsToDp(1), ConvertPixelsToDp(2), ConvertPixelsToDp(1));
                        params3.Gravity = GravityFlags.Center;

                        TableRow rowNav1 = new TableRow(this_context);
                        rowNav1.SetBackgroundColor(Android.Graphics.Color.Wheat);
                        rowNav1.SetMinimumHeight(ConvertPixelsToDp(30f));
                        rowNav1.Id = iDetailSectionPageNavigationRowId + 100;
//                        rowNav1.(ConvertPixelsToDp(12), 0, ConvertPixelsToDp(12), 0);

                        //Add 2 buttons
                        Button btnFirstPage = new Button(this_context);
                        btnFirstPage.Text = "<<";
                        btnFirstPage.Id = iNavPageFirstPageButtonId;
                        btnFirstPage.SetWidth(ConvertPixelsToDp(30));
                        btnFirstPage.SetHeight(ConvertPixelsToDp(30));
                        btnFirstPage.SetBackgroundColor(Android.Graphics.Color.Gray);
                        btnFirstPage.SetTextColor(Android.Graphics.Color.Black);
                        btnFirstPage.SetPadding(ConvertPixelsToDp(2), 0, ConvertPixelsToDp(2), 0);
                        btnFirstPage.Click += (sender, args) => { FirstPage(sender, args); }; ;

                        if (iPageNo == 1)
                        {
                            btnFirstPage.Enabled = false;
                        }
                        else
                        {
                            btnFirstPage.Enabled = true;
                        }
                        rowNav1.AddView(btnFirstPage, params3);
                        giNavBarsWidth += 30;

                        Button btnPrevPage = new Button(this_context);
                        btnPrevPage.Text = "<";
                        btnPrevPage.Id = iNavPagePrevPageButtonId;
                        btnPrevPage.SetWidth(ConvertPixelsToDp(30));
                        btnPrevPage.SetHeight(ConvertPixelsToDp(30));
                        btnPrevPage.SetBackgroundColor(Android.Graphics.Color.Gray);
                        btnPrevPage.SetTextColor(Android.Graphics.Color.Black);
                        btnPrevPage.SetPadding(ConvertPixelsToDp(2), 0, ConvertPixelsToDp(2), 0);
                        btnPrevPage.Click += (sender, args) => { PrevPage(sender, args); }; ;

                        if (iPageNo == 1)
                        {
                            btnPrevPage.Enabled = false;
                        }
                        else
                        {
                            btnPrevPage.Enabled = true;
                        }
                        rowNav1.AddView(btnPrevPage, params3);
                        giNavBarsWidth += 30;

                        TextView lblPage = new TextView(this_context);
                        lblPage.SetPadding(ConvertPixelsToDp(2), ConvertPixelsToDp(0), ConvertPixelsToDp(2), ConvertPixelsToDp(0));
                        lblPage.Text = "Page";
                        lblPage.Id = iNavPageLabelId;
                        lblPage.SetWidth(ConvertPixelsToDp(60));
                        lblPage.SetHeight(ConvertPixelsToDp(30));
                        lblPage.SetTextColor(Android.Graphics.Color.Black);
                        lblPage.SetBackgroundColor(Android.Graphics.Color.Wheat);
                        rowNav1.AddView(lblPage, params3);
                        giNavBarsWidth += 60;

                        EditText txtEditPageNo = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
                        txtEditPageNo.Text = iPageNo.ToString();
                        txtEditPageNo.SetWidth(ConvertPixelsToDp(60));
                        txtEditPageNo.Id = iNavPageNoEditId;
                        txtEditPageNo.SetPadding(ConvertPixelsToDp(2), ConvertPixelsToDp(1), ConvertPixelsToDp(2), ConvertPixelsToDp(1));
                        txtEditPageNo.LayoutParameters = params3;
                        txtEditPageNo.SetHeight(ConvertPixelsToDp(28));
                        txtEditPageNo.SetSingleLine(true);
                        rowNav1.AddView(txtEditPageNo, params3);
                        giNavBarsWidth += 60;

                        TextView lblTotalPage = new TextView(this_context);
                        lblTotalPage.SetPadding(ConvertPixelsToDp(2), ConvertPixelsToDp(0), ConvertPixelsToDp(2), ConvertPixelsToDp(0));
                        lblTotalPage.Text = "of " + iTotalPages.ToString();
                        lblTotalPage.Id = iNavPageLabelTotalPagesId;
                        lblTotalPage.SetWidth(ConvertPixelsToDp(60));
                        lblTotalPage.SetHeight(ConvertPixelsToDp(30));
                        lblTotalPage.SetTextColor(Android.Graphics.Color.Black);
                        lblTotalPage.SetBackgroundColor(Android.Graphics.Color.Wheat);
                        rowNav1.AddView(lblTotalPage, params3);
                        giNavBarsWidth += 60;

                        Button btnGoToPage = new Button(this_context);
                        btnGoToPage.Text = "Go To Page";
                        btnGoToPage.Id = iNavPageGoToPageButtonId;
                        btnGoToPage.SetWidth(ConvertPixelsToDp(60));
                        btnGoToPage.SetHeight(ConvertPixelsToDp(30));
                        btnGoToPage.SetBackgroundColor(Android.Graphics.Color.Gray);
                        btnGoToPage.SetTextColor(Android.Graphics.Color.Black);
                        btnGoToPage.SetPadding(ConvertPixelsToDp(2), 0, ConvertPixelsToDp(2), 0);
                        btnGoToPage.Click += (sender, args) => { GoToPage(sender, args); }; ;
                        rowNav1.AddView(btnGoToPage, params3);
                        giNavBarsWidth += 60;

                        Button btnNextPage = new Button(this_context);
                        btnNextPage.Text = ">";
                        btnNextPage.Id = iNavPageNextPageButtonId;
                        btnNextPage.SetWidth(ConvertPixelsToDp(30));
                        btnNextPage.SetHeight(ConvertPixelsToDp(30));
                        btnNextPage.SetBackgroundColor(Android.Graphics.Color.Gray);
                        btnNextPage.SetTextColor(Android.Graphics.Color.Black);
                        btnNextPage.SetPadding(ConvertPixelsToDp(2), 0, ConvertPixelsToDp(2), 0);
                        btnNextPage.Click += (sender, args) => { NextPage(sender, args); }; ;

                        if (iPageNo == iTotalPages)
                        {
                            btnNextPage.Enabled = false;
                        }
                        else
                        {
                            btnNextPage.Enabled = true;
                        }
                        rowNav1.AddView(btnNextPage, params3);
                        giNavBarsWidth += 30;

                        Button btnLastPage = new Button(this_context);
                        btnLastPage.Text = ">>";
                        btnLastPage.Id = iNavPageLastPageButtonId;
                        btnLastPage.SetWidth(ConvertPixelsToDp(30));
                        btnLastPage.SetHeight(ConvertPixelsToDp(30));
                        btnLastPage.SetBackgroundColor(Android.Graphics.Color.Gray);
                        btnLastPage.SetTextColor(Android.Graphics.Color.Black);
                        btnLastPage.SetPadding(ConvertPixelsToDp(2), 0, ConvertPixelsToDp(2), 0);
                        btnLastPage.Click += (sender, args) => { LastPage(sender, args); }; ;

                        if (iPageNo == iTotalPages)
                        {
                            btnLastPage.Enabled = false;
                        }
                        else
                        {
                            btnLastPage.Enabled = true;
                        }
                        rowNav1.AddView(btnLastPage, params3);
                        giNavBarsWidth += 30;

                        tableNav.AddView(rowNav1);
                        tableNavBarContainer.AddView(tableNav, params2);
//                        table1.AddView(rowNav);
                    }
                    else
                    {
                        TableLayout parent = (TableLayout)tableNavBarContainer.Parent;
                        parent.RemoveView(tableNavBarContainer);
                    }

                    //Now build the Record navigation bar if required
                    TableRow tableNavBarContainer2 = (TableRow)FindViewById(iDetailSectionNavBarContainerId2);
                    if (bNavBarRecord)
                    {
                        giNavBarsWidth = 0;
                        //TableRow rowNavRec = new TableRow(this_context);
                        //rowNavRec.SetBackgroundColor(Android.Graphics.Color.Wheat);
                        //rowNavRec.SetMinimumHeight(ConvertPixelsToDp(30f));
                        //rowNavRec.Id = iDetailSectionRecordNavigationRowId;

                        //Put in another table so the widths can be different
                        TableLayout tableNavRec = new TableLayout(this_context);
                        tableNavRec.Id = iDetailSectionRecordNavigationRowId;
                        TableRow.LayoutParams paramsRec2 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
                        paramsRec2.SetMargins(0, 0, 0, 0);
                        //                        paramsRec2.Span = iCols;

                        TableRow.LayoutParams paramsRec3 = new TableRow.LayoutParams(TableRow.LayoutParams.FillParent, TableRow.LayoutParams.WrapContent);
                        paramsRec3.SetMargins(ConvertPixelsToDp(2), ConvertPixelsToDp(1), ConvertPixelsToDp(2), ConvertPixelsToDp(1));
                        paramsRec3.Gravity = GravityFlags.Center;

                        TableRow rowNav1Rec = new TableRow(this_context);
                        rowNav1Rec.SetBackgroundColor(Android.Graphics.Color.Wheat);
                        rowNav1Rec.SetMinimumHeight(ConvertPixelsToDp(30f));
                        rowNav1Rec.Id = iDetailSectionRecordNavigationRowId + 100;
                        //                        rowNav1.(ConvertPixelsToDp(12), 0, ConvertPixelsToDp(12), 0);

                        //Add 2 buttons
                        Button btnFirstRecord = new Button(this_context);
                        btnFirstRecord.Text = "<<";
                        btnFirstRecord.Id = iNavRecordFirstRecordButtonId;
                        btnFirstRecord.SetWidth(ConvertPixelsToDp(30));
                        btnFirstRecord.SetHeight(ConvertPixelsToDp(30));
                        btnFirstRecord.SetBackgroundColor(Android.Graphics.Color.Gray);
                        btnFirstRecord.SetTextColor(Android.Graphics.Color.Black);
                        btnFirstRecord.SetPadding(ConvertPixelsToDp(2), 0, ConvertPixelsToDp(2), 0);
                        btnFirstRecord.Click += (sender, args) => { FirstRecord(sender, args); }; ;
                        if (iRecordNo == 1)
                        {
                            btnFirstRecord.Enabled = false;
                        }
                        else
                        {
                            btnFirstRecord.Enabled = true;
                        }
                        rowNav1Rec.AddView(btnFirstRecord, paramsRec3);
                        giNavBarsWidth += 30;

                        Button btnPrevRecord = new Button(this_context);
                        btnPrevRecord.Text = "<";
                        btnPrevRecord.Id = iNavRecordPrevRecordButtonId;
                        btnPrevRecord.SetWidth(ConvertPixelsToDp(30));
                        btnPrevRecord.SetHeight(ConvertPixelsToDp(30));
                        btnPrevRecord.SetBackgroundColor(Android.Graphics.Color.Gray);
                        btnPrevRecord.SetTextColor(Android.Graphics.Color.Black);
                        btnPrevRecord.SetPadding(ConvertPixelsToDp(2), 0, ConvertPixelsToDp(2), 0);
                        btnPrevRecord.Click += (sender, args) => { PrevRecord(sender, args); }; ;
                        if (iRecordNo == 1)
                        {
                            btnPrevRecord.Enabled = false;
                        }
                        else
                        {
                            btnPrevRecord.Enabled = true;
                        }
                        rowNav1Rec.AddView(btnPrevRecord, paramsRec3);
                        giNavBarsWidth += 30;

                        TextView lblRecord = new TextView(this_context);
                        lblRecord.SetPadding(ConvertPixelsToDp(2), ConvertPixelsToDp(0), ConvertPixelsToDp(2), ConvertPixelsToDp(0));
                        lblRecord.Text = "Record";
                        lblRecord.Id = iNavRecordLabelId;
                        lblRecord.SetWidth(ConvertPixelsToDp(60));
                        lblRecord.SetHeight(ConvertPixelsToDp(30));
                        lblRecord.SetTextColor(Android.Graphics.Color.Black);
                        lblRecord.SetBackgroundColor(Android.Graphics.Color.Wheat);
                        rowNav1Rec.AddView(lblRecord, paramsRec3);
                        giNavBarsWidth += 60;

                        TextView lblRecordHidden = new TextView(this_context);
                        lblRecordHidden.Text = iRecordNo.ToString();
                        lblRecordHidden.Id = iNavRecordLabelHiddenId;
                        lblRecordHidden.Visibility = ViewStates.Gone;
                        rowNav1Rec.AddView(lblRecordHidden);

                        EditText txtEditRecordNo = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
                        txtEditRecordNo.Text = iRecordNo.ToString();
                        txtEditRecordNo.SetWidth(ConvertPixelsToDp(60));
                        txtEditRecordNo.Id = iNavRecordNoEditId;
                        txtEditRecordNo.SetPadding(ConvertPixelsToDp(2), ConvertPixelsToDp(1), ConvertPixelsToDp(2), ConvertPixelsToDp(1));
                        txtEditRecordNo.LayoutParameters = paramsRec3;
                        txtEditRecordNo.SetHeight(ConvertPixelsToDp(28));
                        txtEditRecordNo.SetSingleLine(true);
                        rowNav1Rec.AddView(txtEditRecordNo, paramsRec3);
                        giNavBarsWidth += 60;

                        TextView lblTotalRecord = new TextView(this_context);
                        lblTotalRecord.SetPadding(ConvertPixelsToDp(2), ConvertPixelsToDp(0), ConvertPixelsToDp(2), ConvertPixelsToDp(0));
                        lblTotalRecord.Text = "of " + iTotalRecords.ToString();
                        lblTotalRecord.Id = iNavRecordLabelTotalRecordsId;
                        lblTotalRecord.SetWidth(ConvertPixelsToDp(60));
                        lblTotalRecord.SetHeight(ConvertPixelsToDp(30));
                        lblTotalRecord.SetTextColor(Android.Graphics.Color.Black);
                        lblTotalRecord.SetBackgroundColor(Android.Graphics.Color.Wheat);
                        rowNav1Rec.AddView(lblTotalRecord, paramsRec3);
                        giNavBarsWidth += 60;

                        Button btnGoToRecord = new Button(this_context);
                        btnGoToRecord.Text = "Go To Record";
                        btnGoToRecord.Id = iNavRecordGoToRecordButtonId;
                        btnGoToRecord.SetWidth(ConvertPixelsToDp(60));
                        btnGoToRecord.SetHeight(ConvertPixelsToDp(30));
                        btnGoToRecord.SetBackgroundColor(Android.Graphics.Color.Gray);
                        btnGoToRecord.SetTextColor(Android.Graphics.Color.Black);
                        btnGoToRecord.SetPadding(ConvertPixelsToDp(2), 0, ConvertPixelsToDp(2), 0);
                        btnGoToRecord.Click += (sender, args) => { GoToRecord(sender, args); }; ;
                        rowNav1Rec.AddView(btnGoToRecord, paramsRec3);
                        giNavBarsWidth += 60;

                        Button btnNextRecord = new Button(this_context);
                        btnNextRecord.Text = ">";
                        btnNextRecord.Id = iNavRecordNextRecordButtonId;
                        btnNextRecord.SetWidth(ConvertPixelsToDp(30));
                        btnNextRecord.SetHeight(ConvertPixelsToDp(30));
                        btnNextRecord.SetBackgroundColor(Android.Graphics.Color.Gray);
                        btnNextRecord.SetTextColor(Android.Graphics.Color.Black);
                        btnNextRecord.SetPadding(ConvertPixelsToDp(2), 0, ConvertPixelsToDp(2), 0);
                        btnNextRecord.Click += (sender, args) => { NextRecord(sender, args); }; ;
                        if (iRecordNo == iTotalRecords)
                        {
                            btnNextRecord.Enabled = false;
                        }
                        else
                        {
                            btnNextRecord.Enabled = true;
                        }
                        rowNav1Rec.AddView(btnNextRecord, paramsRec3);
                        giNavBarsWidth += 30;

                        Button btnLastRecord = new Button(this_context);
                        btnLastRecord.Text = ">>";
                        btnLastRecord.Id = iNavRecordLastRecordButtonId;
                        btnLastRecord.SetWidth(ConvertPixelsToDp(30));
                        btnLastRecord.SetHeight(ConvertPixelsToDp(30));
                        btnLastRecord.SetBackgroundColor(Android.Graphics.Color.Gray);
                        btnLastRecord.SetTextColor(Android.Graphics.Color.Black);
                        btnLastRecord.SetPadding(ConvertPixelsToDp(2), 0, ConvertPixelsToDp(2), 0);
                        btnLastRecord.Click += (sender, args) => { LastRecord(sender, args); }; ;
                        if (iRecordNo == iTotalRecords)
                        {
                            btnLastRecord.Enabled = false;
                        }
                        else
                        {
                            btnLastRecord.Enabled = true;
                        }
                        rowNav1Rec.AddView(btnLastRecord, paramsRec3);
                        giNavBarsWidth += 30;

                        tableNavRec.AddView(rowNav1Rec);
                        tableNavBarContainer2.AddView(tableNavRec, paramsRec2);
                        //                        table1.AddView(rowNavRec);
                    }
                    else
                    {
                        TableLayout parent2 = (TableLayout)tableNavBarContainer2.Parent;
                        parent2.RemoveView(tableNavBarContainer2);
                    }

                }
            }
            catch (Exception ex)
                {
                    string sRtnMsg2 = ex.Message.ToString();
                }
        }

        public View GetCellView(int iCellType, int iRow, int iCol, int iSectionTypeId, bool bSetGridLines, int iTotalColumns, int iItemType, int iRecord, ArrayList arrThisRecordValues)
        {
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            clsLocalUtils utils = new clsLocalUtils();
            int iDefaultColWidth = giDefaultColWidth;
            int iCellId = -1;
            int iColWidth;
            int iRowId;
            int iBaseId = 0;
            int iGridlineWeight = 3;
            string sText = "";
            int iStandardGridItem = 1;
            int iLeftHandColWidth = giTopLeftGridItemWidth;
            int iTopRowHeight = ConvertPixelsToDp(70);
            string sRtnMsg = "";
            int iRowCellId = -1;
            int iRowItemId = -1;
            string sRowHeight = "";
            int iColCellId = -1;
            int iColItemId = -1;
            string sColWidth = "";
            int i;
            int k;
            int iBorderLeft = 0, iBorderTop = 0, iBorderRight = 0, iBorderBottom = 0;
            int iTextPaddingLeft = 0, iTextPaddingTop = 0, iTextPaddingRight = 0, iTextPaddingBottom = 0;
            int iColSpan = 1;
            int iColSectionId = -1;
            int iColWidthSpan = 0;
            int iMainSection = -1;
            int iColSpanBaseId = -1;
            string sValue = "";
            string sBoundColumn = "";
            string sTotalRows = "";
            int iTotalRows = 1;
            string sItalic = "Yes";
            string sBold = "No";

            if (iItemType == (int)SectionType.GridItem)
            {
                if (grdUtils.IsColumnSpanned(giFormId, iSectionTypeId, iRow + 1, iCol + 1, ref sRtnMsg))
                {
                    return null;
                }
            }

            
            switch (iSectionTypeId)
            {
                case (int)SectionType.Header:
                    iBaseId = iHeaderSectionTableId;
                    iRowCellId = iHeaderRowBaseId + ((iRow + 1) * 100000) + 1000; //The +1000 is because the row dialog/control sits in column 1. We use a different base Id though so it will not use the same id as the column 1 of the main grid
                    iRowItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.HeaderRow, iRowCellId, ref sRtnMsg);
                    sRowHeight =grdUtils.GetItemAttribute(giFormId, (int)SectionType.HeaderRow, iRowItemId, "Height", ref sRtnMsg);
                    iColCellId = iHeaderColumnBaseId + 100000 + (iCol + 1) * 1000; //The +100000 is because the column dialog/control sits in row 1. We use a different base Id though so it will not use the same id as the row 1 of the main grid
                    iColItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.HeaderColumn, iColCellId, ref sRtnMsg);
                    sColWidth = grdUtils.GetItemAttribute(giFormId, (int)SectionType.HeaderColumn, iColItemId, "Width", ref sRtnMsg);
                    iColSectionId = (int)SectionType.HeaderColumn;
                    iMainSection = (int)SectionType.Header;
                    iColSpanBaseId = iHeaderColumnBaseId;
                    sTotalRows = grdUtils.GetItemAttribute(giFormId, iSectionTypeId, -1, "Rows", ref sRtnMsg);
                    if (utils.IsNumeric(sTotalRows))
                    {
                        iTotalRows = Convert.ToInt32(sTotalRows);
                    }
                    break;
                case (int)SectionType.Detail:
                    iBaseId = iDetailSectionTableId;
                    iRowCellId = iDetailRowBaseId + ((iRow + 1) * 100000) + 1000; //The +1000 is because the row dialog/control sits in column 1. We use a different base Id though so it will not use the same id as the column 1 of the main grid
                    iRowItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.DetailRow, iRowCellId, ref sRtnMsg);
                    sRowHeight =grdUtils.GetItemAttribute(giFormId, (int)SectionType.DetailRow, iRowItemId, "Height", ref sRtnMsg);
                    iColCellId = iDetailColumnBaseId + 100000 + (iCol + 1) * 1000; //The +100000 is because the column dialog/control sits in row 1. We use a different base Id though so it will not use the same id as the row 1 of the main grid
                    iColItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.DetailColumn, iColCellId, ref sRtnMsg);
                    sColWidth = grdUtils.GetItemAttribute(giFormId, (int)SectionType.DetailColumn, iColItemId, "Width", ref sRtnMsg);
                    iColSectionId = (int)SectionType.DetailColumn;
                    iMainSection = (int)SectionType.Detail;
                    iColSpanBaseId = iDetailColumnBaseId;
                    sTotalRows = grdUtils.GetItemAttribute(giFormId, iSectionTypeId, -1, "Rows", ref sRtnMsg);
                    if (utils.IsNumeric(sTotalRows))
                    {
                        iTotalRows = Convert.ToInt32(sTotalRows);
                    }
                    break;
                case (int)SectionType.Footer:
                    iBaseId = iFooterSectionTableId;
                    iRowCellId = iFooterRowBaseId + ((iRow + 1) * 100000) + 1000; //The +1000 is because the row dialog/control sits in column 1. We use a different base Id though so it will not use the same id as the column 1 of the main grid
                    iRowItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.FooterRow, iRowCellId, ref sRtnMsg);
                    sRowHeight =grdUtils.GetItemAttribute(giFormId, (int)SectionType.FooterRow, iRowItemId, "Height", ref sRtnMsg);
                    iColCellId = iFooterColumnBaseId + 100000 + (iCol + 1) * 1000; //The +100000 is because the column dialog/control sits in row 1. We use a different base Id though so it will not use the same id as the row 1 of the main grid
                    iColItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.FooterColumn, iColCellId, ref sRtnMsg);
                    sColWidth = grdUtils.GetItemAttribute(giFormId, (int)SectionType.FooterColumn, iColItemId, "Width", ref sRtnMsg);
                    iColSectionId = (int)SectionType.FooterColumn;
                    iMainSection = (int)SectionType.Footer;
                    iColSpanBaseId = iFooterColumnBaseId;
                    sTotalRows = grdUtils.GetItemAttribute(giFormId, iSectionTypeId, -1, "Rows", ref sRtnMsg);
                    if (utils.IsNumeric(sTotalRows))
                    {
                        iTotalRows = Convert.ToInt32(sTotalRows);
                    }
                    break;
                case (int)SectionType.HeaderRow:
                    iBaseId = iHeaderRowBaseId;
                    sText = "R" + (iRow+1).ToString();
                    iStandardGridItem = 2;
                    iRowCellId = iHeaderRowBaseId + ((iRow + 1) * 100000) + 1000; //The +1000 is because the row dialog/control sits in column 1. We use a different base Id though so it will not use the same id as the column 1 of the main grid
                    iRowItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.HeaderRow, iRowCellId, ref sRtnMsg);
                    sRowHeight =grdUtils.GetItemAttribute(giFormId, (int)SectionType.HeaderRow, iRowItemId, "Height", ref sRtnMsg);
                    iMainSection = (int)SectionType.Header;
                    iColSpanBaseId = iHeaderColumnBaseId;
                    break;
                case (int)SectionType.HeaderColumn:
                    iBaseId = iHeaderColumnBaseId;
                    sText = "C" + (iCol+1).ToString();
                    iStandardGridItem = 3;
                    iColCellId = iHeaderColumnBaseId + 100000 + (iCol + 1) * 1000; //The +100000 is because the column dialog/control sits in row 1. We use a different base Id though so it will not use the same id as the row 1 of the main grid
                    iColItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.HeaderColumn, iColCellId, ref sRtnMsg);
                    sColWidth = grdUtils.GetItemAttribute(giFormId, (int)SectionType.HeaderColumn, iColItemId, "Width", ref sRtnMsg);
                    iColSectionId = (int)SectionType.HeaderColumn;
                    iMainSection = (int)SectionType.Header;
                    iColSpanBaseId = iHeaderColumnBaseId;
                    break;
                case (int)SectionType.DetailRow:
                    iBaseId = iDetailRowBaseId;
                    sText = "R" + (iRow + 1).ToString();
                    iStandardGridItem = 4;
                    iRowCellId = iDetailRowBaseId + ((iRow + 1) * 100000) + 1000; //The +1000 is because the row dialog/control sits in column 1. We use a different base Id though so it will not use the same id as the column 1 of the main grid
                    iRowItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.DetailRow, iRowCellId, ref sRtnMsg);
                    sRowHeight = grdUtils.GetItemAttribute(giFormId, (int)SectionType.DetailRow, iRowItemId, "Height", ref sRtnMsg);
                    iMainSection = (int)SectionType.Detail;
                    iColSpanBaseId = iDetailColumnBaseId;
                    break;
                case (int)SectionType.DetailColumn:
                    iBaseId = iDetailColumnBaseId;
                    sText = "C" + (iCol + 1).ToString();
                    iStandardGridItem = 5;
                    iColCellId = iDetailColumnBaseId + 100000 + (iCol + 1) * 1000; //The +100000 is because the column dialog/control sits in row 1. We use a different base Id though so it will not use the same id as the row 1 of the main grid
                    iColItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.DetailColumn, iColCellId, ref sRtnMsg);
                    sColWidth = grdUtils.GetItemAttribute(giFormId, (int)SectionType.DetailColumn, iColItemId, "Width", ref sRtnMsg);
                    iColSectionId = (int)SectionType.DetailColumn;
                    iMainSection = (int)SectionType.Detail;
                    iColSpanBaseId = iDetailColumnBaseId;
                    break;
                case (int)SectionType.FooterRow:
                    iBaseId = iFooterRowBaseId;
                    sText = "R" + (iRow + 1).ToString();
                    iStandardGridItem = 6;
                    iRowCellId = iFooterRowBaseId + ((iRow + 1) * 100000) + 1000; //The +1000 is because the row dialog/control sits in column 1. We use a different base Id though so it will not use the same id as the column 1 of the main grid
                    iRowItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.FooterRow, iRowCellId, ref sRtnMsg);
                    sRowHeight = grdUtils.GetItemAttribute(giFormId, (int)SectionType.FooterRow, iRowItemId, "Height", ref sRtnMsg);
                    iMainSection = (int)SectionType.Footer;
                    iColSpanBaseId = iFooterColumnBaseId;
                    break;
                case (int)SectionType.FooterColumn:
                    iBaseId = iFooterColumnBaseId;
                    sText = "C" + (iCol + 1).ToString();
                    iStandardGridItem = 7;
                    iColCellId = iFooterColumnBaseId + 100000 + (iCol + 1) * 1000; //The +100000 is because the column dialog/control sits in row 1. We use a different base Id though so it will not use the same id as the row 1 of the main grid
                    iColItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.FooterColumn, iColCellId, ref sRtnMsg);
                    sColWidth = grdUtils.GetItemAttribute(giFormId, (int)SectionType.FooterColumn, iColItemId, "Width", ref sRtnMsg);
                    iColSectionId = (int)SectionType.FooterColumn;
                    iMainSection = (int)SectionType.Footer;
                    iColSpanBaseId = iFooterColumnBaseId;
                    break;
            }

            if (iSectionTypeId < 0)
            {
                iMainSection = iSectionTypeId * -1;
            }
            //The gridlines are set at the header, detail or footer level, hence the -1 for the itemid
            string sGridLineColor = grdUtils.GetItemAttribute(giFormId, iMainSection, -1, "GridlineColor", ref sRtnMsg);
            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return null;
            }

            iRowId = iBaseId + ((iRow + 1) * 100000);

            if (giBuild == 1)
            {
                iTotalColumns = iTotalColumns + 1;
            }

            if (sColWidth == "")
            {
                iColWidth = GetScreenWidthPixels() / iTotalColumns;

                if (giBuild == 1)
                {
                    if (iColWidth < ConvertPixelsToDp(iDefaultColWidth))
                    {
                        iColWidth = ConvertPixelsToDp(iDefaultColWidth); //The default should be 150
                    }
                }
            }
            else
            {
                if (utils.IsNumeric(sColWidth))
                {
                   iColWidth = ConvertPixelsToDp(Convert.ToInt32(sColWidth));
                   if (giBuild == 1)
                   {
                       if (iColWidth < ConvertPixelsToDp(iDefaultColWidth))
                       {
                           iColWidth = ConvertPixelsToDp(iDefaultColWidth); //The default should be 150
                       }
                   }
                }
                else
                {
                    iColWidth = GetScreenWidthPixels() / iTotalColumns;

                    if (giBuild == 1)
                    {
                        if (iColWidth < ConvertPixelsToDp(iDefaultColWidth))
                        {
                            iColWidth = ConvertPixelsToDp(iDefaultColWidth); //The default should be 150
                        }
                    }
                }
            }


            iCellId = iRowId + (iCol + 1) * 1000 + iRecord;
            AndroidUtils.GridBuildView bv = new AndroidUtils.GridBuildView();
            bv.SetContext(this_context);
            bv.SetDensity(Resources.DisplayMetrics.Density);
            bv.SetGridLineWeight(iGridlineWeight);
            bv.SetGridLines(bSetGridLines);
            bv.SetId(iCellId);

            //Now get all the item attibutes if it is a grid item
            if (iItemType == (int)SectionType.GridItem)
            {
                ArrayList arrGridAttribs = grdUtils.GetGridItemAttributes(giFormId, iSectionTypeId, iRow + 1, iCol + 1);
                if (arrGridAttribs.Count >= 3)
                {
                    ArrayList arrItemType = (ArrayList)arrGridAttribs[0];
                    ArrayList arrId = (ArrayList)arrGridAttribs[1];
                    ArrayList arrParameterValue = (ArrayList)arrGridAttribs[2];
                    ArrayList arrParameterName = (ArrayList)arrGridAttribs[3];

                    iCellType = Convert.ToInt32(arrItemType[0]);

                    for (i = 0; i < arrId.Count; i++)
                    {
                        if (arrParameterName[i].ToString() == "Value")
                        {
                            sValue = arrParameterValue[i].ToString();
                        }

                        if (arrParameterName[i].ToString() == "BoundColumn")
                        {
                            sBoundColumn = arrParameterValue[i].ToString();
                        }

                        if (arrParameterName[i].ToString() == "BorderLeft")
                        {
                            iBorderLeft = Convert.ToInt32(arrParameterValue[i].ToString().Replace("px", ""));
                        }

                        if (arrParameterName[i].ToString() == "BorderRight")
                        {
                            iBorderRight = Convert.ToInt32(arrParameterValue[i].ToString().Replace("px", ""));
                        }

                        if (arrParameterName[i].ToString() == "BorderTop")
                        {
                            iBorderTop = Convert.ToInt32(arrParameterValue[i].ToString().Replace("px", ""));
                        }

                        if (arrParameterName[i].ToString() == "BorderBottom")
                        {
                            iBorderBottom = Convert.ToInt32(arrParameterValue[i].ToString().Replace("px", ""));
                        }

                        if (arrParameterName[i].ToString() == "ColumnSpan")
                        {
                            iColSpan = Convert.ToInt32(arrParameterValue[i].ToString());
                        }

                        if (arrParameterName[i].ToString() == "BackgroundColor")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                bv.SetBackgroundColor(arrParameterValue[i].ToString());
                            }
                        }

                        if (arrParameterName[i].ToString() == "BorderColor")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                bv.SetBorderColor(arrParameterValue[i].ToString());
                            }
                        }

                        if (arrParameterName[i].ToString() == "GridlineColor")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                bv.SetGridLinesColor(arrParameterValue[i].ToString());
                            }
                        }

                        if (arrParameterName[i].ToString() == "DropdownSQL")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                bv.SetDropdownSQL(arrParameterValue[i].ToString());
                            }
                        }

                        if (arrParameterName[i].ToString() == "ItemLabels")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                bv.SetRadioGroupLabels(arrParameterValue[i].ToString());
                            }
                        }

                        if (arrParameterName[i].ToString() == "ItemValues")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                bv.SetRadioGroupValues(arrParameterValue[i].ToString());
                            }
                        }

                        if (arrParameterName[i].ToString() == "TextAlign")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                bv.SetTextAlignment(arrParameterValue[i].ToString());
                            }
                        }

                        if (arrParameterName[i].ToString() == "TextVertAlignment")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                bv.SetTextVerticalAlignment(arrParameterValue[i].ToString());
                            }
                        }

                        if (arrParameterName[i].ToString() == "Font")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                bv.SetTextFont(arrParameterValue[i].ToString());
                            }
                        }

                        if (arrParameterName[i].ToString() == "Italic")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                sItalic = arrParameterValue[i].ToString();
                            }
                        }

                        if (arrParameterName[i].ToString() == "Bold")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                sBold = arrParameterValue[i].ToString();
                            }
                        }

                        if (arrParameterName[i].ToString() == "TextColor")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                bv.SetTextColor(arrParameterValue[i].ToString());
                            }
                        }

                        if (arrParameterName[i].ToString() == "FontSize")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                string sFontSize = arrParameterValue[i].ToString();
                                sFontSize = sFontSize.Replace("pt", "");
                                if (utils.IsNumeric(sFontSize))
                                {
                                    bv.SetTextSize(Convert.ToInt32(sFontSize));
                                }
                            }
                        }

                        if (arrParameterName[i].ToString() == "TextPaddingLeft")
                        {
                            iTextPaddingLeft = Convert.ToInt32(arrParameterValue[i].ToString().Replace("px", ""));
                        }

                        if (arrParameterName[i].ToString() == "TextPaddingRight")
                        {
                            iTextPaddingRight = Convert.ToInt32(arrParameterValue[i].ToString().Replace("px", ""));
                        }

                        if (arrParameterName[i].ToString() == "TextPaddingTop")
                        {
                            iTextPaddingTop = Convert.ToInt32(arrParameterValue[i].ToString().Replace("px", ""));
                        }

                        if (arrParameterName[i].ToString() == "TextPaddingBottom")
                        {
                            iTextPaddingBottom = Convert.ToInt32(arrParameterValue[i].ToString().Replace("px", ""));
                        }

                        if (arrParameterName[i].ToString() == "Orientation")
                        {
                            if (arrParameterValue[i].ToString() != "")
                            {
                                string sOrientation = arrParameterValue[i].ToString();
                                int iOrientation = 0;
                                if (sOrientation == "Vertical")
                                {
                                    iOrientation = 1;
                                }
                                bv.SetRadioGroupOrientation(iOrientation);
                            }
                        }

                    }

                    bv.SetCellPadding(iBorderLeft, iBorderTop, iBorderRight, iBorderBottom);
                    bv.SetTextPadding(iTextPaddingLeft, iTextPaddingTop, iTextPaddingRight, iTextPaddingBottom);
                    bv.SetTextStyle(sBold, sItalic);

                    //If we are spanning columns we have to get the full width
                    for (i = 1; i < iColSpan; i++)
                    {
                        iColCellId = iColSpanBaseId + 100000 + (i + 1) * 1000; //The +100000 is because the column dialog/control sits in row 1. We use a different base Id though so it will not use the same id as the row 1 of the main grid
                        iColItemId = grdUtils.GetGridItemId(giFormId, iColSectionId, iColCellId, ref sRtnMsg);
                        sColWidth = grdUtils.GetItemAttribute(giFormId, iColSectionId, iColItemId, "Width", ref sRtnMsg);

                        if (sColWidth == "")
                        {
                            iColWidthSpan = GetScreenWidthPixels() / iTotalColumns;

                            if (giBuild == 1)
                            {
                                if (iColWidthSpan < ConvertPixelsToDp(iDefaultColWidth))
                                {
                                    iColWidthSpan = ConvertPixelsToDp(iDefaultColWidth); //The default should be 150
                                }
                            }
                        }
                        else
                        {
                            if (utils.IsNumeric(sColWidth))
                            {
                                iColWidthSpan = ConvertPixelsToDp(Convert.ToInt32(sColWidth));
                                if (giBuild == 1)
                                {
                                    if (iColWidthSpan < ConvertPixelsToDp(iDefaultColWidth))
                                    {
                                        iColWidthSpan = ConvertPixelsToDp(iDefaultColWidth); //The default should be 150
                                    }
                                }
                            }
                            else
                            {
                                iColWidthSpan = GetScreenWidthPixels() / iTotalColumns;

                                if (giBuild == 1)
                                {
                                    if (iColWidthSpan < ConvertPixelsToDp(iDefaultColWidth))
                                    {
                                        iColWidthSpan = ConvertPixelsToDp(iDefaultColWidth); //The default should be 150
                                    }
                                }
                            }
                        }

                        iColWidth += iColWidthSpan;
                    }
                }
            }

            if (iItemType == (int)SectionType.GridItem)
            {
                if (sValue == "" && sBoundColumn != "" && sBoundColumn != "[select]")
                {
                    if (giBuild == 1)
                    {
                        sText = sBoundColumn;
                    }
                    else
                    {
                        //Get the bound column value for this SQL and this record
                        if (arrThisRecordValues != null)
                        {
                            for (k = 0; k < garrDBColumns.Count; k++)
                            {
                                if (garrDBColumns[k].ToString() == sBoundColumn)
                                {
                                    sText = arrThisRecordValues[k].ToString();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            sText = "Error with bound column";
                        }
                    }
                }
                else
                {
                    sText = sValue;
                }

            }

            //Set some more attributes
            bv.SetRowWidth(iColWidth);
            bv.SetText(sText);
            bv.SetRecordCounter(iRecord);

            if (sRowHeight != "")
            {
                if (utils.IsNumeric(sRowHeight))
                {
                    bv.SetRowHeight(Convert.ToInt32(sRowHeight)); 
                }
            }
            if (iRow < 0 && iCol < 0)
            {
                bv.SetBuildType(0);
            }
            else
            {
                bv.SetBuildType(giBuild);
            }

            //This is for the top left cell in the build only
            if(iRow < 0 && iCol < 0)
            {
                bv.SetRowWidth(iLeftHandColWidth);
                bv.SetRowHeight(iTopRowHeight);

            }

            //This is for the left hand side row cell which is only applicable in build mode
            if (iSectionTypeId == (int)SectionType.HeaderRow || iSectionTypeId == (int)SectionType.DetailRow || iSectionTypeId == (int)SectionType.FooterRow)
            {
                bv.SetRowWidth(iLeftHandColWidth);
                bv.SetTextAlignment("Center");
                //Also set the row color to the same as the gridlines color
                //Also set the text color to blue, bold and say 14pt
            }

            //This is for the top cell in each column which is only applicable in build mode
            if (iSectionTypeId == (int)SectionType.HeaderColumn || iSectionTypeId == (int)SectionType.DetailColumn || iSectionTypeId == (int)SectionType.FooterColumn)
            {
                if (giBuild == 1)
                {
                    bv.SetRowHeight(iTopRowHeight);
                }
                else
                {
                    bv.SetRowHeight(1);
                }
                bv.SetTextAlignment("Center");
                //Also set the row color to the same as the gridlines color
                //Also set the text color to blue, bold and say 14pt
            }

            bv.SetGridLinesColor(sGridLineColor);
//            bv.SetMainActivity(this);
            View vw = bv.GetCellView(iCellType);
            vw.Id = iCellId;
            vw.SetTag(Resource.String.CellReference, "R" + (iRow + 1) + "C" + (iCol+1));
            vw.SetTag(Resource.Integer.CellType, iCellType);
            vw.SetTag(Resource.Integer.CellSectionId, iSectionTypeId);

            TableRow.LayoutParams vwParams = new TableRow.LayoutParams();
            if (iTotalRows > 1 || (giBuild == 1))
            {
                vwParams.Span = iColSpan;
            }
            vw.LayoutParameters = vwParams;
        
            //Different modes when opening the detail dialog
            if (giBuild == 1 && iRow >= 0 && iCol >= 0)
            {
                Button btnDetail = bv.GetDetailButton();
                if (iStandardGridItem == 1)
                {
                    btnDetail.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.GridItem); };
                }

                if (iStandardGridItem == 2)
                {
                    btnDetail.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.HeaderRow); };
                }

                if (iStandardGridItem == 3)
                {
                    btnDetail.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.HeaderColumn); };
                }

                if (iStandardGridItem == 4)
                {
                    btnDetail.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.DetailRow); };
                }

                if (iStandardGridItem == 5)
                {
                    btnDetail.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.DetailColumn); };
                }

                if (iStandardGridItem == 6)
                {
                    btnDetail.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.FooterRow); };
                }

                if (iStandardGridItem == 7)
                {
                    btnDetail.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.FooterColumn); };
                }
            }

            //Get the main view and add a delegate
            if (giBuild == 0)
            {
                switch (iCellType)
                { 
                    case (int)ItemType.TextBox:
                    case (int)ItemType.TextArea:
                        EditText txtEdit = bv.GetCellEditTextView();
                        txtEdit.FocusChange += (sender, args) => { TextBoxFocusChanged(sender, args); };
//                        txtEdit.Touch += (sender, args) => { TextBoxFocusChanged(sender, args); };
                        break;
                    case (int)ItemType.DropDown:
                        Spinner cmbBox = bv.GetCellDropdownView();
                        cmbBox.ItemSelected += (senderItem, args) => { DropdownSelected(senderItem, args); };
                        cmbBox.Touch += (sender, args) => { DropDownFocusChanged(sender, args); };
                        break;
                    case (int)ItemType.RadioButton:
                        RadioGroup radGrp = bv.GetCellRadioGroupView();
                        for (int kk = 0; kk < radGrp.ChildCount; kk++)
                        {
                            RadioButton radBtn = (RadioButton)radGrp.GetChildAt(kk);
                            radBtn.Touch += (sender, args) => { RadioGroupFocusChanged(sender, args); };
                        }
//                        radGrp.SetOnTouchListener(this);
                        break;
                }
            }

            if (giBuild == 1)
            {
                switch (iCellType)
                {
                    case (int)ItemType.DropDown:
                        Spinner cmbBox = bv.GetCellDropdownView();
                        cmbBox.ItemSelected += (senderItem, args) => { DropdownSelected(senderItem, args); };
                        break;
                }
            }

            return vw;
        }

        public bool OnTouch(View vw, MotionEvent e)
        {
            clsLocalUtils utils = new clsLocalUtils();

            int iViewId = vw.Id;
            int iRCTextView = iViewId + 600;
            TextView txtRC = (TextView)FindViewById(iRCTextView);
            string sRC = txtRC.Text;
            if (utils.IsNumeric(sRC))
            {
                int iRecordNo = Convert.ToInt32(sRC);
                UpdateRecordCounterInfo(iRecordNo);
            }
            return true;
        }
        public void DropdownSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            AndroidUtils.ColorClass clsColor = new AndroidUtils.ColorClass();
            clsLocalUtils utils = new clsLocalUtils();
            string sRtnMsg = "";
            Spinner cmbBox = (Spinner)sender;
            TextView cmbMain = (TextView)cmbBox.GetChildAt(0);
            if (cmbMain != null)
            {
                //Now get the CellId
                int iSpinnerMainViewId = cmbBox.Id - 100;
                int iRC = GetRecordCounterFromCellId(iSpinnerMainViewId);
                int iItemId = grdUtils.GetGridItemId(giFormId, (int)SectionType.Detail, iSpinnerMainViewId - iRC + 1, ref sRtnMsg);
                string sBackgroundColor = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "BackgroundColor", ref sRtnMsg);
                Color clr = clsColor.GetColor(sBackgroundColor, (int)AndroidUtils.ColorType.Background);
                cmbMain.SetBackgroundColor(clr);
                cmbMain.SetSingleLine(true);
//                cmbMain.SetWidth(cmbBox.Width - 30);
                cmbMain.SetHeight(cmbBox.Height - 40); //This has to be dynamic
                string sTextColor = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "TextColor", ref sRtnMsg);
                Color clr2 = clsColor.GetColor(sTextColor, (int)AndroidUtils.ColorType.Text);
                cmbMain.SetTextColor(clr2);
                string sFont = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "Font", ref sRtnMsg);
                string sBold = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "Bold", ref sRtnMsg);
                string sItalic = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "Italic", ref sRtnMsg);
                string sTextsize = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "FontSize", ref sRtnMsg);
                sTextsize = sTextsize.Replace("pt", "");
                int iTextSize = 12;
                if (utils.IsNumeric(sTextsize))
                {
                    iTextSize = Convert.ToInt32(sTextsize);
                }

                AndroidUtils.TextTypeFaceClass typeface = new AndroidUtils.TextTypeFaceClass();
                Typeface  typFace = typeface.GetTypeface(sFont);
                TypefaceStyle typfaceStyle = typeface.GetTextStyle(sBold, sItalic);
                cmbMain.SetTypeface(typFace, typfaceStyle);
                cmbMain.SetTextSize(Android.Util.ComplexUnitType.Pt, iTextSize);
                string sAlign = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "TextAlign", ref sRtnMsg);
                string sVertAlign = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "TextVertAlignment", ref sRtnMsg);
                switch (sAlign)
                {
                    case "Left":
                        switch (sVertAlign)
                        {
                            case "Top":
                                cmbMain.Gravity = GravityFlags.Left | GravityFlags.Top;
                                break;
                            case "Center":
                                cmbMain.Gravity = GravityFlags.Left | GravityFlags.CenterVertical;
                                break;
                            case "Bottom":
                                cmbMain.Gravity = GravityFlags.Left | GravityFlags.Bottom;
                                break;
                        }
                        break;
                    case "Center":
                        switch (sVertAlign)
                        {
                            case "Top":
                                cmbMain.Gravity = GravityFlags.CenterHorizontal | GravityFlags.Top;
                                break;
                            case "Center":
                                cmbMain.Gravity = GravityFlags.CenterHorizontal | GravityFlags.CenterVertical;
                                break;
                            case "Bottom":
                                cmbMain.Gravity = GravityFlags.CenterHorizontal | GravityFlags.Bottom;
                                break;
                        }
                        break;
                    case "Right":
                        switch (sVertAlign)
                        {
                            case "Top":
                                cmbMain.Gravity = GravityFlags.Right | GravityFlags.Top;
                                break;
                            case "Center":
                                cmbMain.Gravity = GravityFlags.Right | GravityFlags.CenterVertical;
                                break;
                            case "Bottom":
                                cmbMain.Gravity = GravityFlags.Right | GravityFlags.Bottom;
                                break;
                        }
                        break;
                }
                string sLeft = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "TextPaddingLeft", ref sRtnMsg);
                string sTop = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "TextPaddingTop", ref sRtnMsg);
                string sRight = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "TextPaddingRight", ref sRtnMsg);
                string sBottom = grdUtils.GetItemAttribute(giFormId, (int)SectionType.GridItem, iItemId, "TextPaddingBottom", ref sRtnMsg);

                int iLeftPaddingText = 2;
                if (utils.IsNumeric(sLeft))
                {
                    iLeftPaddingText = Convert.ToInt32(sLeft);
                }

                int iTopPaddingText = 2;
                if (utils.IsNumeric(sTop))
                {
                    iTopPaddingText = Convert.ToInt32(sTop);
                }

                int iRightPaddingText = 2;
                if (utils.IsNumeric(sRight))
                {
                    iRightPaddingText = Convert.ToInt32(sRight);
                }

                int iBottomPaddingText = 2;
                if (utils.IsNumeric(sBottom))
                {
                    iBottomPaddingText = Convert.ToInt32(sBottom);
                }
                
                cmbMain.SetPadding(ConvertPixelsToDp(iLeftPaddingText), ConvertPixelsToDp(iTopPaddingText), ConvertPixelsToDp(iRightPaddingText), ConvertPixelsToDp(iBottomPaddingText));
            }
            return;
        }

        public int GetRecordCounterFromCellId(int iCellId)
        {
            clsLocalUtils utils = new clsLocalUtils();
            TextView txt = (TextView)FindViewById(iCellId + 700);
            string sRC = txt.Text;

            if (utils.IsNumeric(sRC))
            {
                return Convert.ToInt32(sRC);
            }
            else
            {
                return -1;
            }

        }

        //The item part is a 1 for the cell, 2 for the control, 3 for the next control .. (up to 8 controls) and then 9 for the button
        private int GetRowNoFromCellId(int iId, int iSectionId, int iItemPart)
        {
            int iRow = -1;
            int iBaseId = -1;

            switch (iSectionId)
            {
                case (int)SectionType.Header:
                    iBaseId = iHeaderSectionTableId;
                    break;
                case (int)SectionType.Detail:
                    iBaseId = iDetailSectionTableId;
                    break;
                case (int)SectionType.Footer:
                    iBaseId = iFooterSectionTableId;
                    break;
                case (int)SectionType.HeaderRow:
                    iBaseId = iHeaderRowBaseId;
                    break;
                case (int)SectionType.HeaderColumn:
                    iBaseId = iHeaderColumnBaseId;
                    break;
                case (int)SectionType.DetailRow:
                    iBaseId = iDetailRowBaseId;
                    break;
                case (int)SectionType.DetailColumn:
                    iBaseId = iDetailColumnBaseId;
                    break;
                case (int)SectionType.FooterRow:
                    iBaseId = iFooterRowBaseId;
                    break;
                case (int)SectionType.FooterColumn:
                    iBaseId = iFooterColumnBaseId;
                    break;
            }

            iRow = (iId - iBaseId - (100 * (iItemPart - 1))) / 100000;

            return iRow;
        }

        //The item part is a 1 for the cell, 2 for the control, 3 for the next control .. (up to 8 controls) and then 9 for the button
        private int GetColumnNoFromCellId(int iId, int iSectionId, int iItemPart)
        {
            int iRow = GetRowNoFromCellId(iId, iSectionId, iItemPart);
            int iBaseId = -1;
            int iCol = -1;

            switch (iSectionId)
            {
                case (int)SectionType.Header:
                    iBaseId = iHeaderSectionTableId;
                    break;
                case (int)SectionType.Detail:
                    iBaseId = iDetailSectionTableId;
                    break;
                case (int)SectionType.Footer:
                    iBaseId = iFooterSectionTableId;
                    break;
                case (int)SectionType.HeaderRow:
                    iBaseId = iHeaderRowBaseId;
                    break;
                case (int)SectionType.HeaderColumn:
                    iBaseId = iHeaderColumnBaseId;
                    break;
                case (int)SectionType.DetailRow:
                    iBaseId = iDetailRowBaseId;
                    break;
                case (int)SectionType.DetailColumn:
                    iBaseId = iDetailColumnBaseId;
                    break;
                case (int)SectionType.FooterRow:
                    iBaseId = iFooterRowBaseId;
                    break;
                case (int)SectionType.FooterColumn:
                    iBaseId = iFooterColumnBaseId;
                    break;
            }

            iCol = ((iId - iBaseId) - iRow * 100000 - (100 * (iItemPart -1)))/1000;

            return iCol;
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

        public override bool OnKeyDown(Android.Views.Keycode keyCode, Android.Views.KeyEvent e)
        {
            if (taskA == null)
            {
                return base.OnKeyDown(keyCode, e);
            }

            if (taskA.IsCompleted)
            {
                return base.OnKeyDown(keyCode, e);
            }
            else
            {
                return true;
            }
            //if (keyCode == Keycode.Back)
            //{
            //    AlertDialog ad = new AlertDialog.Builder(this).Create();
            //    ad.SetCancelable(false); // This blocks the 'BACK' button  
            //    ad.SetMessage("You have unsaved changes. Do you wish to go back and lose these changes");
            //    ad.SetButton("Yes", (s, ee) => { LeavePage(); });
            //    ad.SetButton2("No", (s, ee) => { StayOnPage(); });
            //    ad.Show();
            //    return true;
            //}
        }

        //All the event stuff
        public void TextBoxFocusChanged(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            EditText vw = (EditText)sender;
            int iViewId = vw.Id;

            //This is the OnGotFocus event
            if (vw.HasFocus)
            {
                //Check the row we are on
                int iRCTextView = iViewId + 600;
                TextView txtRC = (TextView)FindViewById(iRCTextView);
                string sRC = txtRC.Text;
                if (utils.IsNumeric(sRC))
                {
                    int iRecordNo = Convert.ToInt32(sRC);
                    UpdateRecordCounterInfo(iRecordNo);
                }
            }
            else
            {
                //Find if the value has changed from the original
                TextView vwOrig = (TextView)FindViewById(iViewId + 700);
            }
            return;
        }

        public void DropDownFocusChanged(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            Spinner vw = (Spinner)sender;
            int iViewId = vw.Id;
            int iRecordNo = 1;

            //Check the row we are on
            int iRCTextView = iViewId + 600;
            TextView txtRC = (TextView)FindViewById(iRCTextView);
            string sRC = txtRC.Text;
            if (utils.IsNumeric(sRC))
            {
                iRecordNo = Convert.ToInt32(sRC);
                UpdateRecordCounterInfo(iRecordNo);
            }

            vw.PerformClick();

            FocusFirstEditItemInRecord(iRecordNo, true);
            return;
        }

        public void RadioGroupFocusChanged(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            RadioButton vwChild = (RadioButton)sender;
            RadioGroup vw = (RadioGroup)vwChild.Parent;
            int iViewId = vw.Id;

            //Check the row we are on
            int iRCTextView = iViewId + 600;
            TextView txtRC = (TextView)FindViewById(iRCTextView);
            string sRC = txtRC.Text;
            if (utils.IsNumeric(sRC))
            {
                int iRecordNo = Convert.ToInt32(sRC);
                UpdateRecordCounterInfo(iRecordNo);
            }
            return;
        }

        public void FirstPage(object sender, EventArgs e)
        {
            OpenDetailPage(1, 1);
        }

        public void FirstRecord(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            EditText txtRecord = (EditText)FindViewById(iNavRecordNoEditId);
            string sRecordNo = txtRecord.Text;

            if (utils.IsNumeric(sRecordNo))
            {
                int iRecordNo = Convert.ToInt32(sRecordNo);
                if (iRecordNo > 0)
                {
                    int iPageNo = iRecordNo / giRecordsPerPage;
                    if (iRecordNo % giRecordsPerPage > 0.00001)
                    {
                        iPageNo = iPageNo + 1;
                    }

                    if (iPageNo > 1)
                    {
                        OpenDetailPage(1, 1);
                        FocusFirstEditItemInRecord(1, false);
                    }
                    else
                    {
                        FocusFirstEditItemInRecord(1, false);
                        if (giTotalRecords == 1)
                        {
                            Button btnNextRec = (Button)FindViewById(iNavRecordNextRecordButtonId);
                            btnNextRec.Enabled = false;
                            Button btnLastRec = (Button)FindViewById(iNavRecordLastRecordButtonId);
                            btnLastRec.Enabled = false;
                        }
                        else
                        {
                            Button btnNextRec = (Button)FindViewById(iNavRecordNextRecordButtonId);
                            btnNextRec.Enabled = true;
                            Button btnLastRec = (Button)FindViewById(iNavRecordLastRecordButtonId);
                            btnLastRec.Enabled = true;
                        }
                        Button btnPrevRec = (Button)FindViewById(iNavRecordPrevRecordButtonId);
                        btnPrevRec.Enabled = false;
                        Button btnFirstRec = (Button)FindViewById(iNavRecordFirstRecordButtonId);
                        btnFirstRec.Enabled = false;
                    }
                }
                else
                {
                    alert.SetAlertMessage("The record number cannot be found for record " + iRecordNo.ToString());
                    this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                    return;
                }
            }
            else
            {
                alert.SetAlertMessage("The record number cannot be found for record " + sRecordNo);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }
        }

        public void LastPage(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            TextView txtPage = (TextView)FindViewById(iNavPageLabelTotalPagesId);
            string sPageNo = txtPage.Text;
            sPageNo = sPageNo.Replace("of ", "").Trim();

            if (utils.IsNumeric(sPageNo))
            {
                int iPageNo = Convert.ToInt32(sPageNo);
                if (iPageNo > 0)
                {
                    int iRecordNo = 0;
                    iRecordNo = giTotalRecords - (iPageNo - 1) * giRecordsPerPage;
                    OpenDetailPage(iPageNo, iRecordNo);
                }
                else
                {
                    alert.SetAlertMessage("The page number cannot be found for page " + iPageNo.ToString());
                    this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                    return;
                }
            }
            else
            {
                alert.SetAlertMessage("The page number cannot be found for page " + sPageNo);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }
        }

        public void LastRecord(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            EditText txtRecord = (EditText)FindViewById(iNavRecordNoEditId);
            string sRecordNo = txtRecord.Text;

            if (utils.IsNumeric(sRecordNo))
            {
                int iRecordNo = Convert.ToInt32(sRecordNo);
                if (iRecordNo > 0)
                {
                    int iPageNo = iRecordNo / giRecordsPerPage;
                    if (iRecordNo % giRecordsPerPage > 0.00001)
                    {
                        iPageNo = iPageNo + 1;
                    }

                    int iLastPageNo = giTotalRecords / giRecordsPerPage;
                    if (giTotalRecords % giRecordsPerPage > 0.00001)
                    {
                        iLastPageNo = iLastPageNo + 1;
                    }

                    if (iPageNo < iLastPageNo)
                    {
                        OpenDetailPage(iLastPageNo, giTotalRecords);
                        FocusFirstEditItemInRecord(giTotalRecords, false);
                    }
                    else
                    {
                        FocusFirstEditItemInRecord(giTotalRecords, false);
                        Button btnNextRec = (Button)FindViewById(iNavRecordNextRecordButtonId);
                        btnNextRec.Enabled = false;
                        Button btnLastRec = (Button)FindViewById(iNavRecordLastRecordButtonId);
                        btnLastRec.Enabled = false;
                        if (giTotalRecords == 1)
                        {
                            Button btnPrevRec = (Button)FindViewById(iNavRecordPrevRecordButtonId);
                            btnPrevRec.Enabled = false;
                            Button btnFirstRec = (Button)FindViewById(iNavRecordFirstRecordButtonId);
                            btnFirstRec.Enabled = false;
                        }
                        else
                        {
                            Button btnPrevRec = (Button)FindViewById(iNavRecordPrevRecordButtonId);
                            btnPrevRec.Enabled = true;
                            Button btnFirstRec = (Button)FindViewById(iNavRecordFirstRecordButtonId);
                            btnFirstRec.Enabled = true;
                        }
                    }
                }
                else
                {
                    alert.SetAlertMessage("The record number cannot be found for record " + iRecordNo.ToString());
                    this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                    return;
                }
            }
            else
            {
                alert.SetAlertMessage("The record number cannot be found for record " + sRecordNo);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }
        }

        public void PrevPage(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            EditText txtPage = (EditText)FindViewById(iNavPageNoEditId);
            string sPageNo = txtPage.Text;

            if (utils.IsNumeric(sPageNo))
            {
                int iPageNo = Convert.ToInt32(sPageNo) - 1;
                if (iPageNo > 0)
                {
                    int iRecordNo = 0;
                    iRecordNo = giTotalRecords - (iPageNo - 1) * giRecordsPerPage;
                    OpenDetailPage(iPageNo, iRecordNo);
                }
                else
                {
                    alert.SetAlertMessage("The page number cannot be found for page " + iPageNo.ToString());
                    this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                    return;
                }
            }
            else
            {
                alert.SetAlertMessage("The page number cannot be found for page " + sPageNo);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }
        }

        public void PrevRecord(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            EditText txtRecord = (EditText)FindViewById(iNavRecordNoEditId);
            string sRecordNo = txtRecord.Text;

            if (utils.IsNumeric(sRecordNo))
            {
                int iRecordNo = Convert.ToInt32(sRecordNo) - 1;
                if (iRecordNo > 0)
                {
                    int iPageNo = iRecordNo/giRecordsPerPage;
                    //if (iRecordNo % giRecordsPerPage > 0.00001)
                    //{
                    //    iPageNo = iPageNo + 1;
                    //}

                    if (iRecordNo <= iPageNo * giRecordsPerPage)
                    {
                        OpenDetailPage(iPageNo, iRecordNo);
                        FocusFirstEditItemInRecord(iRecordNo, false);
                    }
                    else
                    {
                        FocusFirstEditItemInRecord(iRecordNo, false);
                        if (iRecordNo < giTotalRecords)
                        {
                            Button btnNextRec = (Button)FindViewById(iNavRecordNextRecordButtonId);
                            btnNextRec.Enabled = true;
                            Button btnLastRec = (Button)FindViewById(iNavRecordLastRecordButtonId);
                            btnLastRec.Enabled = true;
                        }
                        if (iRecordNo == 1)
                        {
                            Button btnPrevRec = (Button)FindViewById(iNavRecordPrevRecordButtonId);
                            btnPrevRec.Enabled = false;
                            Button btnFirstRec = (Button)FindViewById(iNavRecordFirstRecordButtonId);
                            btnFirstRec.Enabled = false;
                        }
                    }
                }
                else
                {
                    alert.SetAlertMessage("The record number cannot be found for record " + iRecordNo.ToString());
                    this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                    return;
                }
            }
            else
            {
                alert.SetAlertMessage("The record number cannot be found for record " + sRecordNo);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }
        }

        public void NextPage(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            EditText txtPage = (EditText)FindViewById(iNavPageNoEditId);
            string sPageNo = txtPage.Text;

            if (utils.IsNumeric(sPageNo))
            {
                int iPageNo = Convert.ToInt32(sPageNo) + 1;
                if (iPageNo > 0)
                {
                    int iRecordNo = 0;
                    iRecordNo = (iPageNo - 1) * giRecordsPerPage + 1;
                    OpenDetailPage(iPageNo, iRecordNo);
                }
                else
                {
                    alert.SetAlertMessage("The page number cannot be found for page " + iPageNo.ToString());
                    this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                    return;
                }
            }
            else
            {
                alert.SetAlertMessage("The page number cannot be found for page " + sPageNo);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }
        }

        public void NextRecord(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            EditText txtRecord = (EditText)FindViewById(iNavRecordNoEditId);
            string sRecordNo = txtRecord.Text;

            if (utils.IsNumeric(sRecordNo))
            {
                int iRecordNo = Convert.ToInt32(sRecordNo);
                int iPageNo = iRecordNo / giRecordsPerPage;
                if (iRecordNo % giRecordsPerPage > 0.00001)
                {
                    iPageNo = iPageNo + 1;
                }
                iRecordNo = iRecordNo + 1;
                if (iRecordNo > 0)
                {

                    if (iRecordNo > iPageNo * giRecordsPerPage)
                    {

                        OpenDetailPage(iPageNo + 1, iRecordNo);
                        FocusFirstEditItemInRecord(iRecordNo, false);
                    }
                    else
                    {
                        FocusFirstEditItemInRecord(iRecordNo, false);
                        if (iRecordNo == giTotalRecords)
                        {
                            Button btnNextRec = (Button)FindViewById(iNavRecordNextRecordButtonId);
                            btnNextRec.Enabled = false;
                            Button btnLastRec = (Button)FindViewById(iNavRecordLastRecordButtonId);
                            btnLastRec.Enabled = false;
                        }
                        if (iRecordNo > 1)
                        {
                            Button btnPrevRec = (Button)FindViewById(iNavRecordPrevRecordButtonId);
                            btnPrevRec.Enabled = true;
                            Button btnFirstRec = (Button)FindViewById(iNavRecordFirstRecordButtonId);
                            btnFirstRec.Enabled = true;
                        }
                    }
                }
                else
                {
                    alert.SetAlertMessage("The record number cannot be found for record " + iRecordNo.ToString());
                    this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                    return;
                }
            }
            else
            {
                alert.SetAlertMessage("The record number cannot be found for record " + sRecordNo);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }
        }

        public void GoToPage(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            EditText txtPage = (EditText)FindViewById(iNavPageNoEditId);
            string sPageNo = txtPage.Text;

            if(utils.IsNumeric(sPageNo))
            {
                int iPageNo = Convert.ToInt32(sPageNo);
                if (iPageNo > 0)
                {
                    int iRecordNo = 0;
                    iRecordNo = giTotalRecords - (iPageNo - 1) * giRecordsPerPage;
                    OpenDetailPage(iPageNo, iRecordNo);
                }
            }
            else
            {
                alert.SetAlertMessage("The page number selected must be a number");
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }
        }

        public void GoToRecord(object sender, EventArgs e)
        {
            clsLocalUtils utils = new clsLocalUtils();
            EditText txtRecord = (EditText)FindViewById(iNavRecordNoEditId);
            string sRecordNo = txtRecord.Text;
            TextView txtRecordOld = (TextView)FindViewById(iNavRecordLabelHiddenId);
            string sRecordOld = txtRecordOld.Text;
            int iRecordOld = -1;
            if (utils.IsNumeric(sRecordOld))
            {
                iRecordOld = Convert.ToInt32(sRecordOld);
            }
            if (utils.IsNumeric(sRecordNo))
            {
                int iRecordNo = Convert.ToInt32(sRecordNo);
                if (iRecordNo > 0)
                {
                    int iPageNo = iRecordNo / giRecordsPerPage;
                    if (iRecordNo % giRecordsPerPage > 0.00001)
                    {
                        iPageNo = iPageNo + 1;
                    }

                    int iExistingPageNo = iRecordOld / giRecordsPerPage;
                    if (iRecordOld % giRecordsPerPage > 0.00001)
                    {
                        iExistingPageNo = iExistingPageNo + 1;
                    }

                    if (iPageNo != iExistingPageNo)
                    {
                        OpenDetailPage(iPageNo, iRecordNo);
                        FocusFirstEditItemInRecord(iRecordNo, false);
                    }
                    else
                    {
                        FocusFirstEditItemInRecord(iRecordNo, false);
                        if (iRecordNo == giTotalRecords)
                        {
                            Button btnNextRec = (Button)FindViewById(iNavRecordNextRecordButtonId);
                            btnNextRec.Enabled = false;
                            Button btnLastRec = (Button)FindViewById(iNavRecordLastRecordButtonId);
                            btnLastRec.Enabled = false;
                        }
                        else
                        {
                            Button btnNextRec = (Button)FindViewById(iNavRecordNextRecordButtonId);
                            btnNextRec.Enabled = true;
                            Button btnLastRec = (Button)FindViewById(iNavRecordLastRecordButtonId);
                            btnLastRec.Enabled = true;
                        }

                        if (iRecordNo == 1)
                        {
                            Button btnPrevRec = (Button)FindViewById(iNavRecordPrevRecordButtonId);
                            btnPrevRec.Enabled = false;
                            Button btnFirstRec = (Button)FindViewById(iNavRecordFirstRecordButtonId);
                            btnFirstRec.Enabled = false;
                        }
                        else
                        {
                            Button btnPrevRec = (Button)FindViewById(iNavRecordPrevRecordButtonId);
                            btnPrevRec.Enabled = true;
                            Button btnFirstRec = (Button)FindViewById(iNavRecordFirstRecordButtonId);
                            btnFirstRec.Enabled = true;
                        }
                    }
                }
                else
                {
                    alert.SetAlertMessage("The record number cannot be found for record " + iRecordNo.ToString());
                    this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                    return;
                }
            }
            else
            {
                alert.SetAlertMessage("The record number cannot be found for record " + sRecordNo);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }
        }

        public void OpenDetailPage(int iPageNo, int iRecordNo)
        {

            //Also remove the nav bars
            TableRow tableContainerNavBar = (TableRow)FindViewById(iDetailSectionNavBarContainerId);
            TableLayout tableExistsNavBar = (TableLayout)FindViewById(iDetailSectionPageNavigationRowId);

            if (tableContainerNavBar != null)
            {
                tableContainerNavBar.RemoveView(tableExistsNavBar);
            }

            TableRow tableContainerNavBar2 = (TableRow)FindViewById(iDetailSectionNavBarContainerId2);
            TableLayout tableExistsNavBar2 = (TableLayout)FindViewById(iDetailSectionRecordNavigationRowId);

            if (tableContainerNavBar2 != null)
            {
                tableContainerNavBar2.RemoveView(tableExistsNavBar2);
            }

            //Find the main table in the detail section
            TableRow tableContainer = (TableRow)FindViewById(iDetailSectionContainerId);
            TableLayout tableExists = (TableLayout)FindViewById(iDetailSectionTableId);

            if (tableContainer != null)
            {
                tableContainer.RemoveView(tableExists);
                InsertTable((int)SectionType.Detail, iPageNo, iRecordNo);
            }


        }

        public void FocusFirstEditItemInRecord(int iRecordNo, bool bSuppressAlert)
        {
            int i;
            int j;
            int iCellId;
            bool bItemFound = false;

            for (i = 0; i < giDetailRows; i++)
            {
                for (j = 0; j < giDetailColumns; j++)
                {
                    iCellId = iDetailSectionTableId + ((i+1) * 100000) + ((j+1) * 1000) + iRecordNo + 100; //Here the +100 means the underlying control
                    View vw = (View)FindViewById(iCellId);
                    if (vw != null)
                    {
                        if (vw.Visibility == ViewStates.Visible && vw.Enabled && (vw.GetType().Name == "EditText"))
                        {
                            vw.RequestFocus();
                            bItemFound = true;
                            //if (vw.GetType().Name == "Spinner")
                            //{
                            //    vw.PerformClick();
                            //}
//                            View vwFocused = (View)mainView.FocusedChild;

                            break;
                        }
                    }
                }

                if (bItemFound)
                {
                    break;
                }
                else
                {
                    //Find any other focusable item if there is no EditText
                    for (j = 0; j < giDetailColumns; j++)
                    {
                        iCellId = iDetailSectionTableId + ((i + 1) * 100000) + ((j + 1) * 1000) + iRecordNo + 100; //Here the +100 means the underlying control
                        View vw = (View)FindViewById(iCellId);
                        if (vw != null)
                        {
                            if (vw.Visibility == ViewStates.Visible && vw.Enabled && (vw.GetType().Name == "EditText" || vw.GetType().Name == "Spinner" || vw.GetType().Name == "RadioGroup"))
                            {
                                vw.RequestFocus();
                                bItemFound = true;
                                View vwFocused = (View)mainView.FocusedChild;
                                break;
                            }
                        }
                    }

                    if (bItemFound)
                    {
                        //Make sure we break right out of the outer loop
                        break;
                    }
                }
            }

            if (!bItemFound && !bSuppressAlert)
            {
                alert.SetAlertMessage("There are no items in record " + iRecordNo + " that the system can focus. Choose another record please.");
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }
            else
            {
                if (giBuild == 0)
                {
                    UpdateRecordCounterInfo(iRecordNo);
                }
            }

            return;
        }


        public void UpdateRecordCounterInfo(int iRecordNo)
        {
            //Set the record counter to this record
            EditText txtEdit = (EditText)FindViewById(iNavRecordNoEditId);
            if (txtEdit != null)
            {
                txtEdit.Text = iRecordNo.ToString();
            }
            TextView txtEditOld = (TextView)FindViewById(iNavRecordLabelHiddenId);
            if (txtEditOld != null)
            {
                txtEditOld.Text = iRecordNo.ToString();
            }
        }

        public int GetWidthOfSectionInDp(int iSectionId)
        {
            int iWidth = 0;
            string sRtnMsg = "";
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            clsLocalUtils utils = new clsLocalUtils();
            int i;
            string sColWidth;
            int iColCellId;
            int iColItemId;
            int iColWidth;
            int iColSpanBaseId = -1;
            int iColSectionId = -1;
            int iTotalColsDiv = 1;

            if (giBuild == 1)
            {
                iWidth = giTopLeftGridItemWidth - giSectionButtonWidth - iOpenTestButtonWidth;
            }

            switch (iSectionId)
            {
                case (int)SectionType.Header:
                    iColSectionId = (int)SectionType.HeaderColumn;
                    iColSpanBaseId = iHeaderColumnBaseId;
                    break;
                case (int)SectionType.Detail:
                    iColSectionId = (int)SectionType.DetailColumn;
                    iColSpanBaseId = iDetailColumnBaseId;
                    break;
                case (int)SectionType.Footer:
                    iColSectionId = (int)SectionType.FooterColumn;
                    iColSpanBaseId = iFooterColumnBaseId;
                    break;
            }

            //The columns are set at the header, detail or footer level, hence the -1 for the itemid
            string sCols = grdUtils.GetItemAttribute(giFormId, iSectionId, -1, "Columns", ref sRtnMsg);
            if (sRtnMsg != "")
            {
                alert.SetAlertMessage(sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return -1;
            }

            int iTotalCols = 0;
            if (utils.IsNumeric(sCols))
            {
                iTotalCols = Convert.ToInt32(sCols);
            }
            

            if (giBuild == 1)
            {
                iTotalColsDiv = iTotalCols + 1;
            }
            else
            {
                iTotalColsDiv = iTotalCols;
            }


            for (i = 0; i < iTotalCols; i++)
            {
                iColCellId = iColSpanBaseId + 100000 + (i + 1) * 1000; //The +100000 is because the column dialog/control sits in row 1. We use a different base Id though so it will not use the same id as the row 1 of the main grid
                iColItemId = grdUtils.GetGridItemId(giFormId, iColSectionId, iColCellId, ref sRtnMsg);
                sColWidth = grdUtils.GetItemAttribute(giFormId, iColSectionId, iColItemId, "Width", ref sRtnMsg);
                
                if (sColWidth == "")
                {
                    iColWidth = GetScreenWidthPixels() / iTotalColsDiv;

                    if (giBuild == 1)
                    {
                        if (iColWidth < ConvertPixelsToDp(giDefaultColWidth))
                        {
                            iColWidth = ConvertPixelsToDp(giDefaultColWidth); //The default should be 150
                        }
                    }
                }
                else
                {
                    if (utils.IsNumeric(sColWidth))
                    {
                        iColWidth = ConvertPixelsToDp(Convert.ToInt32(sColWidth));
                        if (giBuild == 1)
                        {
                            if (iColWidth < ConvertPixelsToDp(giDefaultColWidth))
                            {
                                iColWidth = ConvertPixelsToDp(giDefaultColWidth); //The default should be 150
                            }
                        }
                    }
                    else
                    {
                        iColWidth = GetScreenWidthPixels() / iTotalColsDiv;

                        if (giBuild == 1)
                        {
                            if (iColWidth < ConvertPixelsToDp(giDefaultColWidth))
                            {
                                iColWidth = ConvertPixelsToDp(giDefaultColWidth); //The default should be 150
                            }
                        }
                    }
                }

                iWidth += iColWidth;
            }

            return iWidth;
        }

        public void SetHorizontalScrollWidth()
        {
            int iScreenWidth = GetScreenWidthPixels();
            int iWidthInDp= 0;
            int iWidthInDpHdr = GetWidthOfSectionInDp((int)SectionType.Header);
            int iWidthInDpDet = GetWidthOfSectionInDp((int)SectionType.Detail);
            int iWidthInDpFtr = GetWidthOfSectionInDp((int)SectionType.Footer);

            if (iWidthInDpHdr > iWidthInDp)
            {
                iWidthInDp = iWidthInDpHdr;
            }
            if (iWidthInDpDet > iWidthInDp)
            {
                iWidthInDp = iWidthInDpDet;
            }
            if (iWidthInDpFtr > iWidthInDp)
            {
                iWidthInDp = iWidthInDpFtr;
            }
            iWidthInDp += 10;

            if (iScreenWidth > iWidthInDp)
            {
                iWidthInDp = iScreenWidth;
            }

            if (giNavBarsWidth > iWidthInDp)
            {
                iWidthInDp = giNavBarsWidth;
            }
            
            TextView txtForm = (TextView)FindViewById(iFormLabelId);
            if (txtForm != null)
            {
                txtForm.SetWidth(iWidthInDp);
            }
            TextView txtHdr = (TextView)FindViewById(iHeaderLabelId);
            if (txtHdr != null)
            {
                txtHdr.SetWidth(iWidthInDp);
            }
            TextView txtDet = (TextView)FindViewById(iDetailLabelId);
            if (txtDet != null)
            {
                txtDet.SetWidth(iWidthInDp);
            }
            TextView txtFoot = (TextView)FindViewById(iFooterLabelId);
            if (txtFoot != null)
            {
                txtFoot.SetWidth(iWidthInDp);
            }
        }

        public void OpenTestPage(object sender, EventArgs e)
        {
            var bldScreen = new Intent(this, typeof(BuildScreen));
            bldScreen.PutExtra("BuildNew", 0);
            bldScreen.PutExtra("FormId", giFormId);
            bldScreen.SetFlags(ActivityFlags.NewTask | ActivityFlags.MultipleTask);
            this.StartActivity(bldScreen);
        }

        public bool Evaluate(object sender, EventArgs e, string sMethodName)
        {
            try
            {
                // Evaluate the current expression
                Eval eval = new Eval();
                bool bRtn = true;
//                List<string> tokens = eval.TokenizeExpression(sMethodName);
                //string testEval = "GetRecordId(1, 12, 19) + GetRecordId(1, 12, 19)";

//                FunctionEventArgs funce = new FunctionEventArgs();
//                btnForm.Click += (sender, args) => { OpenDetailDialog(sender, args, (int)SectionType.Form); }; ;
                //Get the info out of the method name
                sMethodName = ProcessMethodName(sender, sMethodName);
                eval.ProcessFunction += (send, funce) => {ProcessFunction(sender, funce); };
                double dblResult = eval.Execute(sMethodName);
                string sResult = eval.sResultAsString;
                return bRtn;
            }
            catch (EvalException ex)
            {
                alert.SetAlertMessage(ex.Message.ToString());
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return false;
            }
            catch (Exception ex)
            {
                alert.SetAlertMessage(ex.Message.ToString());
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return false;
            }

        }




        public string ProcessMethodName(object sender, string sMethodName)
        {
            string sRtnMethodName;
            sMethodName = Regex.Replace(sMethodName, @"\s+","");
            View vw = (View)sender;
            int iCellId;
            int iRowId;
            int iSectionId;

            //Firstly replace any "this" items with values
            sRtnMethodName = sMethodName;
            do
            {
                if (sRtnMethodName.Contains("this.row"))
                {
                    iCellId = vw.Id;
                    Java.Lang.Object tag3 = vw.GetTag(Resource.Integer.CellSectionId);
                    iSectionId = Convert.ToInt32(tag3);
                    iRowId = GetRowNoFromCellId(iCellId, iSectionId, 1);
                    sRtnMethodName = sRtnMethodName.Replace("this.row", iRowId.ToString());
                }
                else if (sRtnMethodName.Contains("this.column"))
                {
                }
            }
            while (sRtnMethodName.Contains("this."));

            //You could still have just "this" left over
            sRtnMethodName = sRtnMethodName.Replace("this", "");

            //If you have any () with nothing inside then the function is called without parameters
//            sRtnMethodName = sRtnMethodName.Replace("()", "");
            sRtnMethodName = sRtnMethodName.Replace(";", "");

            return sRtnMethodName;
        }

        public void InvokeStringMethod(string sMethodName, object[] objParams)
        {
            // Get MethodInfo.
            Type type = typeof(Methods);
            MethodInfo info = type.GetMethod(sMethodName);
            object result = null;
            Methods c = new Methods();

            if (info != null)
            {
                //The result should always be a string. Empty string means success otherwise show as an alert
                result = info.Invoke(null, objParams);

                if (result.ToString() != "")
                {
                    alert.SetAlertMessage(result.ToString());
                    this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                }
            }
            else
            {
                alert.SetAlertMessage("Method " + sMethodName + " does not exist.");
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });

            }

        }

        // Implement expression functions
        protected void ProcessFunction(object sender, FunctionEventArgs e)
        {
            Methods mths = new Methods();

            if (String.Compare(e.Name, "abs", true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    e.Result = Math.Abs(e.Parameters[0]);
                    e.ResultString = e.Result.ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "pow", true) == 0)
            {
                if (e.Parameters.Count == 2)
                {
                    e.Result = Math.Pow(e.Parameters[0], e.Parameters[1]);
                    e.ResultString = e.Result.ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "round", true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    e.Result = Math.Round(e.Parameters[0]);
                    e.ResultString = e.Result.ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "sqrt", true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    e.Result = Math.Sqrt(e.Parameters[0]);
                    e.ResultString = e.Result.ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "CheckColumnSpan", true) == 0)
            {
                e.Result = 0;
                e.ResultString = mths.CheckColumnSpan(giFormId, sender);
                if (e.ResultString != "")
                {
                    alert.SetAlertMessage(e.ResultString);
                    this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                    View vwCol = (View)sender;
                    if (vwCol != null)
                    {
                        SetToOldValue(vwCol.Id);
                        gbCloseDialog = false;
                        return;
                    }
                }
                else
                {
                    gbCloseDialog = true;
                }
            }
            else if (String.Compare(e.Name, "GetRecordId", true) == 0)
            {
                e.Result = mths.GetRecordId(Convert.ToInt32(e.ParametersString[0]), Convert.ToInt32(e.ParametersString[1]), Convert.ToInt32(e.ParametersString[2]));
                e.ResultString = mths.GetRecordId(Convert.ToInt32(e.ParametersString[0]), Convert.ToInt32(e.ParametersString[1]), Convert.ToInt32(e.ParametersString[2])).ToString();
            }
            // Unknown function name
            else e.Status = FunctionStatus.UndefinedFunction;

            return;
        }

        public string GetOldValue(int iCellId)
        {
            string sRtn = "";
            TextView txt = (TextView)FindViewById(iCellId + 100);
            if (txt != null)
            {
                sRtn = txt.Text;
            }

            return sRtn;
        }

        public void SetToOldValue(int iCellId)
        {
            View vw = (View)FindViewById(iCellId);

            switch(vw.GetType().Name)
            {
                case "EditText":
                default:
                    TextView txt = (TextView)vw;
                    txt.Text = GetOldValue(iCellId);
                    break;
            }

        }

    }

    public class Methods
    {
        //Each row add 100,000 and each column add 1,000 and each record add 1, using a base of 1 (NOT zero). The zero base is the table or the row or the column, with columns being a dummy row 100.
        //For example row 6 column 8 record 5 in section detail would be an Id of 20608005.
        //The table would be 20000000 for the detail section
        //The 6th row would be 20600000 in the detail section
        //The 8th column would be 20608000 in the detail section
        //The 5th record would be 20608005 in the detail section
        //That is the cell id. The control in the cell would be 20608105 (+100 for say the drop down box or textbox or radio button group)
        //The hidden field holding the old value before saving would be 20608805 (+800)
        //The button for adding the details would be 20608905 (+900) but of course this is only used in the build version of this screen
        int iHeaderSectionTableId = 20000000; //Allow 99 possible columns and 99 possible rows per section and 99 records. There are up to 10 items in each cell though. The cell, the underlying control and the button for the details/parameters
        int iDetailSectionTableId = 30000000; //Allow 99 possible columns and 99 possible rows per section. There are 3 items in each cell though. The cell, the underlying control and the button for the details/parameters
        int iFooterSectionTableId = 40000000; //Allow 99 possible columns and 99 possible rows per section. There are 3 items in each cell though. The cell, the underlying control and the button for the details/parameters

        public string CheckColumnSpan(int iFormId, object obj) //
        {
            int iColSpan = 1;
            string sRtnMsg = "";
            int iSection = 3;
            int iColumn = 1;
            int iCellId = -1;
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            clsLocalUtils utils = new clsLocalUtils();
            int iTotalCols = 99;
            int iColSectionId = 0;
            //Eval eval = new Eval();
            //string testEval = "abs(42 / 4.1 * 18.3)";

            ////eval.ProcessFunction += ProcessFunction;
            //double dblResult = eval.Execute(testEval);

            if (obj.GetType().Name == "EditText")
            {
                EditText vw = (EditText)obj;
                Java.Lang.Object tag3 = vw.GetTag(Resource.Integer.CellSectionId);
                iSection = Convert.ToInt32(tag3);
                Java.Lang.Object tag5 = vw.GetTag(Resource.Integer.CellColumnId);
                iColumn = Convert.ToInt32(tag5);
                Java.Lang.Object tag6 = vw.GetTag(Resource.Integer.CellId);
                iCellId = Convert.ToInt32(tag6);
                string sColSpan = vw.Text;

                if (utils.IsNumeric(sColSpan))
                {
                    iColSpan = Convert.ToInt32(sColSpan);
                }
            }

            //Get this column number
            string sCols = grdUtils.GetItemAttribute(iFormId, iSection, -1, "Columns", ref sRtnMsg);
            if (sRtnMsg != "")
            {
                return sRtnMsg;
            }

            if (utils.IsNumeric(sCols))
            {
                iTotalCols = Convert.ToInt32(sCols);
            }

            //Take off any hidden columns which are effectively automatically spanned
            switch (iSection)
            {
                case (int)SectionType.Header:
                    iColSectionId = (int)SectionType.HeaderColumn;
                    break;
                case (int)SectionType.Detail:
                    iColSectionId = (int)SectionType.DetailColumn;
                    break;
                case (int)SectionType.Footer:
                    iColSectionId = (int)SectionType.FooterColumn;
                    break;
            }
            int iHiddenCols = grdUtils.ColumnsHiddenInSection(iFormId, iColSectionId, ref sRtnMsg);
            if (sRtnMsg != "")
            {
                return sRtnMsg;
            }

            iTotalCols = iTotalCols - iHiddenCols;

            if (iColumn + iColSpan - 1 > iTotalCols)
            {
                if (iHiddenCols > 0)
                {
                    sRtnMsg = "You cannot span " + iColSpan + " columns from Colunm " + iColumn + ". There are only " + iTotalCols + " in the grid. Take into account that you have " + iHiddenCols + " hidden columns.";
                }
                else
                {
                    sRtnMsg = "You cannot span " + iColSpan + " columns from Colunm " + iColumn + ". There are only " + iTotalCols + " in the grid.";
                }
                return sRtnMsg;
            }

            return "";
        }

        public int GetRecordId(int iSection, int iRow, int iCol)
        {
            int iRecord = 144;
            return iRecord;
        }

        public int GetCellId(int iSection, int iRow, int iColumn)
        {
            return GetCellId(iSection, iRow, iColumn, 1);
        }

        public int GetCellId(int iSection, int iRow, int iColumn, int iRecord)
        {
            int iBaseId = 0;

            switch (iSection)
            {
                case (int)SectionType.Header:
                    iBaseId = iHeaderSectionTableId;
                    break;
                case (int)SectionType.Detail:
                    iBaseId = iDetailSectionTableId;
                    break;
                case (int)SectionType.Footer:
                    iBaseId = iFooterSectionTableId;
                    break;
                default:
                    iBaseId = -1;
                    break;
            }

            int iRowId = iBaseId + ((iRow + 1) * 100000);

            return iRowId + (iColumn + 1) * 1000 + iRecord;

        }
    }
}
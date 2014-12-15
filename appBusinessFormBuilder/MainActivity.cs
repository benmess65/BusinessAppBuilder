using System;
using System.Collections;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Net;
using Android.Telephony;

namespace appBusinessFormBuilder
{
    [Activity(Label = "appBusinessFormBuilder", MainLauncher = true, LaunchMode=Android.Content.PM.LaunchMode.SingleTask,Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        enum SectionType { Form = 1, Header, Detail, Footer, HeaderRow, HeaderColumn, DetailRow, DetailColumn, FooterRow, FooterColumn, GridItem };
        enum ItemType { Label = 1, TextBox, TextArea, DropDown, Checkbox, RadioButton, Button, DatePicker, TimePicker, Image, ColumnHeader, RowHeader, ColumnDetail, RowDetail, ColumnFooter, RowFooter };
        enum VersionType { Free = 0, Base, Pro, Premium };

        Context this_context;
        RelativeLayout mainView;
        HorizontalScrollView mainHSV;
        ScrollView mainSV;
        TableRow mainRow;
        LinearLayout llMain;
        AndroidUtils.AlertBox alert = new AndroidUtils.AlertBox();
        AndroidUtils.ProgressBar progBarDB = new AndroidUtils.ProgressBar();

        int giTesting = 0; //Turn to 1 when you want a test button for something, otherwsie leave at 0
        int giDebugging = 1; //Turn this to zero so that versioning works properly

        int iMainSVId = 100100;
        int iMainHSVId = 100100;
        int iExistingFormsTableId = 10;
        int giVersion = 0;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            progBarDB.SetContext(this);
            progBarDB.SetStyle(1);
            progBarDB.CreateProgressBar();

            // Set our view from the "main" layout resource
            this_context = this;
            SetContentView(Resource.Layout.Main);
            mainView = (RelativeLayout)FindViewById(Resource.Id.MainLayout);

        }

        protected override void OnResume()
        {
            base.OnResume();

            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();

            giVersion = grdUtils.GetVersion(GetDeviceId(this), giDebugging);

            mainRow = (TableRow)FindViewById(Resource.Id.tableRow1);

            RelativeLayout.LayoutParams paramsHSV = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.FillParent,RelativeLayout.LayoutParams.WrapContent);
            paramsHSV.SetMargins(0, ConvertPixelsToDp(110), 0, 0);
            
            HorizontalScrollView hsv = new HorizontalScrollView(this);
            mainView.AddView(hsv, paramsHSV);
            mainHSV = hsv;
            hsv.Id = iMainHSVId;
            ScrollView sv = new ScrollView(this);
            sv = DrawOpeningPage(this);
            sv.Id = iMainSVId;
            if (sv != null)
            {
                hsv.AddView(sv);
                mainSV = sv;
            }

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.btnCreateNewForm);
            button.Click +=  (send, args) => { viewBuildPage(send, args, 0); }; ;
            //delegate { button.Text = string.Format("{0} clicks!", count++); };

            //Put in a test button for stuuf
            if (giTesting == 1)
            {
                TableLayout.LayoutParams params2 = new TableLayout.LayoutParams();
                params2.Height = 300;

                TableLayout testTable = (TableLayout)FindViewById(Resource.Id.tableHomeLayout);
                //                testTable.SetMinimumHeight(ConvertPixelsToDp(300));
                testTable.LayoutParameters = params2;
                BuildTestArea();
            }
        }

        public ScrollView DrawOpeningPage(Android.Content.Context context)
        {
            try
            {
                clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
                int i;
                int iCounter;
                int iHeight = GetScreenHeightPixels() - ConvertPixelsToDp( 200);
                int iWidth = GetScreenWidthPixels();
                int iBoxWidth = 450;
                ScrollView sv = new ScrollView(context);
                ScrollView.LayoutParams paramsv = new ScrollView.LayoutParams(800, iHeight);
                LinearLayout layout = new LinearLayout(context);
                llMain = layout;

                //int iWidthPixels = GetScreenWidthPixels();
                //int iHeightPixels = GetScreenHeightPixels();
                //int iItemWidth = iWidthPixels / 2;

                layout.SetGravity(GravityFlags.CenterHorizontal);
                layout.Id = 1;
                int iPaddingMargin1 = ConvertPixelsToDp(5);
                int iPaddingMargin2 = ConvertPixelsToDp(1);
                int iTextHeight = 40;

                //Set up some default items for use anywhere
                //Create an alert dialog for use later on
                alert.SetContext(context);
                alert.CreateAlertDialog();

                //This simply sets spacing between each of the elements in the row
                TableRow.LayoutParams params2 = new TableRow.LayoutParams();
                params2.SetMargins(iPaddingMargin1, iPaddingMargin2, iPaddingMargin1, iPaddingMargin2);
                params2.Gravity = GravityFlags.CenterHorizontal; 
                params2.Span = 4;

                TableLayout table = new TableLayout(context);
                table.SetBackgroundColor(Android.Graphics.Color.DarkSlateGray);
                table.Id = iExistingFormsTableId;
                iBoxWidth = iWidth - (ConvertPixelsToDp(350));
                if (iBoxWidth > ConvertPixelsToDp(450))
                {
                    iBoxWidth = ConvertPixelsToDp(450);
                }

                if (iBoxWidth < ConvertPixelsToDp(150))
                {
                    //params2.Gravity = GravityFlags.Left;
                    table.SetGravity(GravityFlags.Left);
                    iBoxWidth = ConvertPixelsToDp(150);
                    paramsv.Gravity = GravityFlags.Left;
                    sv.LayoutParameters = paramsv;
                }
                else
                {
                    //params2.Gravity = GravityFlags.CenterHorizontal;
                    table.SetGravity(GravityFlags.CenterHorizontal);
                    paramsv.Gravity = GravityFlags.CenterHorizontal;
                    sv.LayoutParameters = paramsv;
                }

                TableRow rowHdr = new TableRow(context);
                rowHdr.SetBackgroundColor(Android.Graphics.Color.DarkSlateGray);
                rowHdr.SetMinimumHeight(ConvertPixelsToDp(40));

                TextView txtHdr = new TextView(context);
                txtHdr.Text = "Existing Forms";
                txtHdr.Id = 20;
                txtHdr.SetPadding(10, 1, 10, 1);
                txtHdr.LayoutParameters = params2;
                txtHdr.SetHeight(ConvertPixelsToDp(38));
                txtHdr.SetTextColor(Android.Graphics.Color.White);
                txtHdr.SetTextSize(Android.Util.ComplexUnitType.Pt, 12);
                txtHdr.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.BoldItalic);
                txtHdr.SetSingleLine(true);
                rowHdr.AddView(txtHdr);
                table.AddView(rowHdr);

                //The top row
                TableRow row1 = new TableRow(context);
                row1.SetBackgroundColor(Android.Graphics.Color.DarkSlateGray);
                row1.SetMinimumHeight(ConvertPixelsToDp(40));

                TextView txtName = new TextView(context);
                txtName.Text = "Name";
                txtName.SetWidth(ConvertPixelsToDp(150));
                txtName.Id = 30;
                txtName.SetPadding(10, 1, 10, 1);
                txtName.SetTextColor(Android.Graphics.Color.White);
                txtName.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                txtName.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Bold);
                txtName.SetHeight(ConvertPixelsToDp(38));
                row1.AddView(txtName);

                TextView txtDesc = new TextView(context);
                txtDesc.Text = "Description";
                txtDesc.SetWidth(iBoxWidth);
                txtDesc.Id = 40;
                txtDesc.SetPadding(10, 1, 10, 1);
                txtDesc.SetTextColor(Android.Graphics.Color.White);
                txtDesc.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                txtDesc.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Bold);
                txtDesc.SetHeight(ConvertPixelsToDp(38));
                row1.AddView(txtDesc);

                TextView txtButtonOpen = new TextView(context);
                txtButtonOpen.Text = "Open";
                txtButtonOpen.SetWidth(ConvertPixelsToDp(100));
                txtButtonOpen.Id = 50;
                txtButtonOpen.SetPadding(10, 1, 10, 1);
                txtButtonOpen.SetTextColor(Android.Graphics.Color.White);
                txtButtonOpen.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                txtButtonOpen.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Bold);
                txtButtonOpen.SetHeight(ConvertPixelsToDp(38));
                row1.AddView(txtButtonOpen);

                TextView txtButtonMore = new TextView(context);
                txtButtonMore.Text = "More";
                txtButtonMore.SetWidth(ConvertPixelsToDp(100));
                txtButtonMore.Id = 60;
                txtButtonMore.SetPadding(10, 1, 10, 1);
                txtButtonMore.SetTextColor(Android.Graphics.Color.White);
                txtButtonMore.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                txtButtonMore.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.Bold);
                txtButtonMore.SetHeight(ConvertPixelsToDp(38));
                row1.AddView(txtButtonMore);

                table.AddView(row1);

                //Now get all the existing forms
                ArrayList arrForms = grdUtils.GetAllForms();
                ArrayList arrId = new ArrayList();
                ArrayList arrNames = new ArrayList();
                ArrayList arrDesc = new ArrayList();
                if (arrForms.Count > 2)
                {
                    arrId = (ArrayList)arrForms[0];
                    arrNames = (ArrayList)arrForms[1];
                    arrDesc = (ArrayList)arrForms[2];
                }
                else
                {
                }

                if (giVersion == (int)VersionType.Free)
                {
                    if (arrId.Count >= 3)
                    {
                        iCounter = 3;
                        Button button = FindViewById<Button>(Resource.Id.btnCreateNewForm);
                        button.Enabled = false;
                    }
                    else
                    {
                        iCounter = arrId.Count;
                    }
                }
                else
                {
                    iCounter = arrId.Count;
                }

                for (i = 0; i < iCounter; i++)
                {
                    iTextHeight = (arrDesc[i].ToString().Length / (ConvertDpToPixels(iBoxWidth)/10)) * 40 + 20;

                    TableRow row = new TableRow(context);
                    row.SetBackgroundColor(Android.Graphics.Color.WhiteSmoke);
                    row.SetMinimumHeight(ConvertPixelsToDp(40));

                    TextView txtNameList = new TextView(context);
                    txtNameList.Text = arrNames[i].ToString();
                    txtNameList.SetWidth(ConvertPixelsToDp(150));
                    txtNameList.Id = 1000 + i + 1; //Allows for 1000 forms. Should be sufficient.
                    txtNameList.SetPadding(10, 1, 10, 1);
                    txtNameList.SetTextColor(Android.Graphics.Color.Black);
                    txtNameList.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
//                    txtNameList.SetHeight(ConvertPixelsToDp(38));
                    row.AddView(txtNameList);

                    TextView txtDescList = new TextView(context);
                    txtDescList.Text = arrDesc[i].ToString();
                    txtDescList.SetWidth(iBoxWidth);
                    txtDescList.Id = 2000 + i + 1;
                    txtDescList.SetPadding(10, 1, 10, 1);
                    txtDescList.SetTextColor(Android.Graphics.Color.Black);
                    txtDescList.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
                    txtDescList.SetHeight(ConvertPixelsToDp(iTextHeight));
                    txtDescList.SetSingleLine(false);
                    row.AddView(txtDescList);

                    TextView txtIdList = new TextView(context);
                    txtIdList.Text = arrId[i].ToString();
                    txtIdList.SetWidth(iBoxWidth);
                    txtIdList.Id = 3000 + i + 1;
                    txtIdList.Visibility = ViewStates.Gone;
                    row.AddView(txtIdList);

                    Button btnOpen = new Button(context);
                    btnOpen.Text = "Open";
                    btnOpen.Id = 4000 + i + 1;
                    btnOpen.SetWidth(ConvertPixelsToDp(80));
                    btnOpen.SetHeight(ConvertPixelsToDp(30));
                    btnOpen.Click += (sender, args) => { OpenForm(sender, args, 0); }; ;

                    row.AddView(btnOpen);

                    Button btnMore = new Button(context);
                    btnMore.Text = "More";
                    btnMore.Id = 5000 + i + 1;
                    btnMore.SetWidth(ConvertPixelsToDp(80));
                    btnMore.SetHeight(ConvertPixelsToDp(30));
                    btnMore.Click += (sender, args) => { OpenMore(sender, args); }; ;

                    row.AddView(btnMore);
                    table.AddView(row);
                }

                layout.AddView(table);


                sv.AddView(layout);
                return sv;
            }
            catch (Exception except)
            {
                Toast.MakeText(context, except.Message.ToString(), Android.Widget.ToastLength.Long);
                return null;
            }

        }



        public void OpenForm(object sender, EventArgs e, int iBuild)
        {
            Button btn = (Button)sender;
            int iFormIdTextViewId = -1;

            //Find out the Form Id
            if (iBuild == 1) //From button modify
            {
                iFormIdTextViewId = btn.Id - 5100 + 3000; //This is the Id for the textview that holds the form id
                int iId = btn.Id + 101 - 100; //This is the id of the relative layout
                RelativeLayout rl = (RelativeLayout)FindViewById(iId);
                mainView.RemoveView(rl);
                EnableAllButtons();
            }
            else //From button open
            {
                iFormIdTextViewId = btn.Id - 4000 + 3000; //This is the Id for the textview that holds the form id
            }

            TextView txtId = (TextView)FindViewById(iFormIdTextViewId);
            int iFormId = Convert.ToInt32(txtId.Text);

            var bldScreen = new Intent(this, typeof(BuildScreen));
            bldScreen.PutExtra("BuildNew", iBuild);
            bldScreen.PutExtra("FormId", iFormId);
            HorizontalScrollView sv = (HorizontalScrollView)FindViewById(iMainHSVId);
            mainView.RemoveView(sv);
            //this.RunOnUiThread(() =>
            //{
                progBarDB.SetProgressBarTitle("Building screen");
                progBarDB.ShowProgressBar(100);
            //});
            this.StartActivity(bldScreen);
            progBarDB.CloseProgressBar();
        }

        public void OpenMore(object sender, EventArgs e)
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

            RelativeLayout.LayoutParams params1 = new RelativeLayout.LayoutParams(ConvertPixelsToDp(350), ConvertPixelsToDp(200));
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
            txtButtonModify.Text = "Modify";
            txtButtonModify.SetWidth(ConvertPixelsToDp(100));
            txtButtonModify.Id = iId; //This is +100 from the base
            txtButtonModify.SetPadding(5, 1, 5, 1);
            txtButtonModify.LayoutParameters = params4;
            //txtButtonModify.SetTextColor(Android.Graphics.Color.Black);
            //txtButtonModify.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
            txtButtonModify.SetHeight(ConvertPixelsToDp(38));
            txtButtonModify.Click += (send, args) => { OpenForm(send, args, 1); }; ;
            row1.AddView(txtButtonModify);
            table.AddView(row1);

            TableRow row2 = new TableRow(this_context);
            row2.SetBackgroundColor(Android.Graphics.Color.Gray);
            row2.SetMinimumHeight(ConvertPixelsToDp(30));

            Button txtxButtonRemove = new Button(this_context);
            txtxButtonRemove.Text = "Remove";
            txtxButtonRemove.SetWidth(ConvertPixelsToDp(100));
            txtxButtonRemove.Id = iId   + 200; //This is +200 from the base
            txtxButtonRemove.SetPadding(5, 1, 5, 1);
            txtxButtonRemove.LayoutParameters = params4;
            //txtxButtonRemove.SetTextColor(Android.Graphics.Color.Black);
            //txtxButtonRemove.SetTextSize(Android.Util.ComplexUnitType.Pt, 10);
            txtxButtonRemove.SetHeight(ConvertPixelsToDp(38));
            //txtxButtonRemove.Click += (send, args) => { OpenForm(send, args, 1); }; ;
            row2.AddView(txtxButtonRemove);
            table.AddView(row2);

            TableRow row3 = new TableRow(this_context);
            row3.SetBackgroundColor(Android.Graphics.Color.Gray);
            row3.SetMinimumHeight(ConvertPixelsToDp(30));

            Button btnClose = new Button(this_context);
            btnClose.Text = "Close";
            btnClose.SetWidth(ConvertPixelsToDp(100));
            btnClose.Id = iId + 300; //This is 300 from the base
            btnClose.SetPadding(5, 1, 5, 1);
            btnClose.LayoutParameters = params4;
            btnClose.SetHeight(ConvertPixelsToDp(38));
            btnClose.Click += (send, args) => { ClosePopup(send, args); }; ;
            row3.AddView(btnClose);
            table.AddView(row3);

            sv.AddView(table);
            rl.AddView(sv);
            mainView.AddView(rl, params1);

            DisableAllButtons();
        }

        public void ClosePopup(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iId = btn.Id + 101 - 400; //This is the id of the relative layout
            RelativeLayout rl = (RelativeLayout)FindViewById(iId);
            mainView.RemoveView(rl);
            EnableAllButtons();
        }

        public void viewBuildPage(object sender, EventArgs e, int iBuild)
        {

            Intent newFormDlg = new Intent(this, typeof(NewFormDialog));
            newFormDlg.SetFlags(newFormDlg.Flags | ActivityFlags.NoHistory);
            newFormDlg.PutExtra("BuildNew", iBuild);
            HorizontalScrollView sv = (HorizontalScrollView)FindViewById(iMainHSVId);
            mainView.RemoveView(sv);
            this.StartActivity(newFormDlg);

        }

        private void BuildTestArea()
        {
            TableRow maindiv = (TableRow)FindViewById(Resource.Id.tableButtonRow3);
            maindiv.SetMinimumHeight(ConvertPixelsToDp(200));
            Button btnTest = new Button(this);
            btnTest = AddButton(this, -9999, "Test");
            btnTest.SetWidth(ConvertPixelsToDp(90));
            maindiv.AddView(btnTest);

            EditText txtEdit11 = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
            txtEdit11.Text = "Something";
            txtEdit11.SetWidth(200);
            txtEdit11.Id = 1;
            txtEdit11.SetPadding(10, 1, 10, 1);



            maindiv.AddView(txtEdit11);
        }

        public Button AddButton(Android.Content.Context context, int id, string btnText)
        {
            Button newBtn = new Button(context);
            newBtn.Text = btnText;
            newBtn.Id = id;

            newBtn.Click += ButtonClick;
            return newBtn;
        }

        public void ButtonClick(object sender, EventArgs e)
        {
            string sTest = "";
            Button button = (sender as Button);

            try
            {


                int iBtnId = button.Id;

                switch (iBtnId)
                {
                    //Exit the whole app
                    case -9999:
                        RunTest(this);
                        //                        System.Environment.Exit(0);
                        break;

                }
            }
            catch (Exception except)
            {
                sTest = except.Message.ToString();
            }
        }

        private void DisableAllButtons()
        {
            int i;
            int iRows;
            Button btn = (Button)FindViewById(Resource.Id.btnCreateNewForm);
            btn.Gravity = GravityFlags.CenterHorizontal;
            btn.Enabled = false;
            Button btn2 = (Button)FindViewById(Resource.Id.btnPreferences);
            btn2.Enabled = false;
            btn2.Gravity = GravityFlags.Center;

            //Now loop through the table os existing forms
            TableLayout table = (TableLayout)FindViewById(iExistingFormsTableId);
            if (table != null)
            {
                iRows = table.ChildCount;

                for (i = 1; i < iRows; i++)//The 1st row is a header
                {
                    int iId = 4000 + i;
                    Button btnOpen = (Button)FindViewById(iId);
                    if (btnOpen != null)
                    {
                        btnOpen.Enabled = false;
                    }
                    iId = 5000 + i;
                    Button btnMore = (Button)FindViewById(iId);
                    if (btnMore != null)
                    {
                        btnMore.Enabled = false;
                    }
                }
            }


        }

        private void EnableAllButtons()
        {
            int i;
            int iRows;
            Button btn = (Button)FindViewById(Resource.Id.btnCreateNewForm);
            btn.Gravity = GravityFlags.Center;
            btn.Enabled = true;
            Button btn2 = (Button)FindViewById(Resource.Id.btnPreferences);
            btn2.Enabled = true;
            btn2.Gravity = GravityFlags.Center;

            //Now loop through the table os existing forms
            TableLayout table = (TableLayout)FindViewById(iExistingFormsTableId);
            if (table != null)
            {
                iRows = table.ChildCount;

                for (i = 1; i < iRows; i++)//The 1st row is a header
                {
                    int iId = 4000 + i;
                    Button btnOpen = (Button)FindViewById(iId);
                    if (btnOpen != null)
                    {
                        btnOpen.Enabled = true;
                    }
                    iId = 5000 + i;
                    Button btnMore = (Button)FindViewById(iId);
                    if (btnMore != null)
                    {
                        btnMore.Enabled = true;
                    }
                }
            }


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

        public bool HasNetworkConnection()
        {
            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var activeConnection = connectivityManager.ActiveNetworkInfo;
            if ((activeConnection != null) && activeConnection.IsConnected)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public string GetDeviceId(Context context)
        {
            var telephonyManager = (TelephonyManager)context.GetSystemService(Android.Content.Context.TelephonyService);
            return telephonyManager.DeviceId;
        }

        private void RunTest(Android.Content.Context context)
        {
        }

    }
}


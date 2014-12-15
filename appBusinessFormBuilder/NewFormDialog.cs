using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace appBusinessFormBuilder
{
    [Activity(Label = "NewFormDialog")]
    public class NewFormDialog : Activity
    {
        RelativeLayout mainView;
        HorizontalScrollView mainHSV;
        ScrollView mainSV;
        LinearLayout llMain;
        AndroidUtils.AlertBox alert = new AndroidUtils.AlertBox();
        Task taskA;
        AndroidUtils.ProgressBar progBarDB = new AndroidUtils.ProgressBar();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            progBarDB.SetContext(this);
            progBarDB.SetStyle(1);
            progBarDB.CreateProgressBar();
            taskA = new Task(() => BuildBaseTableInfo());
            taskA.Start();

            SetContentView(Resource.Layout.layoutEmpty);
            mainView = (RelativeLayout)FindViewById(Resource.Id.EmptyLayout);

            //            FrameLayout v = new AndroidUtils.ZoomView(this);
            //            SetContentView(v);

            HorizontalScrollView hsv = new HorizontalScrollView(this);
            mainView.AddView(hsv);
            mainHSV = hsv;
            ScrollView sv = new ScrollView(this);
            sv = DrawOpeningPage(this);
            if (sv != null)
            {
                hsv.AddView(sv);
                mainSV = sv;
            }

        }

        public void BuildBaseTableInfo()
        {
            clsTabletDB.GridUtils gridDB = new clsTabletDB.GridUtils();
            this.RunOnUiThread(() =>
            {
                progBarDB.SetProgressBarTitle("Updating database");
                progBarDB.ShowProgressBar(100);
            });

            //Do some settingup first of all
            gridDB.CreateBaseTables();

            this.RunOnUiThread(() =>
            {
                progBarDB.CloseProgressBar();
            });
        }

        public ScrollView DrawOpeningPage(Android.Content.Context context)
        {
            try
            {
                ScrollView sv = new ScrollView(context);
                LinearLayout layout = new LinearLayout(context);
                llMain = layout;
                int iWidthPixels = GetScreenWidthPixels();
                int iHeightPixels = GetScreenHeightPixels();
                int iItemWidth = iWidthPixels / 2;

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
                params2.Span = 2;
                params2.Gravity = GravityFlags.CenterHorizontal;



                TableLayout table = new TableLayout(context);
                table.SetGravity(GravityFlags.CenterHorizontal);
                table.SetBackgroundColor(Android.Graphics.Color.WhiteSmoke);
                table.Id = 10;

                TableRow rowHdr = new TableRow(context);
                rowHdr.SetBackgroundColor(Android.Graphics.Color.Wheat);
                rowHdr.SetMinimumHeight(ConvertPixelsToDp(40));

                TextView txtHdr = new TextView(context);
                txtHdr.Text = "Create a New Form";
//                tyxtHdr.SetWidth(ConvertPixelsToDp(iItemWidth));
                txtHdr.Id = 20;
                txtHdr.SetPadding(10, 1, 10, 1);
                txtHdr.LayoutParameters = params2;
                txtHdr.SetHeight(ConvertPixelsToDp(38));
                txtHdr.SetTextColor(Android.Graphics.Color.Black);
                txtHdr.SetTextSize(Android.Util.ComplexUnitType.Pt, 14);
                txtHdr.SetTypeface(Android.Graphics.Typeface.SansSerif, Android.Graphics.TypefaceStyle.BoldItalic);
                txtHdr.SetSingleLine(true);
                rowHdr.AddView(txtHdr);
                table.AddView(rowHdr);

                //The top row
                TableRow row1 = new TableRow(context);
                row1.SetBackgroundColor(Android.Graphics.Color.Wheat);
                row1.SetMinimumHeight(ConvertPixelsToDp(40));

                TextView txtName = new TextView(context);
                txtName.Text = "Name";
                txtName.SetWidth(iItemWidth);
                txtName.Id = 30;
                txtName.SetPadding(10, 1, 10, 1);
                txtName.SetTextColor(Android.Graphics.Color.Black);
                txtName.SetTextSize(Android.Util.ComplexUnitType.Pt, 12);
                txtName.SetHeight(ConvertPixelsToDp(38));
                row1.AddView(txtName);

                EditText txtEdit1 = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
                txtEdit1.Text = "";
                txtEdit1.SetWidth((iItemWidth));
                txtEdit1.Id = 40;
                txtEdit1.SetTextColor(Android.Graphics.Color.Black);
                txtEdit1.SetTextSize(Android.Util.ComplexUnitType.Pt, 12);
                txtEdit1.SetPadding(10, 1, 10, 1);
                txtEdit1.SetHeight(ConvertPixelsToDp(38));
                txtEdit1.SetSingleLine(true);
                row1.AddView(txtEdit1);
                table.AddView(row1);

                //The nest row
                TableRow row2 = new TableRow(context);
                row2.SetBackgroundColor(Android.Graphics.Color.Wheat);
                row2.SetMinimumHeight(ConvertPixelsToDp(40));

                TextView txtDesc = new TextView(context);
                txtDesc.Text = "Description";
                txtDesc.SetWidth((iItemWidth));
                txtDesc.Id = 50;
                txtDesc.SetTextColor(Android.Graphics.Color.Black);
                txtDesc.SetTextSize(Android.Util.ComplexUnitType.Pt, 12);
                txtDesc.SetPadding(10, 1, 10, 1);
                txtDesc.SetHeight(ConvertPixelsToDp(38));
                row2.AddView(txtDesc);

                EditText txtEdit2 = (EditText)LayoutInflater.Inflate(Resource.Layout.textbox, null);
                txtEdit2.Text = "";
                txtEdit2.SetWidth((iItemWidth));
                txtEdit2.Id = 60;
                txtEdit2.SetTextColor(Android.Graphics.Color.Black);
                txtEdit2.SetTextSize(Android.Util.ComplexUnitType.Pt, 12);
                txtEdit2.SetPadding(10, 1, 10, 1);
//                txtEdit2.LayoutParameters = params2;
                txtEdit2.SetHeight(ConvertPixelsToDp(98));
                txtEdit2.SetSingleLine(false);
                row2.AddView(txtEdit2);
                table.AddView(row2);


                //The nest row
                TableRow row3 = new TableRow(context);
                row3.SetBackgroundColor(Android.Graphics.Color.Wheat);
                row3.SetMinimumHeight(ConvertPixelsToDp(40));

                Button btnForm = new Button(context);
                btnForm.Text = "Save";
                btnForm.SetWidth((iItemWidth/4 - 5));
                btnForm.SetHeight(ConvertPixelsToDp(30));
                btnForm.Click += (sender, args) => { SaveNewForm(sender, args); }; ;

                row3.AddView(btnForm);

                Button btnCancel = new Button(context);
                btnCancel.Text = "Cancel";
                btnCancel.SetWidth((iItemWidth / 4 - 5));
                btnCancel.SetHeight(ConvertPixelsToDp(30));
                btnCancel.Click += (sender, args) => { CancelNewForm(sender, args); }; ;

                row3.AddView(btnCancel);
                table.AddView(row3);

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

        public void SaveNewForm(object sender, EventArgs e)
        {
            int iFormId = -1;
            clsTabletDB.GridUtils grdUtils = new clsTabletDB.GridUtils();
            string sFormName;
            string sFormDescription;
            string sRtnMsg = "";

            //Save to the DB and get the ID back
            EditText txtFormName = (EditText)FindViewById(40);
            sFormName = txtFormName.Text;

            EditText txtFormDesc = (EditText)FindViewById(60);
            sFormDescription = txtFormDesc.Text;

            if (sFormName == "")
            {
                alert.SetAlertMessage("You must supply a name for the form. The maximum length is 50 characters.");
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            if (sFormName.Length > 50)
            {
                alert.SetAlertMessage("The maximum length for the form name is 50 characters. You have " + sFormName.Length.ToString() + " characters.");
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            if (sFormDescription.Length > 500)
            {
                alert.SetAlertMessage("The maximum length for the form description is 500 characters. You have " + sFormDescription.Length.ToString() + " characters.");
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            if(grdUtils.FormNameExists(iFormId, sFormName, ref sRtnMsg))
            {
                alert.SetAlertMessage("The form name " + sFormName + " already exists. Please choose another name.");
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }

            if (!grdUtils.SaveFormDetails(sFormName, sFormDescription, ref iFormId, ref sRtnMsg))
            {
                alert.SetAlertMessage("Failure saving form information. " + sRtnMsg);
                this.RunOnUiThread(() => { alert.ShowAlertBox(); });
                return;
            }
            else
            {
                var bldScreen = new Intent(this, typeof(BuildScreen));
                bldScreen.PutExtra("FormId", iFormId);
                bldScreen.PutExtra("BuildNew", 1);
                this.StartActivity(bldScreen);
                this.Finish();
                return;
            }


        }

        public void CancelNewForm(object sender, EventArgs e)
        {
            this.Finish();
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
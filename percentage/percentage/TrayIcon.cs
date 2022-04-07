using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace percentage
{
    class TrayIcon
    {
        private const int UPDATE_RATE = 5000;  //update rate in milliseconds

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        private const int fontSize = 22;
        private const string font = "Segoe UI";

        private bool showPercentage = true;     // if false instead of the percentage the remaining time is shown in
                                                // the format "hh:mm"
        private NotifyIcon notifyIcon;

        public TrayIcon()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();

            notifyIcon = new NotifyIcon();

            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem });

            menuItem.Click += new System.EventHandler(MenuItemClick);
            menuItem.Index = 0;
            menuItem.Text = "E&xit";

            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Visible = true;
            notifyIcon.Click += new System.EventHandler(NotifyIcon_Click);

            Timer timer = new Timer();
            timer.Interval = UPDATE_RATE;
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();
        }

        private void NotifyIcon_Click(object sender, EventArgs sysEvent)
        {
            showPercentage = !showPercentage;
        }

        private Bitmap GetTextBitmap(String text, Font font, Color fontColor)
        {
            SizeF imageSize = GetStringImageSize(text, font);

            // added by me to manipulate the size
            if (showPercentage)
            {
                imageSize.Width = 32; imageSize.Height = 32;
            }
            else
            {
                imageSize.Width = 55; imageSize.Height = 55;
            }


            Bitmap bitmap = new Bitmap((int)imageSize.Width, (int)imageSize.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                using (Brush brush = new SolidBrush(fontColor))
                {
                    if(showPercentage)
                        graphics.DrawString(text, font, brush, -6, -5);
                    else
                        graphics.DrawString(text, font, brush, -6, -14);
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    graphics.Save();
                }
            }
            return bitmap;
        }

        private static SizeF GetStringImageSize(string text, Font font)
        {
            using (Image image = new Bitmap(1, 1))
            using (Graphics graphics = Graphics.FromImage(image))
                return graphics.MeasureString(text, font);
        }

        private void MenuItemClick(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            String percentage = (powerStatus.BatteryLifePercent * 100).ToString();
            //bool isCharging = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;
            var batteryTotSecRemaining = powerStatus.BatteryLifeRemaining;
            var batteryTotMinutesRemaining = batteryTotSecRemaining / 60;
            var batteryTotHoursRemaining = batteryTotMinutesRemaining / 60;
            var batteryRelMinutesRemaining = batteryTotMinutesRemaining % 60;
            String timeToolTip =
                ((batteryTotHoursRemaining < 10) ? "0" : "") +
                batteryTotHoursRemaining.ToString() +
                ":" +
                ((batteryRelMinutesRemaining < 10) ? "0" : "") +
                batteryRelMinutesRemaining.ToString();
            String timeIcon = 
                ((batteryTotHoursRemaining < 10) ? "0" : "") +
                batteryTotHoursRemaining.ToString() +
                "\n" +
                ((batteryRelMinutesRemaining < 10) ? "0" : "") +
                batteryRelMinutesRemaining.ToString();

            String bitmapText = showPercentage ? percentage : timeIcon;

            Color textColor;
            if(powerStatus.BatteryLifePercent >= 0.4)
                textColor = Color.Lime;
            else if(powerStatus.BatteryLifePercent < 0.4 && powerStatus.BatteryLifePercent >= 0.3)
                textColor = Color.Yellow;
            else if(powerStatus.BatteryLifePercent < 0.3 && powerStatus.BatteryLifePercent >= 0.2)
                textColor = Color.Orange;
            else
                textColor = Color.Red;

            using (Bitmap bitmap = new Bitmap(GetTextBitmap(bitmapText, new Font(font, fontSize, FontStyle.Bold), textColor)))
            {
                System.IntPtr intPtr = bitmap.GetHicon();
                try
                {
                    // display the remaining battery time in the format "hh:mm"
                    using (Icon icon = Icon.FromHandle(intPtr))
                    {
                        notifyIcon.Icon = icon;
                        String toolTipText = showPercentage ? timeToolTip : percentage;
                        notifyIcon.Text = toolTipText;
                    }
                }
                finally
                {
                    DestroyIcon(intPtr);
                }
            }
        }
    }
}

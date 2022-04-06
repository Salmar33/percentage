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

            Timer timer = new Timer();
            timer.Interval = UPDATE_RATE;
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();
        }

        private Bitmap GetTextBitmap(String text, Font font, Color fontColor)
        {
            SizeF imageSize = GetStringImageSize(text, font);

            // added by me to manipulate the size
            imageSize.Width = 32; imageSize.Height = 32;

            Bitmap bitmap = new Bitmap((int)imageSize.Width, (int)imageSize.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                using (Brush brush = new SolidBrush(fontColor))
                {
                    graphics.DrawString(text, font, brush, -6, -5);
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
            bool isCharging = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;
            //String bitmapText = isCharging ? percentage + "*" : percentage;
            String bitmapText = percentage;
            Color textColor;

            if(powerStatus.BatteryLifePercent >= 0.4)
            {
                textColor = Color.Lime;
            }
            else if(powerStatus.BatteryLifePercent < 0.4 && powerStatus.BatteryLifePercent >= 0.3)
            {
                textColor = Color.Yellow;
            }
            else if(powerStatus.BatteryLifePercent < 0.3 && powerStatus.BatteryLifePercent >= 0.2)
            {
                textColor = Color.Orange;
            }
            else
            {
                textColor = Color.Red;
            }

            using (Bitmap bitmap = new Bitmap(GetTextBitmap(bitmapText, new Font(font, fontSize, FontStyle.Bold), textColor)))
            {
                System.IntPtr intPtr = bitmap.GetHicon();
                try
                {
                    using (Icon icon = Icon.FromHandle(intPtr))
                    {
                        notifyIcon.Icon = icon;
                        String toolTipText = percentage + "%" + (isCharging ? " Charging" : "");
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

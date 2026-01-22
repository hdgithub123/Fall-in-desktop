using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text;

namespace RealisticRainApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RainApplicationContext());
        }
    }



    public static class RainConfig
    {
        public static int SoLuong = 150;
        public static float DoDayMin = 0.5f;
        public static float DoDayMax = 1.2f;
        public static int TocDoMin = 25;
        public static int TocDoMax = 45;
        public static Color MauMua = Color.SkyBlue;
        public static int Alpha = 120;
        public static float Gio = 2.0f; // Gió cố định (độ nghiêng mặc định)

        public static int LengthMin = 10;
        public static int LengthMax = 25;


        // THÔNG SỐ GIÓ BIẾN THIÊN MẠNH
        public static float WindCurrent = 0f;      // Tốc độ + Hướng gió thực tế
        public static int WindMin = 0;             // Mức gió yếu nhất (thường là 0)
        public static int WindMax = 20;            // Mức gió mạnh nhất
        public static int WindChangeSpeed = 5;     // Tốc độ biến chuyển hướng gió

        public static float targetWind = 0f;
        private static Random windRand = new Random();

        public static bool EnableLightning = true;
        public static float LightningChance = 0.005f;

        public static int LightningFrequency = 5; // Mặc định là 5 (tương đương 0.005f)

        public static bool AlwaysOnTop = true;

        // Lưu tỉ lệ % (0 đến 100)
        public static int WindowThresholdW_Percent = 50;
        public static int WindowThresholdH_Percent = 50;


        public static void UpdateWindLogic()
        {
            // 1. KIỂM TRA ĐỔI HƯỚNG: Nếu gió hiện tại đã gần bằng mục tiêu
            if (Math.Abs(WindCurrent - targetWind) < 0.3f)
            {
                // Ngẫu nhiên hướng: 0 (trái), 1 (phải), 2 (lặng gió)
                int mode = windRand.Next(0, 3);

                if (mode == 0) // Thổi sang trái
                    targetWind = (float)windRand.Next(WindMin, WindMax + 1) * -1;
                else if (mode == 1) // Thổi sang phải
                    targetWind = (float)windRand.Next(WindMin, WindMax + 1);
                else // Lặng gió
                    targetWind = 0;
            }

            // 2. TÍNH TOÁN BƯỚC DI CHUYỂN (Interpolation)
            // Chia cho 200f để gió đổi hướng từ từ, không bị giật cục
            float step = (float)WindChangeSpeed / 200f;

            if (WindCurrent < targetWind)
            {
                WindCurrent += step;
                if (WindCurrent > targetWind) WindCurrent = targetWind;
            }
            else if (WindCurrent > targetWind)
            {
                WindCurrent -= step;
                if (WindCurrent < targetWind) WindCurrent = targetWind;
            }
        }

        public static int FPS = 24; // Mặc định 60

    }


    public class RainApplicationContext : ApplicationContext
    {
        private RainForm mainForm;
        private SettingsForm settingsForm;
        private NotifyIcon trayIcon;

        // Chuỗi Base64 đại diện cho icon (Thay thế bằng chuỗi của bạn)
        private string iconBase64 = "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";

        public RainApplicationContext()
        {
            mainForm = new RainForm();
            settingsForm = new SettingsForm();

            // Thiết lập Icon cho SettingsForm từ Base64
            try
            {
                settingsForm.Icon = GetIconFromBase64(iconBase64);
            }
            catch { }

            mainForm.Show();

            // Cấu hình Tray Icon
            trayIcon = new NotifyIcon();
            try
            {
                trayIcon.Icon = GetIconFromBase64(iconBase64);
            }
            catch
            {
                trayIcon.Icon = SystemIcons.Information; // Backup nếu Base64 lỗi
            }

            trayIcon.Text = "Rain Fall Desktop";
            trayIcon.Visible = true;

            // Xử lý Double Click để mở cài đặt
            trayIcon.DoubleClick += delegate
            {
                OpenSettings();
            };

            // Khởi tạo Menu chuột phải
            ContextMenu menu = new ContextMenu();

            // 1. Cài đặt
            menu.MenuItems.Add("Cài đặt / Settings", delegate
            {
                OpenSettings();
            });

            // // 2. Dọn sạch (Nếu bạn muốn reset vị trí các hạt mưa)
            // menu.MenuItems.Add("Làm mới mưa / Refresh", delegate
            // {
            //     // Giả định bạn có hàm này trong RainForm để reset lại vị trí hạt
            //     // mainForm.RefreshRain(); 
            // });

            // 3. Giới thiệu / About
            menu.MenuItems.Add("Giới thiệu / About", delegate
            {
                ShowAboutDialog();
            });

            // 4. Thoát
            menu.MenuItems.Add("-"); // Dấu gạch ngang phân cách
            menu.MenuItems.Add("Thoát / Exit", delegate
            {
                trayIcon.Visible = false;
                Application.Exit();
            });

            trayIcon.ContextMenu = menu;
        }


        private void ShowAboutDialog()
        {
            Form aboutForm = new Form();
            aboutForm.Text = "Giới thiệu / About";
            aboutForm.Size = new Size(390, 450);
            aboutForm.StartPosition = FormStartPosition.CenterScreen;
            aboutForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            aboutForm.MaximizeBox = false;
            aboutForm.BackColor = Color.White;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(20)
            };

            // CẤU HÌNH CỘT: Phải chiếm 100% chiều rộng để căn giữa nút
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));

            Label lblInfo = new Label
            {

                Text = "🌧️ Rain Fall Desktop 🌧️\n" +
                "------------------------------------------\n" +
                "• Thân tặng tất cả mọi người / Lovingly presented to everyone\n" +
                "• Phiên bản / Version: 1.0.0\n" +
                "• Trạng thái / Status: Miễn phí hoàn toàn (Free).\n" +
                "• Đặc biệt / Features: Không cần cài đặt, chạy ngay!\n" +
                "  (Portable app - No installation required!)\n" +
                "• Ghi chú / Note: Được viết lúc rảnh rỗi.\n" +
                "  (Created with passion during my free time.)\n" +
                "• Bí danh / Alias: Haha!\n" +
                "------------------------------------------\n" +
                "Chúc bạn có những giây phút làm việc thật thư giãn!\n" +
                "Wish you have relaxing moments at work!",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10.5F),
                TextAlign = ContentAlignment.TopCenter, // Căn giữa văn bản
                ForeColor = Color.FromArgb(40, 44, 52)
            };

            Button btnGithub = new Button
            {
                Text = "Github Repo",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Size = new Size(230, 60),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Image = GetGitHubIcon32(),
                TextImageRelation = TextImageRelation.ImageBeforeText,
                TextAlign = ContentAlignment.MiddleCenter,
                ImageAlign = ContentAlignment.MiddleCenter,

                // GIẢI PHÁP CĂN GIỮA: 
                // 1. Anchor = None trong TableLayoutPanel sẽ đưa Control vào tâm ô
                Anchor = AnchorStyles.None,
                // 2. Xóa sạch Margin để không bị đẩy lệch
                Margin = new Padding(0)
            };
            btnGithub.FlatAppearance.BorderSize = 2;
            btnGithub.FlatAppearance.BorderColor = Color.Black;

            btnGithub.Click += (s, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://github.com/hdgithub123/Fall-in-desktop",
                        UseShellExecute = true
                    });
                }
                catch { }
            };

            layout.Controls.Add(lblInfo, 0, 0);
            layout.Controls.Add(btnGithub, 0, 1); // Đưa nút vào cột 0, hàng 1

            aboutForm.Controls.Add(layout);
            aboutForm.ShowDialog();
        }



        private Image GetGitHubIcon32()
        {
            try
            {
                // Chuỗi Base64 chuẩn cho logo GitHub Black Cat (32x32)
                // Lưu ý: Chuỗi này đã được tối ưu để hiển thị rõ nét trên nền sáng
                string base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAADn0lEQVR4nKXXPYhdVRAH8N97vigm8QNFY1zE+IGK4BeIIGqhEFAQCwuxEcFCi0RIlUJBRQutgkWsBFNYCDZaqfgRTKEYI9GggsEEFRaNSfCDQMgm2fss7pl9887et7tvHbjcd+acmf/MnJm583omUw99zJf1FXgQm3EHZnABhjiBWezHx/gIx4vcAGeXwOmkfvp9I3biSAFbyTOLHbgmOdObFnyAl3AyKZ4v3jTlCX6T9uYT/wSeS+DZsU46p7xnsCcpOlMBLvc0RSbWn2DDckbExvU4XARPF4+mAa+jcrqsD+LqSUb0tWG6HIeS11lhhD4rz0/sxVVk2TDiJ1yqyomeNvQ9fNoBfhxzlSGTvM73f6rI1kZ8WHDjug3Ke1s6GN78rr2768r+bOGfxG/4oTy/ahNuWH4/i2uL7B8pamHE1ozdKwf/TaEMLz83TjN4GFfhvMQ/Fxu1PWJDJfOF8Sucx9+4TLqGl42HPgz4oBwapEithPL53ZXOwHgxDq/Thi2sy3f5o/GMjXyJpM38nEuxNzCqqKy7wS9Y28f9FpfHsLyPFl7m54yX+HUShpGzlc7Qtalge6MKUSiax13l8ELGTkEhc2fSV1fSTthncYiG+LYoWHEP76CQ/aYCDox9fW0o8uGmvL+qPFkNheze8o5rCKxNfVxcMYP+/B/ANR2r1oF1Ud/k8lrbYdRqqFd0ddGavrbN1gJwi1HSrJZC/tZKd9AcbbuNVhnvRttaNxovw2ko5K4suvIMsdDq+/g5WRtWNliP58vvQYf1S1F0zwYvFF1N0hFYh+BVi0vkjDY8DR5PHg0sHY04E0BPWtxj8vo1uNuou9WDR3zbt03hPazRRq/unHlYaXAvba3uT8w9xfI3K2u/xBPaQXNdB+iFuBlbcKAC6hpsDkgV+FTa3B2WJf6ppOAIbjAa26Pn36St9+UGl/gaPh3gfe23/WBl7W1l//XEO4YHklxQdLwtxufAromp0X4hzw8dIbzZaNoZYlcCegzbtQNrDR7rHi7BP8ZLrcv7hyrDF35ERcwVL+IqMi1Xjt8lbzN4RGVHDR5Kg/FOEjiKR42Sbr3JrTsM22s8B/Is+J7xwWWRgkiqXZX1h7Wf7e+1Y7UOBbH+OhmQ/ym9q50d62lqkZLY3G58HB9qW+pKDDhrfLR/pUP/RIpIwO143yiEf2nrvcuAkIkIDPEZ7kn7U31d813fh7fwTAXWZcAjeFs7vgdNHGr+AzcfqSS20xmyAAAAAElFTkSuQmCC";
                byte[] bytes = Convert.FromBase64String(base64);
                using (var ms = new System.IO.MemoryStream(bytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch
            {
                // Trả về một bitmap trống nếu lỗi để không làm treo ứng dụng
                return new Bitmap(32, 32);
            }
        }



        // Hàm bổ trợ mở Settings để tránh lặp code
        private void OpenSettings()
        {
            if (settingsForm == null || settingsForm.IsDisposed)
                settingsForm = new SettingsForm();

            if (settingsForm.Visible)
            {
                settingsForm.Activate();
            }
            else
            {
                settingsForm.Show();
                settingsForm.Activate();
            }
        }

        // Hàm chuyển đổi Base64 sang Icon
        private Icon GetIconFromBase64(string base64)
        {
            try
            {
                byte[] buffer = Convert.FromBase64String(base64);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer))
                {
                    return new Icon(ms);
                }
            }
            catch
            {
                return SystemIcons.Application;
            }
        }
    }

    public class RainDrop
    {
        public float X, Y, FallSpeed, Wind, Thickness;
        public int Length;
        public bool IsSplashing;
        public int SplashLife;
        public int SplashType;

        public RainDrop(int w, Random r) { Reset(w, r, true); }

        public void Reset(int w, Random r, bool firstTime = false)
        {
            X = r.Next(0, w);
            Y = firstTime ? r.Next(0, Screen.PrimaryScreen.Bounds.Height) : r.Next(-500, 0);
            FallSpeed = r.Next(RainConfig.TocDoMin, RainConfig.TocDoMax);
            Wind = (float)(r.NextDouble() * 2 - 1);
            Length = r.Next(RainConfig.LengthMin, Math.Max(RainConfig.LengthMin + 1, RainConfig.LengthMax));
            Thickness = (float)(r.NextDouble() * (RainConfig.DoDayMax - RainConfig.DoDayMin) + RainConfig.DoDayMin);

            IsSplashing = false;
            SplashLife = 0;
        }

        // GÓI GỌN LOGIC KHỞI TẠO SPLASH TẠI ĐÂY
        public void StartSplash(Random r)
        {
            if (!IsSplashing)
            {
                IsSplashing = true;
                SplashLife = 10; // Thời gian sống của hiệu ứng nảy
                SplashType = r.Next(0, 4); // Random 1 lần duy nhất để tránh nhấp nháy
            }
        }
    }

    public class RainForm : Form
    {
        private List<RainDrop> drops = new List<RainDrop>();
        private Random rand = new Random();
        private Timer timer = new Timer();
        private Bitmap backBuffer;
        private Graphics gBuffer;
        private float lightningAlpha = 0;
        private List<PointF> lightningPoints = new List<PointF>();


        #region Win32 API
        [DllImport("user32.dll")] static extern IntPtr WindowFromPoint(Point p);
        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)] static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        #endregion


        public RainForm()
        {

            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;

            for (int i = 0; i < 500; i++) drops.Add(new RainDrop(this.Width, rand));


            timer.Tick += (s, e) => { UpdatePhysics(); DrawToLayeredWindow(); };
            UpdateFPS();
            timer.Start();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80000; // WS_EX_LAYERED
                cp.ExStyle |= 0x20;    // WS_EX_TRANSPARENT
                return cp;
            }
        }


        public void UpdateFPS()
        {
            // Đảm bảo FPS không bằng 0 để tránh lỗi chia cho 0
            int interval = 1000 / Math.Max(1, RainConfig.FPS);
            timer.Interval = interval;
        }


        private void UpdatePhysics()
        {
            // Tự động cập nhật Interval nếu thấy RainConfig.FPS khác với hiện tại
            int targetInterval = 1000 / Math.Max(1, RainConfig.FPS);
            if (timer.Interval != targetInterval)
            {
                timer.Interval = targetInterval;
            }

            if (this.TopMost != RainConfig.AlwaysOnTop)
            {
                this.TopMost = RainConfig.AlwaysOnTop;
                // Thêm lệnh này để đảm bảo Windows áp dụng thay đổi ngay lập tức
                SetWindowPos(this.Handle, (IntPtr)(RainConfig.AlwaysOnTop ? -1 : 1), 0, 0, 0, 0, 0x0001 | 0x0002);
            }

            if (lightningAlpha > 0)
            {
                lightningAlpha -= 15;
            }
            else if (RainConfig.LightningFrequency > 0)
            {
                float chance = RainConfig.LightningFrequency / 2000f;
                if (rand.NextDouble() < chance)
                {
                    lightningAlpha = 255;
                    GenerateLightningPath();
                }
            }

            // 1. CẬP NHẬT LOGIC GIÓ
            if (Math.Abs(RainConfig.WindCurrent - RainConfig.targetWind) < 0.5f)
            {
                int direction = rand.Next(0, 3);
                if (direction == 0) RainConfig.targetWind = -rand.Next(RainConfig.WindMin, RainConfig.WindMax);
                else if (direction == 1) RainConfig.targetWind = rand.Next(RainConfig.WindMin, RainConfig.WindMax);
                else RainConfig.targetWind = 0;
            }

            float windStep = RainConfig.WindChangeSpeed / 100f;
            if (RainConfig.WindCurrent < RainConfig.targetWind) RainConfig.WindCurrent += windStep;
            else if (RainConfig.WindCurrent > RainConfig.targetWind) RainConfig.WindCurrent -= windStep;

            // 2. THÔNG SỐ MÀN HÌNH VÀ TỶ LỆ CỬA SỔ (Thay đổi ở đây)
            int screenW = Screen.PrimaryScreen.Bounds.Width;
            int screenH = Screen.PrimaryScreen.Bounds.Height;
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;

            // Thay vì nhân 0.5, ta nhân với % từ RainConfig
            double thresholdW = screenW * (RainConfig.WindowThresholdW_Percent / 100.0);
            double thresholdH = screenH * (RainConfig.WindowThresholdH_Percent / 100.0);

            // 3. VÒNG LẶP CẬP NHẬT
            for (int i = 0; i < RainConfig.SoLuong; i++)
            {
                RainDrop d = drops[i];

                if (!d.IsSplashing)
                {
                    // DI CHUYỂN HẠT MƯA
                    d.Y += d.FallSpeed;
                    d.X += (d.Wind + RainConfig.WindCurrent);

                    // Xử lý tràn biên ngang
                    if (d.X > this.Width) d.X = 0;
                    else if (d.X < 0) d.X = this.Width;

                    // KIỂM TRA VA CHẠM
                    if (d.Y > 50 && d.Y <= this.Height)
                    {
                        Point cp = new Point((int)d.X, (int)(d.Y + d.Length));
                        bool isInTaskbarZone = !workingArea.Contains(cp);

                        if (isInTaskbarZone && cp.Y < screenH && cp.Y > 0)
                        {
                            d.StartSplash(rand);
                        }
                        else
                        {
                            IntPtr hWnd = WindowFromPoint(cp);
                            if (hWnd != IntPtr.Zero && hWnd != this.Handle)
                            {
                                if (IsWindowVisible(hWnd))
                                {
                                    StringBuilder className = new StringBuilder(256);
                                    GetClassName(hWnd, className, className.Capacity);
                                    string name = className.ToString();

                                    if (name != "Progman" && name != "WorkerW" && name != "FolderView" && name != "SysListView32")
                                    {
                                        RECT rect = new RECT();
                                        if (GetWindowRect(hWnd, out rect))
                                        {
                                            int w = rect.Right - rect.Left;
                                            int h = rect.Bottom - rect.Top;

                                            // Sử dụng thresholdW và thresholdH đã tính theo %
                                            if (w <= thresholdW && h <= thresholdH)
                                            {
                                                d.StartSplash(rand);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Reset nếu rơi quá đáy màn hình
                    if (d.Y > this.Height)
                    {
                        d.Reset(this.Width, rand);
                    }
                }
                else
                {
                    d.SplashLife--;
                    if (d.SplashLife <= 0)
                    {
                        d.Reset(this.Width, rand);
                    }
                }
            }
        }


        private void GenerateLightningPath()
        {
            lightningPoints.Clear();

            // 1. Vị trí bắt đầu ngẫu nhiên ở đỉnh màn hình
            float curX = rand.Next(100, this.Width - 100);
            float curY = 0;
            lightningPoints.Add(new PointF(curX, curY));

            // 2. Xác định chiều cao mục tiêu ngẫu nhiên cho tia sét này
            // Sét có thể đánh ngắn (30% màn hình) hoặc đánh dài (95% màn hình)
            float targetHeight = (float)(rand.NextDouble() * (this.Height * 0.65f) + (this.Height * 0.3f));

            // 3. Vẽ các đoạn zíc-zắc cho đến khi đạt chiều cao mục tiêu
            while (curY < targetHeight)
            {
                // Độ lệch ngang (X) và độ dài mỗi bước rơi (Y)
                curX += rand.Next(-70, 71);
                curY += rand.Next(30, 80);

                // Đảm bảo không vượt quá mục tiêu quá nhiều
                if (curY > targetHeight) curY = targetHeight;

                lightningPoints.Add(new PointF(curX, curY));
            }
        }


        private void DrawSplash(Graphics g, RainDrop d)
        {
            float progress = (10f - d.SplashLife) / 10f;
            int alpha = Math.Max(0, Math.Min(255, (int)(RainConfig.Alpha * (1.0f - progress))));

            float windShift = RainConfig.WindCurrent * progress * 8;
            float originX = d.X + windShift;

            switch (d.SplashType)
            {
                case 0: // Tia chữ V
                    int sc = (int)(progress * 15);
                    // using (Pen p = new Pen(Color.FromArgb(alpha, RainConfig.MauMua), 1.0f))
                    using (Pen p = new Pen(Color.FromArgb(alpha, RainConfig.MauMua), 3.0f))
                    {
                        g.DrawLine(p, originX, d.Y, originX - sc, d.Y - sc);
                        g.DrawLine(p, originX, d.Y, originX + sc, d.Y - sc);
                    }
                    break;
                case 1: // Vòng tròn
                    int r = (int)(progress * 20);
                    using (Pen p = new Pen(Color.FromArgb(alpha, RainConfig.MauMua), 1.0f))
                        g.DrawEllipse(p, originX - r, d.Y - (r / 3), r * 2, (r * 2) / 3);
                    break;
                case 2: // Hạt văng li ti
                    float jH = progress * 10, jW = progress * 12;
                    // using (Pen p = new Pen(Color.FromArgb(alpha, RainConfig.MauMua), 1.5f))
                    using (Pen p = new Pen(Color.FromArgb(alpha, RainConfig.MauMua), 3.0f))
                    {
                        g.DrawLine(p, originX - jW, d.Y - jH, originX - jW - 1, d.Y - jH - 1);
                        g.DrawLine(p, originX + jW, d.Y - jH, originX + jW + 1, d.Y - jH - 1);
                    }
                    break;
                case 3: // Hạt tròn TO (Mới)
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, RainConfig.MauMua)))
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            float dir = (i - 2) * 20f;
                            float arc = (float)(Math.Sin(progress * Math.PI) * 20);
                            float size = Math.Max(2, 8f * (1.0f - progress));
                            g.FillEllipse(b, originX + (dir * progress) - (size / 2), d.Y - arc - (size / 2), size, size);
                        }
                    }
                    break;
            }
        }


        private void DrawToLayeredWindow()
        {
            // Chỉ khởi tạo một lần duy nhất hoặc khi resize
            if (backBuffer == null)
            {
                backBuffer = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
                gBuffer = Graphics.FromImage(backBuffer);
                gBuffer.SmoothingMode = SmoothingMode.AntiAlias;
            }

            gBuffer.Clear(Color.Transparent);

            // set
            if (lightningAlpha > 0 && lightningPoints.Count > 1)
            {
                using (SolidBrush flash = new SolidBrush(Color.FromArgb((int)(lightningAlpha * 0.15f), Color.White)))
                    gBuffer.FillRectangle(flash, 0, 0, this.Width, this.Height);
                using (Pen pGlow = new Pen(Color.FromArgb((int)(lightningAlpha * 0.4f), Color.DeepSkyBlue), 4f))
                    gBuffer.DrawLines(pGlow, lightningPoints.ToArray());
                using (Pen pCore = new Pen(Color.FromArgb((int)lightningAlpha, Color.White), 1.5f))
                    gBuffer.DrawLines(pCore, lightningPoints.ToArray());
            }


            // them cho set

            // Dùng 1 cây bút duy nhất, chỉ đổi thông số
            using (Pen sharedPen = new Pen(RainConfig.MauMua))
            {
                for (int i = 0; i < RainConfig.SoLuong; i++)
                {
                    RainDrop d = drops[i];
                    if (d.IsSplashing)
                    {
                        // Truyền gBuffer vào để vẽ
                        DrawSplash(gBuffer, d);
                    }
                    else
                    {
                        sharedPen.Width = d.Thickness;
                        sharedPen.Color = Color.FromArgb(RainConfig.Alpha, RainConfig.MauMua);
                        gBuffer.DrawLine(sharedPen, d.X, d.Y, d.X + (d.Wind + RainConfig.WindCurrent), d.Y + d.Length);
                    }
                }
            }
            UpdateNativeWindow(backBuffer);
        }




        private void UpdateNativeWindow(Bitmap bmp)
        {
            IntPtr sDc = GetDC(IntPtr.Zero);
            IntPtr mDc = CreateCompatibleDC(sDc);
            IntPtr hBmp = bmp.GetHbitmap(Color.FromArgb(0));
            IntPtr oldBmp = SelectObject(mDc, hBmp);
            BLENDFUNCTION bf = new BLENDFUNCTION { BlendOp = 0, BlendFlags = 0, SourceConstantAlpha = 255, AlphaFormat = 1 };
            Point dPos = new Point(this.Left, this.Top);
            Size sz = new Size(this.Width, this.Height);
            Point sPos = new Point(0, 0);
            UpdateLayeredWindow(this.Handle, sDc, ref dPos, ref sz, mDc, ref sPos, 0, ref bf, 2);
            SelectObject(mDc, oldBmp);
            DeleteObject(hBmp);
            DeleteDC(mDc);
            ReleaseDC(IntPtr.Zero, sDc);
        }

        #region Native Methods
        [StructLayout(LayoutKind.Sequential, Pack = 1)] struct BLENDFUNCTION { public byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat; }
        [DllImport("user32.dll")] static extern bool UpdateLayeredWindow(IntPtr h, IntPtr d, ref Point p, ref Size s, IntPtr sDC, ref Point sP, int c, ref BLENDFUNCTION b, int f);
        [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr h);
        [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr h, IntPtr dc);
        [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr dc);
        [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr dc);
        [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
        [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
        #endregion
    }




    public class SettingsForm : Form
    {
        public bool AllowClose = false;
        private ToolTip tip = new ToolTip();

        public SettingsForm()
        {
            // 1. Ép chế độ hiển thị chuẩn để không bị vỡ chữ trên Win 7/10/11
            this.AutoScaleMode = AutoScaleMode.None;
            this.Font = new Font("Segoe UI", 9F);

            // 2. Thiết lập Form nhỏ gọn, chuyên nghiệp
            this.Text = "Haha Cài đặt Mưa / Rain Fall Settings";
            this.Size = new Size(650, 500);
            this.BackColor = Color.FromArgb(250, 250, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = true;
            this.TopMost = false;

            // 3. Lưới 2 cột chính
            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.ColumnCount = 2;
            grid.Padding = new Padding(15, 15, 15, 80); // Chừa lề dưới cho nút bấm
            grid.AutoScroll = true;

            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));


            // --- CỘT 1 & 2: CÁC THÔNG SỐ CƠ BẢN ---

            AddCheckbox(grid, "Hiện trên cả cửa sổ đang mở / Show above all open windows", RainConfig.AlwaysOnTop, delegate (bool b)
            {
                RainConfig.AlwaysOnTop = b;
            });

            AddItem(grid, "Số lượng mưa / Rain count", 10, 500, RainConfig.SoLuong, v => RainConfig.SoLuong = v);
            AddItem(grid, "Tốc độ / Fall Speed", 10, 100, RainConfig.TocDoMax, v =>
            {
                RainConfig.TocDoMin = Math.Max(5, v - 20);
                RainConfig.TocDoMax = v;
            });


            AddItem(grid, "Độ dày / Thickness", 1, 50, (int)(RainConfig.DoDayMax * 10), v =>
            {
                RainConfig.DoDayMin = v / 20f;
                RainConfig.DoDayMax = v / 10f;
            }, "Độ đậm nhạt của nét vẽ hạt mưa\nRain drop line thickness");

            AddItem(grid, "Độ dài ngắn nhất / Length Min", 2, 20, RainConfig.LengthMin, delegate (int v)
            {
                RainConfig.LengthMin = v;
            });

            AddItem(grid, "Độ dài lớn nhất / Length Max", 10, 100, RainConfig.LengthMax, delegate (int v)
            {
                RainConfig.LengthMax = v;
            });


            AddItem(grid, "Độ rõ / Alpha", 20, 255, RainConfig.Alpha, v => RainConfig.Alpha = v,
                "Độ mờ đục của hạt mưa (255 là đậm nhất)\nTransparency of rain drops");


            // 1. Độ mạnh thấp nhất của cơn gió khi nó bắt đầu thổi (Nên để bằng 0 hoặc thấp)
            AddItem(grid, "Độ mạnh gió tối thiểu / Min Wind Strength", 0, 20, RainConfig.WindMin, delegate (int v)
            {
                RainConfig.WindMin = v;
            });

            // 2. Độ mạnh tối đa của cơn gió (Quy định hạt mưa sẽ nghiêng tối đa bao nhiêu)
            AddItem(grid, "Độ mạnh gió tối đa / Max Wind Strength", 0, 50, RainConfig.WindMax, delegate (int v)
            {
                RainConfig.WindMax = v;
            });

            // 3. Tốc độ biến chuyển hướng gió (Càng cao gió đổi hướng trái/phải càng nhanh/gắt)
            AddItem(grid, "Tốc độ đổi hướng gió / Wind Change Speed", 1, 30, RainConfig.WindChangeSpeed, delegate (int v)
            {
                RainConfig.WindChangeSpeed = v;
            });


            // Các thanh trượt
            AddItem(grid, "Tần suất Sét / Lightning Frequency", 0, 100, RainConfig.LightningFrequency, delegate (int v)
            {
                RainConfig.LightningFrequency = v;
            });



            AddItem(grid, "Khung hình / FPS", 10, 60, RainConfig.FPS, v =>
            {
                RainConfig.FPS = v;
            }, "FPS càng cao mưa càng mượt nhưng tốn CPU hơn.\nLower FPS saves CPU usage.");


            // Thanh trượt từ 0% đến 100%
            AddItem(grid, "Cửa sổ rộng tối đa / Max Window Width (%)", 10, 100, RainConfig.WindowThresholdW_Percent, delegate (int v)
            {
                RainConfig.WindowThresholdW_Percent = v;
            }, "Tỷ lệ chiều rộng cửa sổ ứng dụng để hạt mưa rơi vào \nWindow width ratio for rain to land");

            AddItem(grid, "Cửa sổ cao tối đa / Max Window Height (%)", 10, 100, RainConfig.WindowThresholdH_Percent, delegate (int v)
            {
                RainConfig.WindowThresholdH_Percent = v;
            }, "Tỷ lệ chiều cao cửa sổ ứng dụng để hạt mưa rơi vào \nWindow height ratio for rain to land");


            // 4. Panel nút bấm cố định phía dưới
            Panel pnlBottom = new Panel() { Dock = DockStyle.Bottom, Height = 70, BackColor = Color.White };
            pnlBottom.Paint += (s, e) => e.Graphics.DrawLine(Pens.Gainsboro, 0, 0, pnlBottom.Width, 0);

            // NÚT CHỌN MÀU
            Button btnColor = CreateBtn("MÀU MƯA / CHANGE COLOR", 30, Color.DodgerBlue, () =>
            {
                using (ColorDialog cd = new ColorDialog() { Color = RainConfig.MauMua })
                {
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        RainConfig.MauMua = cd.Color;
                    }
                }
            });

            // NÚT ĐÓNG
            Button btnClose = CreateBtn("ĐÓNG / CLOSE", 330, Color.DimGray, () => this.Hide());

            pnlBottom.Controls.Add(btnColor);
            pnlBottom.Controls.Add(btnClose);

            this.Controls.Add(grid);
            this.Controls.Add(pnlBottom);

            // Xử lý sự kiện đóng: Chỉ ẩn chứ không hủy Form
            this.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                }
            };
        }



        private void AddCheckbox(TableLayoutPanel grid, string text, bool isChecked, Action<bool> onChanged)
        {
            CheckBox cb = new CheckBox();
            cb.Text = text;
            cb.Checked = isChecked;
            cb.AutoSize = true;
            cb.Margin = new Padding(3, 5, 3, 5);

            cb.CheckedChanged += delegate (object sender, EventArgs e)
            {
                onChanged(cb.Checked);
            };

            // CHỈ THÊM 1 LẦN: CheckBox sẽ nằm ở cột 1 của hàng hiện tại
            grid.Controls.Add(cb);

            // Đừng thêm Label rỗng nữa nếu bạn muốn ô bên cạnh để trống cho cái khác
        }



        private void AddItem(TableLayoutPanel parent, string name, int min, int max, int cur, Action<int> onScroll, string description = "")
        {
            Panel p = new Panel() { Width = 310, Height = 65, Margin = new Padding(5) };

            Label l = new Label()
            {
                Text = name + ": " + cur,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            TrackBar t = new TrackBar()
            {
                Minimum = min,
                Maximum = max,
                Value = Math.Max(min, Math.Min(max, cur)),
                Width = 290,
                Top = 25,
                TickStyle = TickStyle.None,
                Height = 30
            };

            if (!string.IsNullOrEmpty(description))
            {
                tip.SetToolTip(l, description);
                tip.SetToolTip(t, description);
            }

            t.Scroll += (s, e) =>
            {
                onScroll(t.Value);
                l.Text = name + ": " + t.Value;
            };

            p.Controls.Add(l);
            p.Controls.Add(t);
            parent.Controls.Add(p);
        }

        private Button CreateBtn(string txt, int x, Color c, Action onClick)
        {
            Button b = new Button()
            {
                Text = txt,
                Location = new Point(x, 15),
                Size = new Size(270, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = c,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (s, e) => onClick();
            return b;
        }
    }





}
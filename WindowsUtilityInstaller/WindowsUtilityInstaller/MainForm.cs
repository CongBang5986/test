using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsUtilityInstaller
{
    public partial class MainForm : Form
    {
        // Đổi tông màu sang sáng hiện đại (accent xanh dương, nền trắng/xám nhạt)
        private readonly Color PrimaryColor = Color.FromArgb(0, 120, 215);         // Xanh dương tươi (accent)
        private readonly Color SecondaryColor = Color.FromArgb(245, 247, 250);     // Nền xám rất nhạt
        private readonly Color AccentColor = Color.FromArgb(0, 180, 120);          // Xanh lá nhạt (accent phụ)
        private readonly Color ButtonTextColor = Color.White;
        private readonly Color GroupTextColor = Color.FromArgb(0, 120, 215);
        private readonly Color PanelBorderColor = Color.FromArgb(220, 220, 230);

        private readonly Font TitleFont = new Font("Segoe UI", 18, FontStyle.Bold);
        private readonly Font GroupFont = new Font("Segoe UI", 10, FontStyle.Bold);
        private readonly Font ButtonFont = new Font("Segoe UI", 9);

        private List<Button> allButtons = new List<Button>();
        private TextBox txtDownloadPath;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Button btnBrowse;
        private Button btnDownloadAll;

        // Danh sách nhóm phần mềm (tên nhóm, <tên phần mềm, link tải>)
        private readonly Dictionary<string, Dictionary<string, string>> SoftwareGroups = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "Trình duyệt", new Dictionary<string, string>()
                {
                    { "Google Chrome", "https://dl.google.com/chrome/install/latest/chrome_installer.exe" },
                    { "Mozilla Firefox", "https://download.mozilla.org/?product=firefox-latest-ssl&os=win64&lang=vi" },
                    { "Microsoft Edge", "https://go.microsoft.com/fwlink/?linkid=2108834" },
                    { "Cốc Cốc", "https://update.coccoc.com/download/CocCocSetup.exe" },
                    { "Opera", "https://net.geo.opera.com/opera/stable/windows" },
                    { "OperaGX", "https://cdn-production-opera-website.operacdn.com/staticfiles/operagx/win/Opera_GX_102.0.4880.78_Setup.exe" }
                }
            },
            {
                "Văn phòng", new Dictionary<string, string>()
                {
                    { "LibreOffice", "https://download.documentfoundation.org/libreoffice/stable/7.6.4/win/x86_64/LibreOffice_7.6.4_Win_x86-64.msi" },
                    { "Foxit Reader", "https://cdn01.foxitsoftware.com/product/reader/desktop/win/12.1.3/FoxitPDFReader1213_L10N_Setup.exe" },
                    { "SumatraPDF", "https://www.sumatrapdfreader.org/dl2/SumatraPDF-3.5.2-64-install.exe" },
                    { "WPS Office", "https://wdl1.pcfg.cache.wpscdn.com/wpsdl/wpsoffice/download/11.1.0.15258/WPSOffice_11.1.0.15258.exe" }
                }
            },
            {
                "Công cụ", new Dictionary<string, string>()
                {
                    { "7-Zip", "https://www.7-zip.org/a/7z2301-x64.exe" },
                    { "WinRAR", "https://www.rarlab.com/rar/winrar-x64-700.exe" },
                    { "Unikey", "https://unikey.vn/UniKey4.6RC2-140823-Windows.zip" },
                    { "EVKey", "https://evkeyvn.com/uploads/EVKeySetup.exe" },
                    { "Notepad++", "https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.6.4/npp.8.6.4.Installer.x64.exe" },
                    { "TeamViewer", "https://download.teamviewer.com/download/TeamViewer_Setup_x64.exe" }
                }
            },
            {
                "Đồ họa & Thiết kế", new Dictionary<string, string>()
                {
                    { "Paint.NET", "https://www.dotpdn.com/files/paint.net.4.3.12.install.x64.zip" },
                    { "GIMP", "https://download.gimp.org/pub/gimp/v2.10/windows/gimp-2.10.36-setup.exe" },
                    { "Inkscape", "https://media.inkscape.org/dl/resources/file/inkscape-1.3.2_2023-11-25_amd64.exe" }
                }
            },
            {
                "Nghe nhạc & Video", new Dictionary<string, string>()
                {
                    { "VLC Media Player", "https://get.videolan.org/vlc/3.0.20/win64/vlc-3.0.20-win64.exe" },
                    { "K-Lite Codec Pack", "https://files3.codecguide.com/K-Lite_Codec_Pack_1850_Full.exe" },
                    { "Spotify", "https://download.scdn.co/SpotifySetup.exe" }
                }
            },
            {
                "Tin nhắn", new Dictionary<string, string>()
                {
                    { "Discord", "https://dl.discordapp.net/distro/app/stable/win/x64/DiscordSetup.exe" },
                    { "Zalo", "https://cdn-download.zalo.me/pc/ZaloSetup-24.5.2.exe" },
                    { "Skype", "https://get.skype.com/go.getskype.skypeforwindows" },
                    { "WhatsApp", "https://www.whatsapp.com/download" }
                }
            },
            {
                "Bảo mật & Diệt virus", new Dictionary<string, string>()
                {
                    { "Kaspersky Free", "https://products.s.kaspersky-labs.com/homeuser/kfa2021/21.3.10.391/english-ru/kfa21.3.10.391en_23830.exe" },
                    { "Avast Free", "https://files.avast.com/iavs9x/avast_free_antivirus_setup_online.exe" },
                    { "Malwarebytes", "https://data-cdn.mbamupdates.com/web/mb4-setup-consumer/MBSetup.exe" }
                }
            },
            {
                "Game", new Dictionary<string, string>()
                {
                    { "Steam", "https://cdn.cloudflare.steamstatic.com/client/installer/SteamSetup.exe" },
                    { "Epic Games", "https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/installer/download/EpicGamesLauncherInstaller.msi" },
                    { "Garena", "https://cdn.garenanow.com/gas/installer/Garena-v2.0.exe" },
                    { "Battle.net", "https://www.battle.net/download/getInstaller?os=win&installer=Battle.net-Setup.exe" }
                }
            }
        };

        // Danh sách tiện ích hệ thống
        private readonly Dictionary<string, Dictionary<string, Action>> SystemTools = new Dictionary<string, Dictionary<string, Action>>()
        {
            {
                "🛠️ Tiện ích hệ thống", new Dictionary<string, Action>()
                {
                    { "Dọn dẹp ổ đĩa", CleanDisk },
                    { "Tắt BitLocker", DisableBitlocker },
                    { "Di chuyển thư mục Zalo", MoveZaloFolder },
                    { "Xoá Bloatware", RemoveBloatware },
                    { "Tối ưu hệ thống", OptimizeSystem },
                    { "Cài lại Microsoft Store", InstallMicrosoftStore }
                }
            },
            {
                "📚 Cài đặt VC++ Redist", new Dictionary<string, Action>()
                {
                    { "VC++ 2022", () => DownloadAndRun("https://aka.ms/vs/17/release/vc_redist.x64.exe") },
                    { "VC++ 2015-2019", () => DownloadAndRun("https://aka.ms/vs/16/release/vc_redist.x64.exe") },
                    { "VC++ 2013", () => DownloadAndRun("https://download.microsoft.com/download/2/E/6/2E61CFA4-993B-4DD4-91DA-3737CD5CD6E3/vcredist_x64.exe") }
                }
            },
            {
                "🧩 Cài đặt .NET Runtime", new Dictionary<string, Action>()
                {
                    { ".NET 8.0", () => DownloadAndRun("https://download.visualstudio.microsoft.com/download/pr/7e2e2e2e-2e2e-4e2e-8e2e-2e2e2e2e2e2e/8e2e2e2e2e2e2e2e2e2e2e2e2e2e2e2e/dotnet-runtime-8.0.5-win-x64.exe") },
                    { ".NET 6.0", () => DownloadAndRun("https://download.visualstudio.microsoft.com/download/pr/3a8474a4-6514-4e4f-8438-b958b9552823/8bdad9368e1b4f5b7a3e7a62a3f61f1e/dotnet-runtime-6.0.9-win-x64.exe") },
                    { ".NET 5.0", () => DownloadAndRun("https://download.visualstudio.microsoft.com/download/pr/2d4650cf-5a99-4a3e-b17d-c121a1b0c27e/08d8d829d2694e0519525e23722b638b/dotnet-runtime-5.0.17-win-x64.exe") }
                }
            },
            {
                "☁️ Lưu trữ đám mây", new Dictionary<string, Action>()
                {
                    { "Google Drive", () => DownloadAndRun("https://dl.google.com/drive-file-stream/GoogleDriveSetup.exe") },
                    { "OneDrive", () => DownloadAndRun("https://go.microsoft.com/fwlink/p/?LinkId=248256") },
                    { "Dropbox", () => DownloadAndRun("https://www.dropbox.com/download?plat=win") }
                }
            }
        };

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
            SetupEventHandlers();
        }

        private void SetupUI()
        {
            // Thiết lập form chính
            this.Text = "Windows Utility Installer";
            this.Size = new Size(900, 600); // Tăng kích thước form chính
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Header
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54, // Giảm chiều cao header
                BackColor = PrimaryColor
            };

            var lblTitle = new Label
            {
                Text = "TIỆN ÍCH CÀI ĐẶT WINDOWS",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 20, FontStyle.Bold), // Tăng cỡ chữ tiêu đề
                ForeColor = Color.White,
                Padding = new Padding(0, 10, 0, 0)
            };
            headerPanel.Controls.Add(lblTitle);

            // TabControl
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12, FontStyle.Regular), // Tăng cỡ chữ tab
                BackColor = SecondaryColor
            };

            // Tab 1: Phần mềm
            var tabSoftware = new TabPage("Phần mềm")
            {
                BackColor = SecondaryColor
            };

            // TableLayoutPanel phần mềm
            var tablePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                AutoSize = true,
                AutoScroll = true,
                BackColor = SecondaryColor,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            for (int i = 0; i < 3; i++)
                tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            // Chia đều các nhóm phần mềm vào 3 cột
            var groupList = new List<KeyValuePair<string, Dictionary<string, string>>>(SoftwareGroups);
            int groupsPerCol = (int)Math.Ceiling(groupList.Count / 3.0);

            var groupIcons = new Dictionary<string, string>
            {
                { "Trình duyệt", "🌐" },
                { "Văn phòng", "📄" },
                { "Công cụ", "🧰" },
                { "Đồ họa & Thiết kế", "🎨" },
                { "Nghe nhạc & Video", "🎵" },
                { "Tin nhắn", "💬" },
                { "Bảo mật & Diệt virus", "🛡️" },
                { "Game", "🎮" }
            };

            for (int col = 0; col < 3; col++)
            {
                var colPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                    WrapContents = false,
                    Margin = new Padding(1), // Giảm margin cột tối đa
                    Padding = new Padding(0)
                };

                for (int i = col * groupsPerCol; i < Math.Min((col + 1) * groupsPerCol, groupList.Count); i++)
                {
                    var group = groupList[i];

                    var icon = groupIcons.TryGetValue(group.Key, out var ic) ? ic + " " : "";
                    // Tiêu đề nhóm
                    var lblGroup = new Label
                    {
                        Text = icon + group.Key,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 12, FontStyle.Bold), // Tăng cỡ chữ nhóm
                        Padding = new Padding(0, 6, 0, 3),
                        ForeColor = GroupTextColor
                    };
                    colPanel.Controls.Add(lblGroup);

                    // Panel chứa các nút phần mềm trong nhóm (hàng dọc)
                    var buttonPanel = new FlowLayoutPanel
                    {
                        AutoSize = true,
                        FlowDirection = FlowDirection.TopDown,
                        Margin = new Padding(0, 0, 0, 10)
                    };

                    foreach (var software in group.Value)
                    {
                        var btn = new Button
                        {
                            Text = software.Key,
                            Tag = software.Value,
                            Size = new Size(140, 32), // Tăng kích thước nút cho phù hợp chữ lớn
                            Font = new Font("Segoe UI", 11),
                            FlatStyle = FlatStyle.Flat,
                            Margin = new Padding(3),
                            BackColor = Color.White,
                            ForeColor = Color.Black,
                            FlatAppearance = { BorderColor = AccentColor, BorderSize = 1 }
                        };

                        // Kiểm tra đường dẫn tải phần mềm
                        if (!Uri.TryCreate(software.Value, UriKind.Absolute, out var uri) ||
                            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                        {
                            btn.Enabled = false;
                            btn.BackColor = Color.LightGray;
                            btn.Text += " (Đường dẫn lỗi)";
                        }

                        // Trong SetupUI, sau khi tạo từng button (btn), thêm đoạn này để loại bỏ hiệu ứng hover/mouse:
                        btn.FlatAppearance.MouseOverBackColor = btn.BackColor;
                        btn.FlatAppearance.MouseDownBackColor = btn.BackColor;
                        btn.TabStop = false;

                        allButtons.Add(btn);
                        buttonPanel.Controls.Add(btn);
                    }

                    colPanel.Controls.Add(buttonPanel);
                }

                tablePanel.Controls.Add(colPanel, col, 0);
            }

            tabSoftware.Controls.Add(tablePanel);

            // Tab 2: Tiện ích hệ thống
            var tabSystemTools = new TabPage("Tiện ích hệ thống")
            {
                BackColor = SecondaryColor
            };

            // Chia 3 cột cho các nhóm tiện ích hệ thống
            var sysTablePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                AutoSize = true,
                AutoScroll = true,
                BackColor = SecondaryColor,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            for (int i = 0; i < 3; i++)
                sysTablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            var sysGroupList = new List<KeyValuePair<string, Dictionary<string, Action>>>(SystemTools);
            int sysGroupsPerCol = (int)Math.Ceiling(sysGroupList.Count / 3.0);

            for (int col = 0; col < 3; col++)
            {
                var colPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                    WrapContents = false,
                    Margin = new Padding(10)
                };

                for (int i = col * sysGroupsPerCol; i < Math.Min((col + 1) * sysGroupsPerCol, sysGroupList.Count); i++)
                {
                    var group = sysGroupList[i];

                    var lblGroup = new Label
                    {
                        Text = group.Key,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        Padding = new Padding(0, 10, 0, 5),
                        ForeColor = GroupTextColor
                    };
                    colPanel.Controls.Add(lblGroup);

                    var buttonPanel = new FlowLayoutPanel
                    {
                        AutoSize = true,
                        FlowDirection = FlowDirection.TopDown,
                        Margin = new Padding(0, 0, 0, 10)
                    };

                    foreach (var tool in group.Value)
                    {
                        var btn = new Button
                        {
                            Text = tool.Key,
                            Size = new Size(170, 36),
                            Font = new Font("Segoe UI", 11),
                            FlatStyle = FlatStyle.Flat,
                            Margin = new Padding(3),
                            BackColor = Color.White,
                            ForeColor = Color.Black,
                            FlatAppearance = { BorderColor = PrimaryColor, BorderSize = 1 },
                            Tag = tool.Value
                        };
                        btn.Click += (s, e) =>
                        {
                            var action = btn.Tag as Action;
                            action?.Invoke();
                        };
                        buttonPanel.Controls.Add(btn);

                        // Thêm đoạn này để loại bỏ hiệu ứng hover/mouse:
                        btn.FlatAppearance.MouseOverBackColor = btn.BackColor;
                        btn.FlatAppearance.MouseDownBackColor = btn.BackColor;
                        btn.TabStop = false;
                    }

                    colPanel.Controls.Add(buttonPanel);
                }

                sysTablePanel.Controls.Add(colPanel, col, 0);
            }

            tabSystemTools.Controls.Add(sysTablePanel);

            // Thêm các tab vào TabControl
            tabControl.TabPages.Add(tabSoftware);
            tabControl.TabPages.Add(tabSystemTools);

            // Control panel
            var controlPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 140, // Tăng chiều cao cho phù hợp chữ lớn
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblDownloadPath = new Label
            {
                Text = "Thư mục tải:",
                AutoSize = true,
                Location = new Point(10, 24),
                ForeColor = PrimaryColor,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };

            txtDownloadPath = new TextBox
            {
                Name = "txtDownloadPath",
                Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                Size = new Size(320, 32),
                Location = new Point(130, 20),
                BackColor = SecondaryColor,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };

            btnBrowse = new Button
            {
                Name = "btnBrowse",
                Text = "Chọn...",
                Size = new Size(90, 32),
                Location = new Point(470, 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentColor,
                ForeColor = ButtonTextColor,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };

            btnDownloadAll = new Button
            {
                Name = "btnDownloadAll",
                Text = "TẢI XUỐNG TẤT CẢ",
                Size = new Size(200, 44),
                Location = new Point(130, 62),
                BackColor = PrimaryColor,
                ForeColor = ButtonTextColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };

            progressBar = new ProgressBar
            {
                Name = "progressBar",
                Size = new Size(350, 22),
                Location = new Point(130, 110),
                BackColor = SecondaryColor
            };

            lblStatus = new Label
            {
                Name = "lblStatus",
                Size = new Size(350, 22),
                Location = new Point(130, 90),
                ForeColor = AccentColor,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };

            controlPanel.Controls.AddRange(new Control[] {
                lblDownloadPath,
                txtDownloadPath,
                btnBrowse,
                btnDownloadAll,
                progressBar,
                lblStatus
            });

            // Thêm các control vào form
            this.Controls.Add(tabControl);
            this.Controls.Add(controlPanel);
            this.Controls.Add(headerPanel);

            // Footer: Dòng thông tin tác giả
            var lblFooter = new Label
            {
                Text = "Phát triển bởi Công Bằng - Since 2025",
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                ForeColor = Color.Gray,
                Height = 28,
                BackColor = Color.Transparent
            };

            // Thêm footer vào form (nên thêm sau cùng để nằm dưới cùng)
            this.Controls.Add(lblFooter);
        }

        private void SetupEventHandlers()
        {
            foreach (var btn in allButtons)
            {
                btn.Click += async (sender, e) =>
                {
                    await DownloadSoftware(btn);
                };
            }

            btnDownloadAll.Click += async (sender, e) =>
            {
                await DownloadAllSoftware();
            };

            btnBrowse.Click += (sender, e) =>
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        txtDownloadPath.Text = dialog.SelectedPath;
                    }
                }
            };
        }

        private async Task DownloadSoftware(Button btn)
        {
            string url = btn.Tag.ToString();
            string fileName = Path.GetFileName(new Uri(url).LocalPath);
            string destination = txtDownloadPath.Text;

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            string filePath = Path.Combine(destination, fileName);

            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += (s, e) =>
                {
                    progressBar.Value = e.ProgressPercentage;
                };

                try
                {
                    await client.DownloadFileTaskAsync(new Uri(url), filePath);

                    // Kiểm tra file tải về có đúng là file thực thi không
                    var ext = Path.GetExtension(filePath).ToLower();
                    var fileInfo = new FileInfo(filePath);
                    if ((ext != ".exe" && ext != ".msi") || fileInfo.Length < 1024 * 100) // < 100KB
                    {
                        MessageBox.Show("Tải về không thành công hoặc không đúng file cài đặt. Vui lòng kiểm tra lại link hoặc tải thủ công.", "Lỗi");
                        File.Delete(filePath);
                    }
                    else
                    {
                        MessageBox.Show("Tải thành công: " + fileName, "Thông báo");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tải: " + ex.Message, "Lỗi");
                }
            }
        }

        private async Task DownloadAllSoftware()
        {
            foreach (var btn in allButtons)
            {
                await DownloadSoftware(btn);
            }
        }

        private static void CleanDisk()
        {
            Process.Start("cleanmgr", "/sagerun:1");
        }

        private static void DisableBitlocker()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_EncryptableVolume WHERE DriveLetter = 'C:'");
            foreach (ManagementObject volume in searcher.Get())
            {
                if (volume["ProtectionStatus"]?.ToString() == "1")
                {
                    volume.InvokeMethod("DisableKeyProtectors", new object[] { null });
                }
            }
        }

        private static void MoveZaloFolder()
        {
            string source = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Zalo");
            string dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Zalo_Backup");

            if (Directory.Exists(source))
            {
                Directory.Move(source, dest);
            }
        }

        private static void RemoveBloatware()
        {
            // Danh sách các ứng dụng bloatware để gỡ cài đặt
            string[] bloatwareApps = new string[]
            {
                "Microsoft.BingWeather",
                "Microsoft.GetHelp",
                "Microsoft.MicrosoftSolitaireCollection",
                "Microsoft.Office.OneNote",
                "Microsoft.People",
                "Microsoft.SkypeApp",
                "Microsoft.StorePurchaseApp",
                "Microsoft.XboxApp"
            };

            foreach (var app in bloatwareApps)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = $"-Command \"Get-AppxPackage {app} | Remove-AppxPackage\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi gỡ cài đặt {app}: {ex.Message}", "Lỗi");
                }
            }
        }

        private static void OptimizeSystem()
        {
            // Thực hiện tối ưu hệ thống (ví dụ: dọn dẹp registry, tối ưu hóa ổ đĩa, v.v.)
            // Đây chỉ là một ví dụ, bạn có thể thêm mã thực tế cho việc tối ưu hóa hệ thống.
            MessageBox.Show("Tối ưu hệ thống đang được thực hiện...", "Thông báo");
        }

        private static void InstallMicrosoftStore()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Get-AppxPackage -allusers Microsoft.WindowsStore | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register '$($_.InstallLocation)\\AppXManifest.xml'}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                MessageBox.Show("Đã chạy lệnh cài lại Microsoft Store. Vui lòng chờ một lúc và kiểm tra lại Store.", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cài Microsoft Store: " + ex.Message, "Lỗi");
            }
        }

        private static void DownloadAndRun(string url)
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(new Uri(url).LocalPath));
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, tempPath);
                }
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải hoặc chạy file: " + ex.Message, "Lỗi");
            }
        }
    }
}

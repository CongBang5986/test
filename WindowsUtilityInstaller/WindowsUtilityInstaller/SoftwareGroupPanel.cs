using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsUtilityInstaller.UI
{
    public class SoftwareGroupsPanel : UserControl
    {
        public List<Button> AllButtons { get; } = new List<Button>();

        public SoftwareGroupsPanel(Dictionary<string, Dictionary<string, string>> softwareGroups, Dictionary<string, string> groupIcons, Color groupTextColor, Color accentColor)
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Transparent;

            var tablePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                AutoSize = true,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            for (int i = 0; i < 3; i++)
                tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            var groupList = new List<KeyValuePair<string, Dictionary<string, string>>>(softwareGroups);
            int groupsPerCol = (int)Math.Ceiling(groupList.Count / 3.0);

            for (int col = 0; col < 3; col++)
            {
                var colPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                    WrapContents = false,
                    Margin = new Padding(1),
                    Padding = new Padding(0)
                };

                for (int i = col * groupsPerCol; i < Math.Min((col + 1) * groupsPerCol, groupList.Count); i++)
                {
                    var group = groupList[i];
                    var icon = groupIcons.TryGetValue(group.Key, out var ic) ? ic + " " : "";
                    var lblGroup = new Label
                    {
                        Text = icon + group.Key,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        Padding = new Padding(0, 6, 0, 3),
                        ForeColor = groupTextColor
                    };
                    colPanel.Controls.Add(lblGroup);

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
                            Size = new Size(140, 32),
                            Font = new Font("Segoe UI", 11),
                            FlatStyle = FlatStyle.Flat,
                            Margin = new Padding(3),
                            BackColor = Color.White,
                            ForeColor = Color.Black,
                            FlatAppearance = { BorderColor = accentColor, BorderSize = 1 }
                        };

                        if (!Uri.TryCreate(software.Value, UriKind.Absolute, out var uri) ||
                            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                        {
                            btn.Enabled = false;
                            btn.BackColor = Color.LightGray;
                            btn.Text += " (Đường dẫn lỗi)";
                        }

                        btn.FlatAppearance.MouseOverBackColor = btn.BackColor;
                        btn.FlatAppearance.MouseDownBackColor = btn.BackColor;
                        btn.TabStop = false;

                        AllButtons.Add(btn);
                        buttonPanel.Controls.Add(btn);
                    }

                    colPanel.Controls.Add(buttonPanel);
                }

                tablePanel.Controls.Add(colPanel, col, 0);
            }

            this.Controls.Add(tablePanel);
        }
    }
}
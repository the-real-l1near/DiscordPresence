#nullable enable

namespace DiscordPresence;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;

    private System.Windows.Forms.Panel _informationPanel = null!;

    private System.Windows.Forms.PictureBox _projectIcon = null!;
    private System.Windows.Forms.Label _projectTitleLabel = null!;
    private System.Windows.Forms.Label _detectedProjectLabel = null!;

    private System.Windows.Forms.PictureBox _applicationIcon = null!;
    private System.Windows.Forms.Label _applicationTitleLabel = null!;
    private System.Windows.Forms.Label _detectedAppLabel = null!;

    private System.Windows.Forms.PictureBox _windowIcon = null!;
    private System.Windows.Forms.Label _windowTitleTitleLabel = null!;
    private System.Windows.Forms.Label _windowTitleLabel = null!;

    private System.Windows.Forms.Panel _suspectedGamePanel = null!;
    private System.Windows.Forms.PictureBox _suspectedGameIcon = null!;
    private System.Windows.Forms.Label _suspectedGameTitleLabel = null!;
    private System.Windows.Forms.Label _suspectedGameNameLabel = null!;
    private System.Windows.Forms.Label _suspectedGameProcessLabel = null!;

    private DiscordPresence.RoundedButton _suspectedGameYesButton = null!;
    private DiscordPresence.RoundedButton _suspectedGameNoButton = null!;
    private DiscordPresence.RoundedButton _suspectedGameLaterButton = null!;

    private System.Windows.Forms.CheckBox _startMinimizedCheckBox = null!;
    private System.Windows.Forms.CheckBox _startWithWindowsCheckBox = null!;

    private DiscordPresence.RoundedButton _manageGameOverridesButton = null!;
    private DiscordPresence.RoundedButton _refreshButton = null!;
    private DiscordPresence.RoundedButton _clearButton = null!;

    private System.Windows.Forms.PictureBox _statusIcon = null!;
    private System.Windows.Forms.Label _statusLabel = null!;

    // =========================================================
    // Dispose
    // =========================================================

    protected override void Dispose(
        bool disposing)
    {
        if (
            disposing &&
            components is not null
        )
        {
            components.Dispose();
        }

        base.Dispose(
            disposing
        );
    }

    // =========================================================
    // InitializeComponent
    // =========================================================

    private void InitializeComponent()
    {
        components =
            new System.ComponentModel.Container();

        _informationPanel =
            new System.Windows.Forms.Panel();

        _projectIcon =
            new System.Windows.Forms.PictureBox();

        _projectTitleLabel =
            new System.Windows.Forms.Label();

        _detectedProjectLabel =
            new System.Windows.Forms.Label();

        _applicationIcon =
            new System.Windows.Forms.PictureBox();

        _applicationTitleLabel =
            new System.Windows.Forms.Label();

        _detectedAppLabel =
            new System.Windows.Forms.Label();

        _windowIcon =
            new System.Windows.Forms.PictureBox();

        _windowTitleTitleLabel =
            new System.Windows.Forms.Label();

        _windowTitleLabel =
            new System.Windows.Forms.Label();

        _suspectedGamePanel =
            new System.Windows.Forms.Panel();

        _suspectedGameIcon =
            new System.Windows.Forms.PictureBox();

        _suspectedGameTitleLabel =
            new System.Windows.Forms.Label();

        _suspectedGameNameLabel =
            new System.Windows.Forms.Label();

        _suspectedGameProcessLabel =
            new System.Windows.Forms.Label();

        _suspectedGameYesButton =
            new DiscordPresence.RoundedButton();

        _suspectedGameNoButton =
            new DiscordPresence.RoundedButton();

        _suspectedGameLaterButton =
            new DiscordPresence.RoundedButton();

        _startMinimizedCheckBox =
            new System.Windows.Forms.CheckBox();

        _startWithWindowsCheckBox =
            new System.Windows.Forms.CheckBox();

        _manageGameOverridesButton =
            new DiscordPresence.RoundedButton();

        _refreshButton =
            new DiscordPresence.RoundedButton();

        _clearButton =
            new DiscordPresence.RoundedButton();

        _statusIcon =
            new System.Windows.Forms.PictureBox();

        _statusLabel =
            new System.Windows.Forms.Label();

        ((System.ComponentModel.ISupportInitialize)
            _projectIcon).BeginInit();

        ((System.ComponentModel.ISupportInitialize)
            _applicationIcon).BeginInit();

        ((System.ComponentModel.ISupportInitialize)
            _windowIcon).BeginInit();

        ((System.ComponentModel.ISupportInitialize)
            _suspectedGameIcon).BeginInit();

        ((System.ComponentModel.ISupportInitialize)
            _statusIcon).BeginInit();

        _informationPanel.SuspendLayout();
        _suspectedGamePanel.SuspendLayout();

        // =====================================================
        // Information panel
        // =====================================================

        _informationPanel.BackColor =
            System.Drawing.Color.FromArgb(
                241,
                245,
                249
            );

        _informationPanel.Location =
            new System.Drawing.Point(
                7,
                9
            );

        _informationPanel.Name =
            "_informationPanel";

        _informationPanel.Size =
            new System.Drawing.Size(
                373,
                185
            );

        _informationPanel.TabIndex =
            0;

        // =====================================================
        // Project icon
        // =====================================================

        _projectIcon.BackColor =
            System.Drawing.Color.Transparent;

        _projectIcon.Location =
            new System.Drawing.Point(
                3,
                3
            );

        _projectIcon.Name =
            "_projectIcon";

        _projectIcon.Size =
            new System.Drawing.Size(
                59,
                60
            );

        _projectIcon.SizeMode =
            System.Windows.Forms.PictureBoxSizeMode.Zoom;

        _projectIcon.TabIndex =
            0;

        _projectIcon.TabStop =
            false;

        // =====================================================
        // Project title
        // =====================================================

        _projectTitleLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _projectTitleLabel.Location =
            new System.Drawing.Point(
                72,
                12
            );

        _projectTitleLabel.Name =
            "_projectTitleLabel";

        _projectTitleLabel.Size =
            new System.Drawing.Size(
                160,
                20
            );

        _projectTitleLabel.TabIndex =
            1;

        _projectTitleLabel.Text =
            "Detected project";

        // =====================================================
        // Project value
        // =====================================================

        _detectedProjectLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                9F,
                System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point
            );

        _detectedProjectLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _detectedProjectLabel.Location =
            new System.Drawing.Point(
                72,
                33
            );

        _detectedProjectLabel.Name =
            "_detectedProjectLabel";

        _detectedProjectLabel.Size =
            new System.Drawing.Size(
                298,
                22
            );

        _detectedProjectLabel.TabIndex =
            2;

        _detectedProjectLabel.Text =
            "None";

        // =====================================================
        // Application icon
        // =====================================================

        _applicationIcon.BackColor =
            System.Drawing.Color.Transparent;

        _applicationIcon.Location =
            new System.Drawing.Point(
                3,
                64
            );

        _applicationIcon.Name =
            "_applicationIcon";

        _applicationIcon.Size =
            new System.Drawing.Size(
                59,
                58
            );

        _applicationIcon.SizeMode =
            System.Windows.Forms.PictureBoxSizeMode.Zoom;

        _applicationIcon.TabIndex =
            3;

        _applicationIcon.TabStop =
            false;

        // =====================================================
        // Application title
        // =====================================================

        _applicationTitleLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _applicationTitleLabel.Location =
            new System.Drawing.Point(
                72,
                72
            );

        _applicationTitleLabel.Name =
            "_applicationTitleLabel";

        _applicationTitleLabel.Size =
            new System.Drawing.Size(
                160,
                20
            );

        _applicationTitleLabel.TabIndex =
            4;

        _applicationTitleLabel.Text =
            "Detected application";

        // =====================================================
        // Application value
        // =====================================================

        _detectedAppLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                9F,
                System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point
            );

        _detectedAppLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _detectedAppLabel.Location =
            new System.Drawing.Point(
                72,
                93
            );

        _detectedAppLabel.Name =
            "_detectedAppLabel";

        _detectedAppLabel.Size =
            new System.Drawing.Size(
                298,
                22
            );

        _detectedAppLabel.TabIndex =
            5;

        _detectedAppLabel.Text =
            "Idle";

        // =====================================================
        // Window icon
        // =====================================================

        _windowIcon.BackColor =
            System.Drawing.Color.Transparent;

        _windowIcon.Location =
            new System.Drawing.Point(
                3,
                123
            );

        _windowIcon.Name =
            "_windowIcon";

        _windowIcon.Size =
            new System.Drawing.Size(
                59,
                60
            );

        _windowIcon.SizeMode =
            System.Windows.Forms.PictureBoxSizeMode.Zoom;

        _windowIcon.TabIndex =
            6;

        _windowIcon.TabStop =
            false;

        // =====================================================
        // Window title
        // =====================================================

        _windowTitleTitleLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _windowTitleTitleLabel.Location =
            new System.Drawing.Point(
                72,
                132
            );

        _windowTitleTitleLabel.Name =
            "_windowTitleTitleLabel";

        _windowTitleTitleLabel.Size =
            new System.Drawing.Size(
                160,
                20
            );

        _windowTitleTitleLabel.TabIndex =
            7;

        _windowTitleTitleLabel.Text =
            "Window title";

        // =====================================================
        // Window value
        // =====================================================

        _windowTitleLabel.AutoEllipsis =
            true;

        _windowTitleLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _windowTitleLabel.Location =
            new System.Drawing.Point(
                72,
                153
            );

        _windowTitleLabel.Name =
            "_windowTitleLabel";

        _windowTitleLabel.Size =
            new System.Drawing.Size(
                298,
                30
            );

        _windowTitleLabel.TabIndex =
            8;

        _windowTitleLabel.Text =
            "-";

        // =====================================================
        // Information children
        // =====================================================

        _informationPanel.Controls.Add(
            _projectIcon
        );

        _informationPanel.Controls.Add(
            _projectTitleLabel
        );

        _informationPanel.Controls.Add(
            _detectedProjectLabel
        );

        _informationPanel.Controls.Add(
            _applicationIcon
        );

        _informationPanel.Controls.Add(
            _applicationTitleLabel
        );

        _informationPanel.Controls.Add(
            _detectedAppLabel
        );

        _informationPanel.Controls.Add(
            _windowIcon
        );

        _informationPanel.Controls.Add(
            _windowTitleTitleLabel
        );

        _informationPanel.Controls.Add(
            _windowTitleLabel
        );

        // =====================================================
        // Suspected game panel
        // =====================================================

        _suspectedGamePanel.BackColor =
            System.Drawing.Color.FromArgb(
                244,
                243,
                255
            );

        _suspectedGamePanel.BorderStyle =
            System.Windows.Forms.BorderStyle.FixedSingle;

        _suspectedGamePanel.Location =
            new System.Drawing.Point(
                9,
                204
            );

        _suspectedGamePanel.Name =
            "_suspectedGamePanel";

        _suspectedGamePanel.Size =
            new System.Drawing.Size(
                373,
                120
            );

        _suspectedGamePanel.TabIndex =
            1;

        /*
         * Designer preview.
         * Runtime sẽ tự hide/show.
         */
        _suspectedGamePanel.Visible =
            true;

        // =====================================================
        // Suspected game icon
        // =====================================================

        _suspectedGameIcon.BackColor =
            System.Drawing.Color.Transparent;

        _suspectedGameIcon.Location =
            new System.Drawing.Point(
                10,
                11
            );

        _suspectedGameIcon.Name =
            "_suspectedGameIcon";

        _suspectedGameIcon.Size =
            new System.Drawing.Size(
                64,
                63
            );

        _suspectedGameIcon.SizeMode =
            System.Windows.Forms.PictureBoxSizeMode.Zoom;

        _suspectedGameIcon.TabIndex =
            0;

        _suspectedGameIcon.TabStop =
            false;

        // =====================================================
        // Suspected title
        // =====================================================

        _suspectedGameTitleLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                9F,
                System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point
            );

        _suspectedGameTitleLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                88,
                101,
                242
            );

        _suspectedGameTitleLabel.Location =
            new System.Drawing.Point(
                74,
                10
            );

        _suspectedGameTitleLabel.Name =
            "_suspectedGameTitleLabel";

        _suspectedGameTitleLabel.Size =
            new System.Drawing.Size(
                180,
                21
            );

        _suspectedGameTitleLabel.TabIndex =
            1;

        _suspectedGameTitleLabel.Text =
            "Suspected game";

        // =====================================================
        // Suspected question
        // =====================================================

        _suspectedGameNameLabel.AutoEllipsis =
            true;

        _suspectedGameNameLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _suspectedGameNameLabel.Location =
            new System.Drawing.Point(
                74,
                33
            );

        _suspectedGameNameLabel.Name =
            "_suspectedGameNameLabel";

        _suspectedGameNameLabel.Size =
            new System.Drawing.Size(
                280,
                20
            );

        _suspectedGameNameLabel.TabIndex =
            2;

        _suspectedGameNameLabel.Text =
            "Is \"SnippingTool.exe\" a game?";

        // =====================================================
        // Suspected window
        // =====================================================

        _suspectedGameProcessLabel.AutoEllipsis =
            true;

        _suspectedGameProcessLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _suspectedGameProcessLabel.Location =
            new System.Drawing.Point(
                74,
                54
            );

        _suspectedGameProcessLabel.Name =
            "_suspectedGameProcessLabel";

        _suspectedGameProcessLabel.Size =
            new System.Drawing.Size(
                280,
                20
            );

        _suspectedGameProcessLabel.TabIndex =
            3;

        _suspectedGameProcessLabel.Text =
            "Window: Snipping Tool Overlay";

        // =====================================================
        // Yes
        // =====================================================

        _suspectedGameYesButton.BackColor =
            System.Drawing.Color.FromArgb(
                88,
                101,
                242
            );

        _suspectedGameYesButton.CornerRadius =
            8;

        _suspectedGameYesButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                88,
                101,
                242
            );

        _suspectedGameYesButton.FlatAppearance.BorderSize =
            1;

        _suspectedGameYesButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                71,
                82,
                196
            );

        _suspectedGameYesButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                71,
                82,
                196
            );

        _suspectedGameYesButton.ForeColor =
            System.Drawing.Color.White;

        _suspectedGameYesButton.HoverForeColor =
            System.Drawing.Color.White;

        _suspectedGameYesButton.Location =
            new System.Drawing.Point(
                14,
                82
            );

        _suspectedGameYesButton.Name =
            "_suspectedGameYesButton";

        _suspectedGameYesButton.Size =
            new System.Drawing.Size(
                108,
                28
            );

        _suspectedGameYesButton.TabIndex =
            4;

        _suspectedGameYesButton.Text =
            "Yes";

        _suspectedGameYesButton.UseVisualStyleBackColor =
            false;

        // =====================================================
        // No
        // =====================================================

        _suspectedGameNoButton.BackColor =
            System.Drawing.Color.White;

        _suspectedGameNoButton.CornerRadius =
            8;

        _suspectedGameNoButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                210,
                216,
                226
            );

        _suspectedGameNoButton.FlatAppearance.BorderSize =
            1;

        _suspectedGameNoButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                241,
                245,
                249
            );

        _suspectedGameNoButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                232,
                234,
                255
            );

        _suspectedGameNoButton.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _suspectedGameNoButton.Location =
            new System.Drawing.Point(
                130,
                82
            );

        _suspectedGameNoButton.Name =
            "_suspectedGameNoButton";

        _suspectedGameNoButton.Size =
            new System.Drawing.Size(
                108,
                28
            );

        _suspectedGameNoButton.TabIndex =
            5;

        _suspectedGameNoButton.Text =
            "No";

        _suspectedGameNoButton.UseVisualStyleBackColor =
            false;

        // =====================================================
        // Later
        // =====================================================

        _suspectedGameLaterButton.BackColor =
            System.Drawing.Color.White;

        _suspectedGameLaterButton.CornerRadius =
            8;

        _suspectedGameLaterButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                210,
                216,
                226
            );

        _suspectedGameLaterButton.FlatAppearance.BorderSize =
            1;

        _suspectedGameLaterButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                241,
                245,
                249
            );

        _suspectedGameLaterButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                232,
                234,
                255
            );

        _suspectedGameLaterButton.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _suspectedGameLaterButton.Location =
            new System.Drawing.Point(
                246,
                82
            );

        _suspectedGameLaterButton.Name =
            "_suspectedGameLaterButton";

        _suspectedGameLaterButton.Size =
            new System.Drawing.Size(
                108,
                28
            );

        _suspectedGameLaterButton.TabIndex =
            6;

        _suspectedGameLaterButton.Text =
            "Later";

        _suspectedGameLaterButton.UseVisualStyleBackColor =
            false;

        // =====================================================
        // Suspected children
        // =====================================================

        _suspectedGamePanel.Controls.Add(
            _suspectedGameIcon
        );

        _suspectedGamePanel.Controls.Add(
            _suspectedGameTitleLabel
        );

        _suspectedGamePanel.Controls.Add(
            _suspectedGameNameLabel
        );

        _suspectedGamePanel.Controls.Add(
            _suspectedGameProcessLabel
        );

        _suspectedGamePanel.Controls.Add(
            _suspectedGameYesButton
        );

        _suspectedGamePanel.Controls.Add(
            _suspectedGameNoButton
        );

        _suspectedGamePanel.Controls.Add(
            _suspectedGameLaterButton
        );

        // =====================================================
        // Start minimized
        // =====================================================

        _startMinimizedCheckBox.AutoSize =
            true;

        _startMinimizedCheckBox.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _startMinimizedCheckBox.Location =
            new System.Drawing.Point(
                43,
                348
            );

        _startMinimizedCheckBox.Name =
            "_startMinimizedCheckBox";

        _startMinimizedCheckBox.Size =
            new System.Drawing.Size(
                111,
                19
            );

        _startMinimizedCheckBox.TabIndex =
            2;

        _startMinimizedCheckBox.Text =
            "Start minimized";

        _startMinimizedCheckBox.UseVisualStyleBackColor =
            true;

        // =====================================================
        // Start with Windows
        // =====================================================

        _startWithWindowsCheckBox.AutoSize =
            true;

        _startWithWindowsCheckBox.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _startWithWindowsCheckBox.Location =
            new System.Drawing.Point(
                196,
                348
            );

        _startWithWindowsCheckBox.Name =
            "_startWithWindowsCheckBox";

        _startWithWindowsCheckBox.Size =
            new System.Drawing.Size(
                128,
                19
            );

        _startWithWindowsCheckBox.TabIndex =
            3;

        _startWithWindowsCheckBox.Text =
            "Start with Windows";

        _startWithWindowsCheckBox.UseVisualStyleBackColor =
            true;

        // =====================================================
        // Manage game
        // =====================================================

        _manageGameOverridesButton.BackColor =
            System.Drawing.Color.FromArgb(
                226,
                232,
                240
            );

        _manageGameOverridesButton.CornerRadius =
            8;

        _manageGameOverridesButton.ContentSpacing =
            5;

        _manageGameOverridesButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                203,
                213,
                225
            );

        _manageGameOverridesButton.FlatAppearance.BorderSize =
            1;

        _manageGameOverridesButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                148,
                163,
                184
            );

        _manageGameOverridesButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _manageGameOverridesButton.ForeColor =
            System.Drawing.Color.FromArgb(
                71,
                85,
                105
            );

        _manageGameOverridesButton.HoverForeColor =
            System.Drawing.Color.White;

        _manageGameOverridesButton.ImageAlign =
            System.Drawing.ContentAlignment.MiddleCenter;

        _manageGameOverridesButton.Location =
            new System.Drawing.Point(
                28,
                367
            );

        _manageGameOverridesButton.Name =
            "_manageGameOverridesButton";

        _manageGameOverridesButton.Padding =
            System.Windows.Forms.Padding.Empty;

        _manageGameOverridesButton.Size =
            new System.Drawing.Size(
                334,
                34
            );

        _manageGameOverridesButton.TabIndex =
            4;

        _manageGameOverridesButton.Text =
            "Manage game";

        _manageGameOverridesButton.TextAlign =
            System.Drawing.ContentAlignment.MiddleCenter;

        _manageGameOverridesButton.TextImageRelation =
            System.Windows.Forms.TextImageRelation.ImageBeforeText;

        _manageGameOverridesButton.UseVisualStyleBackColor =
            false;

        // =====================================================
        // Refresh
        // =====================================================

        _refreshButton.BackColor =
            System.Drawing.Color.FromArgb(
                231,
                243,
                255
            );

        _refreshButton.CornerRadius =
            8;

        _refreshButton.ContentSpacing =
            5;

        _refreshButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                170,
                215,
                255
            );

        _refreshButton.FlatAppearance.BorderSize =
            1;

        _refreshButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                24,
                119,
                242
            );

        _refreshButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                13,
                92,
                182
            );

        _refreshButton.ForeColor =
            System.Drawing.Color.FromArgb(
                13,
                92,
                182
            );

        _refreshButton.HoverForeColor =
            System.Drawing.Color.White;

        _refreshButton.ImageAlign =
            System.Drawing.ContentAlignment.MiddleCenter;

        _refreshButton.Location =
            new System.Drawing.Point(
                28,
                407
            );

        _refreshButton.Name =
            "_refreshButton";

        _refreshButton.Padding =
            System.Windows.Forms.Padding.Empty;

        _refreshButton.Size =
            new System.Drawing.Size(
                191,
                34
            );

        _refreshButton.TabIndex =
            5;

        _refreshButton.Text =
            "Refresh Presence";

        _refreshButton.TextAlign =
            System.Drawing.ContentAlignment.MiddleCenter;

        _refreshButton.TextImageRelation =
            System.Windows.Forms.TextImageRelation.ImageBeforeText;

        _refreshButton.UseVisualStyleBackColor =
            false;

        // =====================================================
        // Clear
        // =====================================================

        _clearButton.BackColor =
            System.Drawing.Color.FromArgb(
                255,
                238,
                241
            );

        _clearButton.CornerRadius =
            8;

        _clearButton.ContentSpacing =
            5;

        _clearButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                246,
                190,
                200
            );

        _clearButton.FlatAppearance.BorderSize =
            1;

        _clearButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                220,
                53,
                69
            );

        _clearButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                190,
                40,
                55
            );

        _clearButton.ForeColor =
            System.Drawing.Color.FromArgb(
                220,
                53,
                69
            );

        _clearButton.HoverForeColor =
            System.Drawing.Color.White;

        _clearButton.ImageAlign =
            System.Drawing.ContentAlignment.MiddleCenter;

        _clearButton.Location =
            new System.Drawing.Point(
                229,
                407
            );

        _clearButton.Name =
            "_clearButton";

        _clearButton.Padding =
            System.Windows.Forms.Padding.Empty;

        _clearButton.Size =
            new System.Drawing.Size(
                133,
                34
            );

        _clearButton.TabIndex =
            6;

        _clearButton.Text =
            "Clear";

        _clearButton.TextAlign =
            System.Drawing.ContentAlignment.MiddleCenter;

        _clearButton.TextImageRelation =
            System.Windows.Forms.TextImageRelation.ImageBeforeText;

        _clearButton.UseVisualStyleBackColor =
            false;

        // =====================================================
        // Status icon
        // =====================================================

        _statusIcon.BackColor =
            System.Drawing.Color.Transparent;

        _statusIcon.Location =
            new System.Drawing.Point(59, 435);

        _statusIcon.Name =
            "_statusIcon";

        _statusIcon.Size =
            new System.Drawing.Size(
                20,
                20
            );

        _statusIcon.SizeMode =
            System.Windows.Forms.PictureBoxSizeMode.Zoom;

        _statusIcon.TabIndex =
            7;

        _statusIcon.TabStop =
            false;

        // =====================================================
        // Status label
        // =====================================================

        _statusLabel.AutoEllipsis =
            true;

        _statusLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _statusLabel.Location =
            new System.Drawing.Point(
                82,
                444
            );

        _statusLabel.Name =
            "_statusLabel";

        _statusLabel.Size =
            new System.Drawing.Size(
                242,
                22
            );

        _statusLabel.TabIndex =
            8;

        _statusLabel.Text =
            "Suspected game: SnippingTool.exe";

        // =====================================================
        // MainForm
        // =====================================================

        AutoScaleDimensions =
            new System.Drawing.SizeF(
                7F,
                15F
            );

        AutoScaleMode =
            System.Windows.Forms.AutoScaleMode.Font;

        BackColor =
            System.Drawing.Color.FromArgb(
                247,
                249,
                252
            );

        ClientSize =
            new System.Drawing.Size(392, 456);

        Controls.Add(
            _informationPanel
        );

        Controls.Add(
            _suspectedGamePanel
        );

        Controls.Add(
            _startMinimizedCheckBox
        );

        Controls.Add(
            _startWithWindowsCheckBox
        );

        Controls.Add(
            _manageGameOverridesButton
        );

        Controls.Add(
            _refreshButton
        );

        Controls.Add(
            _clearButton
        );

        Controls.Add(
            _statusIcon
        );

        Controls.Add(
            _statusLabel
        );

        Font =
            new System.Drawing.Font(
                "Segoe UI",
                9F
            );

        ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        FormBorderStyle =
            System.Windows.Forms.FormBorderStyle.FixedSingle;

        MaximizeBox =
            false;

        Name =
            "MainForm";

        StartPosition =
            System.Windows.Forms.FormStartPosition.CenterScreen;

        Text =
            "Custom Discord Presence";

        ((System.ComponentModel.ISupportInitialize)
            _projectIcon).EndInit();

        ((System.ComponentModel.ISupportInitialize)
            _applicationIcon).EndInit();

        ((System.ComponentModel.ISupportInitialize)
            _windowIcon).EndInit();

        ((System.ComponentModel.ISupportInitialize)
            _suspectedGameIcon).EndInit();

        ((System.ComponentModel.ISupportInitialize)
            _statusIcon).EndInit();

        _informationPanel.ResumeLayout(
            false
        );

        _suspectedGamePanel.ResumeLayout(
            false
        );

    }
}
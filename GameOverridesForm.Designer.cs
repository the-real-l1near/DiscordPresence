#nullable enable

namespace DiscordPresence;

partial class GameOverridesForm
{
    private System.ComponentModel.IContainer components = null!;

    // =========================================================
    // Form header
    // =========================================================

    private System.Windows.Forms.PictureBox _headerIcon = null!;
    private System.Windows.Forms.Label _titleLabel = null!;
    private System.Windows.Forms.Label _subtitleLabel = null!;

    // =========================================================
    // Included card
    // =========================================================

    private DiscordPresence.RoundedPanel _includedCard = null!;
    private System.Windows.Forms.Panel _includedHeader = null!;
    private System.Windows.Forms.PictureBox _includedHeaderIcon = null!;
    private System.Windows.Forms.Label _includedTitleLabel = null!;
    private System.Windows.Forms.Label _includedSubtitleLabel = null!;

    private DiscordPresence.RoundedPanel _includedCountBadge = null!;
    private System.Windows.Forms.Label _includedCountLabel = null!;

    private System.Windows.Forms.Panel _includedSeparator = null!;
    private System.Windows.Forms.ListBox _includedList = null!;

    private DiscordPresence.RoundedButton _removeIncludedButton = null!;

    // =========================================================
    // Excluded card
    // =========================================================

    private DiscordPresence.RoundedPanel _excludedCard = null!;
    private System.Windows.Forms.Panel _excludedHeader = null!;
    private System.Windows.Forms.PictureBox _excludedHeaderIcon = null!;
    private System.Windows.Forms.Label _excludedTitleLabel = null!;
    private System.Windows.Forms.Label _excludedSubtitleLabel = null!;

    private DiscordPresence.RoundedPanel _excludedCountBadge = null!;
    private System.Windows.Forms.Label _excludedCountLabel = null!;

    private System.Windows.Forms.Panel _excludedSeparator = null!;
    private System.Windows.Forms.ListBox _excludedList = null!;

    private DiscordPresence.RoundedButton _removeExcludedButton = null!;

    // =========================================================
    // Transfer
    // =========================================================

    private DiscordPresence.RoundedButton _moveToExcludedButton = null!;
    private DiscordPresence.RoundedButton _moveToIncludedButton = null!;

    // =========================================================
    // Hint
    // =========================================================

    private System.Windows.Forms.PictureBox _hintIcon = null!;
    private System.Windows.Forms.Label _hintLabel = null!;

    // =========================================================
    // Footer
    // =========================================================

    private System.Windows.Forms.Panel _footerDivider = null!;

    private DiscordPresence.RoundedButton _clearAllButton = null!;

    private System.Windows.Forms.PictureBox _savedIcon = null!;
    private System.Windows.Forms.Label _savedLabel = null!;

    private DiscordPresence.RoundedButton _doneButton = null!;

    // =========================================================
    // Dispose
    // =========================================================

    protected override void Dispose(
        bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(
            disposing
        );
    }

    // =========================================================
    // Initialize
    // =========================================================

    private void InitializeComponent()
    {
        components =
            new System.ComponentModel.Container();

        _headerIcon =
            new System.Windows.Forms.PictureBox();

        _titleLabel =
            new System.Windows.Forms.Label();

        _subtitleLabel =
            new System.Windows.Forms.Label();

        _includedCard =
            new DiscordPresence.RoundedPanel();

        _includedHeader =
            new System.Windows.Forms.Panel();

        _includedHeaderIcon =
            new System.Windows.Forms.PictureBox();

        _includedTitleLabel =
            new System.Windows.Forms.Label();

        _includedSubtitleLabel =
            new System.Windows.Forms.Label();

        _includedCountBadge =
            new DiscordPresence.RoundedPanel();

        _includedCountLabel =
            new System.Windows.Forms.Label();

        _includedSeparator =
            new System.Windows.Forms.Panel();

        _includedList =
            new System.Windows.Forms.ListBox();

        _removeIncludedButton =
            new DiscordPresence.RoundedButton();

        _excludedCard =
            new DiscordPresence.RoundedPanel();

        _excludedHeader =
            new System.Windows.Forms.Panel();

        _excludedHeaderIcon =
            new System.Windows.Forms.PictureBox();

        _excludedTitleLabel =
            new System.Windows.Forms.Label();

        _excludedSubtitleLabel =
            new System.Windows.Forms.Label();

        _excludedCountBadge =
            new DiscordPresence.RoundedPanel();

        _excludedCountLabel =
            new System.Windows.Forms.Label();

        _excludedSeparator =
            new System.Windows.Forms.Panel();

        _excludedList =
            new System.Windows.Forms.ListBox();

        _removeExcludedButton =
            new DiscordPresence.RoundedButton();

        _moveToExcludedButton =
            new DiscordPresence.RoundedButton();

        _moveToIncludedButton =
            new DiscordPresence.RoundedButton();

        _hintIcon =
            new System.Windows.Forms.PictureBox();

        _hintLabel =
            new System.Windows.Forms.Label();

        _footerDivider =
            new System.Windows.Forms.Panel();

        _clearAllButton =
            new DiscordPresence.RoundedButton();

        _savedIcon =
            new System.Windows.Forms.PictureBox();

        _savedLabel =
            new System.Windows.Forms.Label();

        _doneButton =
            new DiscordPresence.RoundedButton();

        ((System.ComponentModel.ISupportInitialize)
            _headerIcon).BeginInit();

        ((System.ComponentModel.ISupportInitialize)
            _includedHeaderIcon).BeginInit();

        ((System.ComponentModel.ISupportInitialize)
            _excludedHeaderIcon).BeginInit();

        ((System.ComponentModel.ISupportInitialize)
            _hintIcon).BeginInit();

        ((System.ComponentModel.ISupportInitialize)
            _savedIcon).BeginInit();

        _includedCard.SuspendLayout();
        _includedHeader.SuspendLayout();
        _includedCountBadge.SuspendLayout();

        _excludedCard.SuspendLayout();
        _excludedHeader.SuspendLayout();
        _excludedCountBadge.SuspendLayout();

        // =====================================================
        // Header icon
        // =====================================================

        _headerIcon.BackColor =
            System.Drawing.Color.Transparent;

        _headerIcon.Location =
            new System.Drawing.Point(29, -1);

        _headerIcon.Name =
            "_headerIcon";

        _headerIcon.Size =
            new System.Drawing.Size(
                72,
                72
            );

        _headerIcon.SizeMode =
            System.Windows.Forms.PictureBoxSizeMode.Zoom;

        _headerIcon.TabIndex =
            0;

        _headerIcon.TabStop =
            false;

        // =====================================================
        // Title
        // =====================================================

        _titleLabel.AutoSize =
            true;

        _titleLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                14F,
                System.Drawing.FontStyle.Bold
            );

        _titleLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _titleLabel.Location =
            new System.Drawing.Point(107, 10);

        _titleLabel.Name =
            "_titleLabel";

        _titleLabel.TabIndex =
            1;

        _titleLabel.Text =
            "Manage game";

        // =====================================================
        // Subtitle
        // =====================================================

        _subtitleLabel.AutoSize =
            true;

        _subtitleLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                9.5F
            );

        _subtitleLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _subtitleLabel.Location =
            new System.Drawing.Point(108, 36);

        _subtitleLabel.Name =
            "_subtitleLabel";

        _subtitleLabel.TabIndex =
            2;

        _subtitleLabel.Text =
            "Control how your apps appear on Discord.";

        // =====================================================
        // Included card
        // =====================================================

        _includedCard.BackColor =
            System.Drawing.Color.White;

        _includedCard.BorderColor =
            System.Drawing.Color.FromArgb(
                203,
                213,
                225
            );

        _includedCard.BorderSize =
            1;

        _includedCard.CornerRadius =
            9;

        _includedCard.Location =
            new System.Drawing.Point(23, 71);

        _includedCard.Name =
            "_includedCard";

        _includedCard.Size =
            new System.Drawing.Size(
                348,
                390
            );

        _includedCard.TabIndex =
            3;

        // =====================================================
        // Included header
        // =====================================================

        _includedHeader.BackColor =
            System.Drawing.Color.FromArgb(
                244,
                243,
                255
            );

        _includedHeader.Location =
            new System.Drawing.Point(
                1,
                1
            );

        _includedHeader.Name =
            "_includedHeader";

        _includedHeader.Size =
            new System.Drawing.Size(
                346,
                77
            );

        _includedHeader.TabIndex =
            0;

        // =====================================================
        // Included header icon
        // =====================================================

        _includedHeaderIcon.BackColor =
            System.Drawing.Color.Transparent;

        _includedHeaderIcon.Location =
            new System.Drawing.Point(16, 15);

        _includedHeaderIcon.Name =
            "_includedHeaderIcon";

        _includedHeaderIcon.Size =
            new System.Drawing.Size(
                46,
                46
            );

        _includedHeaderIcon.SizeMode =
            System.Windows.Forms.PictureBoxSizeMode.Zoom;

        _includedHeaderIcon.TabIndex =
            0;

        _includedHeaderIcon.TabStop =
            false;

        // =====================================================
        // Included title
        // =====================================================

        _includedTitleLabel.AutoSize =
            true;

        _includedTitleLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                9.5F,
                System.Drawing.FontStyle.Bold
            );

        _includedTitleLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _includedTitleLabel.Location =
            new System.Drawing.Point(70, 23);

        _includedTitleLabel.Name =
            "_includedTitleLabel";

        _includedTitleLabel.TabIndex =
            1;

        _includedTitleLabel.Text =
            "Always a game";

        // =====================================================
        // Included subtitle
        // =====================================================

        _includedSubtitleLabel.AutoSize =
            true;

        _includedSubtitleLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                8.5F
            );

        _includedSubtitleLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _includedSubtitleLabel.Location =
            new System.Drawing.Point(70, 38);

        _includedSubtitleLabel.Name =
            "_includedSubtitleLabel";

        _includedSubtitleLabel.TabIndex =
            2;

        _includedSubtitleLabel.Text =
            "Show game activity for these apps.";

        // =====================================================
        // Included count badge
        // =====================================================

        _includedCountBadge.BackColor =
            System.Drawing.Color.FromArgb(
                238,
                242,
                247
            );

        _includedCountBadge.BorderColor =
            System.Drawing.Color.FromArgb(
                214,
                222,
                232
            );

        _includedCountBadge.BorderSize =
            1;

        _includedCountBadge.CornerRadius =
            12;

        _includedCountBadge.Location =
            new System.Drawing.Point(
                304,
                26
            );

        _includedCountBadge.Name =
            "_includedCountBadge";

        _includedCountBadge.Size =
            new System.Drawing.Size(
                28,
                24
            );

        _includedCountBadge.TabIndex =
            3;

        // =====================================================
        // Included count
        // =====================================================

        _includedCountLabel.Dock =
            System.Windows.Forms.DockStyle.Fill;

        _includedCountLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                8F,
                System.Drawing.FontStyle.Bold
            );

        _includedCountLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                71,
                85,
                105
            );

        _includedCountLabel.Name =
            "_includedCountLabel";

        _includedCountLabel.TabIndex =
            0;

        _includedCountLabel.Text =
            "0";

        _includedCountLabel.TextAlign =
            System.Drawing.ContentAlignment.MiddleCenter;

        _includedCountBadge.Controls.Add(
            _includedCountLabel
        );

        // =====================================================
        // Included header children
        // =====================================================

        _includedHeader.Controls.Add(
            _includedHeaderIcon
        );

        _includedHeader.Controls.Add(
            _includedTitleLabel
        );

        _includedHeader.Controls.Add(
            _includedSubtitleLabel
        );

        _includedHeader.Controls.Add(
            _includedCountBadge
        );

        // =====================================================
        // Included separator
        // =====================================================

        _includedSeparator.BackColor =
            System.Drawing.Color.FromArgb(
                226,
                232,
                240
            );

        _includedSeparator.Location =
            new System.Drawing.Point(
                1,
                78
            );

        _includedSeparator.Name =
            "_includedSeparator";

        _includedSeparator.Size =
            new System.Drawing.Size(
                346,
                1
            );

        _includedSeparator.TabIndex =
            1;

        // =====================================================
        // Included list
        // =====================================================

        _includedList.BackColor =
            System.Drawing.Color.White;

        _includedList.BorderStyle =
            System.Windows.Forms.BorderStyle.None;

        _includedList.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _includedList.IntegralHeight =
            false;

        _includedList.Location =
            new System.Drawing.Point(
                1,
                79
            );

        _includedList.Name =
            "_includedList";

        _includedList.SelectionMode =
            System.Windows.Forms.SelectionMode.MultiExtended;

        _includedList.Size =
            new System.Drawing.Size(
                346,
                252
            );

        _includedList.TabIndex =
            0;

        // =====================================================
        // Remove included
        // =====================================================

        _removeIncludedButton.BackColor =
            System.Drawing.Color.White;

        _removeIncludedButton.ContentSpacing =
            7;

        _removeIncludedButton.CornerRadius =
            8;

        _removeIncludedButton.Enabled =
            false;

        _removeIncludedButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                203,
                213,
                225
            );

        _removeIncludedButton.FlatAppearance.BorderSize =
            1;

        _removeIncludedButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                190,
                40,
                55
            );

        _removeIncludedButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                220,
                53,
                69
            );

        _removeIncludedButton.ForeColor =
            System.Drawing.Color.FromArgb(
                148,
                163,
                184
            );

        _removeIncludedButton.HoverForeColor =
            System.Drawing.Color.White;

        _removeIncludedButton.Location =
            new System.Drawing.Point(
                14,
                344
            );

        _removeIncludedButton.Name =
            "_removeIncludedButton";

        _removeIncludedButton.Size =
            new System.Drawing.Size(
                320,
                32
            );

        _removeIncludedButton.TabIndex =
            2;

        _removeIncludedButton.Text =
            "Remove selected";

        // =====================================================
        // Included card children
        // =====================================================

        _includedCard.Controls.Add(
            _includedHeader
        );

        _includedCard.Controls.Add(
            _includedSeparator
        );

        _includedCard.Controls.Add(
            _includedList
        );

        _includedCard.Controls.Add(
            _removeIncludedButton
        );

        // =====================================================
        // Excluded card
        // =====================================================

        _excludedCard.BackColor =
            System.Drawing.Color.White;

        _excludedCard.BorderColor =
            System.Drawing.Color.FromArgb(
                203,
                213,
                225
            );

        _excludedCard.BorderSize =
            1;

        _excludedCard.CornerRadius =
            9;

        _excludedCard.Location =
            new System.Drawing.Point(447, 71);

        _excludedCard.Name =
            "_excludedCard";

        _excludedCard.Size =
            new System.Drawing.Size(
                348,
                390
            );

        _excludedCard.TabIndex =
            4;

        // =====================================================
        // Excluded header
        // =====================================================

        _excludedHeader.BackColor =
            System.Drawing.Color.FromArgb(
                248,
                250,
                252
            );

        _excludedHeader.Location =
            new System.Drawing.Point(
                1,
                1
            );

        _excludedHeader.Name =
            "_excludedHeader";

        _excludedHeader.Size =
            new System.Drawing.Size(
                346,
                77
            );

        _excludedHeader.TabIndex =
            0;

        // =====================================================
        // Excluded header icon
        // =====================================================

        _excludedHeaderIcon.BackColor =
            System.Drawing.Color.Transparent;

        _excludedHeaderIcon.Location =
            new System.Drawing.Point(17, 17);

        _excludedHeaderIcon.Name =
            "_excludedHeaderIcon";

        _excludedHeaderIcon.Size =
            new System.Drawing.Size(
                44,
                44
            );

        _excludedHeaderIcon.SizeMode =
            System.Windows.Forms.PictureBoxSizeMode.Zoom;

        _excludedHeaderIcon.TabIndex =
            0;

        _excludedHeaderIcon.TabStop =
            false;

        // =====================================================
        // Excluded title
        // =====================================================

        _excludedTitleLabel.AutoSize =
            true;

        _excludedTitleLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                9.5F,
                System.Drawing.FontStyle.Bold
            );

        _excludedTitleLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _excludedTitleLabel.Location =
            new System.Drawing.Point(67, 22);

        _excludedTitleLabel.Name =
            "_excludedTitleLabel";

        _excludedTitleLabel.TabIndex =
            1;

        _excludedTitleLabel.Text =
            "Never a game";

        // =====================================================
        // Excluded subtitle
        // =====================================================

        _excludedSubtitleLabel.AutoSize =
            true;

        _excludedSubtitleLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                8.5F
            );

        _excludedSubtitleLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _excludedSubtitleLabel.Location =
            new System.Drawing.Point(67, 38);

        _excludedSubtitleLabel.Name =
            "_excludedSubtitleLabel";

        _excludedSubtitleLabel.TabIndex =
            2;

        _excludedSubtitleLabel.Text =
            "Exclude these apps from detection.";

        // =====================================================
        // Excluded count badge
        // =====================================================

        _excludedCountBadge.BackColor =
            System.Drawing.Color.FromArgb(
                238,
                242,
                247
            );

        _excludedCountBadge.BorderColor =
            System.Drawing.Color.FromArgb(
                214,
                222,
                232
            );

        _excludedCountBadge.BorderSize =
            1;

        _excludedCountBadge.CornerRadius =
            12;

        _excludedCountBadge.Location =
            new System.Drawing.Point(
                304,
                26
            );

        _excludedCountBadge.Name =
            "_excludedCountBadge";

        _excludedCountBadge.Size =
            new System.Drawing.Size(
                28,
                24
            );

        _excludedCountBadge.TabIndex =
            3;

        // =====================================================
        // Excluded count
        // =====================================================

        _excludedCountLabel.Dock =
            System.Windows.Forms.DockStyle.Fill;

        _excludedCountLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                8F,
                System.Drawing.FontStyle.Bold
            );

        _excludedCountLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                71,
                85,
                105
            );

        _excludedCountLabel.Name =
            "_excludedCountLabel";

        _excludedCountLabel.TabIndex =
            0;

        _excludedCountLabel.Text =
            "0";

        _excludedCountLabel.TextAlign =
            System.Drawing.ContentAlignment.MiddleCenter;

        _excludedCountBadge.Controls.Add(
            _excludedCountLabel
        );

        // =====================================================
        // Excluded header children
        // =====================================================

        _excludedHeader.Controls.Add(
            _excludedHeaderIcon
        );

        _excludedHeader.Controls.Add(
            _excludedTitleLabel
        );

        _excludedHeader.Controls.Add(
            _excludedSubtitleLabel
        );

        _excludedHeader.Controls.Add(
            _excludedCountBadge
        );

        // =====================================================
        // Excluded separator
        // =====================================================

        _excludedSeparator.BackColor =
            System.Drawing.Color.FromArgb(
                226,
                232,
                240
            );

        _excludedSeparator.Location =
            new System.Drawing.Point(
                1,
                78
            );

        _excludedSeparator.Name =
            "_excludedSeparator";

        _excludedSeparator.Size =
            new System.Drawing.Size(
                346,
                1
            );

        _excludedSeparator.TabIndex =
            1;

        // =====================================================
        // Excluded list
        // =====================================================

        _excludedList.BackColor =
            System.Drawing.Color.White;

        _excludedList.BorderStyle =
            System.Windows.Forms.BorderStyle.None;

        _excludedList.ForeColor =
            System.Drawing.Color.FromArgb(
                30,
                41,
                59
            );

        _excludedList.IntegralHeight =
            false;

        _excludedList.Location =
            new System.Drawing.Point(
                1,
                79
            );

        _excludedList.Name =
            "_excludedList";

        _excludedList.SelectionMode =
            System.Windows.Forms.SelectionMode.MultiExtended;

        _excludedList.Size =
            new System.Drawing.Size(
                346,
                252
            );

        _excludedList.TabIndex =
            1;

        // =====================================================
        // Remove excluded
        // =====================================================

        _removeExcludedButton.BackColor =
            System.Drawing.Color.White;

        _removeExcludedButton.ContentSpacing =
            7;

        _removeExcludedButton.CornerRadius =
            8;

        _removeExcludedButton.Enabled =
            false;

        _removeExcludedButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                203,
                213,
                225
            );

        _removeExcludedButton.FlatAppearance.BorderSize =
            1;

        _removeExcludedButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                190,
                40,
                55
            );

        _removeExcludedButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                220,
                53,
                69
            );

        _removeExcludedButton.ForeColor =
            System.Drawing.Color.FromArgb(
                148,
                163,
                184
            );

        _removeExcludedButton.HoverForeColor =
            System.Drawing.Color.White;

        _removeExcludedButton.Location =
            new System.Drawing.Point(
                14,
                344
            );

        _removeExcludedButton.Name =
            "_removeExcludedButton";

        _removeExcludedButton.Size =
            new System.Drawing.Size(
                320,
                32
            );

        _removeExcludedButton.TabIndex =
            2;

        _removeExcludedButton.Text =
            "Remove selected";

        // =====================================================
        // Excluded card children
        // =====================================================

        _excludedCard.Controls.Add(
            _excludedHeader
        );

        _excludedCard.Controls.Add(
            _excludedSeparator
        );

        _excludedCard.Controls.Add(
            _excludedList
        );

        _excludedCard.Controls.Add(
            _removeExcludedButton
        );

        // =====================================================
        // Move to excluded
        // =====================================================

        _moveToExcludedButton.BackColor =
            System.Drawing.Color.FromArgb(
                248,
                250,
                252
            );

        _moveToExcludedButton.CornerRadius =
            8;

        _moveToExcludedButton.Enabled =
            false;

        _moveToExcludedButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                203,
                213,
                225
            );

        _moveToExcludedButton.FlatAppearance.BorderSize =
            1;

        _moveToExcludedButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                71,
                82,
                196
            );

        _moveToExcludedButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                88,
                101,
                242
            );

        _moveToExcludedButton.Font =
            new System.Drawing.Font(
                "Segoe UI Symbol",
                14F
            );

        _moveToExcludedButton.ForeColor =
            System.Drawing.Color.FromArgb(
                148,
                163,
                184
            );

        _moveToExcludedButton.HoverForeColor =
            System.Drawing.Color.White;

        _moveToExcludedButton.Location =
            new System.Drawing.Point(388, 187);

        _moveToExcludedButton.Name =
            "_moveToExcludedButton";

        _moveToExcludedButton.Size =
            new System.Drawing.Size(
                42,
                42
            );

        _moveToExcludedButton.TabIndex =
            5;

        _moveToExcludedButton.Text =
            "→";

        // =====================================================
        // Move to included
        // =====================================================

        _moveToIncludedButton.BackColor =
            System.Drawing.Color.FromArgb(
                248,
                250,
                252
            );

        _moveToIncludedButton.CornerRadius =
            8;

        _moveToIncludedButton.Enabled =
            false;

        _moveToIncludedButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                203,
                213,
                225
            );

        _moveToIncludedButton.FlatAppearance.BorderSize =
            1;

        _moveToIncludedButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                203,
                213,
                225
            );

        _moveToIncludedButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                226,
                232,
                240
            );

        _moveToIncludedButton.Font =
            new System.Drawing.Font(
                "Segoe UI Symbol",
                14F
            );

        _moveToIncludedButton.ForeColor =
            System.Drawing.Color.FromArgb(
                148,
                163,
                184
            );

        _moveToIncludedButton.Location =
            new System.Drawing.Point(388, 239);

        _moveToIncludedButton.Name =
            "_moveToIncludedButton";

        _moveToIncludedButton.Size =
            new System.Drawing.Size(
                42,
                42
            );

        _moveToIncludedButton.TabIndex =
            6;

        _moveToIncludedButton.Text =
            "←";

        // =====================================================
        // Hint
        // =====================================================

        _hintIcon.BackColor =
            System.Drawing.Color.Transparent;

        _hintIcon.Location =
            new System.Drawing.Point(25, 474);

        _hintIcon.Name =
            "_hintIcon";

        _hintIcon.Size =
            new System.Drawing.Size(
                16,
                16
            );

        _hintIcon.SizeMode =
            System.Windows.Forms.PictureBoxSizeMode.Zoom;

        _hintIcon.TabIndex =
            7;

        _hintIcon.TabStop =
            false;

        _hintLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                8.5F
            );

        _hintLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _hintLabel.Location =
            new System.Drawing.Point(49, 472);

        _hintLabel.Name =
            "_hintLabel";

        _hintLabel.Size =
            new System.Drawing.Size(
                420,
                21
            );

        _hintLabel.TabIndex =
            8;

        _hintLabel.Text =
            "Remove an app to restore automatic detection.";

        // =====================================================
        // Footer divider
        // =====================================================

        _footerDivider.BackColor =
            System.Drawing.Color.FromArgb(
                226,
                232,
                240
            );

        _footerDivider.Location =
            new System.Drawing.Point(23, 495);

        _footerDivider.Name =
            "_footerDivider";

        _footerDivider.Size =
            new System.Drawing.Size(
                772,
                1
            );

        _footerDivider.TabIndex =
            9;

        // =====================================================
        // Clear all
        // =====================================================

        _clearAllButton.BackColor =
            System.Drawing.Color.White;

        _clearAllButton.ContentSpacing =
            7;

        _clearAllButton.CornerRadius =
            8;

        _clearAllButton.Enabled =
            false;

        _clearAllButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                203,
                213,
                225
            );

        _clearAllButton.FlatAppearance.BorderSize =
            1;

        _clearAllButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                190,
                40,
                55
            );

        _clearAllButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                220,
                53,
                69
            );

        _clearAllButton.ForeColor =
            System.Drawing.Color.FromArgb(
                148,
                163,
                184
            );

        _clearAllButton.HoverForeColor =
            System.Drawing.Color.White;

        _clearAllButton.Location =
            new System.Drawing.Point(23, 506);

        _clearAllButton.Name =
            "_clearAllButton";

        _clearAllButton.Size =
            new System.Drawing.Size(
                178,
                36
            );

        _clearAllButton.TabIndex =
            10;

        _clearAllButton.Text =
            "Clear all overrides";

        // =====================================================
        // Saved
        // =====================================================

        _savedIcon.BackColor =
            System.Drawing.Color.Transparent;

        _savedIcon.Location =
            new System.Drawing.Point(314, 509);

        _savedIcon.Name =
            "_savedIcon";

        _savedIcon.Size =
            new System.Drawing.Size(24, 26);

        _savedIcon.SizeMode =
            System.Windows.Forms.PictureBoxSizeMode.Zoom;

        _savedIcon.TabIndex =
            11;

        _savedIcon.TabStop =
            false;

        _savedLabel.Font =
            new System.Drawing.Font(
                "Segoe UI",
                8.5F
            );

        _savedLabel.ForeColor =
            System.Drawing.Color.FromArgb(
                100,
                116,
                139
            );

        _savedLabel.Location =
            new System.Drawing.Point(341, 515);

        _savedLabel.Name =
            "_savedLabel";

        _savedLabel.Size =
            new System.Drawing.Size(178, 21);

        _savedLabel.TabIndex =
            12;

        _savedLabel.Text =
            "Changes saved automatically";

        // =====================================================
        // Done
        // =====================================================

        _doneButton.BackColor =
            System.Drawing.Color.FromArgb(
                88,
                101,
                242
            );

        _doneButton.CornerRadius =
            8;

        _doneButton.DialogResult =
            System.Windows.Forms.DialogResult.OK;

        _doneButton.FlatAppearance.BorderColor =
            System.Drawing.Color.FromArgb(
                88,
                101,
                242
            );

        _doneButton.FlatAppearance.BorderSize =
            1;

        _doneButton.FlatAppearance.MouseDownBackColor =
            System.Drawing.Color.FromArgb(
                71,
                82,
                196
            );

        _doneButton.FlatAppearance.MouseOverBackColor =
            System.Drawing.Color.FromArgb(
                71,
                82,
                196
            );

        _doneButton.ForeColor =
            System.Drawing.Color.White;

        _doneButton.HoverForeColor =
            System.Drawing.Color.White;

        _doneButton.Location =
            new System.Drawing.Point(681, 506);

        _doneButton.Name =
            "_doneButton";

        _doneButton.Size =
            new System.Drawing.Size(
                114,
                36
            );

        _doneButton.TabIndex =
            13;

        _doneButton.Text =
            "Done";

        // =====================================================
        // Form
        // =====================================================

        AcceptButton =
            _doneButton;

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

        CancelButton =
            _doneButton;

        ClientSize =
            new System.Drawing.Size(820, 552);

        Controls.Add(
            _headerIcon
        );

        Controls.Add(
            _titleLabel
        );

        Controls.Add(
            _subtitleLabel
        );

        Controls.Add(
            _includedCard
        );

        Controls.Add(
            _excludedCard
        );

        Controls.Add(
            _moveToExcludedButton
        );

        Controls.Add(
            _moveToIncludedButton
        );

        Controls.Add(
            _hintIcon
        );

        Controls.Add(
            _hintLabel
        );

        Controls.Add(
            _footerDivider
        );

        Controls.Add(
            _clearAllButton
        );

        Controls.Add(
            _savedIcon
        );

        Controls.Add(
            _savedLabel
        );

        Controls.Add(
            _doneButton
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

        MinimizeBox =
            true;

        Name =
            "GameOverridesForm";

        ShowInTaskbar =
            false;

        StartPosition =
            System.Windows.Forms.FormStartPosition.CenterParent;

        Text =
            "Custom Discord Presence";

        ((System.ComponentModel.ISupportInitialize)
            _headerIcon).EndInit();

        ((System.ComponentModel.ISupportInitialize)
            _includedHeaderIcon).EndInit();

        ((System.ComponentModel.ISupportInitialize)
            _excludedHeaderIcon).EndInit();

        ((System.ComponentModel.ISupportInitialize)
            _hintIcon).EndInit();

        ((System.ComponentModel.ISupportInitialize)
            _savedIcon).EndInit();

        _includedCountBadge.ResumeLayout(
            false
        );

        _includedHeader.ResumeLayout(
            false
        );


        _includedCard.ResumeLayout(
            false
        );

        _excludedCountBadge.ResumeLayout(
            false
        );

        _excludedHeader.ResumeLayout(
            false
        );


        _excludedCard.ResumeLayout(
            false
        );

    }
}
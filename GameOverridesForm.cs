using System.Drawing.Drawing2D;

namespace DiscordPresence;

internal sealed partial class GameOverridesForm : Form
{
    // =========================================================
    // Controller
    // =========================================================

    private readonly PresenceController
        _presenceController = null!;

    // =========================================================
    // Refresh
    // =========================================================

    private readonly System.Windows.Forms.Timer?
        _refreshTimer;

    private readonly System.Windows.Forms.Timer?
        _savedIndicatorTimer;

    // =========================================================
    // Selection
    // =========================================================

    private bool
        _isUpdatingSelection;

    // =========================================================
    // Process icons
    // =========================================================

    private readonly Dictionary<
        (ListBox List, int Index),
        Image>
        _paintedIcons =
            new();

    // =========================================================
    // Snapshots
    // =========================================================

    private string[]
        _lastIncludedSnapshot = [];

    private string[]
        _lastExcludedSnapshot = [];

    // =========================================================
    // Designer constructor
    // =========================================================

    public GameOverridesForm()
    {
        SuspendLayout();

        try
        {
            InitializeComponent();

            ApplyHeaderRegions();
        }
        finally
        {
            ResumeLayout(
                true
            );
        }
    }

    // =========================================================
    // Runtime constructor
    // =========================================================

    public GameOverridesForm(
        PresenceController presenceController)
        : this()
    {
        _presenceController =
            presenceController;

        ApplyRuntimeAssets();

        WireEvents();

        ConfigureLists();

        RefreshLists(
            force: true
        );

        _savedIcon.Visible =
            false;

        _savedLabel.Visible =
            false;

        _savedIndicatorTimer =
            new System.Windows.Forms.Timer(
                components
            )
            {
                Interval =
                    1600
            };

        _savedIndicatorTimer.Tick += (_, _) =>
        {
            _savedIndicatorTimer.Stop();

            _savedIcon.Visible =
                false;

            _savedLabel.Visible =
                false;
        };

        _refreshTimer =
            new System.Windows.Forms.Timer(
                components
            )
            {
                Interval =
                    500
            };

        _refreshTimer.Tick += (_, _) =>
        {
            RefreshLists();

            RefreshChangedIcons(
                _includedList
            );

            RefreshChangedIcons(
                _excludedList
            );
        };

        _refreshTimer.Start();
    }

    // =========================================================
    // Header regions
    // =========================================================

    private void ApplyHeaderRegions()
    {
        ApplyTopRoundedRegion(
            _includedHeader,
            8
        );

        ApplyTopRoundedRegion(
            _excludedHeader,
            8
        );
    }

    private static void ApplyTopRoundedRegion(
        Control control,
        int radius)
    {
        if (
            control.Width <= 0 ||
            control.Height <= 0
        )
        {
            return;
        }

        using var path =
            CreateTopRoundedRectanglePath(
                new RectangleF(
                    0,
                    0,
                    control.Width,
                    control.Height
                ),
                radius
            );

        var newRegion =
            new Region(
                path
            );

        var oldRegion =
            control.Region;

        control.Region =
            newRegion;

        oldRegion?.Dispose();
    }

    // =========================================================
    // Runtime assets
    // =========================================================

    private void ApplyRuntimeAssets()
    {
        // -----------------------------------------------------
        // PictureBox icons
        //
        // Match MainForm exactly: render the SVG at the same
        // pixel size as the PictureBox instead of rasterizing
        // smaller and letting PictureBox upscale it.
        // -----------------------------------------------------

        _headerIcon.Image =
            UiAssets.Gamepad(
                Math.Min(
                    _headerIcon.Width,
                    _headerIcon.Height
                )
            );

        _includedHeaderIcon.Image =
            UiAssets.Gamepad(
                Math.Min(
                    _includedHeaderIcon.Width,
                    _includedHeaderIcon.Height
                )
            );

        _excludedHeaderIcon.Image =
            UiAssets.Ban(
                Math.Min(
                    _excludedHeaderIcon.Width,
                    _excludedHeaderIcon.Height
                )
            );

        _hintIcon.Image =
            UiAssets.Info(
                Math.Min(
                    _hintIcon.Width,
                    _hintIcon.Height
                )
            );

        _savedIcon.Image =
            UiAssets.Check(
                Math.Min(
                    _savedIcon.Width,
                    _savedIcon.Height
                )
            );

        // -----------------------------------------------------
        // Button icons
        //
        // MainForm also uses explicit small icon sizes for
        // buttons, so keep that same pattern here.
        // -----------------------------------------------------

        _removeIncludedButton.Image =
            UiAssets.TrashOutline(
                24
            );

        _removeIncludedButton.HoverImage =
            UiAssets.TrashOutlineHover(
                24
            );

        _removeExcludedButton.Image =
            UiAssets.TrashOutline(
                24
            );

        _removeExcludedButton.HoverImage =
            UiAssets.TrashOutlineHover(
                24
            );

        _clearAllButton.Image =
            UiAssets.TrashOutline(
                26
            );

        _clearAllButton.HoverImage =
            UiAssets.TrashOutlineHover(
                26
            );
    }

    // =========================================================
    // Saved indicator
    // =========================================================

    private void ShowSavedIndicator()
    {
        _savedIndicatorTimer?.Stop();

        _savedIcon.Visible =
            true;

        _savedLabel.Visible =
            true;

        _savedIndicatorTimer?.Start();
    }

    // =========================================================
    // Events
    // =========================================================

    private void WireEvents()
    {
        _includedList.SelectedIndexChanged += (_, _) =>
        {
            HandleIncludedSelectionChanged();
        };

        _excludedList.SelectedIndexChanged += (_, _) =>
        {
            HandleExcludedSelectionChanged();
        };

        _moveToExcludedButton.Click += (_, _) =>
        {
            MoveIncludedToExcluded();
        };

        _moveToIncludedButton.Click += (_, _) =>
        {
            MoveExcludedToIncluded();
        };

        _removeIncludedButton.Click += (_, _) =>
        {
            RemoveSelectedIncluded();
        };

        _removeExcludedButton.Click += (_, _) =>
        {
            RemoveSelectedExcluded();
        };

        _clearAllButton.Click += (_, _) =>
        {
            ClearAllOverrides();
        };
    }

    // =========================================================
    // Lists
    // =========================================================

    private void ConfigureLists()
    {
        ConfigureList(
            _includedList
        );

        ConfigureList(
            _excludedList
        );
    }

    private void ConfigureList(
        ListBox list)
    {
        EnableDoubleBuffer(
            list
        );

        list.DrawMode =
            DrawMode.OwnerDrawFixed;

        list.ItemHeight =
            Math.Max(
                46,
                list.Font.Height + 26
            );

        list.DrawItem += (_, e) =>
        {
            DrawListItem(
                list,
                e
            );
        };
    }

    private static void EnableDoubleBuffer(
        Control control)
    {
        typeof(Control)
            .GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic
            )
            ?.SetValue(
                control,
                true
            );
    }

    // =========================================================
    // List drawing
    // =========================================================

    private void DrawListItem(
        ListBox list,
        DrawItemEventArgs e)
    {
        if (e.Index < 0)
        {
            return;
        }

        var selected =
            (
                e.State &
                DrawItemState.Selected
            ) != 0;

        var bounds =
            e.Bounds;

        e.Graphics.SmoothingMode =
            SmoothingMode.AntiAlias;

        // -----------------------------------------------------
        // Background
        // -----------------------------------------------------

        using (
            var backgroundBrush =
                new SolidBrush(
                    selected
                        ? Color.FromArgb(
                            238,
                            236,
                            255
                        )
                        : Color.White
                )
        )
        {
            e.Graphics.FillRectangle(
                backgroundBrush,
                bounds
            );
        }

        // -----------------------------------------------------
        // Selected accent
        // -----------------------------------------------------

        if (selected)
        {
            using var accentBrush =
                new SolidBrush(
                    UiTheme.Accent
                );

            e.Graphics.FillRectangle(
                accentBrush,
                bounds.Left,
                bounds.Top + 3,
                3,
                Math.Max(
                    0,
                    bounds.Height - 6
                )
            );
        }

        // -----------------------------------------------------
        // Separator
        // -----------------------------------------------------

        using (
            var separatorPen =
                new Pen(
                    Color.FromArgb(
                        226,
                        232,
                        240
                    )
                )
        )
        {
            e.Graphics.DrawLine(
                separatorPen,
                bounds.Left + 12,
                bounds.Bottom - 1,
                bounds.Right - 12,
                bounds.Bottom - 1
            );
        }

        // -----------------------------------------------------
        // Icon tile
        // -----------------------------------------------------

        const int tileSize =
            32;

        var tileX =
            bounds.Left + 14;

        var tileY =
            bounds.Top +
            (
                bounds.Height -
                tileSize
            ) /
            2;

        using var tilePath =
            CreateRoundedRectanglePath(
                new RectangleF(
                    tileX,
                    tileY,
                    tileSize,
                    tileSize
                ),
                5
            );

        using (
            var tileBrush =
                new SolidBrush(
                    selected
                        ? Color.White
                        : Color.FromArgb(
                            248,
                            250,
                            252
                        )
                )
        )
        {
            e.Graphics.FillPath(
                tileBrush,
                tilePath
            );
        }

        using (
            var tilePen =
                new Pen(
                    Color.FromArgb(
                        226,
                        232,
                        240
                    )
                )
        )
        {
            e.Graphics.DrawPath(
                tilePen,
                tilePath
            );
        }

        // -----------------------------------------------------
        // Process icon
        // -----------------------------------------------------

        var iconSize =
            Math.Max(
                22,
                (int)Math.Round(
                    24.0 *
                    DeviceDpi /
                    96.0
                )
            );

        var processName =
            list.Items[e.Index]
                ?.ToString() ??
            string.Empty;

        var processIcon =
            AppCacheService.Shared
                .GetProcessIcon(
                    processName,
                    iconSize
                ) ??
            UiAssets.App(
                iconSize
            );

        _paintedIcons[
            (
                list,
                e.Index
            )
        ] =
            processIcon;

        var iconX =
            tileX +
            (
                tileSize -
                iconSize
            ) /
            2;

        var iconY =
            tileY +
            (
                tileSize -
                iconSize
            ) /
            2;

        e.Graphics.DrawImage(
            processIcon,
            new Rectangle(
                iconX,
                iconY,
                iconSize,
                iconSize
            )
        );

        // -----------------------------------------------------
        // Process name
        // -----------------------------------------------------

        var textBounds =
            new Rectangle(
                tileX +
                tileSize +
                12,
                bounds.Top,
                Math.Max(
                    0,
                    bounds.Width -
                    tileSize -
                    54
                ),
                bounds.Height
            );

        TextRenderer.DrawText(
            e.Graphics,
            processName,
            list.Font,
            textBounds,
            UiTheme.TextPrimary,
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.EndEllipsis
        );
    }

    // =========================================================
    // Selection
    // =========================================================

    private void HandleIncludedSelectionChanged()
    {
        if (_isUpdatingSelection)
        {
            return;
        }

        if (
            _includedList
                .SelectedItems
                .Count >
            0
        )
        {
            try
            {
                _isUpdatingSelection =
                    true;

                _excludedList
                    .ClearSelected();
            }
            finally
            {
                _isUpdatingSelection =
                    false;
            }
        }

        UpdateActions();
    }

    private void HandleExcludedSelectionChanged()
    {
        if (_isUpdatingSelection)
        {
            return;
        }

        if (
            _excludedList
                .SelectedItems
                .Count >
            0
        )
        {
            try
            {
                _isUpdatingSelection =
                    true;

                _includedList
                    .ClearSelected();
            }
            finally
            {
                _isUpdatingSelection =
                    false;
            }
        }

        UpdateActions();
    }

    // =========================================================
    // Action state
    // =========================================================

    private void UpdateActions()
    {
        var canMoveToExcluded =
            _includedList
                .SelectedItems
                .Count >
            0;

        var canMoveToIncluded =
            _excludedList
                .SelectedItems
                .Count >
            0;

        var hasOverrides =
            _includedList.Items.Count +
            _excludedList.Items.Count >
            0;

        StyleTransferButton(
            _moveToExcludedButton,
            canMoveToExcluded,
            primary: true
        );

        StyleTransferButton(
            _moveToIncludedButton,
            canMoveToIncluded,
            primary: false
        );

        StyleDangerButton(
            _removeIncludedButton,
            canMoveToExcluded
        );

        StyleDangerButton(
            _removeExcludedButton,
            canMoveToIncluded
        );

        StyleDangerButton(
            _clearAllButton,
            hasOverrides
        );
    }

    private static void StyleTransferButton(
        RoundedButton button,
        bool enabled,
        bool primary)
    {
        button.Enabled =
            enabled;

        if (!enabled)
        {
            button.BackColor =
                Color.FromArgb(
                    248,
                    250,
                    252
                );

            button.ForeColor =
                Color.FromArgb(
                    148,
                    163,
                    184
                );

            button.FlatAppearance.BorderColor =
                Color.FromArgb(
                    203,
                    213,
                    225
                );

            return;
        }

        if (primary)
        {
            button.BackColor =
                UiTheme.AccentSoft;

            button.ForeColor =
                UiTheme.Accent;

            button.FlatAppearance.BorderColor =
                UiTheme.Accent;

            return;
        }

        button.BackColor =
            Color.White;

        button.ForeColor =
            UiTheme.TextSecondary;

        button.FlatAppearance.BorderColor =
            Color.FromArgb(
                203,
                213,
                225
            );
    }

    private static void StyleDangerButton(
        RoundedButton button,
        bool enabled)
    {
        button.Enabled =
            enabled;

        if (!enabled)
        {
            button.BackColor =
                Color.FromArgb(
                    241,
                    245,
                    249
                );

            button.ForeColor =
                Color.FromArgb(
                    148,
                    163,
                    184
                );

            button.FlatAppearance.BorderColor =
                Color.FromArgb(
                    203,
                    213,
                    225
                );

            return;
        }

        button.BackColor =
            Color.White;

        button.ForeColor =
            UiTheme.Danger;

        button.FlatAppearance.BorderColor =
            Color.FromArgb(
                246,
                120,
                140
            );
    }

    // =========================================================
    // Move included -> excluded
    // =========================================================

    private void MoveIncludedToExcluded()
    {
        var selected =
            _includedList
                .SelectedItems
                .Cast<string>()
                .ToArray();

        if (
            selected.Length ==
            0
        )
        {
            return;
        }

        foreach (
            var processName
            in selected
        )
        {
            _presenceController
                .RejectGameOverride(
                    processName
                );
        }

        RefreshLists(
            force: true
        );

        SelectItems(
            _excludedList,
            selected
        );

        UpdateActions();

        ShowSavedIndicator();
    }

    // =========================================================
    // Move excluded -> included
    // =========================================================

    private void MoveExcludedToIncluded()
    {
        var selected =
            _excludedList
                .SelectedItems
                .Cast<string>()
                .ToArray();

        if (
            selected.Length ==
            0
        )
        {
            return;
        }

        foreach (
            var processName
            in selected
        )
        {
            _presenceController
                .ConfirmGameOverride(
                    processName
                );
        }

        RefreshLists(
            force: true
        );

        SelectItems(
            _includedList,
            selected
        );

        UpdateActions();

        ShowSavedIndicator();
    }

    // =========================================================
    // Remove included
    // =========================================================

    private void RemoveSelectedIncluded()
    {
        var selected =
            _includedList
                .SelectedItems
                .Cast<string>()
                .ToArray();

        if (
            selected.Length ==
            0
        )
        {
            return;
        }

        var processText =
            selected.Length ==
            1
                ? $"\"{selected[0]}.exe\" will return to automatic game detection."
                : $"{selected.Length} selected processes will return to automatic game detection.";

        using var dialog =
            new AppDialog(
                "Remove game override",
                "Remove from Always a game?",
                processText,
                AppDialogTone.Warning,

                new AppDialogAction(
                    "Cancel",
                    DialogResult.Cancel,
                    AppDialogButtonKind.Secondary
                ),

                new AppDialogAction(
                    "Remove",
                    DialogResult.Yes,
                    AppDialogButtonKind.Danger
                )
            );

        if (
            dialog.ShowDialog(
                this
            ) !=
            DialogResult.Yes
        )
        {
            return;
        }

        foreach (
            var processName
            in selected
        )
        {
            _presenceController
                .RemoveIncludedGameOverride(
                    processName
                );
        }

        RefreshLists(
            force: true
        );

        ShowSavedIndicator();
    }

    // =========================================================
    // Remove excluded
    // =========================================================

    private void RemoveSelectedExcluded()
    {
        var selected =
            _excludedList
                .SelectedItems
                .Cast<string>()
                .ToArray();

        if (
            selected.Length ==
            0
        )
        {
            return;
        }

        var processText =
            selected.Length ==
            1
                ? $"\"{selected[0]}.exe\" will return to automatic game detection."
                : $"{selected.Length} selected processes will return to automatic game detection.";

        using var dialog =
            new AppDialog(
                "Remove game override",
                "Remove from Never a game?",
                processText,
                AppDialogTone.Warning,

                new AppDialogAction(
                    "Cancel",
                    DialogResult.Cancel,
                    AppDialogButtonKind.Secondary
                ),

                new AppDialogAction(
                    "Remove",
                    DialogResult.Yes,
                    AppDialogButtonKind.Danger
                )
            );

        if (
            dialog.ShowDialog(
                this
            ) !=
            DialogResult.Yes
        )
        {
            return;
        }

        foreach (
            var processName
            in selected
        )
        {
            _presenceController
                .RemoveExcludedGameOverride(
                    processName
                );
        }

        RefreshLists(
            force: true
        );

        ShowSavedIndicator();
    }

    // =========================================================
    // Clear all
    // =========================================================

    private void ClearAllOverrides()
    {
        var overrideCount =
            _includedList.Items.Count +
            _excludedList.Items.Count;

        if (overrideCount <= 0)
        {
            return;
        }

        using var dialog =
            new AppDialog(
                "Clear game overrides",
                "Clear all game overrides?",
                $"This will remove all {overrideCount} manual game rules and return them to automatic detection.",
                AppDialogTone.Danger,

                new AppDialogAction(
                    "Cancel",
                    DialogResult.Cancel,
                    AppDialogButtonKind.Secondary
                ),

                new AppDialogAction(
                    "Clear all",
                    DialogResult.Yes,
                    AppDialogButtonKind.Danger
                )
            );

        if (
            dialog.ShowDialog(
                this
            ) !=
            DialogResult.Yes
        )
        {
            return;
        }

        _presenceController
            .ClearGameOverrides();

        RefreshLists(
            force: true
        );

        ShowSavedIndicator();
    }

    // =========================================================
    // Refresh
    // =========================================================

    private void RefreshLists(
        bool force = false)
    {
        var included =
            _presenceController
                .GetIncludedGameOverrides()
                .ToArray();

        var excluded =
            _presenceController
                .GetExcludedGameOverrides()
                .ToArray();

        if (
            !force &&
            included.SequenceEqual(
                _lastIncludedSnapshot,
                StringComparer.OrdinalIgnoreCase
            ) &&
            excluded.SequenceEqual(
                _lastExcludedSnapshot,
                StringComparer.OrdinalIgnoreCase
            )
        )
        {
            return;
        }

        var includedSelected =
            _includedList
                .SelectedItems
                .Cast<string>()
                .ToArray();

        var excludedSelected =
            _excludedList
                .SelectedItems
                .Cast<string>()
                .ToArray();

        _isUpdatingSelection =
            true;

        _includedList.BeginUpdate();

        _excludedList.BeginUpdate();

        try
        {
            _paintedIcons.Clear();

            _includedList
                .Items
                .Clear();

            foreach (
                var process
                in included
            )
            {
                _includedList
                    .Items
                    .Add(
                        process
                    );
            }

            _excludedList
                .Items
                .Clear();

            foreach (
                var process
                in excluded
            )
            {
                _excludedList
                    .Items
                    .Add(
                        process
                    );
            }

            SelectItemsInternal(
                _includedList,
                includedSelected
            );

            SelectItemsInternal(
                _excludedList,
                excludedSelected
            );

            _lastIncludedSnapshot =
                included;

            _lastExcludedSnapshot =
                excluded;

            _includedCountLabel.Text =
                included.Length.ToString();

            _excludedCountLabel.Text =
                excluded.Length.ToString();
        }
        finally
        {
            _includedList.EndUpdate();

            _excludedList.EndUpdate();

            _isUpdatingSelection =
                false;
        }

        UpdateActions();
    }

    // =========================================================
    // Refresh process icons
    // =========================================================

    private void RefreshChangedIcons(
        ListBox list)
    {
        var iconSize =
            Math.Max(
                22,
                (int)Math.Round(
                    24.0 *
                    DeviceDpi /
                    96.0
                )
            );

        for (
            var index =
                Math.Max(
                    0,
                    list.TopIndex
                );

            index <
            list.Items.Count;

            index++
        )
        {
            var bounds =
                list.GetItemRectangle(
                    index
                );

            if (
                bounds.Top >=
                list.ClientSize.Height
            )
            {
                break;
            }

            var processName =
                list.Items[index]
                    ?.ToString() ??
                string.Empty;

            var icon =
                AppCacheService.Shared
                    .GetProcessIcon(
                        processName,
                        iconSize
                    ) ??
                UiAssets.App(
                    iconSize
                );

            if (
                _paintedIcons.TryGetValue(
                    (
                        list,
                        index
                    ),
                    out var painted
                ) &&
                ReferenceEquals(
                    painted,
                    icon
                )
            )
            {
                continue;
            }

            _paintedIcons[
                (
                    list,
                    index
                )
            ] =
                icon;

            list.Invalidate(
                bounds
            );
        }
    }

    // =========================================================
    // Selection helpers
    // =========================================================

    private void SelectItems(
        ListBox listBox,
        IEnumerable<string> items)
    {
        try
        {
            _isUpdatingSelection =
                true;

            _includedList
                .ClearSelected();

            _excludedList
                .ClearSelected();

            SelectItemsInternal(
                listBox,
                items
            );
        }
        finally
        {
            _isUpdatingSelection =
                false;
        }
    }

    private static void SelectItemsInternal(
        ListBox listBox,
        IEnumerable<string> items)
    {
        foreach (
            var item
            in items
        )
        {
            var index =
                listBox
                    .Items
                    .IndexOf(
                        item
                    );

            if (index < 0)
            {
                continue;
            }

            listBox.SetSelected(
                index,
                true
            );
        }
    }

    // =========================================================
    // Drawing helpers
    // =========================================================

    private static GraphicsPath
        CreateRoundedRectanglePath(
            RectangleF bounds,
            int radius)
    {
        var path =
            new GraphicsPath();

        var diameter =
            radius *
            2F;

        path.AddArc(
            bounds.Left,
            bounds.Top,
            diameter,
            diameter,
            180F,
            90F
        );

        path.AddArc(
            bounds.Right -
            diameter,
            bounds.Top,
            diameter,
            diameter,
            270F,
            90F
        );

        path.AddArc(
            bounds.Right -
            diameter,
            bounds.Bottom -
            diameter,
            diameter,
            diameter,
            0F,
            90F
        );

        path.AddArc(
            bounds.Left,
            bounds.Bottom -
            diameter,
            diameter,
            diameter,
            90F,
            90F
        );

        path.CloseFigure();

        return path;
    }

    private static GraphicsPath
        CreateTopRoundedRectanglePath(
            RectangleF bounds,
            int radius)
    {
        var path =
            new GraphicsPath();

        radius =
            Math.Clamp(
                radius,
                0,
                (int)Math.Min(
                    bounds.Width / 2F,
                    bounds.Height
                )
            );

        if (radius <= 0)
        {
            path.AddRectangle(
                bounds
            );

            path.CloseFigure();

            return path;
        }

        var diameter =
            radius *
            2F;

        path.StartFigure();

        // Top-left
        path.AddArc(
            bounds.Left,
            bounds.Top,
            diameter,
            diameter,
            180F,
            90F
        );

        // Top-right
        path.AddArc(
            bounds.Right -
            diameter,
            bounds.Top,
            diameter,
            diameter,
            270F,
            90F
        );

        // Right side
        path.AddLine(
            bounds.Right,
            bounds.Top +
            radius,
            bounds.Right,
            bounds.Bottom
        );

        // Bottom
        path.AddLine(
            bounds.Right,
            bounds.Bottom,
            bounds.Left,
            bounds.Bottom
        );

        // Left side
        path.AddLine(
            bounds.Left,
            bounds.Bottom,
            bounds.Left,
            bounds.Top +
            radius
        );

        path.CloseFigure();

        return path;
    }

    // =========================================================
    // Cleanup
    // =========================================================

    protected override void OnFormClosed(
        FormClosedEventArgs e)
    {
        _refreshTimer?.Stop();

        _refreshTimer?.Dispose();

        _savedIndicatorTimer?.Stop();

        _savedIndicatorTimer?.Dispose();

        base.OnFormClosed(
            e
        );
    }
}
namespace DiscordPresence;

internal sealed partial class GameOverridesForm : Form
{
    private readonly PresenceController _presenceController = null!;
    private readonly System.Windows.Forms.Timer? _refreshTimer;
    private bool _isUpdatingSelection;
    private readonly Dictionary<(ListBox List, int Index), Image> _paintedIcons = new();
    private string[] _lastIncludedSnapshot = [];
    private string[] _lastExcludedSnapshot = [];

    // Designer creates the form without starting application services.
    public GameOverridesForm()
    {
        // Keep initialization atomic so font/DPI scaling runs only after
        // the final font, client size and child bounds have all been set.
        SuspendLayout();
        try
        {
            InitializeComponent();
        }
        finally
        {
            ResumeLayout(true);
        }
    }

    public GameOverridesForm(PresenceController presenceController) : this()
    {
        _presenceController = presenceController;
        _headerIcon.Image = UiAssets.Gamepad(48);
        _moveSelectedButton.Click += (_, _) => MoveSelectedItems();
        _includedList.SelectedIndexChanged += (_, _) => HandleIncludedSelectionChanged();
        _excludedList.SelectedIndexChanged += (_, _) => HandleExcludedSelectionChanged();
        _removeIncludedButton.Click += (_, _) => RemoveSelectedIncluded();
        _removeExcludedButton.Click += (_, _) => RemoveSelectedExcluded();
        _clearAllButton.Click += (_, _) =>
        {
            if (MessageBox.Show(this, "Clear all game overrides?", "Discord Presence",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _presenceController.ClearGameOverrides();
            RefreshLists(force: true);
        };
        foreach (var button in new[] { _moveSelectedButton, _removeIncludedButton,
            _removeExcludedButton, _clearAllButton, _closeButton })
        {
            Color original = button.ForeColor;
            button.MouseEnter += (_, _) => { if (button.Enabled) button.ForeColor = Color.White; };
            button.MouseLeave += (_, _) => button.ForeColor = original;
            button.EnabledChanged += (_, _) => button.ForeColor = original;
        }
        foreach (var list in new[] { _includedList, _excludedList })
        {
            list.DrawMode = DrawMode.OwnerDrawFixed;
            list.ItemHeight = Math.Max(28, list.Font.Height + 10);
            list.DrawItem += (_, e) =>
            {
                if (e.Index < 0) return;
                bool selected = (e.State & DrawItemState.Selected) != 0;
                using (var brush = new SolidBrush(selected ? UiTheme.Accent : Color.White))
                    e.Graphics.FillRectangle(brush, e.Bounds);
                var bounds = e.Bounds;
                int iconSize = Math.Max(16, (int)Math.Round(20.0 * DeviceDpi / 96.0));
                var processIcon = AppCacheService.Shared.GetProcessIcon(
                    list.Items[e.Index].ToString() ?? "", iconSize) ?? UiAssets.App(iconSize);
                _paintedIcons[(list, e.Index)] = processIcon;
                e.Graphics.DrawImage(processIcon, bounds.X + 7,
                    bounds.Y + (bounds.Height - iconSize) / 2, iconSize, iconSize);
                bounds.X += iconSize + 14;
                bounds.Width -= iconSize + 20;
                TextRenderer.DrawText(e.Graphics, list.Items[e.Index].ToString(), list.Font,
                    bounds, selected ? Color.White : UiTheme.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
                e.DrawFocusRectangle();
            };
        }
        RefreshLists(force: true);
        _refreshTimer = new System.Windows.Forms.Timer(components) { Interval = 500 };
        _refreshTimer.Tick += (_, _) =>
        {
            RefreshLists();
            RefreshChangedIcons(_includedList);
            RefreshChangedIcons(_excludedList);
        };
        _refreshTimer.Start();
    }

    private void RefreshChangedIcons(ListBox list)
    {
        int iconSize = Math.Max(16, (int)Math.Round(20.0 * DeviceDpi / 96.0));
        for (int index = Math.Max(0, list.TopIndex); index < list.Items.Count; index++)
        {
            var bounds = list.GetItemRectangle(index);
            if (bounds.Top >= list.ClientSize.Height) break;
            var icon = AppCacheService.Shared.GetProcessIcon(
                list.Items[index].ToString() ?? "", iconSize) ?? UiAssets.App(iconSize);
            if (_paintedIcons.TryGetValue((list, index), out var painted) &&
                ReferenceEquals(painted, icon)) continue;
            _paintedIcons[(list, index)] = icon;
            list.Invalidate(bounds);
        }
    }

    private void UpdateActions()
    {
        _removeIncludedButton.Enabled = _includedList.SelectedItems.Count > 0;
        _removeExcludedButton.Enabled = _excludedList.SelectedItems.Count > 0;
        _clearAllButton.Enabled = _includedList.Items.Count + _excludedList.Items.Count > 0;
    }

    private void HandleIncludedSelectionChanged()
    {
        if (_isUpdatingSelection)
        {
            return;
        }

        if (_includedList.SelectedItems.Count == 0)
        {
            UpdateMoveButton();

            return;
        }

        try
        {
            _isUpdatingSelection =
                true;

            /*
             * Chọn Yes thì clear selection bên No.
             */
            _excludedList.ClearSelected();
        }
        finally
        {
            _isUpdatingSelection =
                false;
        }

        UpdateMoveButton();
    }

    // =========================================================
    // Excluded selection
    // =========================================================

    private void HandleExcludedSelectionChanged()
    {
        if (_isUpdatingSelection)
        {
            return;
        }

        if (_excludedList.SelectedItems.Count == 0)
        {
            UpdateMoveButton();

            return;
        }

        try
        {
            _isUpdatingSelection =
                true;

            /*
             * Chọn No thì clear selection bên Yes.
             */
            _includedList.ClearSelected();
        }
        finally
        {
            _isUpdatingSelection =
                false;
        }

        UpdateMoveButton();
    }

    // =========================================================
    // Move button
    // =========================================================

    private void UpdateMoveButton()
    {
        UpdateActions();
        if (_includedList.SelectedItems.Count > 0)
        {
            /*
             * Yes -> No
             */
            _moveSelectedButton.Text =
                ">>";

            _moveSelectedButton.Enabled =
                true;

            return;
        }

        if (_excludedList.SelectedItems.Count > 0)
        {
            /*
             * No -> Yes
             */
            _moveSelectedButton.Text =
                "<<";

            _moveSelectedButton.Enabled =
                true;

            return;
        }

        _moveSelectedButton.Text =
            "<<";

        _moveSelectedButton.Enabled =
            false;
    }

    // =========================================================
    // Move selected
    // =========================================================

    private void MoveSelectedItems()
    {
        // -----------------------------------------------------
        // Yes -> No
        // -----------------------------------------------------

        if (_includedList.SelectedItems.Count > 0)
        {
            var selected =
                _includedList
                    .SelectedItems
                    .Cast<string>()
                    .ToArray();

            foreach (var processName in selected)
            {
                _presenceController
                    .RejectGameOverride(
                        processName
                    );
            }

            RefreshLists(
                force: true
            );

            /*
             * Select lại bên No.
             */
            SelectItems(
                _excludedList,
                selected
            );

            UpdateMoveButton();

            return;
        }

        // -----------------------------------------------------
        // No -> Yes
        // -----------------------------------------------------

        if (_excludedList.SelectedItems.Count > 0)
        {
            var selected =
                _excludedList
                    .SelectedItems
                    .Cast<string>()
                    .ToArray();

            foreach (var processName in selected)
            {
                _presenceController
                    .ConfirmGameOverride(
                        processName
                    );
            }

            RefreshLists(
                force: true
            );

            /*
             * Select lại bên Yes.
             */
            SelectItems(
                _includedList,
                selected
            );

            UpdateMoveButton();
        }
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

        if (selected.Length == 0)
        {
            return;
        }

        foreach (var processName in selected)
        {
            _presenceController
                .RemoveIncludedGameOverride(
                    processName
                );
        }

        RefreshLists(
            force: true
        );
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

        if (selected.Length == 0)
        {
            return;
        }

        foreach (var processName in selected)
        {
            _presenceController
                .RemoveExcludedGameOverride(
                    processName
                );
        }

        RefreshLists(
            force: true
        );
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

        /*
         * Không rebuild UI nếu data không đổi.
         */
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
            _includedList.Items.Clear();

            foreach (var process in included)
            {
                _includedList.Items.Add(
                    process
                );
            }

            _excludedList.Items.Clear();

            foreach (var process in excluded)
            {
                _excludedList.Items.Add(
                    process
                );
            }

            /*
             * Preserve selection nếu item vẫn còn
             * ở đúng list cũ.
             */
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
        }
        finally
        {
            _includedList.EndUpdate();

            _excludedList.EndUpdate();

            _isUpdatingSelection =
                false;
        }

        UpdateMoveButton();
    }

    // =========================================================
    // Select items
    // =========================================================

    private void SelectItems(
        ListBox listBox,
        IEnumerable<string> items)
    {
        try
        {
            _isUpdatingSelection =
                true;

            _includedList.ClearSelected();

            _excludedList.ClearSelected();

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
        foreach (var item in items)
        {
            var index =
                listBox.Items.IndexOf(
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
    // Cleanup
    // =========================================================

    protected override void OnFormClosed(
        FormClosedEventArgs e)
    {
        _refreshTimer?.Stop();

        _refreshTimer?.Dispose();

        base.OnFormClosed(
            e
        );
    }
}

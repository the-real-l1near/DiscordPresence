namespace DiscordPresence;

internal sealed class GameOverridesForm : Form
{
    private readonly PresenceController
        _presenceController;

    private readonly ListBox
        _includedList;

    private readonly ListBox
        _excludedList;

    private readonly Button
        _moveSelectedButton;

    private readonly System.Windows.Forms.Timer
        _refreshTimer;

    private bool
        _isUpdatingSelection;

    private string[]
        _lastIncludedSnapshot =
            [];

    private string[]
        _lastExcludedSnapshot =
            [];

    // =========================================================
    // Constructor
    // =========================================================

    public GameOverridesForm(
        PresenceController presenceController)
    {
        _presenceController =
            presenceController;

        // -----------------------------------------------------
        // Window
        // -----------------------------------------------------

        Text =
            "Game Overrides";

        Width =
            600;

        Height =
            460;

        StartPosition =
            FormStartPosition.CenterParent;

        FormBorderStyle =
            FormBorderStyle.FixedDialog;

        MaximizeBox =
            false;

        MinimizeBox =
            false;

        // -----------------------------------------------------
        // Included label
        // -----------------------------------------------------

        var includedLabel =
            new Label
            {
                Text =
                    "Always treat as game",

                Left =
                    20,

                Top =
                    20,

                Width =
                    220,

                Font =
                    new Font(
                        Font,
                        FontStyle.Bold
                    )
            };

        // -----------------------------------------------------
        // Included list
        // -----------------------------------------------------

        _includedList =
            new ListBox
            {
                Left =
                    20,

                Top =
                    45,

                Width =
                    230,

                Height =
                    250,

                SelectionMode =
                    SelectionMode.MultiExtended
            };

        // -----------------------------------------------------
        // Excluded label
        // -----------------------------------------------------

        var excludedLabel =
            new Label
            {
                Text =
                    "Never treat as game",

                Left =
                    330,

                Top =
                    20,

                Width =
                    220,

                Font =
                    new Font(
                        Font,
                        FontStyle.Bold
                    )
            };

        // -----------------------------------------------------
        // Excluded list
        // -----------------------------------------------------

        _excludedList =
            new ListBox
            {
                Left =
                    330,

                Top =
                    45,

                Width =
                    230,

                Height =
                    250,

                SelectionMode =
                    SelectionMode.MultiExtended
            };

        // -----------------------------------------------------
        // Move button
        // -----------------------------------------------------

        _moveSelectedButton =
            new Button
            {
                Text =
                    "<<",

                Left =
                    265,

                Top =
                    145,

                Width =
                    50,

                Height =
                    36,

                Enabled =
                    false
            };

        _moveSelectedButton.Click += (_, _) =>
        {
            MoveSelectedItems();
        };

        // -----------------------------------------------------
        // Selection events
        // -----------------------------------------------------

        _includedList.SelectedIndexChanged += (_, _) =>
        {
            HandleIncludedSelectionChanged();
        };

        _excludedList.SelectedIndexChanged += (_, _) =>
        {
            HandleExcludedSelectionChanged();
        };

        // -----------------------------------------------------
        // Remove included
        // -----------------------------------------------------

        var removeIncludedButton =
            new Button
            {
                Text =
                    "Remove selected",

                Left =
                    20,

                Top =
                    310,

                Width =
                    140,

                Height =
                    32
            };

        removeIncludedButton.Click += (_, _) =>
        {
            RemoveSelectedIncluded();
        };

        // -----------------------------------------------------
        // Remove excluded
        // -----------------------------------------------------

        var removeExcludedButton =
            new Button
            {
                Text =
                    "Remove selected",

                Left =
                    330,

                Top =
                    310,

                Width =
                    140,

                Height =
                    32
            };

        removeExcludedButton.Click += (_, _) =>
        {
            RemoveSelectedExcluded();
        };

        // -----------------------------------------------------
        // Clear all
        // -----------------------------------------------------

        var clearAllButton =
            new Button
            {
                Text =
                    "Clear all overrides",

                Left =
                    20,

                Top =
                    365,

                Width =
                    160,

                Height =
                    32
            };

        clearAllButton.Click += (_, _) =>
        {
            var result =
                MessageBox.Show(
                    "Clear all game overrides?",
                    "Discord Presence",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (result !=
                DialogResult.Yes)
            {
                return;
            }

            _presenceController
                .ClearGameOverrides();

            RefreshLists(
                force: true
            );
        };

        // -----------------------------------------------------
        // Close
        // -----------------------------------------------------

        var closeButton =
            new Button
            {
                Text =
                    "Close",

                Left =
                    450,

                Top =
                    365,

                Width =
                    110,

                Height =
                    32,

                DialogResult =
                    DialogResult.OK
            };

        AcceptButton =
            closeButton;

        // -----------------------------------------------------
        // Controls
        // -----------------------------------------------------

        Controls.Add(
            includedLabel
        );

        Controls.Add(
            _includedList
        );

        Controls.Add(
            _moveSelectedButton
        );

        Controls.Add(
            excludedLabel
        );

        Controls.Add(
            _excludedList
        );

        Controls.Add(
            removeIncludedButton
        );

        Controls.Add(
            removeExcludedButton
        );

        Controls.Add(
            clearAllButton
        );

        Controls.Add(
            closeButton
        );

        // -----------------------------------------------------
        // Initial data
        // -----------------------------------------------------

        RefreshLists(
            force: true
        );

        // -----------------------------------------------------
        // Live refresh timer
        // -----------------------------------------------------

        _refreshTimer =
            new System.Windows.Forms.Timer
            {
                Interval =
                    500
            };

        _refreshTimer.Tick += (_, _) =>
        {
            RefreshLists();
        };

        _refreshTimer.Start();
    }

    // =========================================================
    // Included selection
    // =========================================================

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
        _refreshTimer.Stop();

        _refreshTimer.Dispose();

        base.OnFormClosed(
            e
        );
    }
}
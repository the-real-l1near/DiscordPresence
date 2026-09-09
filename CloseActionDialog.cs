namespace DiscordPresence;

public sealed class CloseActionDialog : Form
{
    public CloseActionDialog()
    {
        Text = "Close Discord Presence";

        Width = 400;
        Height = 180;

        StartPosition =
            FormStartPosition.CenterParent;

        FormBorderStyle =
            FormBorderStyle.FixedDialog;

        MaximizeBox = false;
        MinimizeBox = false;

        ShowInTaskbar = false;

        var messageLabel =
            new Label
            {
                Text =
                    "What do you want to do with Discord Presence?",

                Left = 20,
                Top = 20,
                Width = 340,
                Height = 30
            };

        var minimizeButton =
            new System.Windows.Forms.Button
            {
                Text = "Minimize to tray",

                Left = 20,
                Top = 70,

                Width = 130,
                Height = 35,

                DialogResult =
                    DialogResult.Yes
            };

        var exitButton =
            new System.Windows.Forms.Button
            {
                Text = "Exit",

                Left = 160,
                Top = 70,

                Width = 90,
                Height = 35,

                DialogResult =
                    DialogResult.No
            };

        var cancelButton =
            new System.Windows.Forms.Button
            {
                Text = "Cancel",

                Left = 260,
                Top = 70,

                Width = 90,
                Height = 35,

                DialogResult =
                    DialogResult.Cancel
            };

        Controls.Add(
            messageLabel
        );

        Controls.Add(
            minimizeButton
        );

        Controls.Add(
            exitButton
        );

        Controls.Add(
            cancelButton
        );

        AcceptButton =
            minimizeButton;

        CancelButton =
            cancelButton;
    }
}
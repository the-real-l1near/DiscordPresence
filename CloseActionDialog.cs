namespace DiscordPresence;

public sealed class CloseActionDialog : AppDialog
{
    public CloseActionDialog()
        : base(
            "Close Discord Presence",
            "Close Discord Presence?",
            "Choose what you want to do with Discord Presence.",
            AppDialogTone.Info,

            new AppDialogAction(
                "Minimize to tray",
                DialogResult.Yes,
                AppDialogButtonKind.Primary
            ),

            new AppDialogAction(
                "Exit",
                DialogResult.No,
                AppDialogButtonKind.Danger
            ),

            new AppDialogAction(
                "Cancel",
                DialogResult.Cancel,
                AppDialogButtonKind.Secondary
            )
        )
    {
    }
}

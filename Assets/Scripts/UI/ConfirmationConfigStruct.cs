using System;

/// <summary>
///     A struct for holding ConfirmationDialogMenu settings.
/// </summary>
public struct ConfirmationConfigStruct
{
    /// <summary>
    ///     The warning message to display to the player.
    /// </summary>
    public string WarningText;
    /// <summary>
    ///     The text to show on the confirm button.
    /// </summary>
    public string ConfirmText;
    /// <summary>
    ///     The text to show on the cancel button.
    /// </summary>
    public string CancelText;
    /// <summary>
    ///     The function to execute when the confirm button is pressed.
    /// </summary>
    public Action ConfirmAction;
    /// <summary>
    ///     The function to execute when the cancel button is pressed.
    /// </summary>
    public Action CancelAction;

    public ConfirmationConfigStruct(string WarningText = "Are you sure?", string ConfirmText = "Confirm",
                              string CancelText = "Cancel", Action ConfirmAction = null,
                              Action CancelAction = null)
    {
        this.WarningText = WarningText;
        this.ConfirmText = ConfirmText;
        this.CancelText = CancelText;
        this.ConfirmAction = ConfirmAction;
        this.CancelAction = CancelAction;
    }
}
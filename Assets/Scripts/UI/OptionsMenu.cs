using TMPro;
using UnityEngine.UI;

/// <summary>
///     A class defining the options menu of the game.
/// </summary>
public class OptionsMenu : Menu
{
    /// <summary>
    ///     The menu to return to after closing the options menu.
    /// </summary>
    public Menu returnMenu;
    /// <summary>
    ///     <tt>True</tt> iff there are unsaved changes to any of the new options.
    /// </summary>
    private bool unsavedChanges = false;
    /// <summary>
    ///     The editable options as displayed to the player.
    /// </summary>
    private OptionsStruct newOptions;
    /// <summary>
    ///     Settings for the unsaved changes confirmation dialog
    /// </summary>
    private ConfirmationConfigStruct confirmationSettings = new(
        WarningText: "You have unsaved changes. Are you sure you want to return without saving?",
        ConfirmText: "Yes, return without saving.",
        CancelText: "No, keep editing."
    );

    protected override void OnStart()
    {
        GameInfo.options = Utilities.LoadPersistentOrDefaultData<OptionsStruct>("options");
        GameInfo.OptionsMenu = this;
        confirmationSettings.ConfirmAction = ReturnToPreviousMenu;
    }

    public void ShowMenu(Menu returnMenu)
    {
        this.returnMenu = returnMenu;
        ShowMenu();
    }

    protected override void OnShow() {
        newOptions = GameInfo.options.Clone();
        mouseSlider.value = newOptions.mouseSensitivity;
    }

    public void ApplyButton() {
        GameInfo.options = newOptions;
        Utilities.SavePersistentData(newOptions, "options");
        unsavedChanges = false;
    }

    public void ReturnButton() {
        if (unsavedChanges) {
            GameInfo.ConfirmationDialogMenu.ShowMenu(confirmationSettings);
        }
        else
        {
            ReturnToPreviousMenu();
        }
    }

    private void ReturnToPreviousMenu()
    {
        HideMenu();
        returnMenu.ShowMenu();
    }

    /// <summary>
    ///     The slider for the mouse sensitivity option.
    /// </summary>
    public Slider mouseSlider;
    /// <summary>
    ///     The text displaying the current mouse sensitivity option.
    /// </summary>
    public TextMeshProUGUI mouseInfoText;
    public void MouseSensitivitySlider() {
        unsavedChanges = true;
        newOptions.mouseSensitivity = mouseSlider.value;
        mouseInfoText.SetText("Mouse Sensitivity: " + (int)(100*mouseSlider.value) + "%");
    }
}

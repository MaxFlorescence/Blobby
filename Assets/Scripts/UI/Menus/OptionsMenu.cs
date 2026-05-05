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
        GameInfo.options = FileUtilities.LoadPersistentOrDefaultData<OptionsStruct>("options");
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
        mouseSlider.value = 100 * newOptions.mouseSensitivity;
        environmentIntensitySlider.value = 100 * newOptions.environmentLightIntensity;
        gammaSlider.value = 100 * newOptions.gamma;
        gainSlider.value = 100 * newOptions.gain;
    }

    public void ApplyButton() {
        GameInfo.options = newOptions;
        FileUtilities.SavePersistentData(newOptions, "options");
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

    /// <summary>
    ///     Hide this menu and show the previous menu.
    /// </summary>
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
        newOptions.mouseSensitivity = mouseSlider.value / 100;
        mouseInfoText.SetText($"Mouse Sensitivity: {newOptions.mouseSensitivity:P0}");
    }

    /// <summary>
    ///     The slider for the environment light intensity option.
    /// </summary>
    public Slider environmentIntensitySlider;
    /// <summary>
    ///     The text displaying the current environment light intensity option.
    /// </summary>
    public TextMeshProUGUI intensityInfoText;
    public void EnvironmentLightIntensitySlider() {
        unsavedChanges = true;
        newOptions.environmentLightIntensity = environmentIntensitySlider.value / 100;
        intensityInfoText.SetText($"Environment Light Intensity: {newOptions.environmentLightIntensity:P0}");
    }

    /// <summary>
    ///     The slider for the environment gamma option.
    /// </summary>
    public Slider gammaSlider;
    /// <summary>
    ///     The text displaying the current gain option.
    /// </summary>
    public TextMeshProUGUI gammaInfoText;
    public void GammaSlider() {
        unsavedChanges = true;
        newOptions.gamma = gammaSlider.value / 100;
        gammaInfoText.SetText($"Gamma: {newOptions.gamma:F2}");
    }

    /// <summary>
    ///     The slider for the environment gamma option.
    /// </summary>
    public Slider gainSlider;
    /// <summary>
    ///     The text displaying the current gain option.
    /// </summary>
    public TextMeshProUGUI gainInfoText;
    public void GainSlider() {
        unsavedChanges = true;
        newOptions.gain = gainSlider.value / 100;
        gainInfoText.SetText($"Gain: {newOptions.gain:F2}");
    }
}

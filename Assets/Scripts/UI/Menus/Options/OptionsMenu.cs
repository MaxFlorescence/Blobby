using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public enum OptionName
{
    MouseSensitivity = 0,
    EnvironmentLightIntensity = 1,
    Gamma = 2,
    Gain = 3
}

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
    ///     The editable options as displayed to the player.
    /// </summary>
    private SliderController[] sliderControllers;
    private ToggleController[] toggleControllers;
    private DropdownController[] dropdownControllers;

    /// <summary>
    ///     Settings for the unsaved changes confirmation dialog
    /// </summary>
    private ConfirmationConfigStruct confirmationSettings = new(
        WarningText: "You have unsaved changes. Returning now will discard them.",
        ConfirmText: "Yes, return without saving.",
        CancelText: "No, keep editing."
    );

    protected override void OnStart()
    {
        GameInfo.options = FileUtilities.LoadPersistentOrDefaultData<GameOptionsStruct>("options");
        GameInfo.OptionsMenu = this;
        confirmationSettings.ConfirmAction = ReturnToPreviousMenu;

        sliderControllers = gameObject.GetComponentsInChildren<SliderController>(true);
        toggleControllers = gameObject.GetComponentsInChildren<ToggleController>(true);
        dropdownControllers = gameObject.GetComponentsInChildren<DropdownController>(true);
    }

    public void ShowMenu(Menu returnMenu)
    {
        this.returnMenu = returnMenu;
        ShowMenu();
    }

    protected override void OnShow() {
        foreach (SliderController slider in sliderControllers)
        {
            slider.InitializeOption(
                GameInfo.options.SliderOptions[(int)slider.optionName]
            );
        }
        foreach (ToggleController toggle in toggleControllers)
        {
            toggle.InitializeOption(
                GameInfo.options.ToggleOptions[(int)toggle.optionName]
            );
        }
        foreach (DropdownController dropdown in dropdownControllers)
        {
            dropdown.InitializeOption(
                GameInfo.options.DropdownOptions[(int)dropdown.optionName]
            );
        }
    }

    public void ApplyButton() {
        GameInfo.options = new GameOptionsStruct(
            sliderControllers.Select(slider => slider.option).ToArray(),
            toggleControllers.Select(toggle => toggle.option).ToArray(),
            dropdownControllers.Select(dropdown => dropdown.option).ToArray()
        );

        FileUtilities.SavePersistentData(GameInfo.options, "options");
        OnShow(); // set as default options
    }

    public void ReturnButton() {
        bool unsavedChanges = sliderControllers.Any(slider => slider.option.unsaved) ||
            toggleControllers.Any(toggle => toggle.option.unsaved) ||
            dropdownControllers.Any(dropdown => dropdown.option.unsaved);

        if (unsavedChanges)
        {
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
}

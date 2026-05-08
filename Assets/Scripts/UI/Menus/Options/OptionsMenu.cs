using UnityEngine;
using System.Linq;
using System.IO;

/// <summary>
///     The available game options.
/// </summary>
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
    ///     The name of the file that contains the game's options.
    /// </summary>
    private const string OPTIONS_FILE = "options";

    /// <summary>
    ///     The menu to return to after closing the options menu.
    /// </summary>
    public Menu returnMenu;

    /// <summary>
    ///     The editable slider options as displayed to the player.
    /// </summary>
    private SliderController[] sliderControllers;

    /// <summary>
    ///     The editable toggle options as displayed to the player.
    /// </summary>
    private ToggleController[] toggleControllers;

    /// <summary>
    ///     The editable dropdown options as displayed to the player.
    /// </summary>
    private DropdownController[] dropdownControllers;

    /// <summary>
    ///     Settings for the unsaved changes confirmation dialog.
    /// </summary>
    private ConfirmationConfigStruct confirmationSettings = new(
        WarningText: "You have unsaved changes. Returning now will discard them.",
        ConfirmText: "Yes, return without saving.",
        CancelText: "No, keep editing."
    );

    protected override void OnAwake()
    {
        GameInfo.Options = FileUtilities.LoadPersistentOrDefaultData<GameOptionsStruct>(OPTIONS_FILE);
        GameInfo.DefaultOptions = JsonUtility.FromJson<GameOptionsStruct>(
            Resources.Load<TextAsset>(Path.Combine(FileUtilities.DEFAULT_DATA, OPTIONS_FILE)).text
        );
    }

    protected override void OnStart()
    {
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
                GameInfo.Options.SliderOptions[(int)slider.optionName]
            );
        }
        foreach (ToggleController toggle in toggleControllers)
        {
            toggle.InitializeOption(
                GameInfo.Options.ToggleOptions[(int)toggle.optionName]
            );
        }
        foreach (DropdownController dropdown in dropdownControllers)
        {
            dropdown.InitializeOption(
                GameInfo.Options.DropdownOptions[(int)dropdown.optionName]
            );
        }
    }

    /// <summary>
    ///     The button that the player presses to save their changes to the game options.
    /// </summary>
    public void ApplyButton() {
        GameInfo.Options = new GameOptionsStruct(
            sliderControllers.OrderBy(slider => slider.optionName)
                .Select(slider => slider.option).ToArray(),
            toggleControllers.OrderBy(toggle => toggle.optionName)
                .Select(toggle => toggle.option).ToArray(),
            dropdownControllers.OrderBy(dropdown => dropdown.optionName)
                .Select(dropdown => dropdown.option).ToArray()
        );

        FileUtilities.SavePersistentData(GameInfo.Options, OPTIONS_FILE);
        OnShow(); // set as default options
    }

    /// <summary>
    ///     The button that the player presses to return to the previous menu.
    /// </summary>
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

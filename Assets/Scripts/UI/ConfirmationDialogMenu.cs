using TMPro;

public class ConfirmationDialogMenu : Menu
{
    public static readonly ConfirmationConfigStruct DEFAULT_CONFIG = new();

    private ConfirmationConfigStruct config;
    public TMP_Text warningText;
    public TMP_Text confirmText;
    public TMP_Text cancelText;


    protected override void OnStart()
    {
        isSubmenu = true;
        GameInfo.ConfirmationDialogMenu = this;
    }

    public void ShowMenu(ConfirmationConfigStruct config)
    {
        this.config = config;

        warningText.SetText(this.config.WarningText);
        confirmText.SetText(this.config.ConfirmText);
        cancelText.SetText(this.config.CancelText);

        ShowMenu();
    }

    public void ConfirmButton()
    {
        config.ConfirmAction?.Invoke();
        HideMenu();
    }

    public void CancelButton()
    {
        config.CancelAction?.Invoke();
        HideMenu();
    }
}

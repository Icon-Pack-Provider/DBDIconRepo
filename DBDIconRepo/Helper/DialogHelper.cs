using DBDIconRepo.Dialog;

namespace DBDIconRepo.Helper;

public static class DialogHelper
{
    public static void Show(string message)
        => Show(message, string.Empty, DialogButtons.Ok, DialogSymbol.None);

    public static void Show(string message, string caption)
        => Show(message, caption, DialogButtons.Ok, DialogSymbol.None);

    public static void Show(string message, string caption, DialogButtons button)
        => Show(message, caption, button, DialogSymbol.None);

    public static void Show(string message, string caption, DialogSymbol symbol)
        => Show(message, caption, DialogButtons.Ok, symbol);

    public static void Show(string message, string caption, DialogButtons button, DialogSymbol symbol)
    {
        MessageDialog dialog = new(new DialogOptions(message, caption, button, symbol));
        dialog.ShowDialog();
    }


    public static bool Inquire(string message)
        => Inquire(message, string.Empty, DialogButtons.Ok, DialogSymbol.None);

    public static bool Inquire(string message, string caption)
        => Inquire(message, caption, DialogButtons.Ok, DialogSymbol.None);

    public static bool Inquire(string message, string caption, DialogButtons button)
        => Inquire(message, caption, button, DialogSymbol.None);

    public static bool Inquire(string message, string caption, DialogSymbol symbol)
        => Inquire(message, caption, DialogButtons.Ok, symbol);

    public static bool Inquire(string message, string caption, DialogButtons button, DialogSymbol symbol)
    {
        MessageDialog dialog = new(new DialogOptions(message, caption, button, symbol));
        var response = dialog.ShowDialog();
        if (response is null)
            return false;
        return response.Value;
    }
}
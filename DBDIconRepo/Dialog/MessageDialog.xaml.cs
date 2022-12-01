using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using ModernWpf;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Dialog;

[INotifyPropertyChanged]
public partial class MessageDialog : Window
{
    public MessageDialog()
    {
        InitializeComponent();
    }

    public MessageDialog(DialogOptions option)
    {
        InitializeComponent();
        DataContext = this;
        Closing += unregisterMessenger;
        Messenger.Default.Register<MessageDialog, DialogResponseMessage, string>(this, 
            MessageToken.SendDialogResponseToken,
            SetDialogResponseToThis);

        DialogTitle = option.Title;
        DialogMessage = option.Message;
        DialogButtons = option.Buttons;
        DialogSymbol = option.Symbol;
        switch (DialogSymbol)
        {
            case DialogSymbol.Question:
                SystemSounds.Question.Play();
                break;
            case DialogSymbol.Warning:
                SystemSounds.Asterisk.Play();
                break;
            case DialogSymbol.Error:
                SystemSounds.Exclamation.Play();
                break;
            case DialogSymbol.Information:
                SystemSounds.Question.Play();
                break;
        }
    }

    private void unregisterMessenger(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        Messenger.Default.Unregister<DialogResponseMessage, string>(this, MessageToken.SendDialogResponseToken);
    }

    private void SetDialogResponseToThis(MessageDialog recipient, DialogResponseMessage message)
    {
        switch (message.Response)
        {
            case DialogResponse.Ok:
            case DialogResponse.Yes:
                DialogResult = true;
                break;
            case DialogResponse.No:
            case DialogResponse.Cancel:
                DialogResult = false;
                break;
        }
    }

    [ObservableProperty]
    string dialogTitle = "";

    [ObservableProperty]
    string dialogMessage = "";

    #region Buttons
    [ObservableProperty]
    ObservableCollection<ButtonDisplay> allDialogButtons = new();

    DialogButtons buttons = DialogButtons.Nothing;
    public DialogButtons DialogButtons
    {
        get => buttons;
        set
        {
            if (SetProperty(ref buttons, value))
            {
                //Set buttons display
                switch (value)
                {
                    case DialogButtons.Ok:
                        AllDialogButtons.Add(new(ButtonType.Ok));
                        break;
                    case DialogButtons.OkCancel:
                        AllDialogButtons.Add(new(ButtonType.Ok));
                        AllDialogButtons.Add(new(ButtonType.Cancel));
                        break;
                    case DialogButtons.YesNo:
                        AllDialogButtons.Add(new(ButtonType.Yes));
                        AllDialogButtons.Add(new(ButtonType.No));
                        break;
                    case DialogButtons.YesNoCancel:
                        AllDialogButtons.Add(new(ButtonType.Yes));
                        AllDialogButtons.Add(new(ButtonType.No));
                        AllDialogButtons.Add(new(ButtonType.Cancel));
                        break;
                }
            }
        }
    }
    #endregion

    #region Symbol
    DialogSymbol symbol = DialogSymbol.None;
    public DialogSymbol DialogSymbol
    {
        get => symbol;
        set
        {
            if (!SetProperty(ref symbol, value))
                return;
            SymbolVisibility = value != DialogSymbol.None ? Visibility.Visible : Visibility.Collapsed;
            switch (value)
            {
                /*None, Information, Question, Warning, Error*/
                case DialogSymbol.Information:
                    SymbolGlyph = "\uE946";
                    if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light)
                        SymbolBrush = new(Colors.MidnightBlue);
                    else
                        SymbolBrush = new(Colors.RoyalBlue);
                    break;
                case DialogSymbol.Question:
                    SymbolGlyph = "\uE9CE";
                    if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light)
                        SymbolBrush = new(Colors.MidnightBlue);
                    else
                        SymbolBrush = new(Colors.RoyalBlue);
                    break;
                case DialogSymbol.Warning:
                    SymbolGlyph = "\uE7BA";
                    if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light)
                        SymbolBrush = new(Colors.OrangeRed);
                    else
                        SymbolBrush = new(Colors.Yellow);
                    break;
                case DialogSymbol.Error:
                    SymbolGlyph = "\uEA39";
                    if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light)
                        SymbolBrush = new(Colors.DarkRed);
                    else
                        SymbolBrush = new(Colors.Red);
                    break;
                default:
                case DialogSymbol.None:
                    SymbolGlyph = string.Empty;
                    SymbolBrush = new(Colors.Gray);
                    break;
            }
        }
    }

    [ObservableProperty]
    string symbolGlyph = string.Empty;

    [ObservableProperty]
    SolidColorBrush symbolBrush = new(Colors.Gray);

    [ObservableProperty]
    Visibility symbolVisibility = Visibility.Collapsed;

    #endregion

    private void AutoResponse(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
        {
            //Response with positive
            DialogResult = true;
            this.Close();
        }
        else if (e.Key == System.Windows.Input.Key.Escape)
        {
            DialogResult = false;
        }
    }

}

public class DialogOptions
{
    public string Message { get; set; }
    public string Title { get; set; }

    public DialogButtons Buttons { get; set; } = DialogButtons.Ok;

    public DialogSymbol Symbol { get; set; } = DialogSymbol.None;

    public DialogOptions(string message, string title, DialogButtons buttons, DialogSymbol symbol)
    {
        Message = message;
        Title = title;
        Buttons = buttons;
        Symbol = symbol;
    }

    public DialogOptions(string message, string title, DialogButtons buttons)
    {
        Message = message;
        Title = title;
        Buttons = buttons;
    }

    public DialogOptions(string message, string title)
    {
        Message = message;
        Title = title;
        Buttons = DialogButtons.Ok;
    }

    public DialogOptions(string message)
    {
        Message = message;
        Buttons = DialogButtons.Ok;
    }
}

public enum DialogButtons
{
    Nothing,
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel
}

public enum DialogSymbol
{
    None,
    Information,
    Question,
    Warning,
    Error
}

public enum DialogResponse
{
    Ok,
    Yes,
    No,
    Cancel,
    Other
}

public partial class ButtonDisplay : ObservableObject
{
    public ButtonDisplay(string text, bool show)
    {
        ButtonText = text;
        ButtonVisibility = show;
    }

    public ButtonDisplay(bool show)
    {
        ButtonVisibility = show;
        ButtonText = string.Empty;
    }

    public ButtonDisplay(ButtonType type)
    {
        ButtonVisibility = true;
        ButtonText = type.ToString();
        PossibleResponse = Enum.Parse<DialogResponse>(type.ToString());
    }

    [ObservableProperty]
    string buttonText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShouldShowButton))]
    bool buttonVisibility = false;

    public Visibility ShouldShowButton => ButtonVisibility ? Visibility.Visible : Visibility.Collapsed;

    [ObservableProperty]
    public DialogResponse possibleResponse = DialogResponse.Ok;

    [RelayCommand]
    private void SendResponse()
    {
        Messenger.Default.Send(new DialogResponseMessage(PossibleResponse), MessageToken.SendDialogResponseToken);
    }
}
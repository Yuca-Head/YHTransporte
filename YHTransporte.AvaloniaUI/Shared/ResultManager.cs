using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YHTransporte.AvaloniaUI.Shared;

public partial class ResultManager : ObservableObject
{
    
    public event Action? GotError;

    public event Action? GotSuccess;

    public enum InnerCleaning   
    {
        BeforeAction,
        AfterAction,
        Never
    }

    [ObservableProperty]
    public partial bool HasError {get; private set;}

    [ObservableProperty]
    public partial string ResultMessage {get; set;}

    public void Clear(Action? action = null, InnerCleaning cleaning = InnerCleaning.AfterAction)
    {
        Action _ = cleaning switch
        {
            InnerCleaning.BeforeAction => () => {ClearInsides(); action?.Invoke();},
            InnerCleaning.AfterAction => () => {action?.Invoke(); ClearInsides();},
            _ => () => action?.Invoke()
        };

        _();
    }

    private void ClearInsides()
    {
        ResultMessage = "";
        HasError = false;
    }

    public void MarkError(string message = "")
    {
        ResultMessage = message;
        GotError?.Invoke();
    }

    public void Success(string message = "")
    {
        ResultMessage = message;
        GotSuccess?.Invoke();
    }

    /// <summary>Marca varios errores en una sola lista (por ejemplo, los de un ValidationError).</summary>
    public void MarkErrors(IEnumerable<string> errors, string header = "Se han encontrado los siguientes errores:")
    {
        StringBuilder sb = new($"{header}\n");

        foreach(var error in errors)
            sb.Append($"• {error}\n");

        MarkError(sb.ToString().TrimEnd());
    }

    /// <summary>Para errores que llegan desde la base de datos u otros que no se esperaban.</summary>
    public void MarkUnexpected(Exception exception)
    => MarkError($"Ocurrió un error inesperado: {exception.Message}");
}
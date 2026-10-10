using System;
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
}
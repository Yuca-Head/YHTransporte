using System.Threading.Tasks;

namespace YHTransporte.AvaloniaUI.Shared.Contexts;

internal interface IInitiableContext
{
    Task Init();
}
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.Reports;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Reports.ViewModels;

public partial class ReportsMenuViewModel : ViewModelBase
{
    // Solo para la demo: pausa artificial (en milisegundos) antes de consultar, para que se pueda
    // mostrar el botón Cancelar con una base chica. En 0 no hace nada. Ejemplo para la defensa: 2000.
    private static readonly int SimulatedLatencyMs = 0;

    private static readonly ReportOption AllOption = new(0, "Todos");

    private readonly IShipmentReportRepository _repository;
    private bool _optionsLoaded;
    private bool _initialized;

    public ReportsMenuViewModel(IShipmentReportRepository repository)
    {
        _repository = repository;

        StatusOptions = [AllOption];
        SupplierOptions = [AllOption];
        SelectedStatus = AllOption;
        SelectedSupplier = AllOption;
        StatusMessage = "";
        TotalsText = "";
    }

    public ObservableCollection<ReportOption> StatusOptions { get; }
    public ObservableCollection<ReportOption> SupplierOptions { get; }
    public ObservableCollection<ShipmentReportRow> Rows { get; } = [];
    public ObservableCollection<ShipmentStatusSummary> Summary { get; } = [];

    [ObservableProperty]
    public partial ReportOption? SelectedStatus { get; set; }

    [ObservableProperty]
    public partial ReportOption? SelectedSupplier { get; set; }

    [ObservableProperty]
    public partial DateTime? FromDate { get; set; }

    [ObservableProperty]
    public partial DateTime? ToDate { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string TotalsText { get; set; }

    /// <summary>La vista lo llama al cargarse: genera el primer reporte sin bloquear la ventana.</summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;
        await GenerateCommand.ExecuteAsync(null);
    }

    // IncludeCancelCommand crea además GenerateCancelCommand, que cancela el token mientras corre.
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task GenerateAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            if (!_optionsLoaded)
            {
                StatusMessage = "Cargando filtros...";
                await LoadOptionsAsync(cancellationToken);
            }

            var filter = BuildFilter();

            if (filter.From is not null && filter.To is not null && filter.From > filter.To)
            {
                ErrorMessage = "La fecha 'Desde' no puede ser mayor que 'Hasta'.";
                StatusMessage = "";
                return;
            }

            StatusMessage = "Consultando envíos...";

            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, cancellationToken);

            // La consulta corre en un hilo del pool: la ventana sigue respondiendo.
            var report = await Task.Run(
                () => _repository.GetShipmentReportAsync(filter, cancellationToken), cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // Estamos otra vez en el hilo de la interfaz (await dentro del comando), se puede tocar la UI.
            Rows.Clear();
            foreach (var row in report.Rows)
                Rows.Add(row);

            Summary.Clear();
            foreach (var item in report.ByStatus)
                Summary.Add(item);

            TotalsText = $"{report.TotalShipments} envíos · {report.TotalWeightKg:N2} kg";
            IsEmpty = report.Rows.Count == 0;
            StatusMessage = $"Reporte generado a las {report.GeneratedAt:HH:mm:ss}";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operación cancelada.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"No se pudo generar el reporte: {ex.Message}";
            StatusMessage = "";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        FromDate = null;
        ToDate = null;
        SelectedStatus = AllOption;
        SelectedSupplier = AllOption;
    }

    private async Task LoadOptionsAsync(CancellationToken cancellationToken)
    {
        // Las dos consultas de filtros corren a la vez.
        var statusesTask = _repository.GetShipmentStatusOptionsAsync(cancellationToken);
        var suppliersTask = _repository.GetSupplierOptionsAsync(cancellationToken);

        var statuses = await statusesTask;
        var suppliers = await suppliersTask;

        foreach (var s in statuses)
            StatusOptions.Add(s);
        foreach (var s in suppliers)
            SupplierOptions.Add(s);

        _optionsLoaded = true;
    }

    private ShipmentReportFilter BuildFilter()
    {
        // "Hasta" incluye todo ese día.
        DateTimeOffset? from = FromDate is DateTime f ? new DateTimeOffset(f.Date) : null;
        DateTimeOffset? to = ToDate is DateTime t ? new DateTimeOffset(t.Date.AddDays(1).AddTicks(-1)) : null;

        int? statusId = SelectedStatus is { Id: > 0 } st ? st.Id : null;
        int? supplierId = SelectedSupplier is { Id: > 0 } sp ? sp.Id : null;

        return new ShipmentReportFilter(from, to, statusId, supplierId);
    }
}

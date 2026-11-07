using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using YHTransporte.AvaloniaUI.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;

namespace YHTransporte.AvaloniaUI.Modules.Dashboard.ViewModels;

public partial class DashboardMenuViewModel : ViewModelBase
{

    // ── Valores (para los gráficos) ──
    public ObservableCollection<ObservableValue> TripsValues { get; } =
        new() { new(12), new(5), new(20), new(15), new(40), new(32), new(40) };

    public ObservableCollection<ObservableValue> DeliveriesValues { get; } =
        new() { new(0), new(12), new(38), new(30), new(35), new(45), new(30) };

    public ObservableCollection<ObservableValue> IncidentsValues { get; } =
        new() { new(0), new(5), new(15), new(10), new(20), new(25), new(20) };

    public ObservableValue PendingValue { get; } = new(8);
    public ObservableValue InTransitValue { get; } = new(12);
    public ObservableValue DeliveredValue { get; } = new(9);
    public ObservableValue CanceledValue { get; } = new(2);

    // ── Series (apuntan a los valores de arriba) ──
    public ISeries[] ActivitySeries { get; }
    public ISeries[] LoadStatusSeries { get; }

    public DashboardMenuViewModel(DashboardListSectionViewModel dashboardListSection)
    {
        DashboardListSection = dashboardListSection;

        ActivitySeries = new ISeries[]
        {
        new LineSeries<ObservableValue> { Values = TripsValues, Name = "Viajes" },
        new LineSeries<ObservableValue>
        {
            Values = DeliveriesValues, Name = "Entregas",
            Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 3 }
        },
        new LineSeries<ObservableValue>
        {
            Values = IncidentsValues, Name = "Incidencias",
            Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 },
            Fill = null
        }
        };

        LoadStatusSeries = new ISeries[]
        {
        new PieSeries<ObservableValue> { Values = new[] { PendingValue },   Name = "Pendientes" },
        new PieSeries<ObservableValue> { Values = new[] { InTransitValue }, Name = "En tránsito" },
        new PieSeries<ObservableValue> { Values = new[] { DeliveredValue }, Name = "Entregados" },
        new PieSeries<ObservableValue> { Values = new[] { CanceledValue },  Name = "Canceladas" }
        };
    }
    public DashboardListSectionViewModel DashboardListSection { get; }

    [RelayCommand]
    private void OpenCreator() => DashboardListSection.IsOpen = true;



    // Definir los ejes con texto blanco para que se vean sobre el fondo oscuro
    public Axis[] XAxes { get; set; } = new Axis[]
    {
        new Axis
        {
            Labels = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul" },
            LabelsPaint = new SolidColorPaint(SKColors.White)
        }
    };

    public Axis[] YAxes { get; set; } = new Axis[]
    {
        new Axis
        {
            LabelsPaint = new SolidColorPaint(SKColors.White)
        }
    };


};

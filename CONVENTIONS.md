# Super convenciones a seguir para tener coherencia a lo largo del proyecto

## Para Gui (Avalonia)
1. Código y nombres técnicos en inglés.
2. Funcionalidades de UI organizadas verticalmente.
3. Mantener las propiedades en una sola línea mientras el código siga siendo legible. Cuando la longitud o cantidad de propiedades dificulte la lectura, dividirlas en varias líneas con alineación consistente.
4. Las definiciones simples de filas y columnas deben declararse 
mediante RowDefinitions y ColumnDefinitions directamente en el elemento Grid.
Evitar:
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
Preferir:
<Grid ColumnDefinitions="*,*">

Utilizar <Grid.RowDefinitions> o <Grid.ColumnDefinitions> cuando sea necesario especificar características adicionales en las definiciones.
A nos ser que requiera características en específico.
5. Recursos visuales compartidos se definen centralmente.
6. No duplicar controles reutilizables siempre que sea posible.
7. Views no contienen lógica de negocio.
8. La interacción con Application ocurre mediante ViewModels.


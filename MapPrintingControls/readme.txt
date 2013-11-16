1. To use the control with the example application provided, simply open the solution
and run the application.

or

2a. To use the MapPrinterDialog control within a different WPF application, first build the 
MapPrintingControls project. 

2b. Add a reference to the MapPrintingControls dll (located at
MapPrintingControls/Bin/Release) to your WPF application. 

With Blend 4:

2c. Create a MapPrinterDialog control by drag and drop

2d. Add a MapPrinter control to this print dialog control (by drag and drop )

2e. Bind the Map property of the MapPrinter to your map

In your XAML, you should get something like:
<ESRI_ArcGIS_Client_Samples_MapPrinting:MapPrinterDialog>
	<ESRI_ArcGIS_Client_Samples_MapPrinting:MapPrinter Map="{Binding ElementName=MyMap}"/>
</ESRI_ArcGIS_Client_Samples_MapPrinting:MapPrinterDialog>

2f. You are ready to go.


With VS2010:

2c. Add a reference to the MapPrintingControls namespace within your MainPage.xaml file 
(or whatever you use) : xmlns:printing="clr-namespace:MapPrintingControls;assembly=MapPrintingControls"

2d. Add the MapPrinterDialog control to the xaml file.

2e. Add a MapPrinter control as Content of the MapPrinterDialog and bind the Map property of the MapPrinter
to your map element.
You should get something like:
    <printing:MapPrinterDialog Height="410" Width="340">
	    <printing:MapPrinter x:Name="mapPrinter" Map="{Binding ElementName=MyMap}"/>
    </printing:MapPrinterDialog>

Optionaly you can add a BusyIndicator over your main window, to prevent any user actions during the print:
    <printing:MapPrinterIndicator MapPrinter="{Binding ElementName=mapPrinter}" />

2f. You are ready to go.

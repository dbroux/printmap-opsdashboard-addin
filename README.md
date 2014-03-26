printmap-opsdashboard-addin
===========================

Add-in for Operations Dashboard for ArcGIS allowing to print a map at client side.

## Features

* Print Map Tool
* Allow multi pages printing at fixed scale
* Allow selection of print layouts
* Print layouts configurable and extensible by using XAML description.

## Instructions

### Running this add-in

1. Ensure all requirements below are met.
2. Open and compile project in Visual Studio 2012.
3. Update Project Debug properties to correctly locate add-in build path in /addinpath command line argument.
4. Run project in Debug mode.
5. When Operations Dashboard application starts, edit an operation view that contains a map widget, and select the Add/Remove and Configure map tools. 
6. Select the Print Map tool, optionaly configure the print layouts, and click Close.
7. The tool should be added to the operation view.

Alternatively, build the project, and upload the .opdashboardaddin file from the build output directory to ArcGIS Online, and then download using Manage Add-Ins in Operations Dashboard, then follow the steps above from step 5.

### General Help
[New to Github? Get started here.](http://htmlpreview.github.com/?https://github.com/Esri/esri.github.com/blob/master/help/esri-getting-to-know-github.html)

## Requirements

### ArcGIS for Organizations

* Operations Dashboard for ArcGIS requires an [ArcGIS for Organizations account] (http://www.esri.com/software/arcgis/arcgisonline/evaluate).

### ArcGIS Runtime SDK for WPF

* Requires an installation of the Esri ArcGIS Runtime SDK for WPF version 10.2 or later [Esri website](http://resources.arcgis.com/en/communities/runtime-wpf/)
* Also requires an installation of Microsoft Visual Studio 2012.

## Resources

Learn more about Esri's [Operations Dashboard for ArcGIS](http://www.esri.com/software/arcgis/arcgisonline/features/operations-dashboard).

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue.

## Contributing

Anyone and everyone is welcome to contribute.

## Licensing

Copyright 2013 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0
         
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

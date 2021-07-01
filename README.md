# WestCore-GUI
A modular C# GUI designed to debug several variables of a VEX robot, ranging from variable charts to odometry visualizations.
![image](https://user-images.githubusercontent.com/36551149/124071337-c8e35c80-da04-11eb-999c-a1ccbff2fd30.png)

## Notice
**Because this project was built with WPF, it is only Windows compatible.** The Grafana versions of this project are fully cross-platform and will likely replace this repository as soon as it becomes stable. 

Info relating to the Grafana development can be found in the [pros-cli grafana-migration branch](https://github.com/BWHS-Robotics/pros-cli/tree/grafana-migration). 

## Installation from Releases
To install the C# GUI, navigate to the [Releases](https://github.com/BWHS-Robotics/WestCore-GUI/releases) panel of the repository. From there, download the latest MSI installer and run it. The installer will create a new folder containing the executable in your ``Programs Filex (x86)`` labelled ``GUI_WPF_Migration-Installer``. Once you're done, follow
the steps on the [pros-cli fork](https://github.com/BWHS-Robotics/pros-cli), as well as the example usage of the GUI in the [pros-gui-template](https://github.com/BWHS-Robotics/pros-gui-template) repository. 
## Installation Manually
If you want to either use this project for development or wish to build it from source, you must have the following prerequisites:
- Visual Studio with `.NET desktop development` installed,
  as well as the individual component ``Windows Universal CRT SDK``   
- .NET Framework v4.7.2
### Setting up
To build the project from source, first follow the steps below:
1. Clone the repository 
2. Open the project through the ``GUI-WPF-Migration.sln`` file 
3. Ensure the ``GUI-WPF-Migration`` assembly is currently selected in the ``Solution Explorer``, designated by a bolded name as seen
   below. If another assembly happens to be selected, change it to ``GUI-WPF-Migration`` by right clicking it in the Solution Explorer and selecting 
   ``Set as Startup Project``.

![image](https://user-images.githubusercontent.com/36551149/124170374-f5cb5a00-da6c-11eb-9389-ac715b8e7b34.png)
  
### Building
From now on, to build the project you can simply right click the ``GUI-WPF-Migration-Installer`` in the ``Solution Explorer`` and hit ``Build``. This should create a new MSI installer for the project in your ``{PROJECT_DIRECTORY}/`` folder. Installing the msi while a version is already installed will overwrite the previously installed executable. 

## Uninstalling
If at any point you want to uninstall the GUI, simply navigate to your Apps & features settings and uninstall ``GUI_WPF_MIGRATION``. All folders used for ``WestCore-GUI`` will then be completely removed from your system.

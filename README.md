# qlik-connectors

This is a QVX wrapper for Qlik Sense

Anyone can build their own driver by implementing the IQlikConnector interface and this without recompiling the whole project. The generated driver can then be deployed in the "connectors" directory.

An installer is provided with an example JSON driver:

https://github.com/pouc/QlikConnectors/tree/master/Installer

WARNING : this installer has only been tested on Windows 7 x64. To install on other Windows versions you might need to clone the project and recompile it for your own architecture.

It requires .net 4.5

Use at your own risks. No support will be provided!

Also this work was done buy a Qlik Employee in his spare time. This work was not done by Qlik and will thus NOT BE SUPPORTED by Qlik.

Please use Github to provide feedback or to help.

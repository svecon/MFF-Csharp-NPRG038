# Sverge – a Flexible Tool for Comparing & Merging

**Title:** Sverge – A Flexible Tool for Comparing & Merging

**Author:** Ondřej Švec

**Department:** Department of Distributed and Dependable Systems

**Supervisor **of the bachelor thesis: Mgr. Pavel Ježek, Ph.D., Department of Distributed and Dependable Systems (jezek@d3s.mff.cuni.cz)

**Abstract:** Developers, who work on larger projects, are using revision control systems, which usually does not have a built-in tool for visualising differences between directories and individual files or they only offer a console interface. Even though there are numerous tools for visualising the differences, none of these tools are modular, so that a visualisation for a different file types can be added. A tool called Sverge that is modular and pluginable was created, it can visualise differences between directories and text files by default and more visualisations can easily be added. The tool was created using C# programming language, it can natively run on Microsoft Windows operating system and it can easily be integrated with popular revision control systems.

**Keywords:** visualisation, differences, 2 way, 3 way, files, directories, modular, extensible

## Project structure

### A. The implementation of the Sverge application
The solution Sverge.sln can be found in \sources directory. It contains source codes of all the projects mentioned in the development documentation.

### B. Documentation created from the source codes of the project
The documentation, which has been generated from the source codes using the tool Sandcastle [12], can be found in \documentation directory.

### C. Built application
The application’s binary files (libraries and executables) are located in the \build directory. The console user interface can be executed by running SvergeConsole.exe and the graphical user interface can be executed by running Sverge.exe. All plugins are included in \build\plugins directory.

### D. Test data
The test data are located in the \test_data directory. It contains sample files lao.txt, tzu.txt and tao.txt, which were used in some of the figures as a reference.

### E. Integration with control version systems
The configuration files and a documentation for integrating our tool with popular control version systems are located in \integration directory.

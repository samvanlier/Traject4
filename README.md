# Evolution of Speech
This porject is part of the course **Evolution of Speech** given by *Bart de Boer* at the [VUB](https://www.vub.be/).
The project is based on [(De Boer and Zuidema, 2010)](https://journals.sagepub.com/doi/abs/10.1177/1059712309345789) and the original source code was provided by *de Boer*.

The following sections will cover the technical details of the project.

## Project structure
This project is written in [.NET-core 3.1](https://dotnet.microsoft.com/en-us/download/dotnet/3.1) and contains two console application: [Traject4](./Traject4) and [Friends](./Friends).
The former is a direct port of the C++ codebase provided by *de Boer*, the latter is a changed version that is used to test a new hypothesis (for more information click [here](#hypothesis).
Each project has its own `README`-file describing the project in more details.

**Project overview:**
- [DTO](./DTO): A project that contains POCO object for converting the internal model to a common JSON output (used by the R-script in [R_files](./R_files))
- [Friends](./Friends): An updated version of the original project, used to test the hypotesis
- [Graph](./Graph): A custom graph library, used for creating the friend network and for importing and exporting the graph
- [Tests](./Tests): Unit Tests
- [R_files](./R_files): A folder containing the script used to plot the resulting Trajectories and some example `json`-files
- [Traject4](./Traject4): A direct port of the original C++ codebase

### Run the projects
It is advised to use [Visual Studio](https://visualstudio.microsoft.com/).
The perfomance of the project is very dependend of the type of run.
**It is thus advised to run the project in `RELEASE` mode**, instead of debug mode.

### Potential errors
Certain project rely on undelying structures and software.

TODO: document underlying software.

## Hypothesis
TODO

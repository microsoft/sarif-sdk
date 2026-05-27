# Building the SDK

If you want to build the SDK from source, rather than consuming the NuGet package,
use the following steps to set up your machine for development and validate it:

1. Install .NET Core SDK 2.1 and 3.1 from https://dotnet.microsoft.com/download

2. Ensure that Visual Studio 2019 is installed on your machine.

    You can build in VS 2017 as well.

3. Ensure that your Visual Studio installation includes the components that support
    - C# development

# Validating the environment

1. Open a Visual Studio 2019 Developer Command Prompt Window.

2. From the root directory of your local repo, run the command `BuildAndTest.cmd`.
    This restores all necessary NuGet packages, builds the SDK, and runs all the tests.

    All build output appears in the `bld\` subdirectory of the repo root directory.

    NOTE: You must run `BuildAndTest.cmd` once _before_ attempting to build in
    Visual Studio, to ensure that all required NuGet packages are available.


3. After you have run `BuildAndTest.cmd` once, you can open any of the solution files
in the `src\` directory in Visual Studio 2017, and build them by running **Rebuild Solution**.


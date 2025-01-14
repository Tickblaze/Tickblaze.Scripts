# Getting started using VS Code

Follow these steps to create your first custom indicator for Tickblaze using VS Code.

> Prerequisites
>
> - [VS Code](https://code.visualstudio.com/)
> - Install [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) 9.0

### 1. Create a New Class Library

Create a new class class library targeting .NET 9.0:

```
dotnet new classlib -n CustomIndicator -f net9.0
```

And navigate to its directory:

```
cd CustomIndicator
```

### 2. Add the Tickblaze.Scripts NuGet Package

Add the Tickblaze.Scripts.Api NuGet package to your project:

```
dotnet add package Tickblaze.Scripts.Api --version *
```

The `--version *` ensures you always get the latest version.

### 3. Create Your Indicator

Open the _Class1.cs_ file in your project, and replace its contents with the following code:

```cs
namespace CustomIndicator;

/// <summary>
/// A custom indicator that calculates the typical price for a bar.
/// </summary>
public partial class TypicalPrice : Indicator
{
    [Plot("Result")]
    public PlotSeries Result { get; set; } = new(Color.Blue, LineStyle.Solid, 1);

    public TypicalPrice()
    {
        Name = "Typical Price";
        IsOverlay = true;
    }

    /// <summary>
    /// Calculates the typical price for the given bar.
    /// </summary>
    protected override void Calculate(int index)
    {
        var bar = Bars[index];

        Result[index] = (bar.High + bar.Low + bar.Close) / 3;
    }
}
```

This code defines a new indicator called **Typical Price** that calculates the average of a bar's high, low, and close prices.

### 4. Build Your Project

Build your project to compile the indicator:

```
dotnet build
```

This will generate the necessary output files and prepare your indicator for use in Tickblaze.

### 5. Run the Indicator in Tickblaze

After building your project, the indicator should be automatically imported into Tickblaze. To use it:

1. **Right-click** on the chart in Tickblaze.
2. Select `Indicators` and click `Add / Edit Settings`.
3. From the dropdown menu, select `Typical Price`.
4. Click the down arrow to add it to the chart.
5. Click the `OK` button.

### 6. Debug the Indicator

#### Start Debugging

1. **Run Tickblaze** as usual if not running yet.
2. Attach your debugger to the `Tickblaze.View.exe` process:
   - Open the **Run and Debug** view in VS Code (`Ctrl+Shift+D`)
   - Click **Create a launch.json file** if prompted, and configure it ot attach to an existing .NET process.
   - In the `launch.json` file and add the following configuration:
     ```json
     {
       "version": "0.2.0",
       "configurations": [
         {
           "name": "Tickblaze Attach",
           "type": "coreclr",
           "request": "attach",
           "processName": "Tickblaze.View.exe"
         }
       ]
     }
     ```
   - Click the green play button to attach the debugger.

#### Debugging Steps

1. Set breakpoints in your `TypicalPrice.cs` file by clicking to the left of the line numbers.
2. Trigger the indicator in Tickblaze by adding it to a chart.
3. Use the debugger controls in Visual Studio Code to step through your code, inspect variables, and analyze execution.

#### Stop Debugging

When you’re done, stop debugging by clicking the **Stop** button in Visual Studio Code or by pressing `Shift+F5`.

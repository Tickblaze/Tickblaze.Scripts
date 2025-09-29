# Adding an UI element to Tickblaze Chart Toolbar

This tutorial will walk you through creating and adding custom UI elements to your chart's toolbar in Tickblaze using WPF.

## 1. Update your project file

- Ensure your project targets Windows and enables WPF support.
- Make these changes in your `.csproj` file:

    ```xml
    <PropertyGroup>
        <TargetFramework>net9.0-windows</TargetFramework>
        <UseWPF>true</UseWPF>
    </PropertyGroup>
    ```

## 2. Create a new indicator or strategy

- Create an indicator or strategy and override the `CreateChartToolbarMenuItem()` method that returns a WPF control:

    ```csharp
    public partial class MyToolbarButton : Indicator
    {
        private System.Windows.Controls.Button _button;

        public override object? CreateChartToolbarMenuItem()
        {
            _button = new System.Windows.Controls.Button { Content = "Click Me!" };
            _button.Click += (s, e) => System.Windows.MessageBox.Show("Button clicked!");

            return _button;
        }
    }
    ```

## That's it!

The button will appear in your chart toolbar when the indicator is loaded or strategy running live.

### Extra Tip:
For multiple items, use `ContextMenu` with multiple controls.

Please refer to [GitHub Action](../.github/workflows/docs.yml) for more details how the process is automated.

To run it locally:

1. Install the latest docfx
   ```
   dotnet tool update -g docfx
   ```
2. Build the project
   ```
   dotnet build
   ```
3. Build the docset and run:
   ```
   docfx docfx.json --serve
   ```
4. Now you can preview the docs website on http://localhost:8080.
I want you to run the script `generate-apps.ps1` and babysit it. If there are issues, fix the script and try again. make sure to run the script in the directory repo root. I don't want you modifying any code files that the script generates. 

The script accepts an optional `-Apps` parameter to generate only specific variants. Valid values are: `no-skills`, `dotnet-webapi`, `dotnet-artisan`. You can pass one or more values. If omitted, all three variants are generated.

Examples:
- `.\generate-apps.ps1` — generates all three variants
- `.\generate-apps.ps1 -Apps no-skills` — generates only the no-skills variant
- `.\generate-apps.ps1 -Apps dotnet-webapi, dotnet-artisan` — generates two variants

When it's done, I want you to check to see if the generated projects successfully build and run with `dotnet build` and `dotnet run`. If they don't build/run, don't make any changes just report that to me so that I am aware. Also check the `gen-notes.md` in each app to ensure that it was built using the correct skill configuration. Show me a summary of the projects that were created including if they build/run and what skill config was used. Write this to a file named `generate-all-apps-notes.md` in the repo root.
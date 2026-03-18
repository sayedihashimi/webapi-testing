I want you to help me create a script. I'm trying to automate calling copilot to generate a few different apps with different copilot config. The script should first delete the three folders `src-no-skills`, `src-dotnet-webapi` and `src-dotnet-artisan`. This should be a PowerShell Core script written to the repo root named 'generate-apps.ps1'. It should have a parameter for the folder to generate the files in, the default value should be the current working directory.

Then the script will call copilot three times.

The first time that it calls copilot cli it should call it with the following prompt:

> Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-no-skills`. Do NOT use any skills during this process.

The second time that it calls the copilot cli it should call it with the following prompt:

> Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-dotnet-webapi`. Use the `dotnet-webapi` but do NOT use any other skills.

The third time that it calls the copilot cli it should call it with the following prompt:

> Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-dotnet-artisan`. Use the `dotnet-artisan` skills but do NOT use the `dotnet-webapi` skill.
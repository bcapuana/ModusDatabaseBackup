# ModusDatabaseBackup
This combination of DMIS macro, and C# executable backups the specified database to the specified zip file.

## How to use
1. Download the Executable from the releases area
2. Copy the macro from [CommonMacros.dmi](https://github.com/bcapuana/ModusDatabaseBackup/blob/master/CommonMacros.dmi) to your macro file, or use CommonMacros.dmi as your starting point for a macro file
3. Put the executable from 1 in the same folder as the Macro File
4. Call the macro from your Modus program
```
    CALL/EXTERN,DMIS,M(CreateDatabaseZipFile),1.9,ProgramName,DatabaseZipFileName
```
**Note:** The arguments are described in the comment block for the macro.

# Ai-malarky

TextAdventure is a .NET 10 console text adventure in the style of late 1970s parser games.

## Run

From the repository root:

```powershell
dotnet run --project .\TextAdventure\TextAdventure.csproj
```

The goal is to recover five treasures from a world of 20 interlinked locations and return them to the Village Green.

The game supports named save slots. `SAVE` and `LOAD` use the default slot, while `SAVE chapter one` and `LOAD chapter one` read and write slot-specific files in the current working directory. `LIST SAVES` shows available slots, `DELETE chapter one` removes a named slot, `CONFIRM DELETE` removes the protected default slot, and `RENAME SAVE old TO chapter one` renames a slot.

Additional vintage verbs include `PUSH`, `PULL`, `LISTEN`, and `SEARCH`.

## Test

From the repository root:

```powershell
dotnet test .\TextAdventure.Tests\TextAdventure.Tests.csproj
```

In-game save help is available with `HELP SAVES`.

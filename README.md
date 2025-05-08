# Memory Match Game

## 🧠 Overview

**Memory Match Game** is a C# application that simulates a classic memory matching game. It demonstrates the usage of various advanced C# features and best practices including interfaces, generics, pattern matching, LINQ, events

---

## 🚀 Getting Started


### Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download) or later
- Visual Studio 2022 or later
- SQL Server or a compatible DBMS

### Installation

```bash
git clone https://github.com/VaniaAnton/MemoryMatchGame.git
cd MemoryMatchGame
dotnet restore
dotnet run
```
### 📁 Project Structure
- MemoryMatchGame.Game/ – Data access layer, Screen, main files.
- MemoryMatchGame.Core/ – Models and background logic.

## ✅ Implemented C# Features

| Feature | Implementation |
|--------|----------------|
| **Custom Interface** | `IGameElement` in `Interfaces/IGameElement.cs` |
| **IComparable<T>** | Implemented in `Card.cs` |
| **IEquatable<T>** | Implemented in `Card.cs` |
| **IFormattable** | Implemented in `GameScreen.cs` |
| **switch with when** | Used in `GameScreen.cs` |
| **Range Type** | Used in `GameBoard.cs` |
| **Multiple Assemblies** | Split into `MemoryMatchGame.Core`, `MemoryMatchGame.Game` |
| **abstract Class** | `GameEntity` is an abstract class |
| **Static Constructor** | Defined in `Card.cs` |
| **Deconstructor** | Implemented in `Card.cs` |
| **Operator Overloading** | `==` and `!=` in `Card.cs` |
| **System.Collections.Generic** | `List<T>`, `Dictionary<TKey, TValue>` used throughout |
| **is Operator** | Used in `Card.cs` |
| **Default and Named Arguments** | Used in methods in `GameEntity.cs` |
| **params Keyword** | Used in `ScoreCalculator.cs` |
| **out Arguments** | Used in `GameLogic.cs` |
| **Delegates/Lambda Functions** | Used in LINQ and event handlers, example in `ScoreCalculator.cs`|
| **Bitwise Operations** | example: Bitwise Left Shift Used in `Card.cs` |
| **?., ??, ??= Operators** | Used in `GameScreen.cs` |
| **Pattern Matching** | Implemented in `Card.cs` |
| **IEnumerable<T>** | Implemented in `CardCollection.cs` |
| **IEnumerator<T>** | Implemented in `CardCollection.cs` |
| **Iterator (yield return)** | Used in `GameBoard.cs` |
| **Extension Methods** | Found in `CardExtensions.cs` |
| **Custom Exception** | Defined in `CustomExtension.cs` |
| **try-catch** | Used in `CardExtensions.cs` |
| **Generic Type** | Implemented in `CardCollection.cs` |
| **where Constraints** | Applied in `CardCollection.cs` |
| **Generic Extension Method** | Found in `CardExtensions.cs` |
| **Extension Deconstructor** | Found in `CardExtensions.cs` |
| **ICloneable** | Implemented in `Card.cs` |
| **Events** | Used in `GameLogic.cs` |
| **LINQ** | Used extensively in queries |

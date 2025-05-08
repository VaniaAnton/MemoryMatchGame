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

### ✅ Implemented C# Features
## ✅ Implemented C# Features

| Feature | Implementation |
|--------|----------------|
| **Custom Interface** | `ICard` in `Models/Card.cs` |
| **IComparable<T>** | Implemented in `Card.cs` |
| **IEquatable<T>** | Implemented in `Card.cs` |
| **IFormattable** | Implemented in `Card.cs` |
| **switch with when** | Used in `GameLogic.cs` |
| **Range Type** | Used in `GameBoard.cs` |
| **Multiple Assemblies** | Split into `MemoryMatchGame`, `MemoryMatchGame.Data`, `MemoryMatchGame.Models` |
| **sealed Class** | `GameEngine` is a sealed class |
| **abstract Class** | `BaseCard` is an abstract class |
| **Static Constructor** | Defined in `GameSettings.cs` |
| **Deconstructor** | Implemented in `Card.cs` |
| **Operator Overloading** | `==` and `!=` in `Card.cs` |
| **System.Collections.Generic** | `List<T>`, `Dictionary<TKey, TValue>` used throughout |
| **is Operator** | Used in `GameLogic.cs` |
| **Default and Named Arguments** | Used in methods in `GameEngine.cs` |
| **params Keyword** | Used in `Logger.cs` |
| **out Arguments** | Used in `Parser.cs` |
| **Delegates/Lambda Functions** | Used in LINQ and event handlers |
| **Bitwise Operations** | Used in `Permissions.cs` |
| **?., ??, ??= Operators** | Used in `GameSettings.cs` |
| **Pattern Matching** | Implemented in `GameLogic.cs` |
| **IEnumerable<T>** | Implemented in `Deck.cs` |
| **IEnumerator<T>** | Implemented in `DeckEnumerator.cs` |
| **Iterator (yield return)** | Used in `Deck.cs` |
| **Extension Methods** | Found in `StringExtensions.cs` |
| **Custom Exception** | Defined in `InvalidCardException.cs` |
| **try-catch** | Used in `DatabaseService.cs` |
| **Generic Type** | `Repository<T>` in `Repository.cs` |
| **where Constraints** | Applied in `Repository<T>` |
| **Generic Extension Method** | Found in `EnumerableExtensions.cs` |
| **Extension Deconstructor** | Found in `CardExtensions.cs` |
| **ICloneable** | Implemented in `Card.cs` |
| **Events** | Used in `GameEngine.cs` |
| **LINQ** | Used extensively in queries |

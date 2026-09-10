# Campus Equipment Borrowing System

Laboratory Activity 1 — ITSD 81 Desktop Application Development
"From Requirements to Application Structure"

This repository contains the architectural foundation for the Campus Equipment
Borrowing System: domain models, application services, repository
abstractions, and an in-memory implementation, with a console program
demonstrating the "Borrow Equipment" use case. No database and no graphical
user interface are implemented yet — that is intentional (see Part XV of the
lab spec).

The Part A requirements analysis (actors, use cases, domain concepts) is in
[`docs/Analysis.md`](docs/Analysis.md).

---

## 1. Solution Structure

```text
EquipmentBorrowing/
│
├── EquipmentBorrowing.sln
│
├── src/
│   ├── EquipmentBorrowing.Domain/          Core business concepts and rules
│   ├── EquipmentBorrowing.Application/     Use cases / application services + repository interfaces
│   ├── EquipmentBorrowing.Infrastructure/  In-memory repository implementations
│   └── EquipmentBorrowing.ConsoleDemo/     Executable that wires everything together and demonstrates the flow
│
└── tests/
    └── EquipmentBorrowing.Tests/           Automated tests for BorrowEquipmentService
```

- **Domain** — `Student`, `Equipment`, `Borrowing`, `BorrowingStatus`. These
  classes know nothing about the application, storage, or UI. They only
  express the concepts and rules that belong to the problem itself (e.g. an
  Equipment can be marked borrowed/returned; a Student can be
  suspended/reinstated).

- **Application** — `BorrowEquipmentService` and the repository interfaces
  (`IStudentRepository`, `IEquipmentRepository`, `IBorrowingRepository`).
  This layer coordinates domain objects to carry out a use case, but never
  touches a database or a UI framework directly.

- **Infrastructure** — `InMemoryStudentRepository`,
  `InMemoryEquipmentRepository`, `InMemoryBorrowingRepository`. These are
  the current, temporary implementations of the Application layer's
  repository interfaces, using plain C# `List<T>` collections instead of a
  real database.

- **Tests** — xUnit tests covering the success case and each failure
  condition of `BorrowEquipmentService`.

---

## 2. Dependency Direction

```text
     ConsoleDemo (Executable / future Avalonia UI)
          │
          ▼
     Application
       │      ▲
       ▼      │
     Domain   │
              │
     Infrastructure
```

- `ConsoleDemo` depends on `Application`, `Domain`, and `Infrastructure`,
  because something has to construct the concrete repositories and hand them
  to the service. A future Avalonia UI project would take exactly this same
  role.
- `Application` depends on `Domain` (it coordinates domain objects) but
  **not** on `Infrastructure`. It only knows about repository *interfaces*.
- `Infrastructure` depends on `Application` (to implement its interfaces) and
  `Domain` (to store domain objects).
- `Domain` depends on nothing else in the solution — it is the innermost,
  most stable layer.

This means the arrow between Application and Infrastructure only exists
because Infrastructure implements Application's interfaces — Application
itself never references Infrastructure directly. This is what allows the
storage mechanism to be swapped later without changing Application code.

---

## 3. Use Case Mapping

```text
Actor: Student
Use Case: Borrow Equipment
Application Service: BorrowEquipmentService
Domain Objects Used: Student, Equipment, Borrowing, BorrowingStatus
Repository Interfaces Used: IStudentRepository, IEquipmentRepository, IBorrowingRepository
Infrastructure Implementations Used: InMemoryStudentRepository, InMemoryEquipmentRepository, InMemoryBorrowingRepository
```

---

## 4. Reflection

**1. Why should the application service depend on a repository interface
instead of directly depending on a database implementation?**

Because `BorrowEquipmentService` should only care about *what* data
operations it needs (get a student, get equipment, count active borrowings),
not *how* that data is stored. Depending on an interface means the service's
code never has to change if the storage technology changes — only a new
implementation of the interface has to be written and swapped in.

**2. Which parts of your current solution could remain unchanged if SQLite
were added later?**

The entire `Domain` project and the entire `Application` project (including
`BorrowEquipmentService` and all three repository interfaces) would remain
unchanged. Only `Infrastructure` would change — new classes like
`SqliteStudentRepository` would be added there, implementing the same
interfaces the in-memory classes implement today.

**3. Which project would eventually contain Avalonia Views?**

A new UI project (e.g. `EquipmentBorrowing.UI` or
`EquipmentBorrowing.Desktop`), sitting alongside `ConsoleDemo` at the same
level, depending on `Application` (and `Infrastructure`, to wire up
dependencies at startup) — the same relationship `ConsoleDemo` has now.

**4. Should an Avalonia button directly execute database queries? Why or why
not?**

No. A button's click handler should call an application service (like
`BorrowEquipmentService.ExecuteAsync`), the same way `ConsoleDemo` does. If
the UI executed SQL directly, the business rules (student allowed to borrow,
equipment availability, borrowing limits) would either be duplicated in the
UI layer or skipped entirely, and the rules would no longer be testable or
reusable outside that specific UI.

**5. What part of your implementation represents the actual business
operation requested by the actor?**

`BorrowEquipmentService.ExecuteAsync` — it is the single place where all six
borrowing rules are checked and where the actual decision to create (or
reject) a Borrowing record is made.

## Desktop Project

`EquipmentBorrowing.Desktop` is responsible for displaying information and
collecting input. It references Application, Infrastructure, and Domain, but
those three projects reference nothing back — they remain unaware Avalonia
exists.

## Updated Architecture

Avalonia View
    |  Binding / Command
    v
ViewModel
    |  Application Operation
    v
Application Service
    |
    +--> Domain
    |
    v
Repository Interface
    ^
    |
Infrastructure Implementation

## Borrow Equipment Flow

The user selects a student and equipment in EquipmentView, then clicks
"Borrow Equipment". This triggers EquipmentViewModel's BorrowCommand, which
performs presentation validation (is a student/equipment actually selected?)
before calling BorrowEquipmentService.ExecuteAsync. The service checks all
business rules and returns a result, which the ViewModel turns into a status
message. The equipment list is then reloaded so the UI reflects the new
availability state.

## Return Equipment Flow

The user selects an active borrowing in BorrowingsView and clicks "Return
Equipment". BorrowingsViewModel's ReturnCommand calls
ReturnEquipmentService.ExecuteAsync, which locates the borrowing, checks it
hasn't already been returned, updates its status, and marks the equipment
available again. The active borrowings list is reloaded afterward.

## Architectural Reflection

1. Why should the View not call a repository directly? Because the View
   would then need to know about storage details and would have no single
   place enforcing borrowing rules — logic would end up duplicated or
   skipped entirely.
2. Why should business rules not be implemented in the ViewModel? Because
   the ViewModel exists to manage presentation state, not to decide whether
   a borrowing is valid — that decision needs Student, Equipment, and other
   Borrowing records together, which is exactly what the Application service
   already does.
3. What is the responsibility of the ViewModel? To hold presentation state,
   expose commands the View can bind to, perform basic input validation, and
   delegate the actual operation to an application service.
4. Why can the existing Application layer work without knowing Avalonia is
   being used? Because it only depends on repository interfaces and plain
   C# types, none of which reference any UI framework.
5. What advantage comes from registering dependencies in one composition
   point? Changing an implementation (e.g. swapping in-memory repositories
   for SQLite ones later) only requires editing App.axaml.cs — no ViewModel
   or Service code changes.
6. If the in-memory repository were replaced by SQLite later, which parts
   should remain unchanged? Domain, Application (including both services
   and all three interfaces), and every View and ViewModel — only the
   Infrastructure implementations and the composition root registrations
   would change.
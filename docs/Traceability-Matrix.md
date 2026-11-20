# Traceability Matrix

Use case → rules → implemented behavior → verification evidence. Evidence IDs refer to [Verification-Evidence.md](Verification-Evidence.md). Full case tables: [Use-Case-Suite.md](Use-Case-Suite.md).

| UC | Use case | Rules | Implemented behavior (screen → service) | Database protection | Automated tests | Evidence | Rubric |
|---|---|---|---|---|---|---|---|
| UC-01 | View Equipment Catalog | BR-05–07, 10, 18–19 | Manage Equipment / Borrow screens → `EquipmentManagementService` | `Equipment` table, unique `Identifier` | – | A01 | R1 |
| UC-02 | Find Available Equipment | BR-10, 18–19 | Available-only equipment list → `EquipmentManagementService.SearchAsync` | – | – | A02 | R1 |
| UC-03 | Borrow Equipment | BR-08–11, 14–16, 18–19 | Borrow screen → `BorrowEquipmentService` in one transaction | FKs, `UX_Borrowings_EquipmentId_Active` partial unique index, check constraints | `BorrowEquipmentServiceTests` | A03 | R1, R2 |
| UC-04 | View Active Borrowings | BR-14, 18–19 | Active Borrowings screen → `BorrowingQueryService.GetActiveAsync` | FKs | – | A04 | R1 |
| UC-05 | Return Equipment | BR-10, 13–16, 18–19 | Active Borrowings screen → `ReturnEquipmentService` | Status and return-date check constraints | `ReturnEquipmentServiceTests` | A05 | R1, R2 |
| UC-06 | Check Out Multiple Items | BR-08–12, 14–16, 18–19 | Borrow screen multi-select → `CheckOutMultipleService` (all-or-nothing) | Single transaction, partial unique index | `CheckOutMultipleServiceTests` | A06, A18 | R3 |
| UC-07 | Student Borrowing History | BR-14, 18–19 | Histories screen → `BorrowingQueryService.GetStudentHistoryAsync` (async) | FKs | – | A07 | R6, R7 |
| UC-08 | Equipment Borrowing History | BR-14, 18–19 | Histories screen → `BorrowingQueryService.GetEquipmentHistoryAsync` (async) | FKs | – | A08 | R6, R7 |
| UC-09 | Search, Filter, Sort | BR-17–19 | Borrowing Records screen → `BorrowingQueryService.SearchAsync` (async) | – | – | A09 | R6, R7 |
| UC-12 | Register Student | BR-01–02, 16, 18–19 | Students screen → `StudentManagementService.RegisterAsync` | Unique `citext` `StudentNumber`, NOT NULL | – | A12 | R4, R2 |
| UC-13 | Update Student Information | BR-01–03, 14, 16, 18–19 | Students screen → `StudentManagementService.UpdateAsync` | Same Id, FKs, unique index | – | A13 | R4 |
| UC-14 | Change Eligibility | BR-04, 08, 13–14, 18–19 | Students screen → `StudentManagementService.SetEligibilityAsync` | – | – | A14 | R4 |
| UC-15 | Register Equipment | BR-05–06, 10, 16, 18–19 | Manage Equipment screen → `EquipmentManagementService.RegisterAsync` | Unique `citext` `Identifier`, NOT NULL | – | A15 | R5, R2 |
| UC-16 | Update Equipment Information | BR-05–07, 10, 14, 16, 18–19 | Manage Equipment screen → `EquipmentManagementService.UpdateAsync` | Same Id, unique index | – | A16 | R5 |
| Cross-cutting | Schema, constraints, migrations | BR-01, 05, 10, 14, 16 | `dotnet ef` migrations | All of the above | – | A17 | R2 |
| Cross-cutting | Failure, unknown outcome, lifetimes | BR-15, 18–19 | `IUseCaseExecutor`, `IUnitOfWork`, `CommitOutcomeUnknownException` | – | `CheckOutMultipleServiceTests` | A18 | R3, R7, R8 |
| Cross-cutting | Documentation | All | README, diagram, suite, this matrix, Git history | – | – | A19 | R8 |

## Business rule → where it is enforced

| Rule | Application layer | PostgreSQL |
|---|---|---|
| BR-01, BR-05 identity | Trim, blank and duplicate checks in the management services | `citext` unique indexes on `StudentNumber` and `Identifier` |
| BR-02, BR-06 defaults | New student eligible, new item available, blank optional text stored as NULL | NOT NULL on required columns |
| BR-03, BR-07 updates | Same Id updated in place; availability not editable | Foreign keys keep history |
| BR-04 eligibility | `SetEligibilityAsync`; return does not check eligibility | – |
| BR-08–09, 11–12 | Revalidated inside the transaction (student, items, limit of 3, later date, distinct items ≥ 2) | Check constraints |
| BR-10 | Availability check plus `IsAvailable` update | Partial unique index `UX_Borrowings_EquipmentId_Active` (`Status = 'Active'`) |
| BR-13 | Return only when active; repeat rejected | Return-date check constraint |
| BR-14 | No delete operations exist | Foreign keys |
| BR-15 | `IUnitOfWork.ExecuteInTransactionAsync` | Transaction rollback |
| BR-16 | Supplements the database | PK, FK, NOT NULL, unique, check constraints |
| BR-17 | `BorrowingQueryService` / `EfBorrowingQueryRepository` (`ILike`, status, sort, Id tie-breaker) | – |
| BR-18 | `PersistenceException`, `DuplicateValueException`, `CommitOutcomeUnknownException`, messages without credentials | – |
| BR-19 | `IUseCaseExecutor` async loads, loading and error flags, stale-response guard | – |

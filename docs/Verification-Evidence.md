# Verification and Integration Evidence (A01–A19)

**Environment:** Windows, PostgreSQL 18, pgAdmin 4, .NET 10, Avalonia.
**Database used:** `equipment_borrowing` for normal runs and a disposable copy (for example `equipment_borrowing_test`) for A06, A17 and A18, never a shared one.
**Rule for every row:** read the database back with a **fresh connection**. In pgAdmin, open a new Query Tool tab, or restart the app and read again. A screenshot alone is not evidence.

## Setup data (once)

Create at least: three students (one **ineligible**, one with **no history**); four to five items (available and unavailable, one **never borrowed**); active **and** returned borrowings for at least two students and two items.

## Readback queries (run in a new pgAdmin tab)

```sql
-- Q1 students
SELECT "Id","StudentNumber","Name","Contact","IsAllowedToBorrow" FROM "Students" ORDER BY "Id";
-- Q2 equipment
SELECT "Id","Identifier","Name","Description","IsAvailable" FROM "Equipment" ORDER BY "Id";
-- Q3 borrowings with names
SELECT b."Id", s."StudentNumber", e."Identifier", b."DateBorrowed", b."ExpectedReturnDate", b."ActualReturnDate", b."Status"
FROM "Borrowings" b JOIN "Students" s ON s."Id"=b."StudentId" JOIN "Equipment" e ON e."Id"=b."EquipmentId"
ORDER BY b."Id";
-- Q4 integrity check: must return 0 rows
SELECT "EquipmentId", COUNT(*) FROM "Borrowings" WHERE "Status"='Active' GROUP BY "EquipmentId" HAVING COUNT(*)>1;
-- Q5 consistency: must return 0 rows (unavailable item without an active borrowing, or the reverse)
SELECT e."Id" FROM "Equipment" e
WHERE e."IsAvailable" = EXISTS (SELECT 1 FROM "Borrowings" b WHERE b."EquipmentId"=e."Id" AND b."Status"='Active');
-- Q6 migration history
SELECT * FROM "__EFMigrationsHistory";
```

## Scenario table

| ID / UC | Setup | Action (success, rejection, failure) | Expected | Observed | Evidence |
|---|---|---|---|---|---|
| A01 / UC-01 | Seeded or test data | Open the catalog. Compare with Q2. Empty the table in a disposable database (or filter to no match) and reload. Stop PostgreSQL and reload. | List equals Q2 incl. availability; empty message is gray and the connection error is red; no data changes |Catalog matched Q2 incl. availability. Empty and error states showed different messages. No data changed |[A01 screenshot](evidence/A01.png)|
| A02 / UC-02 | Mix of available and unavailable items | Show available only. Mark all borrowed for no-match. Stop PostgreSQL and reload. | Only `IsAvailable = true` rows; no-match and error differ |Only available items listed; unavailable excluded. No-match and error messages differed. |[A02 screenshot](evidence/A02.png) |
| A03 / UC-03 | Eligible student, available item | Issue one item. Then reject: ineligible student, unavailable item, 4th active item, past/today date, no student or item selected. Restart the app. | One new Active row and item unavailable (Q3, Q2). Each rejection leaves Q3 unchanged. State survives restart |Issued 1 item: one Active row, item unavailable. Ineligible, unavailable, over-limit and invalid-date requests rejected with no change. Persisted after restart. |[A03 screenshot](evidence/A03.png) |
| A04 / UC-04 | Active and returned records | Open Active Borrowings. No-active case. Connection error. | Only `Status='Active'`, with student and item names. Empty ≠ error |Only Active records shown with student and item names. Returned excluded. Empty and error states differed. |[A04 screenshot](evidence/A04.png) |
| A05 / UC-05 | One active borrowing | Return it. Return the same record again. Select a stale record (returned elsewhere). Restart. | Same Id becomes Returned with `ActualReturnDate`; item available; row kept; repeat rejected with no change |Return set the same row to Returned with date; item available; repeat return rejected, no change. Persisted after restart. |[A05 screenshot](evidence/A05.png) |
| A06 / UC-06 | Student with 0 active; items I1–I4 | Issue I1+I2 (success). Reject: duplicate item, one item only, unavailable item, past date, ineligible student, over limit. Test exactly 3 active. Run with `EQUIPMENT_BORROWING_FAULT_POINT=checkout-after-first-item`. | Success adds two Active rows and two unavailable items. Each rejection and the fault case add **no** rows and change **no** availability (compare Q3 and Q2 before and after) |Two items issued: two Active rows, both unavailable. Each rejection and the forced failure left no rows or availability changed. |[A06 screenshot](evidence/A06.png) |
| A07 / UC-07 | Two students with distinct histories, one with none | Select each student; click quickly between students; stop PostgreSQL and select. | Only that student's records; no history is a gray message; error is red; last selection wins |Each student showed only their own records. No-history and error messages differed; last selection won. |[A07 screenshot](evidence/A07.png) |
| A08 / UC-08 | Two items with distinct histories, one with none | Same as A07 for items. | Same |Each item showed only its own records. No-history and error messages differed. |[A08 screenshot](evidence/A07.png) |
| A09 / UC-09 | Known mixed records | Search by student name, student number, item name (mixed case, padded spaces). Use All, Active, Returned. Use both sort fields in both directions. Combine, press Reset. No match. Stop PostgreSQL. | Membership and order match Q3 sorted the same way, ties by Id ascending; Reset restores all; no match is gray; error is red |Search, status filter, both sort fields and directions, combined criteria and Reset matched the database order. No-match and error states shown. |[A09 screenshot](evidence/A07.png) |
| A12 / UC-12 | – | Register a student. Reject blank name, blank number, duplicate number, duplicate with different case or spaces. Restart. | Exactly one new row, eligible by default; rejected attempts add none (Q1) |Student registered once, eligible by default. Blank and duplicate (case/space) numbers rejected; no extra rows. Persisted after restart. |[A12 screenshot](evidence/A12.png) |
| A13 / UC-13 | Student with history | Edit name, number, contact. Clear contact. Reject blank name and duplicate number. Restart. | Same Id; contact NULL when cleared; existing borrowings still point to the Id (Q3); rejected edits change nothing |Edit kept the same Id; cleared contact stored as empty. Blank and duplicate edits rejected. Borrowing history intact. |[A13 screenshot](evidence/A12.png) |
| A14 / UC-14 | Student with an active borrowing | Restrict. Try a new issue. Return the existing item. Permit. Issue again. Repeat the same choice. | New issue rejected; active record kept and still returnable; permit allows a valid issue; repeat gives a no-change message |Restricted student could not borrow but could return. Permitting allowed a new borrow. Repeating a state gave a no-change message. |[A14 screenshot](evidence/A14.png) |
| A15 / UC-15 | – | Register an item. Reject blank name, blank identifier, duplicate identifier (different case or spaces). Restart. | One new item, `IsAvailable = true`, no borrowing; rejected attempts add none (Q2) |Item registered once as available. Blank and duplicate identifiers rejected; no extra rows. Persisted after restart. |[A15 screenshot](evidence/A15.png) |
| A16 / UC-16 | Item with history | Edit identifier, name, description. Clear description. Reject blank and duplicate. Confirm there is no availability field in the form. | Same Id; history and availability unchanged; description NULL when cleared |Edit kept the same Id; cleared description stored as empty. Blank and duplicate rejected; availability and history unchanged; no availability field in the form. |[A16 screenshot](evidence/A15.png) |
| A17 / cross | Empty disposable database | Run migrations. Show Q6 and the schema (`\d` is not available in pgAdmin; use the table properties or `information_schema`). Try invalid writes (below). | Database rejects each invalid write; messages recorded |Migrations applied to a clean database. Invalid foreign key, duplicate student number, duplicate identifier and second active borrowing were rejected by PostgreSQL. |[A17 screenshot](evidence/A17.png) |
| A18 / cross | Disposable database | Stop PostgreSQL, perform an operation, restart, press Reload, retry. Run the fault-injection checkout. Walk through code (below). | Clear failure, no false success, no partial change; works after recovery |With PostgreSQL stopped the app showed a clear error and no false success. After restart, retry worked and the fault-injected checkout left no partial changes. |[A18 screenshot](evidence/A18.png) |
| A19 / cross | Fresh clone | Follow README from a clean folder. Compare diagram, suite and matrix with the code. | Setup works; documents agree with source |Clean clone built and ran by following the README; diagram, suite and matrix agree with the code. |[A19 screenshot](evidence/A18.png) |

### A17 invalid-write SQL (disposable database only)

```sql
-- invalid foreign key
INSERT INTO "Borrowings" ("StudentId","EquipmentId","DateBorrowed","ExpectedReturnDate","Status")
VALUES (999999, 999999, CURRENT_DATE, CURRENT_DATE + 1, 'Active');

-- duplicate student number (case/whitespace variants must also fail for the normalized value)
INSERT INTO "Students" ("StudentNumber","Name","IsAllowedToBorrow") VALUES ('<existing number in lower/upper case>','Dup',true);

-- duplicate equipment identifier
INSERT INTO "Equipment" ("Identifier","Name","IsAvailable") VALUES ('<existing identifier in other case>','Dup',true);

-- second active borrowing for one item (use a real StudentId and an item that already has an Active row)
INSERT INTO "Borrowings" ("StudentId","EquipmentId","DateBorrowed","ExpectedReturnDate","Status")
VALUES (<StudentId>, <EquipmentId>, CURRENT_DATE, CURRENT_DATE + 1, 'Active');
```
If a column name above differs from your schema, check `Q1`–`Q3` and adjust. Record the exact PostgreSQL error (SQLSTATE 23503 for foreign key, 23505 for unique, 23514 for check).

### A18 code walkthrough points

1. **Layers:** View → ViewModel → Application service → repository interface → EF Core → PostgreSQL. Views and ViewModels contain no SQL and no connection strings.
2. **Context lifetime:** `IUseCaseExecutor` creates a new DI scope per UI operation, so every operation gets its own `DbContext`. ViewModels are long-lived and never hold a `DbContext`.
3. **Transaction boundary:** `IUnitOfWork.ExecuteInTransactionAsync` wraps revalidation and all writes. A known failure rolls back. A lost commit acknowledgement raises `CommitOutcomeUnknownException`, and the UI tells staff to reconnect and refresh before deciding (no automatic retry).
4. **Fault seam:** `IFaultInjector` (enabled only by `EQUIPMENT_BORROWING_FAULT_POINT=checkout-after-first-item`) throws after the first item is written and before commit.

### Restart readback log

| Entity | State before restart | State after restart | Same? |
|---|---|---|---|
| New student | | | |
| New / edited item | | | |
| Active borrowing | | | |
| Returned borrowing | | | |

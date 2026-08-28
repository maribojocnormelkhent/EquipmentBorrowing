# Part A – System Analysis

## A. Actors

| Actor | What they expect from the system |
|---|---|
| **Student** | To be able to request equipment they need, be told immediately if the request is not allowed (and why), and to return equipment when finished. |
| **Laboratory Staff / Custodian** *(implied actor)* | To trust that the system will not approve a borrowing that breaks the rules (unavailable equipment, suspended student, borrowing limit), and to see equipment become available again once returned. |

The scenario is driven by a single primary actor — the **Student** — who initiates every request. Laboratory staff are a secondary actor who benefit from the system enforcing rules correctly, even though they do not directly trigger use cases in this version of the system.

---

## B. Use Cases

### Use Case 1

| Item | Description |
|---|---|
| Use Case | Borrow Equipment |
| Primary Actor | Student |
| Preconditions | Student is registered in the system; equipment record exists. |
| Main Action | Student requests to borrow a specific piece of equipment. |
| Expected Result | A new Borrowing record is created with status `Active`; the equipment becomes unavailable. |
| Possible Failure | Student is not allowed to borrow; equipment does not exist; equipment is unavailable; student already has the maximum number of active borrowings. |

### Use Case 2

| Item | Description |
|---|---|
| Use Case | Return Equipment |
| Primary Actor | Student |
| Preconditions | An Active borrowing exists linking the student to the equipment. |
| Main Action | Student returns the borrowed equipment. |
| Expected Result | The Borrowing record status changes to `Returned`; the equipment becomes available again. |
| Possible Failure | No active borrowing exists for that student/equipment pair; equipment record does not exist. |

### Use Case 3

| Item | Description |
|---|---|
| Use Case | Find Available Equipment |
| Primary Actor | Student |
| Preconditions | None. |
| Main Action | Student browses or searches for equipment that is currently available. |
| Expected Result | A list of equipment where `IsAvailable = true` is returned. |
| Possible Failure | No equipment currently available (empty result — not an error, just an empty list). |

*(This laboratory activity implements Use Case 1 in full, per the Part E instruction to choose one major use case. Use Cases 2 and 3 are analyzed here to satisfy Part A but are left as an exercise for extending the system — their repository methods were intentionally **not** added to the interfaces, per the "don't add methods before they're needed" rule in Part D.)*

---

## C. Domain Concepts

### Student

1. **Must contain:** an identifier, a name, and whether the student is currently allowed to borrow.
2. **Rules/state it owns:** its own `IsAllowedToBorrow` flag, and the ability to be suspended/reinstated.
3. **Not its responsibility:** knowing how many items it currently has borrowed (that is derived from Borrowing records via the repository), and not responsible for deciding whether a *specific* borrow request should succeed — that decision needs information from Equipment and Borrowing too, so it belongs in the application service.

### Equipment

1. **Must contain:** an identifier, a name, and whether it is currently available.
2. **Rules/state it owns:** its own `IsAvailable` flag, and the ability to be marked borrowed/returned.
3. **Not its responsibility:** knowing *who* currently has it, or how long they can keep it — that belongs to Borrowing.

### Borrowing

1. **Must contain:** the student, the equipment, the date borrowed, the expected return date, and the current status.
2. **Rules/state it owns:** its own `Status` (Active/Returned), and the ability to mark itself as returned.
3. **Not its responsibility:** deciding whether it was *allowed* to be created in the first place — that check requires looking at the Student, the Equipment, and other Borrowing records at once, which is exactly why `BorrowEquipmentService` exists as a separate coordinating object instead of stuffing that logic into the Borrowing class itself.

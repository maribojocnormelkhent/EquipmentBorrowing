# Lab 4 Submission Checklist

Students: JOHN PAUL G. AGUILAR (2024301621) and NORMEL KHENT C. MARIBOJOC (2024300638)
Repository: https://github.com/maribojocnormelkhent/EquipmentBorrowing

Tick each box before submitting. Each pair member submits individually; both may point to the same repository or archive.

## A. Identification
- [x] Both names and student IDs are in the README author table
- [ ] Both names and student IDs are also in a private comment on each partner's submission page
- [ ] Working alone? Only your own name and ID are listed

## B. Source and migrations (Deliverable 1)
- [ ] Complete updated source: Domain, Application, Infrastructure, Desktop, Tests
- [ ] PostgreSQL migrations are included (`src/EquipmentBorrowing.Infrastructure/Migrations`)
- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet test` passes
- [ ] Meaningful Git history, with Labs 1–3 history preserved (`git log --oneline`)
- [ ] Commits are **pushed** (`git push`) and visible on GitHub, or an archive that includes the `.git` folder is made
- [ ] No credentials, passwords or connection strings in any committed file (search: `git grep -i "password="`)
- [ ] No `*.db`, `bin/`, `obj/` or `.vs/` folders committed (check `.gitignore`)

## C. README (Deliverable 2)
- [ ] Prerequisites, installation, secret-free configuration (user-secrets or environment variable)
- [ ] Migration commands, initial-data behavior, how to run
- [ ] Architecture, DbContext lifetime and transaction decisions
- [ ] Earlier Lab 1–3 documentation preserved
- [ ] Followed once from a clean folder on a clean database (A19)

## D. Use-case documents (Deliverable 3)
- [ ] `docs/Use-Case-Diagram.md` and `.png` (custodian as primary actor)
- [ ] `docs/Use-Case-Suite.md` (14 cases: UC-01–09 and UC-12–16)
- [ ] `docs/Traceability-Matrix.md` (use case → implementation → evidence)

## E. Verification evidence (Deliverable 4)
- [ ] `docs/Verification-Evidence.md` has Observed and Evidence filled for A01–A19
- [ ] Each scenario has setup, action, expected and observed result
- [ ] Success, rejection and safe-failure cases are shown
- [ ] Database readbacks used a **fresh** connection (new pgAdmin tab or after restart)
- [ ] Restart readback log completed
- [ ] A06 safe-failure demo done with `EQUIPMENT_BORROWING_FAULT_POINT=checkout-after-first-item` on a disposable database
- [ ] A17 constraint tests done and error codes recorded (23503, 23505)
- [ ] A18 disconnect and recovery shown, code walkthrough notes ready
- [ ] Screenshots saved in `docs/evidence/`

## F. Demonstration readiness
- [ ] A01–A05 existing workflows
- [ ] A12–A14 student management
- [ ] A15–A16 equipment management
- [ ] A06 multi-item checkout, rejections and rollback
- [ ] A07–A09 histories and search
- [ ] A17–A18 migrations, constraints and recovery
- [ ] A19 documents match the code

## G. Final steps
- [ ] Late policy: submit before the deadline (late work loses 10 points)
- [ ] Repository link or ZIP archive attached to the submission page
- [ ] The instructor can open the repository (public, or access given if private)

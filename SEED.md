# Test data seed

From the repository root:

```bash
docker compose up -d mongo
docker compose exec mongo mongosh timesheet /seed/seed-data.js
```

The script is reproducible: it clears only `time_entries`, `closed_periods`,
`employees`, and `projects`, then inserts the acceptance dataset.

Expected records:

- 2 employees
- 2 projects
- 4 time entries
- 0 closed periods

Expected report values:

- February 2026: П-001 — 8 hours, 4 000 ₽, 20%.
- March 2026:
  - П-001 — 12 hours, 7 600 ₽, 38%.
  - П-002 — 10 hours, 7 000 ₽, 140%, overspent.
  - Total — 22 hours, 14 600 ₽.

Error scenarios from the task are intentionally not pre-inserted; they are
performed through the API against this clean base dataset.

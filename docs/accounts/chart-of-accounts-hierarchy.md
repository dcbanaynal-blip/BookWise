# Chart of Accounts Hierarchy Design

## Overview
BookWise uses a deterministic multi-level hierarchy so every segment has a defined business meaning. This keeps brownfield imports predictable and gives greenfield builds a clear template. The default terminology for each level is:

| Level | Default Term      | Typical Values                                       | Purpose                                                    |
|-------|-------------------|------------------------------------------------------|------------------------------------------------------------|
| 1     | Category          | Assets, Liabilities, Equity, Revenue, Expense        | Aligns hierarchy with the chart of accounts core sections. |
| 2     | Department        | Sales, Engineering, HR, Support, Operations          | Provides cost-center visibility and departmental rollups.  |
| 3     | Account Purpose   | Cash, Tools, Travel, Supplies, Software, Equipment   | Captures the GL usage within a department.                 |
| 4     | Posting Entity    | Individual Employee, Project, Store, Cost Object     | Leaf-level account that accepts journal entries.           |

- **Category** aligns with the core financial statements.
- **Department** provides cost-center visibility without extra dimensions.
- **Account Purpose** captures the GL usage (cash vs. tools, travel vs. supplies).
- **Posting Entity** is the leaf-level account where entries are recorded (often an employee, but could be a project or store).

### Example – Employee Accounts
> Alice needs both a cash and a tools account inside the Sales department:
>
> ```
> Assets (Level 1)
> └── Sales (Level 2)
>     ├── Cash (Level 3)
>     │   └── Alice (Level 4)  ← leaf account for cash transactions
>     └── Tools (Level 3)
>         └── Alice (Level 4)  ← leaf account for tools purchases
> ```

This lets reports roll up by department (Level 2) or account purpose (Level 3) while still giving each employee their own leaf account.

## Constraints & Rules
- Only Level‑4 nodes (leaves) accept journal entries.
- `AccountType` follows the parent chain and cannot be changed for children.
- `SegmentCode + ParentAccountId` pairs are unique, preventing duplicates at a given level.
- The four-level structure is the baseline, but additional levels (e.g., Region) can be inserted if a tenant needs more context.

## Implementation Notes
- The UI locks the parent/type when you create a child via “+ Child”; root accounts can choose a category/type.
- API validation enforces inheritance and the uniqueness rules above.
- Reporting aggregates by walking the tree: Level 2 totals give departmental spend, Level 3 totals show spending by purpose, etc.

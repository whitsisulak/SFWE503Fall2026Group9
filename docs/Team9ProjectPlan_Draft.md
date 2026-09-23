# CLMS Project Plan

Planned for a team of 4 using AI coding assistance. MVP1 cuts at the end of Sprint 2 (Oct 16, 2026) as one end-to-end slice — order a test, collect and barcode the sample, enter a result, read it on the patient record — and MVP2 takes Sprints 3 to 6 through access control, inventory, instrument ingest, point of sale, reporting and the release build, finishing Dec 11, 2026. 42 stories, 118 points, 12 epics, traced to the ConOps requirement numbers.

## MVP goals

Two increments are demonstrated and graded. Both are cumulative — each builds on everything before it.

**MVP1 — Sprint 2, cut Fri Oct 16, 2026.** One test travels the whole path end to end: a patient walks in, a test is ordered with its ordering physician and insurance captured, a barcoded sample is collected, a technician enters the result, and the result appears on the patient's record. Everything in Sprint 2 exists to make that single path demonstrable; anything that does not serve it — role-based access control included — is deliberately held back to Sprint 3. Demoed in the week of Oct 19 alongside the MVP1 summary.

**MVP2 — Sprints 3 to 6, cut Fri Dec 11, 2026.** The whole system, not the last sprint alone. On top of MVP1 it adds access control closed across the six user types and no test running on missing or expired reagents (Sprint 3); analyzer files posting themselves, point-of-sale checkout and one unified activity log (Sprint 4); and the closed replenishment loop with the financial, inventory and operational reports (Sprint 5); and the tagged release build and review pack (Sprint 6). It ships as a tagged release with a seeded demo dataset, and it is what the final status review runs against.

## Sprint calendar and MVP split

The course calendar runs six sprints across weeks 5 through 16, two weeks each, Monday to Friday. A sprint ends on its second Friday; hold the sprint review and retrospective that Friday and plan the next sprint on the following Monday. The backlog runs across all six, closing on Friday, Dec 11.

| Sprint | Weeks | Start (Mon) | End (Fri) | Sprint goal | Planned | Capacity |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 5–6 | Sep 21 | Oct 2 | A manager can log in and create staff accounts on a deployed stack | 21 | 22 |
| 2 | 7–8 | Oct 5 | Oct 16 | **MVP1** — one test goes order → sample → result → patient record | 25 | 25 |
| 3 | 9–10 | Oct 19 | Oct 30 | Access control closed; no test runs on missing or expired reagents | 27 | 25 |
| 4 | 11–12 | Nov 2 | Nov 13 | Analyzer files post themselves; patients check out and pay; everything is logged | 25 | 25 |
| 5 | 13–14 | Nov 16 | Nov 27 | Replenishment loop closes; financial, inventory and operational reports | 15 | 19 |
| 6 | 15–16 | Nov 30 | Dec 11 | **MVP2** — tagged release, review pack, contingency for carryover | 5 | 15 |

**Velocity with AI assistance: 25 points per sprint for 4 people.** The scale is unchanged — 1 point ≈ 2–3 hours of unassisted work — so do not re-point stories because a tool writes them faster. Points measure the work; velocity measures how fast you get through it. Let Sprint 1 measure the real multiplier and reflow from actuals at the Oct 2 retrospective.

The 25-point figure assumes roughly a third more throughput than 4 people unassisted, not double. Code generation compresses the forms, CRUD screens, migrations, report queries and unit tests that make up most of E3, E4, E8 and E10. It compresses almost nothing in Sprint 1: container networking, database wiring and environment debugging are where these projects actually stall, which is why Sprint 1 is held at 22. Sprint 5 is cut to 19 because Thanksgiving on Nov 26 takes two to three working days out of it.

**Review capacity becomes the constraint, not authorship.** Add to the definition of done: every generated file is read line by line by someone who did not prompt for it, and any team member can explain any file they merged. Your final status review is graded partly on discussing technical debt — a codebase nobody can account for is the failure mode here, and it shows up in exactly that conversation.

## Epics

Twelve epics cover the ConOps. Two of them (E1, E12) are enabler epics with no user-facing value — name them that way in Jira so the burndown does not look padded.

| Epic | Covers | ConOps trace | Sprints | Stories | Points |
| --- | --- | --- | --- | --- | --- |
| <a id="epic-e1"></a>E1 Platform & delivery foundation | Repo, containers, database, migrations, client shell | §5.1 | 1 | 4 | 11 |
| <a id="epic-e2"></a>E2 Identity, access & account security | Account creation, login, passwords, lockout, RBAC | 6.1.3, 6.2.1–6.2.7 | 1, 3 | 7 | 16 |
| <a id="epic-e3"></a>E3 Patient & provider records | Patient demographics, insurance, search, deletion | 6.1.4, 6.1.5 | 2–3 | 2 | 6 |
| <a id="epic-e4"></a>E4 Test order intake | Walk-in and phone-in orders, required field set, reagent pre-check | 6.3.1–6.3.3 | 2–3 | 3 | 14 |
| <a id="epic-e5"></a>E5 Sample tracking & activity log | Barcode print, traceability, unified event log | 6.3.4–6.3.6, 6.5.1 | 2, 4 | 3 | 11 |
| <a id="epic-e6"></a>E6 Results, validation & alerts | Manual entry, two-step approval, critical result escalation | §4 Result Validation, Critical Alerts | 2–3 | 3 | 8 |
| <a id="epic-e7"></a>E7 Instrument integration | HL7 ORU file ingest, barcode matching, instrument registry | 6.3.5 note (Oct 1) | 4 | 2 | 10 |
| <a id="epic-e8"></a>E8 Inventory & reagent control | Stock, lots, expiry, low-stock alerts, purchase orders | 6.3.7, 6.4.1–6.4.7 | 3–5 | 7 | 18 |
| <a id="epic-e9"></a>E9 Billing, POS & purchasing | Checkout, payment types, signature, receipts | 6.6.1–6.6.4 | 4–5 | 4 | 9 |
| <a id="epic-e10"></a>E10 Reporting & analytics | Financial, inventory and operational reports on one engine | 6.5.2–6.5.4 | 5 | 3 | 6 |
| <a id="epic-e11"></a>E11 Lab profile | Laboratory information record | 6.1.1 | 3 | 1 | 2 |
| <a id="epic-e12"></a>E12 Quality & release readiness | CI, release build, review pack | Course deliverables | 1, 6 | 3 | 7 |

Story IDs run CLMS-1 to CLMS-47 with gaps: six stories were folded into their parents where they shared a screen, a query or an engine — patient search into the patient record (CLMS-15), ordering physician into the order field set (CLMS-16), instrument registry into result matching (CLMS-32), the transaction log into the activity log (CLMS-42), supply receipt into the purchase order (CLMS-39), and the two secondary reports onto the financial report's engine. Priority is MoSCoW **against the sprint it sits in**, not against the whole project — a "Could" is the row you drop first when that sprint is in trouble.

## Sprint 1 — Foundation and the login spine (Sep 21 – Oct 2)

**Goal:** a laboratory manager can log in to a running, containerized system and create staff accounts for the other five user types. 21 points against 22 capacity.

| ID | Epic | Story | Points | Priority | Trace | Acceptance Criteria |
| --- | --- | --- | --- | --- | --- | --- |
| CLMS-1 | [E1](#epic-e1) | As a developer, I want a Git repository with a branch strategy and PR template so that all work is reviewed and traceable | 2 | Must | — | Repo has branch protection requiring PR review before merge. PR template covers description, testing, and story trace. One branch-to-merge cycle completed successfully. |
| CLMS-2 | [E1](#epic-e1) | As a developer, I want the API and Postgres to start from one compose command so that every teammate runs an identical stack | 3 | Must | §5.1 | `docker compose up` starts API and Postgres with no manual steps. API connects to Postgres on startup. Teardown and restart reproduces an identical working stack. |
| CLMS-3 | [E1](#epic-e1) | As a developer, I want schema migrations and seed data so that the database can be rebuilt from scratch on demand | 3 | Must | §5.1 | Running migrations against an empty database produces the full schema with no manual SQL. Seed script populates enough data to exercise core screens. Database can be dropped and rebuilt from migrations + seed in one command. |
| CLMS-4 | [E1](#epic-e1) | As a lab user, I want a navigable client shell wired to the API so that new screens have somewhere to live | 3 | Must | 6.1.2 | Client shell renders and calls at least one API endpoint successfully. Navigation between two or more placeholder screens works. New screens can be added without modifying shell wiring. |
| CLMS-7 | [E2](#epic-e2) | As a laboratory manager, I want to create staff user accounts so that only I control who has system access | 3 | Must | 6.2.1 | Manager can create an account for each of the five other user types. Duplicate username/email is rejected with a clear error. Created account can log in immediately. |
| CLMS-9 | [E2](#epic-e2) | As a staff user, I want to log in and hold an authenticated session so that the system knows who is acting | 3 | Must | 6.2.2, 6.2.5 | Valid credentials produce an authenticated session; invalid credentials are rejected. Session persists across navigation without re-login. Session identifies the acting user/role for all subsequent actions. |
| CLMS-8 | [E2](#epic-e2) | As a new staff user, I want to be forced to set a unique password on first login so that manager-issued credentials are never reused | 2 | Must | 6.2.3 | First login after account creation forces a password change before any other action. New password cannot match the manager-issued temporary password. Enforcement happens once, not on every login. |
| CLMS-5 | [E12](#epic-e12) | As a developer, I want CI to build and run tests on every PR so that generated code cannot merge broken | 2 | Should | — | Every PR triggers an automated build and test run. A failing test blocks merge. Build/test status is visible on the PR. |

**This is the sprint where AI assistance helps least and the schedule risk is highest.** CLMS-2 and CLMS-3 are container networking, connection strings and migration tooling — environment problems, not authorship problems, and generated configs that look right but do not run will cost you more time than writing them. Put your two strongest infrastructure people on CLMS-1 to CLMS-4 and do not parallelize behind them until the stack comes up clean.

**Land CLMS-5 early, not last.** With four people generating code, CI is the only thing standing between you and a main branch nobody has read. It is listed last by dependency, not by importance.

**Definition of done from Sprint 1 onward:** merged to main behind review by someone who did not write or prompt for it, unit tests for new logic, migration included, runs from a clean compose up, and every merged file explainable by the person who merged it.

## Sprint 2 — MVP1: one test end to end (Oct 5 – Oct 16)

**Goal:** a patient walks in, a test is ordered with its physician and insurance captured, a barcoded sample is collected, a technician enters the result, and it appears on the patient's record. 25 points against 25 capacity.

This is the MVP1 scope. Everything in it exists to make that single path demonstrable; anything that does not serve the path was pushed to Sprint 3, including role-based access control.

| ID | Epic | Story | Points | Priority | Trace | Acceptance Criteria |
| --- | --- | --- | --- | --- | --- | --- |
| CLMS-18 | [E4](#epic-e4) | As laboratory staff, I want to enter a phoned-in test carrying the full required field set — report, patient, ordering physician, execution details, results and insurance — so that orders are complete, traceable and billable | 8 | Must | 6.3.2 + Oct 1 note | Order captures report type, patient, ordering physician, execution details, results placeholder, and insurance in one submission. Order cannot be saved with a required field missing. Saved order is retrievable and billable. |
| CLMS-14 | [E3](#epic-e3) | As laboratory personnel, I want to create, update and search patients with name, DOB, address, phone, email and insurance so that test requests attach to a real record without duplicates | 5 | Must | 6.1.4 | Patient can be created, updated and searched by name, DOB, address, phone, email and insurance. Search surfaces an existing patient before a duplicate is created. Test order can attach to a searched-and-selected patient record. |
| CLMS-17 | [E4](#epic-e4) | As laboratory personnel, I want to accept and enter a test brought in directly by a patient so that walk-ins can be served | 3 | Must | 6.3.1 | Walk-in test entry does not require a prior phone-in order. Patient and test details are captured directly at intake. Walk-in order reaches the same downstream state as a phoned-in order. |
| CLMS-19 | [E5](#epic-e5) | As a specimen collector, I want to print a sample barcode exactly once so that every sample is uniquely identified and cannot be duplicated | 3 | Must | 6.3.4 | Each sample gets exactly one barcode on print; reprinting does not issue a new identifier. Barcode uniquely identifies the sample. Barcode is scannable/readable by downstream steps. |
| CLMS-20 | [E6](#epic-e6) | As a laboratory technician, I want to enter a result with value, units, reference range and status so that results are captured before machine integration exists | 3 | Must | §4 Result Validation | Result can be entered with value, units, reference range, and status with no instrument connection. Entered result is attached to the correct sample and order. Result is visible before any instrument integration exists. |
| CLMS-22 | [E5](#epic-e5) | As laboratory personnel, I want to see every sample and test for a patient with its current status so that work in progress is traceable | 3 | Must | 6.3.5 | Every sample and test for a patient displays with its current status. Status reflects the true state (ordered, collected, resulted, etc.). List updates as a sample/test progresses. |

**CLMS-18 is the story to start first and the one to keep away from a code generator until the model is settled.** The Oct 1 note fixes six field groups, and that field set is effectively the core data model for the whole system — Sprints 3 to 6 all read from it. Design the schema as a team on a whiteboard, agree it, then generate the forms and validation against it. Reversed, you get a plausible-looking model that quietly does not support instrument results or billing, and you find out in Sprint 4.

**Freeze the CLMS-18 field set at the MVP1 demo** and change it only by change request from then on.

**CLMS-21 has moved to Sprint 3 to make room for the re-pointed CLMS-18 and CLMS-14. A single-signature result still demonstrates the end-to-end path at MVP1; the pathologist's second signature lands in Sprint 3**.

## Sprint 3 — Access control and reagent gating (Oct 19 – Oct 30)

**Goal:** the six user types see only what they should, accounts survive an attack, and no sample is collected or tested against missing or expired reagents. 27 points against 25 capacity — 2 over after re-pointing and taking CLMS-21 from Sprint 2.

Demo MVP1 in the first two days of this sprint off the Sprint 2 increment, run the retrospective, then pull this backlog. Your first individual team evaluation is due in this window.

| ID | Epic | Story | Points | Priority | Trace | Acceptance Criteria |
| --- | --- | --- | --- | --- | --- | --- |
| CLMS-10 | [E2](#epic-e2) | As a laboratory manager, I want each of the six user types to see only their permitted screens and actions so that sensitive functions are restricted | 3 | Must | 6.1.3 | Each of the six roles sees only the screens and actions permitted to it in the RBAC matrix. Attempting a restricted action via URL/API, not just the UI, is also blocked. No role has access beyond what the matrix specifies. |
| CLMS-12 | [E2](#epic-e2) | As the system, I want to lock an account after 5 failed logins and email the laboratory manager so that credential attacks are stopped and noticed | 3 | Must | 6.2.6 | 5 consecutive failed logins locks the account. Manager receives an email notification on lockout. Locked account cannot log in even with the correct password until unlocked. |
| CLMS-11 | [E2](#epic-e2) | As an authorized user, I want to change my own password so that I can rotate a credential I think is exposed | 1 | Must | 6.2.4 | Authorized user can change their own password while logged in. Old password must be verified before the change is accepted. New password takes effect on next login. |
| CLMS-13 | [E2](#epic-e2) | As a laboratory manager, I want to unlock a locked-out account so that staff are not blocked from working | 1 | Must | 6.2.7 | Manager can unlock a locked-out account. Unlocked account can log in normally. Unlock action is recorded (who, when). |
| CLMS-24 | [E8](#epic-e8) | As a laboratory manager, I want a reagent and consumable catalog carrying quantity, batch/lot number and expiration date so that stock can be tracked and recalled | 3 | Must | 6.4.1, §4 Batch & Lot | Catalog records quantity, batch/lot number and expiration date per reagent/consumable. Catalog can be searched or filtered by lot or expiration. A recall can identify every unit under a given lot. |
| CLMS-25 | [E4](#epic-e4) | As a specimen collector, I want to check reagent availability before collecting a sample so that I do not draw a specimen the lab cannot run | 3 | Must | 6.3.3 | System checks reagent availability before a sample is collected. Collection is blocked or flagged when the needed reagent is unavailable. Check reflects current, not stale, inventory levels. |
| CLMS-26 | [E8](#epic-e8) | As the system, I want to decrement inventory when a test is filled so that stock levels reflect actual consumption without manual entry | 3 | Must | 6.4.1 | Inventory decrements automatically when a test is filled, with no manual entry step. Decrement reflects the correct reagent and quantity consumed. Stock level after decrement matches actual consumption. |
| CLMS-27 | [E8](#epic-e8) | As the system, I want to warn and block testing with an expired reagent, test kit or consumable so that invalid results are never produced | 2 | Must | 6.3.7 | Testing with an expired reagent, kit or consumable is blocked, not just warned. Warning is shown before the block occurs. No result can be produced from an expired input. |
| CLMS-21 | [E6](#epic-e6) | As a pathologist, I want to give final approval after the technician's approval so that no result reaches a patient on one signature | 2 | Should | §4 Result Validation | Pathologist can give a second, final approval only after technician approval is recorded. Result is not visible as final to the patient record on technician approval alone. Both approvals are timestamped and attributed. |
| CLMS-33 | [E6](#epic-e6) | As a laboratory technician, I want critical results flagged and escalated on entry so that life-threatening findings are not sitting in a queue | 3 | Should | §4 Critical Result Alerts | A critical result is flagged automatically on entry against defined critical thresholds. Flagged result triggers an escalation (e.g., notification) beyond normal result review. Escalation is visible/traceable, not just logged silently. |
| CLMS-6 | [E11](#epic-e11) | As a laboratory manager, I want to store the lab's name, address, phone, website, owner and working hours so that the system identifies the laboratory | 2 | Should | 6.1.1 | Lab name, address, phone, website, owner and working hours can be stored and edited. Stored profile displays wherever the lab identifies itself in the system. Missing profile fields do not block other system functions. |
| CLMS-23 | [E3](#epic-e3) | As a specimen collector or laboratory manager, I want to remove a patient so that records can be corrected, with no other role able to do it | 1 | Could | 6.1.5 | Specimen collector or laboratory manager can remove a patient record. No other role can perform this action. Removal is recorded in the activity log with who performed it. |

**Dependency:** CLMS-25, CLMS-26 and CLMS-27 all read the catalog from CLMS-24, so CLMS-24 must land in week 1. Split it — schema and seed data separate from the management screens.

**Write the RBAC permission matrix by hand before generating anything for CLMS-10.** Six roles across every screen is exactly the kind of table a generator will fill in plausibly and wrongly, and an over-permissive default is not visible in a demo. One person should own that matrix and review every guard against it.

**Spillover candidates:** CLMS-23, then CLMS-6. Both are single screens with no dependents.

## Sprint 4 — Instrument ingest, point of sale and the event log (Nov 2 – Nov 13)

**Goal:** analyzer output files post themselves onto the right sample, a patient can check out, sign and pay, and everything the system does is on one log. 25 points against 25 capacity.

| ID | Epic | Story | Points | Priority | Trace | Acceptance Criteria |
| --- | --- | --- | --- | --- | --- | --- |
| CLMS-30 | [E7](#epic-e7) | As the system, I want to read the plain-text ORU files the machines produce and parse the pipe/caret message into structured results so that results are captured without retyping | 5 | Must | 6.3.5 note | Parser correctly handles both ConOps sample messages (quantitative and qualitative) with no database or UI dependency. Malformed or unexpected message content fails gracefully rather than crashing. Parser is covered by unit tests written from the message spec, not against the parser's own output. |
| CLMS-31 | [E7](#epic-e7) | As the system, I want to match each parsed result to its sample by barcode, stamp the instrument that produced it, and queue unmatched messages for review so that no result is silently lost | 5 | Must | 6.3.5 note | Each parsed result matches to its sample by barcode with no manual lookup. Instrument that produced the result is stamped on the record. A message with no matching barcode is queued for review instead of being silently dropped or discarded. |
| CLMS-29 | [E5](#epic-e5) | As a laboratory manager, I want one append-only log carrying test activity — collector, sample, patient, times, machine — alongside logins, logouts, purchases and inventory updates, so that the lab can answer an audit from a single source | 5 | Must | 6.3.6, 6.5.1 | Test activity (collector, sample, patient, times, machine) and system events (logins, logouts, purchases, inventory updates) write to one append-only log. Log entries cannot be edited or deleted after write. A single query against the log can answer a basic audit question (e.g., who touched sample X and when). |
| CLMS-28 | [E8](#epic-e8) | As a laboratory manager, I want notification when a machine's reagents drop below 20% so that I can reorder before the lab stops | 3 | Must | 6.4.2 | Manager is notified when a machine's reagent stock drops below 20%. Notification identifies which reagent and which machine. Notification fires once per threshold crossing, not repeatedly for the same low level. |
| CLMS-34 | [E9](#epic-e9) | As a cashier, I want to process test items for purchase from the GUI so that patients can pay for the tests they received | 3 | Must | 6.6.1 | Cashier can select test items for a patient's visit and start a purchase from the GUI. Purchase total reflects the items selected. Purchase can be completed without leaving the checkout screen. |
| CLMS-35 | [E9](#epic-e9) | As a cashier, I want to take cash, debit or credit payment so that patients are not turned away over payment method | 2 | Must | 6.6.4 | Cashier can record payment as cash, debit, or credit. Payment method is stored with the transaction. Purchase cannot complete without a payment method recorded. |
| CLMS-36 | [E9](#epic-e9) | As a cashier, I want the customer to sign for all taken samples at the point of sale so that consent and receipt are on record | 2 | Should | 6.6.2 | Customer signs for all samples taken at the point of sale before checkout completes. Signature is stored with the transaction record. Checkout cannot complete for a required signature that is missing. |

**Build CLMS-30 against the two sample messages in the** **ConOps** **first, as a unit-tested parser with no database and no UI.** 1234\|Test^B123^T3 Uptake\|28\|202208221340 and 5678\|Test^U123^Nitrite\|Absent\|202208221340 give you both a quantitative and a qualitative result. This is the best code-generation target in the project — a fixed format, two worked examples, and an obvious test harness — but generate the tests from the spec before you generate the parser, or you get a parser that passes tests written to match its own bugs. A watched folder the demo drops files into is sufficient; do not build a real instrument interface.

**CLMS-29 is a merge of two requirements.** 6.3.6 wants a test activity log and 6.5.1 wants a transaction log; one append-only event table with a type discriminator satisfies both and halves the work. Say so explicitly in the sprint backlog so the traceability is visible to your grader.

**Sequencing:** CLMS-30/31 are one pair's work for the whole sprint; CLMS-34 to CLMS-36 are an independent pair's and touch almost nothing the instrument stories touch. Split the team that way and the sprint has no internal blocking.

**Spillover candidate****: CLMS-36. CLMS-37 has moved to Sprint 5 to make room for the re-pointed CLMS-31 and CLMS-29**. A sale that records payment is demonstrable without a signature capture or a printed receipt.

## Sprint 5 — Replenishment and reporting (Nov 16 – Nov 27)

**Goal:** the inventory loop closes and managers can pull the required reports. 15 points against 19 capacity — comfortable after moving the release build and review pack to Sprint 6, with the two Could stories as the release valve — in a sprint that loses its last two working days to Thanksgiving on Nov 26.

**MVP2 is the cumulative Sprints 3–6 increment, not this sprint alone** — see [MVP goals](#mvp-goals) for what the final demonstration has to cover.

| ID | Epic | Story | Points | Priority | Trace | Acceptance Criteria |
| --- | --- | --- | --- | --- | --- | --- |
| CLMS-38 | [E8](#epic-e8) | As a laboratory manager, I want to raise a purchase order when stock runs low and have inventory update when the distributor's shipment is received so that the low-stock alert leads all the way to restocked shelves | 3 | Must | 6.4.3, 6.4.4 | Manager can raise a purchase order when stock falls below the low-stock threshold. Receiving the distributor's shipment updates inventory to reflect the received quantity. Loop is traceable end to end: alert → PO → receipt → updated stock. |
| CLMS-40 | [E8](#epic-e8) | As a laboratory manager, I want to adjust inventory on use and remove expired stock with the reason recorded in the activity log so that write-offs are accountable | 3 | Must | 6.4.5, 6.4.7 | Manager can adjust inventory for use and remove expired stock through the UI. Each adjustment/removal writes a reason to the activity log. Adjusted stock level matches the recorded reason and quantity. |
| CLMS-43 | [E10](#epic-e10) | As a laboratory manager, I want a financial report with statistics for a date range so that I can report on lab revenue | 3 | Must | 6.5.2 | Financial report runs for a manager-selected date range. Report totals reconcile against the underlying transaction/event log for that range. Report can be generated with no manual data entry beyond selecting the date range. |
| CLMS-37 | [E9](#epic-e9) | As a cashier, I want to generate and print a receipt for the whole purchase so that the patient leaves with proof of payment | 2 | Should | 6.6.3 | Receipt can be generated and printed for the whole purchase. Receipt lists items, payment method, and total. Receipt generation does not require re-entering transaction data. |
| CLMS-45 | [E10](#epic-e10) | As a laboratory technician, I want an operational report covering turnaround time, machine performance and maintenance so that bottlenecks are visible | 2 | Should | 6.5.4 | Operational report covers turnaround time, machine performance, and maintenance for a selectable range. Report data derives from the event log, with no separate manual tracking. Bottlenecks (e.g., slow turnaround) are visible directly in the report output. |
| CLMS-44 | [E10](#epic-e10) | As a laboratory manager, I want an inventory report for a date range so that stock turnover is visible | 1 | Could | 6.5.3 | Inventory report covers a manager-selected date range. Report shows stock turnover (in/out movement), not just a point-in-time snapshot. Report reuses the Sprint 5 report engine rather than a separate query path. |
| CLMS-41 | [E8](#epic-e8) | As a laboratory manager, I want notification of reagents expiring within 30 days so that stock is used before it is wasted | 1 | Could | 6.4.6 | Manager is notified of reagents expiring within 30 days. Notification identifies the specific reagent, lot, and expiration date. Notification does not duplicate the existing below-20%-stock alert. |

**Order matters here more than in any other sprint.** CLMS-43 reports on what CLMS-29 logged in Sprint 4, so the log has to be right before the report is built. Build the report engine once for CLMS-43 and CLMS-44 and CLMS-45 become a query and a view each — that is why they are 1 and 2 points rather than 3 apiece.

**If velocity slips:** drop CLMS-41 and CLMS-44, then CLMS-45. They land in Sprint 6, which is planned light for exactly this.

## Sprint 6 — Release and final review (Nov 30 – Dec 11)

**Goal:** a tagged release build that reproduces the demo the same way every time, and a review pack assembled before the final status review rather than during it. 5 points against 15 capacity.

**The slack is deliberate.** This sprint runs into the end of the semester, when final exams and other courses compete for the same hours, and it is the landing zone for anything that slipped: Sprint 3 is planned 2 points over capacity and Sprint 5 loses its last two days to Thanksgiving. Plan it light and let carryover fill it. If nothing slips, the time goes into rehearsing the demo and reading code nobody has read yet.

| ID | Epic | Story | Points | Priority | Trace | Acceptance Criteria |
| --- | --- | --- | --- | --- | --- | --- |
| CLMS-46 | [E12](#epic-e12) | As the team, I want a tagged release build with a seeded demo dataset so that the final demonstration runs the same way every time | 3 | Must | Course deliverable | A tagged release build exists with a seeded demo dataset. Running the release build from a clean environment reproduces the same demo state every time. Release is tagged/versioned so it can be re-run without rebuilding from source changes. |
| CLMS-47 | [E12](#epic-e12) | As the team, I want the review pack assembled — velocity, technical debt, risk burndown | 2 | Must | Course deliverable | Review pack includes velocity, technical debt, and risk burndown sections. Technical debt section names what was generated, reviewed, and not yet reviewed. Review pack is assembled and ready before the final status review, not during it. |

**Plan the final status review for the week of Dec 7 and treat Dec 8 as the content freeze.** Your final individual team evaluation and personal reflection also land here.

**Never drop CLMS-46 or CLMS-47.** They are the inputs to a 15% deliverable. The review pack is also where your AI-assisted development shows up: the technical debt discussion should name what was generated, what was reviewed, and what you know you have not read.

## Deferred — in the ConOps, out of the six sprints

Every numbered requirement in §6 is covered by a story. These are §4 capabilities with no numbered requirement behind them — put them in the Jira backlog as ranked-but-unscheduled so the product backlog is visibly groomed and the exclusions read as deliberate.

| Capability | ConOps | Why it is out |
| --- | --- | --- |
| Insurance claim generation and submission | §4 Billing | Needs a payer interface with no available test endpoint; policy number is captured in CLMS-18, which is the part that matters for traceability |
| Patient portal and email result delivery | §4 Report Delivery | A second authenticated front end for external users; results are viewable in-system, which demonstrates the workflow |
| Equipment calibration and maintenance tracking | §4 Equipment Mgmt | Distinct domain model; CLMS-45 reports maintenance data from what the system already holds |
| Medication history | §4 Patient & Provider | Belongs to a pharmacy system, not a diagnostic lab; no §6 requirement depends on it |
| Controlled substances management | §4 Regulatory | Regulatory depth out of proportion to a 14-week project; note it as a known compliance gap in the final review |
| Third-party EHR / accounting integration | §4 Integration | No counterpart system exists to integrate against |
| Multi-branch laboratory network | §4 Integration, §5.1 | §5.1 states multi-lab and cloud scaling as future expansion, not current scope |
| Compliance reports (CDC disease spread, audit exports) | §4 Reporting | CLMS-29's event log is the data source; the export itself is a later story |
| Multi-platform / mobile client | §5.1 | §5.1 states desktop-optimized now, multi-platform later; a MAUI prototype exists as a feasibility spike, not committed sprint scope |
| Data security and privacy (HIPAA/GDPR) | §4 Regulatory Compliance, Objective 8 | No numbered §6 requirement specifies encryption, access-logging depth, or a compliance control set; RBAC (E2) and the activity log (CLMS-29) cover access control and audit trail, but formal HIPAA/GDPR compliance work is undefined scope and out of the six sprints |

If any of these matter to your stakeholder, raise them as change requests using the template on page 11 rather than dropping them silently — the ConOps has a change-request process and using it is cheap evidence of process discipline.

## Schedule risks and what to cut first

Five risks shape the ordering above; each has its mitigation built into a sprint.

| Risk | Impact | Mitigation in the plan |
| --- | --- | --- |
| The 25-point velocity assumes an AI multiplier the team has not measured | Every sprint after the first is planned against a guess | Sprint 1 held to 22; re-baseline from actuals at the Oct 2 retrospective and reflow the bottom rows of Sprints 3–6 before committing |
| Sprint 1 is environment work, where code generation helps least | The sprint that everything queues behind is the one most likely to overrun | Strongest infrastructure pair on CLMS-1 to CLMS-4, no parallel work until the stack comes up clean, CI landed inside the sprint |
| The CLMS-18 field set is the de facto data model | Getting it wrong reworks every later sprint, and a generator will produce a plausible wrong one fast | Schema agreed by the team before any generation, scheduled first in Sprint 2, frozen at the MVP1 demo, changed only by change request |
| Generated code outpaces review | Nobody can explain the codebase at the final status review, where technical debt is graded | Review by a non-author in the definition of done; the Sprint 5 review pack names what was generated, reviewed, and not read |
| Sprint 5 loses its last two days to Thanksgiving, and Sprint 6 carries the 15% deliverable into finals | Final review prepared in a rush or not at all | Release build and review pack moved to Sprint 6, which is planned at 5 points against 15 capacity; two Could stories held as the release valve; content freeze Dec 8 |

**Course deliverables riding on this schedule.** A sprint backlog and a retrospective are due after each of the six sprints — 10% each, 20% together, more than the final demo's 15%. The MVP1 summary and demo plus your first individual team evaluation fall in the week of Oct 19. The risk and opportunity assessment is due early and gets re-burned-down at each review; the five rows above are your starting register. As a 503 student you also owe a Jira dashboard and metrics interpretation after every sprint — stand the dashboard up during Sprint 1 while there is slack, not in Sprint 5.

**Open question:** if any of the four of you is carrying a materially lighter or heavier load than the others, say so before Sprint 1 planning. The 25-point velocity assumes four roughly equal contributors, and the team-evaluation factor at the end of the semester makes an unbalanced split expensive for everyone.

## Appendix A — Stories that warrant a task breakdown

Tasks live in Jira, not in this plan — they change daily, and this document should stay stable enough to point at. This appendix is the decomposition guide: which stories are too large or too layered to start as one unit of work, and where they split. Every story at 5 points or more is broken down before it is pulled into a sprint; 3-point stories are broken down when they cross layers or carry two distinct behaviors; 1- and 2-point stories stay whole.

| Story | Points | Why it splits | Suggested tasks |
| --- | --- | --- | --- |
| CLMS-18 | 8 | Largest story in the plan: a wide required field set, validation rules and a phoned-in intake flow | Order schema and migration; required-field validation (patient, physician, insurance); intake form UI; save and retrieve; test that each required field is rejected when blank |
| CLMS-30 | 5 | File handling and parsing are separate risks; the parser can be built and tested before the file watcher exists | ORU file reader and watched folder; pipe/caret segment parser; malformed-message handling; parser tests against sample ORU files |
| CLMS-31 | 5 | Matching, instrument attribution and the unmatched case are three distinct behaviors | Barcode-to-sample matching; instrument stamp on the result; unmatched-result queue and review; end-to-end test from file to matched result |
| CLMS-29 | 5 | One log schema, but write hooks spread across collection, testing, inventory and point-of-sale flows | Append-only log schema; write hooks in each source flow; block edits and deletes; manager log view with filters |
| CLMS-14 | 5 | Create, update, search and validation can each be demonstrated on their own | Patient schema and migration; create and update screens and endpoints; search by name, DOB or ID; field validation and duplicate check |
| CLMS-9 | 3 | Crosses session storage, API and UI; the failure path is its own test surface | Session model and token; login endpoint; login screen; invalid-credential and session-expiry handling |
| CLMS-10 | 3 | The permission matrix is a design decision; server enforcement and UI gating are separate builds | Permission matrix for the six user types; server-side enforcement; hide screens and actions by role; per-role access test |
| CLMS-12 | 3 | Lockout logic and outbound email fail in unrelated ways | Failed-attempt counter and lock state; enforce lock at login; email to laboratory manager; test at 4, 5 and 6 attempts |
| CLMS-20 | 3 | Result entry, reference-range evaluation and technician approval are distinct steps | Result schema (value, units, range, status); entry form; range check sets status; technician approval step |
| CLMS-34 | 3 | Item selection, totals and the saved transaction can be built separately | Select test items from catalog; cart totals; save transaction; link transaction to test order |
| CLMS-38 | 3 | Raising the order and receiving the shipment are two events days apart | Create purchase order (manager only); PO status tracking; receive shipment and update inventory; log both events |
| CLMS-43 | 3 | Builds the report engine that CLMS-44 and CLMS-45 reuse, so engine and report are different work | Date-range query engine; financial totals and statistics; report display and export; shared interface for the inventory and operational reports |

Stories left whole: CLMS-1, 5, 6, 8, 11, 13, 21, 23, 27, 35, 36, 37, 41, 44, 45 and 47 are single-behavior, single-layer work at 1–2 points — splitting them adds tracking overhead without reducing risk. The remaining 3-point stories (CLMS-2, 3, 4, 7, 17, 19, 22, 24, 25, 26, 28, 33, 40 and 46) can be split at the team's discretion during sprint planning.

## Appendix B — Story points or ideal days

Both are ways to size stories before a sprint. Story points are relative: a 3 is about three times a 1, and nobody claims to know how long a 1 takes until velocity shows it. Ideal days are absolute: one ideal day is the work one person finishes in a day with no meetings, no context switching and no waiting on anyone. This plan uses story points on the Fibonacci scale (1, 2, 3, 5, 8), pinned to a rough anchor of 1 point ≈ 2–3 hours of unassisted work.

**How ideal days would work here.** Take an ideal day as about 6 focused hours. With the plan's anchor, 1 point is roughly a third to half an ideal day, so the whole backlog of 118 points comes to about 39–59 ideal days of unassisted work. Each story would be estimated directly in days instead of points, and sprint capacity would be the ideal days the team can actually give a sprint, not calendar days. The conversion for the point values used in this plan:

| Story points | Ideal days | Example in this plan |
| --- | --- | --- |
| 1 | ½ | CLMS-11 change own password |
| 2 | 1 | CLMS-21 pathologist final approval |
| 3 | 1–1½ | CLMS-9 log in and hold a session |
| 5 | 1½–2½ | CLMS-30 ORU file parsing |
| 8 | 2½–4 | CLMS-18 phoned-in test entry |

**Where ideal days get awkward on this project.** Capacity turns out to be small once it is written in days. The plan's 25-point velocity is about a third above unassisted pace, so unassisted the team clears roughly 19 points a sprint — about 6–9 ideal days for the whole team, or about 1½–2 per person per two-week sprint. That is accurate for students carrying other courses, but next to a 10-working-day sprint it reads like under-commitment, and it invites people to plan by the calendar instead of by capacity. AI assistance makes it worse: an ideal day with a code generator and one without are different amounts of work, so the unit drifts from story to story depending on who estimates and what tool they picture using. Points avoid this — the size of the work stays fixed and the AI speed-up shows up in velocity, where Sprint 1 will measure it.

**Recommendation:** keep story points. The backlog is already pointed, the anchor is written down, and the Oct 2 retrospective will give a real velocity to plan Sprints 2–5 against. Use the conversion table only as a translation when an instructor or stakeholder asks how long something takes, and never estimate the same backlog in both units — two sets of numbers that disagree will cost more time in planning than either one saves.

**Estimating with planning poker.** Point new or changed stories as a team using planning poker with a Fibonacci deck (1, 2, 3, 5, 8, plus ? for "too unclear to size"). Each person picks a card privately, everyone reveals at once, and the highest and lowest voters explain their reasoning before a re-vote — that conversation is where hidden work and misunderstood acceptance criteria come out. Vote on the size of the work, not on how fast a code generator could write it. An 8 means split the story before it enters a sprint, and a ? means refine the acceptance criteria first. The team uses [pointingpoker.com](https://www.pointingpoker.com) or [planningpokeronline.com](https://planningpokeronline.com) to run sessions, so remote members can vote from a browser.

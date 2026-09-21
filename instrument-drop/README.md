# Instrument drop folder

This folder stands in for the shared directory real lab instruments write their
result files into (CLMS ConOps 6.3.5).

Drop a `.txt` file here and the API's background worker picks it up within a few
seconds, parses it, and attaches the results to the test order whose barcode
matches.

## Format

```
BARCODE|Message type (Test)^Test ID^Test description|Value|Date and time
```

Example:

```
1234|Test^B123^T3 Uptake|28|202208221340
5678|Test^U123^Nitrite|Absent|202208221340
```

The timestamp is `yyyyMMddHHmm`.

## What happens to files

| Outcome | Destination |
|---|---|
| Every line parsed and matched an order | `_processed/` |
| Any bad line, or a barcode with no matching order | `_failed/` plus a `.error.txt` |

Nothing is ever deleted — a failed file stays on disk with an explanation next to it.

## Try it

```bash
./scripts/simulate-instrument.sh          # drops one good file
./scripts/simulate-instrument.sh --bad    # drops a file with malformed lines
```

The seeded demo orders use barcodes `1234` and `5678`, so the sample files match
out of the box.

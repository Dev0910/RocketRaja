"""Verify the Unity source tree against the hashes taken before HTML work."""
import json,hashlib
from pathlib import Path
OUT=Path(__file__).resolve().parent;ROOT=OUT.parents[3]
before=json.loads((OUT/'unity-source-before.json').read_text())
now={p.relative_to(ROOT).as_posix():p for folder in ['Assets','Packages','ProjectSettings'] for p in (ROOT/folder).rglob('*') if p.is_file()}
changed=[p for p,h in before.items() if p in now and hashlib.sha256(now[p].read_bytes()).hexdigest()!=h]
missing=sorted(set(before)-set(now));added=sorted(set(now)-set(before))
report={'passed':not(changed or missing or added),'verifiedFiles':len(before),'changed':changed,'missing':missing,'added':added,'scope':['Assets','Packages','ProjectSettings']}
(OUT/'unity-readonly-check.json').write_text(json.dumps(report,indent=2)+'\n')
assert report['passed'],report
print(f"PASS: {len(before)} Unity source files unchanged; no additions or removals.")

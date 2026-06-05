#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const REQ_ROOT = path.join(process.cwd(), "docs", "req");
let manifest;
const manifestPath = path.join(REQ_ROOT, ".req-manifest.json");
if (fs.existsSync(manifestPath)) {
  manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
} else {
  manifest = { reqs: [] };
}

const domains = {
  identity: "Identity (auth)",
  users: "Users",
  invitations: "Invitations",
  access: "Access (roles & permissions)",
  passkeys: "Passkeys",
  issues: "Issues",
};

const byDomain = {};
for (const r of manifest.reqs) {
  (byDomain[r.domain] ??= []).push(r);
}

let md = `# Requirements index

> Manifest of atomic REQ files. Process: \`docs/requirements-change-process.md\`. Validate: \`npm run req:validate\`.

## Shared cross-cutting docs

| Document | Purpose |
| -------- | ------- |
| [_shared/glossary.md](_shared/glossary.md) | Business terms (account, sign-in, invitations) |
| [_shared/account-model.md](_shared/account-model.md) | Observable account attributes |
| [_shared/compliance-gates.md](_shared/compliance-gates.md) | Post-sign-in gate ordering |
| [_shared/permissions.md](_shared/permissions.md) | Permission catalog summary |

## Pending changes

See [changes/](changes/) for open requirement deltas.

`;

for (const [domain, label] of Object.entries(domains)) {
  md += `\n## ${label} (\`${domain}/\`)\n\n| ID | Title | File |\n| -- | ----- | ---- |\n`;
  for (const r of byDomain[domain] ?? []) {
    const fileName = r.file.split("/").pop();
    md += `| ${r.id} | ${r.title} | [${fileName}](${r.file}) |\n`;
  }
}

md += `\n---\n\n_Auto-generated from \`.req-manifest.json\` via \`node scripts/req-readme.mjs\` (also refreshed by \`npm run req:validate\`)._\n`;

fs.writeFileSync(path.join(REQ_ROOT, "README.md"), md);
console.log("Wrote docs/req/README.md");

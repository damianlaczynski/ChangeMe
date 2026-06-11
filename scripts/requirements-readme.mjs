#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";
import { ROOT, formatDomainLabel } from "./requirements-lib.mjs";

const manifestPath = path.join(ROOT, ".requirements-manifest.json");
const manifest = fs.existsSync(manifestPath)
  ? JSON.parse(fs.readFileSync(manifestPath, "utf8"))
  : {
      functional: [],
      non_functional: [],
      reference: [],
      shared_functional: [],
    };

const byDomain = {};
for (const spec of manifest.functional ?? []) {
  (byDomain[spec.domain] ??= []).push(spec);
}

let md = `# Requirements index

> Functional specifications (\`FR-*\`), non-functional requirements (\`NFR-*\`), and reference docs.
> Process: \`docs/requirements/requirements-change-process.md\`. Validate: \`npm run requirements:validate\`.

## Reference documents (\`_shared/reference/\`)

| Document | File |
| -------- | ---- |
`;

for (const doc of manifest.reference ?? []) {
  const fileName = doc.file.split("/").pop();
  md += `| ${fileName} | [${fileName}](${doc.file}) |\n`;
}

md += `
## Shared functional patterns (\`_shared/functional/\`)

| ID | Title | File |
| -- | ----- | ---- |
`;

for (const s of manifest.shared_functional ?? []) {
  const fileName = s.file.split("/").pop();
  md += `| ${s.id} | ${s.title || fileName} | [${fileName}](${s.file}) |\n`;
}

md += `
## Non-functional requirements (\`_shared/non-functional/\`)

| ID | Title | File |
| -- | ----- | ---- |
`;

for (const n of manifest.non_functional ?? []) {
  const fileName = n.file.split("/").pop();
  md += `| ${n.id} | ${n.title || fileName} | [${fileName}](${n.file}) |\n`;
}

md += `
## Pending changes

See [changes/](changes/) for open requirement deltas.

`;

const domainOrder = Object.keys(byDomain).sort((a, b) => a.localeCompare(b));

for (const domain of domainOrder) {
  const specs = byDomain[domain];
  if (!specs?.length) continue;
  const label = formatDomainLabel(domain);
  md += `\n## ${label} (\`functional/${domain}/\`)\n\n| ID | Title | File |\n| -- | ----- | ---- |\n`;
  for (const s of specs.sort((a, b) => a.id.localeCompare(b.id))) {
    const fileName = s.file.split("/").pop();
    md += `| ${s.id} | ${s.title} | [${fileName}](${s.file}) |\n`;
  }
}

md += `\n---\n\n_Auto-generated from \`.requirements-manifest.json\` via \`node scripts/requirements-readme.mjs\` (also refreshed by \`npm run requirements:validate\`)._\n`;

fs.writeFileSync(path.join(ROOT, "README.md"), md);
console.log("Wrote docs/requirements/README.md");

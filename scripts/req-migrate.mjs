#!/usr/bin/env node
/**
 * One-time migration: split monolithic *-requirements.md into atomic REQ files.
 * Run: node scripts/req-migrate.mjs
 */

import fs from "node:fs";
import path from "node:path";

const REQ_ROOT = path.join(process.cwd(), "docs", "req");

const MONOLITHS = [
  { file: "auth-requirements.md", domain: "identity", prefix: "AUTH" },
  { file: "users-requirements.md", domain: "users", prefix: "USR" },
  { file: "invitations-requirements.md", domain: "invitations", prefix: "INV" },
  { file: "roles-requirements.md", domain: "access", prefix: "ROL" },
  { file: "passkeys-requirements.md", domain: "passkeys", prefix: "PKY" },
  { file: "issues-requirements.md", domain: "issues", prefix: "ISS" },
];

function slugify(title) {
  return title
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-|-$/g, "")
    .slice(0, 80);
}

function parseReqSections(content) {
  const lines = content.split("\n");
  const preamble = [];
  const sections = [];
  let current = null;
  let i = 0;

  while (i < lines.length && !lines[i].startsWith("# REQ-")) {
    preamble.push(lines[i]);
    i++;
  }

  for (; i < lines.length; i++) {
    const line = lines[i];
    const match = line.match(/^# (REQ-[A-Z]+-\d+): (.+)$/);
    if (match) {
      if (current) sections.push(current);
      current = { id: match[1], title: match[2], lines: [] };
    } else if (current) {
      current.lines.push(line);
    }
  }
  if (current) sections.push(current);

  return { preamble: preamble.join("\n").trim(), sections };
}

function extractDependsOn(body) {
  const refs = new Set();
  const re = /REQ-[A-Z]+-\d+/g;
  let m;
  while ((m = re.exec(body)) !== null) {
    if (m[0]) refs.add(m[0]);
  }
  return [...refs].sort();
}

function applySharedRefs(text) {
  return text
    .replace(
      /`docs\/req\/users-requirements\.md` \(\*\*Business terms\*\*\)/g,
      "`docs/req/_shared/glossary.md`",
    )
    .replace(
      /`docs\/req\/invitations-requirements\.md` \(\*\*Business terms\*\*\)/g,
      "`docs/req/_shared/glossary.md`",
    )
    .replace(
      /`docs\/req\/invitations-requirements\.md` and `docs\/req\/users-requirements\.md` \(\*\*Business terms\*\*\)/g,
      "`docs/req/_shared/glossary.md`",
    )
    .replace(
      /`docs\/req\/users-requirements\.md` and `docs\/req\/invitations-requirements\.md` \(\*\*Business terms\*\*\)/g,
      "`docs/req/_shared/glossary.md`",
    )
    .replace(
      /`docs\/req\/invitations-requirements\.md`/g,
      "`docs/req/_shared/glossary.md`",
    )
    .replace(/`docs\/req\/passkeys-requirements\.md`/g, "`docs/req/passkeys/`")
    .replace(/`docs\/req\/auth-requirements\.md`/g, "`docs/req/identity/`")
    .replace(/`docs\/req\/users-requirements\.md`/g, "`docs/req/users/`")
    .replace(/`docs\/req\/roles-requirements\.md`/g, "`docs/req/access/`")
    .replace(/`docs\/req\/issues-requirements\.md`/g, "`docs/req/issues/`")
    .replace(
      /See `docs\/req\/invitations-requirements\.md` \(\*\*Business terms\*\*\)\./g,
      "See `docs/req/_shared/glossary.md`.",
    )
    .replace(
      /Combined account compliance gates/g,
      "Combined account compliance gates (see `docs/req/_shared/compliance-gates.md`)",
    )
    .replace(
      /### Combined account compliance gates\n\nWhen both password expiration/g,
      "### Combined account compliance gates\n\nSee `docs/req/_shared/compliance-gates.md` for the full ordered gate list including passkeys.\n\nWhen both password expiration",
    );
}

function stripDocLevelSections(preamble, domain) {
  // Remove sections that move to _shared/
  let text = preamble;
  if (domain === "users") {
    text = text.replace(
      /## Business terms[\s\S]*?(?=\n## Account model|\n---|\n# REQ-|$)/,
      "",
    );
    text = text.replace(/## Account model[\s\S]*?(?=\n---|\n# REQ-|$)/, "");
  }
  if (domain === "invitations") {
    text = text.replace(
      /## Business terms[\s\S]*?(?=\n## Account invitation|\n---|\n# REQ-|$)/,
      "",
    );
    text = text.replace(
      /## Account invitation[\s\S]*?(?=\n---|\n# REQ-|$)/,
      "",
    );
  }
  return text.trim();
}

function stripReqSections(body, id) {
  let text = body;
  if (id === "REQ-AUTH-013") {
    text = text.replace(
      /### Combined account compliance gates[\s\S]*?(?=\n### Sign-in order|\n### Administrator reset|\n### User details|\n### States)/,
      "### Combined account compliance gates\n\nSee `docs/req/_shared/compliance-gates.md`.\n\n",
    );
  }
  if (id === "REQ-PKY-006") {
    text = text.replace(
      /### Combined account compliance gates \(full order\)[\s\S]*?(?=\n### Strict passkey setup)/,
      "### Combined account compliance gates (full order)\n\nSee `docs/req/_shared/compliance-gates.md`.\n\n",
    );
  }
  if (id === "REQ-ROL-001") {
    // Keep full content in REQ file; permissions.md will be a pointer
  }
  return text;
}

const allReqs = [];

for (const { file, domain, prefix } of MONOLITHS) {
  const filePath = path.join(REQ_ROOT, file);
  if (!fs.existsSync(filePath)) {
    console.warn(`Skip missing: ${file}`);
    continue;
  }

  const content = fs.readFileSync(filePath, "utf8");
  const { preamble, sections } = parseReqSections(content);
  const domainDir = path.join(REQ_ROOT, domain);
  fs.mkdirSync(domainDir, { recursive: true });

  for (const section of sections) {
    const slug = slugify(section.title);
    const filename = `${section.id.toLowerCase()}-${slug}.md`;
    const filepath = path.join(domainDir, filename);
    let body = section.lines.join("\n").trim();
    body = stripReqSections(body, section.id);
    body = applySharedRefs(body);
    const dependsOn = extractDependsOn(body).filter((r) => r !== section.id);

    const frontmatter = [
      "---",
      `id: ${section.id}`,
      `title: ${section.title}`,
      `domain: ${domain}`,
      "status: active",
      `depends_on: [${dependsOn.join(", ")}]`,
      "---",
      "",
    ].join("\n");

    const intro =
      domain === "identity" && section.id === "REQ-AUTH-001"
        ? "> Passkeys: `docs/req/passkeys/`. Account terms: `docs/req/_shared/glossary.md`.\n\n"
        : "";

    fs.writeFileSync(filepath, frontmatter + intro + body + "\n", "utf8");
    allReqs.push({
      id: section.id,
      title: section.title,
      domain,
      file: `${domain}/${filename}`,
      dependsOn,
    });
    console.log(`Created ${domain}/${filename}`);
  }
}

// Write manifest data for validate script
const manifestPath = path.join(REQ_ROOT, ".req-manifest.json");
fs.writeFileSync(
  manifestPath,
  JSON.stringify({ reqs: allReqs }, null, 2),
  "utf8",
);
console.log(
  `\nWrote ${allReqs.length} REQ files; manifest at .req-manifest.json`,
);

#!/usr/bin/env node
/**
 * Validate docs/req structure: atomic REQ files, shared docs, change records, cross-references.
 * Run: npm run req:validate
 */

import { execSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";

const REQ_ROOT = path.join(process.cwd(), "docs", "req");
const DOMAIN_DIRS = [
  "identity",
  "users",
  "invitations",
  "access",
  "passkeys",
  "projects",
  "issues",
  "time",
  "billing",
];
const SHARED_FILES = [
  "glossary.md",
  "account-model.md",
  "compliance-gates.md",
  "permissions.md",
];
const DEPRECATED_MONOLITHS = [
  "auth-requirements.md",
  "users-requirements.md",
  "invitations-requirements.md",
  "roles-requirements.md",
  "passkeys-requirements.md",
  "issues-requirements.md",
];
const DEPRECATED_CHANGELOGS = [
  "auth-requirements-changelog.md",
  "users-requirements-changelog.md",
  "invitations-requirements-changelog.md",
  "issues-requirements-changelog.md",
  "passkeys-requirements-changelog.md",
];

const errors = [];
const warnings = [];

function error(msg) {
  errors.push(msg);
}

function warn(msg) {
  warnings.push(msg);
}

function parseFrontmatter(content) {
  if (!content.startsWith("---\n")) return null;
  const end = content.indexOf("\n---\n", 4);
  if (end === -1) return null;
  const block = content.slice(4, end);
  const meta = {};
  for (const line of block.split("\n")) {
    const m = line.match(/^(\w+):\s*(.+)$/);
    if (!m) continue;
    const [, key, raw] = m;
    if (raw.startsWith("[") && raw.endsWith("]")) {
      meta[key] = raw
        .slice(1, -1)
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean);
    } else {
      meta[key] = raw;
    }
  }
  return meta;
}

function collectReqFiles() {
  const files = [];
  for (const domain of DOMAIN_DIRS) {
    const dir = path.join(REQ_ROOT, domain);
    if (!fs.existsSync(dir)) {
      error(`Missing domain directory: docs/req/${domain}/`);
      continue;
    }
    for (const name of fs.readdirSync(dir)) {
      if (name.endsWith(".md")) {
        files.push({ domain, name, path: path.join(dir, name) });
      }
    }
  }
  return files;
}

function main() {
  // Shared docs
  for (const file of SHARED_FILES) {
    const p = path.join(REQ_ROOT, "_shared", file);
    if (!fs.existsSync(p))
      error(`Missing shared doc: docs/req/_shared/${file}`);
  }

  // Deprecated files should be removed
  for (const file of [...DEPRECATED_MONOLITHS, ...DEPRECATED_CHANGELOGS]) {
    if (fs.existsSync(path.join(REQ_ROOT, file))) {
      error(
        `Deprecated file still present (remove after migration): docs/req/${file}`,
      );
    }
  }

  if (!fs.existsSync(path.join(REQ_ROOT, "changes", "_template.md"))) {
    error("Missing docs/req/changes/_template.md");
  }

  const reqFiles = collectReqFiles();
  const ids = new Map();
  const idPattern = /^req-[a-z]+-\d{3}-.+\.md$/;

  for (const { domain, name, path: filePath } of reqFiles) {
    if (!idPattern.test(name)) {
      warn(
        `Non-standard filename (expected req-<area>-<nnn>-<slug>.md): ${domain}/${name}`,
      );
    }

    const content = fs.readFileSync(filePath, "utf8");
    const meta = parseFrontmatter(content);
    if (!meta) {
      error(`Missing YAML frontmatter: docs/req/${domain}/${name}`);
      continue;
    }

    if (!meta.id || !/^REQ-[A-Z]+-\d{3}$/.test(meta.id)) {
      error(`Invalid or missing id in frontmatter: docs/req/${domain}/${name}`);
    }
    if (meta.domain !== domain) {
      error(
        `Frontmatter domain "${meta.domain}" does not match folder "${domain}": ${name}`,
      );
    }
    if (!meta.title) {
      error(`Missing title in frontmatter: ${domain}/${name}`);
    }
    if (
      meta.status !== "active" &&
      meta.status !== "draft" &&
      meta.status !== "deprecated"
    ) {
      warn(`Unexpected status "${meta.status}" in ${domain}/${name}`);
    }

    if (ids.has(meta.id)) {
      error(
        `Duplicate REQ id ${meta.id}: ${ids.get(meta.id)} and ${domain}/${name}`,
      );
    } else {
      ids.set(meta.id, `${domain}/${name}`);
    }

    const expectedPrefix = meta.id.toLowerCase().replace(/_/g, "-");
    if (!name.startsWith(expectedPrefix)) {
      warn(`Filename should start with ${expectedPrefix}: ${domain}/${name}`);
    }
  }

  // Cross-reference validation
  const allIds = new Set(ids.keys());
  const reqRefRe = /REQ-[A-Z]+-\d{3}/g;

  for (const { domain, name, path: filePath } of reqFiles) {
    const content = fs.readFileSync(filePath, "utf8");
    const body = content.replace(/^---[\s\S]*?---\n/, "");
    const refs = [...new Set(body.match(reqRefRe) ?? [])];
    const meta = parseFrontmatter(content);
    for (const ref of refs) {
      if (!allIds.has(ref)) {
        error(`Broken REQ reference ${ref} in docs/req/${domain}/${name}`);
      }
    }
    if (meta?.depends_on) {
      for (const dep of meta.depends_on) {
        if (!allIds.has(dep)) {
          error(
            `depends_on references missing REQ ${dep} in ${domain}/${name}`,
          );
        }
      }
    }
  }

  // Markdown path references under docs/req
  for (const { domain, name, path: filePath } of reqFiles) {
    const content = fs.readFileSync(filePath, "utf8");
    const links = content.match(/`docs\/req\/[^`]+`/g) ?? [];
    for (const link of links) {
      const target = link.slice(1, -1);
      if (target.includes("*")) continue;
      const resolved = path.join(process.cwd(), target);
      const asFile = resolved.endsWith(".md") ? resolved : null;
      const asDir =
        fs.existsSync(resolved) && fs.statSync(resolved).isDirectory();
      if (asFile && !fs.existsSync(asFile)) {
        error(`Broken path reference ${link} in docs/req/${domain}/${name}`);
      } else if (!asFile && !asDir && !fs.existsSync(resolved)) {
        // Allow domain folder references like docs/req/passkeys/
        if (
          !target.match(
            /docs\/req\/(identity|users|invitations|access|passkeys|projects|issues|time|billing|_shared)\/?$/,
          )
        ) {
          error(`Broken path reference ${link} in docs/req/${domain}/${name}`);
        }
      }
    }
  }

  // Pending change records
  const changesDir = path.join(REQ_ROOT, "changes");
  if (fs.existsSync(changesDir)) {
    for (const name of fs.readdirSync(changesDir)) {
      if (
        !name.endsWith(".md") ||
        name === "_template.md" ||
        name === "README.md"
      )
        continue;
      const content = fs.readFileSync(path.join(changesDir, name), "utf8");
      if (!/\*\*Status:\*\*\s*(pending|done)/i.test(content)) {
        warn(`Change record missing Status (pending|done): changes/${name}`);
      }
      const touched = content.match(/REQ-[A-Z]+-\d{3}/g) ?? [];
      for (const ref of [...new Set(touched)]) {
        if (!allIds.has(ref)) {
          error(`Change record references missing REQ ${ref}: changes/${name}`);
        }
      }
    }
  }

  // Regenerate manifest for tooling
  const manifest = {
    generatedAt: new Date().toISOString(),
    reqs: [...ids.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([id, file]) => {
        const filePath = path.join(REQ_ROOT, file);
        const meta = parseFrontmatter(fs.readFileSync(filePath, "utf8"));
        return {
          id,
          title: meta?.title ?? "",
          domain: meta?.domain ?? "",
          status: meta?.status ?? "active",
          file,
          depends_on: meta?.depends_on ?? [],
        };
      }),
  };
  fs.writeFileSync(
    path.join(REQ_ROOT, ".req-manifest.json"),
    JSON.stringify(manifest, null, 2),
  );

  // Refresh README manifest
  execSync("node scripts/req-readme.mjs", { stdio: "pipe" });

  if (!fs.existsSync(path.join(REQ_ROOT, "README.md"))) {
    error("Failed to generate docs/req/README.md");
  }

  // Report
  if (warnings.length) {
    console.warn(`\n${warnings.length} warning(s):`);
    for (const w of warnings) console.warn(`  ⚠ ${w}`);
  }

  if (errors.length) {
    console.error(`\n${errors.length} error(s):`);
    for (const e of errors) console.error(`  ✗ ${e}`);
    process.exit(1);
  }

  console.log(
    `✓ Requirements valid: ${ids.size} REQ files, ${SHARED_FILES.length} shared docs`,
  );
}

main();

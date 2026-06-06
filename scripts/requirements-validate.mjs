#!/usr/bin/env node
/**
 * Validate docs/requirements structure: FR specs, NFR docs, acceptance scenarios, cross-references.
 * Run: npm run requirements:validate
 */

import { execSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import {
  ROOT,
  collectFunctionalFiles,
  collectNfrDocs,
  collectReferenceDocs,
  collectSharedFunctionalDocs,
  parseAcceptanceScenarios,
  parseFrontmatter,
} from "./requirements-lib.mjs";

const errors = [];
const warnings = [];

function error(msg) {
  errors.push(msg);
}

function warn(msg) {
  warnings.push(msg);
}

function validateSharedDoc(doc, idPattern) {
  const rel = doc.relPath;
  if (!doc.id) {
    error(`Missing id in frontmatter: ${rel}`);
    return false;
  }
  if (!idPattern.test(doc.id)) {
    error(`Invalid id "${doc.id}" in frontmatter: ${rel}`);
    return false;
  }
  if (doc.type && doc.type !== doc.expectedType) {
    warn(`Expected type: ${doc.expectedType} in ${rel}`);
  }
  if (!doc.title) {
    warn(`Missing title in frontmatter: ${rel}`);
  }
  return true;
}

function main() {
  if (!fs.existsSync(ROOT)) {
    error("Missing docs/requirements/");
    report();
    return;
  }

  const referenceDocs = collectReferenceDocs();
  if (referenceDocs.length === 0) {
    warn("No reference docs found under docs/requirements/_shared/reference/");
  }

  const nfrDocs = collectNfrDocs();
  if (nfrDocs.length === 0) {
    warn("No NFR docs found under docs/requirements/_shared/non-functional/");
  }

  const sharedFunctionalDocs = collectSharedFunctionalDocs();
  if (sharedFunctionalDocs.length === 0) {
    warn(
      "No shared functional docs found under docs/requirements/_shared/functional/",
    );
  }

  const nfrIds = new Set();
  for (const doc of nfrDocs) {
    if (!validateSharedDoc(doc, /^NFR-[A-Z0-9]+-\d{3}$/)) continue;
    if (nfrIds.has(doc.id)) {
      error(`Duplicate NFR id ${doc.id}`);
    } else {
      nfrIds.add(doc.id);
    }
  }

  const sharedFrIds = new Map();
  for (const doc of sharedFunctionalDocs) {
    if (!validateSharedDoc(doc, /^FR-[A-Z0-9]+-\d{3}$/)) continue;
    if (sharedFrIds.has(doc.id)) {
      error(`Duplicate shared functional id ${doc.id}`);
    } else {
      sharedFrIds.set(doc.id, doc.relPath);
    }
  }

  if (!fs.existsSync(path.join(ROOT, "changes", "_template.md"))) {
    error("Missing docs/requirements/changes/_template.md");
  }

  const frFiles = collectFunctionalFiles();
  if (frFiles.length === 0) {
    warn(
      "No functional specifications found under docs/requirements/functional/",
    );
  }

  const frIds = new Map();
  const allFrIds = new Set([...sharedFrIds.keys()]);

  const frFilenamePattern = /^fr-[a-z]+-\d{3}-.+\.md$/;

  for (const { domain, name, path: filePath } of frFiles) {
    if (!frFilenamePattern.test(name)) {
      warn(
        `Non-standard filename (expected fr-<area>-<nnn>-<slug>.md): functional/${domain}/${name}`,
      );
    }

    const content = fs.readFileSync(filePath, "utf8");
    const meta = parseFrontmatter(content);
    if (!meta) {
      error(`Missing YAML frontmatter: functional/${domain}/${name}`);
      continue;
    }

    if (!meta.id || !/^FR-[A-Z0-9]+-\d{3}$/.test(meta.id)) {
      error(
        `Invalid or missing id in frontmatter: functional/${domain}/${name}`,
      );
    }
    if (meta.type !== "functional") {
      warn(`Expected type: functional in functional/${domain}/${name}`);
    }
    if (meta.domain !== domain) {
      error(
        `Frontmatter domain "${meta.domain}" does not match folder "${domain}": ${name}`,
      );
    }
    if (!meta.title) {
      error(`Missing title in frontmatter: functional/${domain}/${name}`);
    }

    if (frIds.has(meta.id)) {
      error(
        `Duplicate FR id ${meta.id}: ${frIds.get(meta.id)} and functional/${domain}/${name}`,
      );
    } else {
      frIds.set(meta.id, `functional/${domain}/${name}`);
      allFrIds.add(meta.id);
    }

    const expectedPrefix = meta.id.toLowerCase().replace(/_/g, "-");
    if (!name.startsWith(expectedPrefix)) {
      warn(
        `Filename should start with ${expectedPrefix}: functional/${domain}/${name}`,
      );
    }

    const body = content.replace(/^---[\s\S]*?---\n/, "");
    if (!body.includes("## Functional requirements")) {
      error(`Missing ## Functional requirements: functional/${domain}/${name}`);
    }
    if (!body.includes("## Acceptance scenarios")) {
      error(`Missing ## Acceptance scenarios: functional/${domain}/${name}`);
    }
    if (!body.includes("## Non-functional requirements")) {
      error(
        `Missing ## Non-functional requirements: functional/${domain}/${name}`,
      );
    }

    const scenarios = parseAcceptanceScenarios(body);
    if (scenarios.length === 0) {
      error(`No acceptance scenarios in table: functional/${domain}/${name}`);
    }
    for (const sc of scenarios) {
      if (!sc.id?.startsWith("AC-")) {
        error(`Invalid acceptance scenario id in ${domain}/${name}`);
      }
      if (!sc.given || !sc.when || !sc.then) {
        error(`Incomplete acceptance scenario ${sc.id} in ${domain}/${name}`);
      }
    }

    if (/\bREQ-[A-Z]+-\d{3}\b/.test(content)) {
      error(`Legacy REQ- reference found in functional/${domain}/${name}`);
    }
    if (/docs\/req\//.test(content)) {
      error(`Legacy docs/req/ path found in functional/${domain}/${name}`);
    }
  }

  allFrIds.clear();
  for (const id of frIds.keys()) allFrIds.add(id);
  for (const id of sharedFrIds.keys()) allFrIds.add(id);

  const frRefRe = /(?<![A-Z/])FR-[A-Z0-9]+-\d{3}/g;

  for (const { domain, name, path: filePath } of frFiles) {
    const content = fs.readFileSync(filePath, "utf8");
    const body = content.replace(/^---[\s\S]*?---\n/, "");
    const refs = [...new Set(body.match(frRefRe) ?? [])];
    const meta = parseFrontmatter(content);
    for (const ref of refs) {
      if (!allFrIds.has(ref)) {
        error(`Broken FR reference ${ref} in functional/${domain}/${name}`);
      }
    }
    if (meta?.depends_on) {
      for (const dep of meta.depends_on) {
        if (!allFrIds.has(dep)) {
          error(`depends_on references missing FR ${dep} in ${domain}/${name}`);
        }
      }
    }
    if (meta?.inherits_fr) {
      for (const dep of meta.inherits_fr) {
        if (!allFrIds.has(dep)) {
          error(
            `inherits_fr references missing FR ${dep} in ${domain}/${name}`,
          );
        }
      }
    }
    if (meta?.inherits_nfr) {
      for (const dep of meta.inherits_nfr) {
        if (!nfrIds.has(dep)) {
          error(
            `inherits_nfr references missing NFR ${dep} in ${domain}/${name}`,
          );
        }
      }
    }
  }

  for (const { domain, name, path: filePath } of frFiles) {
    const content = fs.readFileSync(filePath, "utf8");
    const links = content.match(/`docs\/requirements\/[^`]+`/g) ?? [];
    for (const link of links) {
      const target = link.slice(1, -1);
      if (target.includes("*")) continue;
      const resolved = path.join(process.cwd(), target);
      if (!fs.existsSync(resolved)) {
        error(`Broken path reference ${link} in functional/${domain}/${name}`);
      }
    }
  }

  const changesDir = path.join(ROOT, "changes");
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
      if (/\bREQ-[A-Z]+-\d{3}\b/.test(content)) {
        error(`Legacy REQ- reference in changes/${name}`);
      }
      const touched = content.match(/(?<![A-Z/])FR-[A-Z0-9]+-\d{3}/g) ?? [];
      for (const ref of [...new Set(touched)]) {
        if (!allFrIds.has(ref)) {
          error(`Change record references missing FR ${ref}: changes/${name}`);
        }
      }
    }
  }

  const manifest = {
    generatedAt: new Date().toISOString(),
    functional: [...frIds.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([id, file]) => {
        const filePath = path.join(ROOT, file);
        const content = fs.readFileSync(filePath, "utf8");
        const meta = parseFrontmatter(content);
        const body = content.replace(/^---[\s\S]*?---\n/, "");
        const scenarios = parseAcceptanceScenarios(body);
        return {
          id,
          title: meta?.title ?? "",
          domain: meta?.domain ?? "",
          type: "functional",
          status: meta?.status ?? "active",
          file,
          depends_on: meta?.depends_on ?? [],
          inherits_nfr: meta?.inherits_nfr ?? [],
          inherits_fr: meta?.inherits_fr ?? [],
          acceptance_scenarios: scenarios.map((s) => s.id),
        };
      }),
    non_functional: nfrDocs
      .filter((doc) => doc.id)
      .sort((a, b) => a.id.localeCompare(b.id))
      .map((doc) => ({
        id: doc.id,
        title: doc.title,
        file: doc.relPath,
        type: "non-functional",
        status: doc.status,
      })),
    shared_functional: sharedFunctionalDocs
      .filter((doc) => doc.id)
      .sort((a, b) => a.id.localeCompare(b.id))
      .map((doc) => ({
        id: doc.id,
        title: doc.title,
        file: doc.relPath,
        type: "functional",
        status: doc.status,
      })),
    reference: referenceDocs.map((doc) => ({
      file: doc.relPath,
    })),
  };

  fs.writeFileSync(
    path.join(ROOT, ".requirements-manifest.json"),
    JSON.stringify(manifest, null, 2),
  );

  execSync("node scripts/requirements-readme.mjs", { stdio: "pipe" });

  if (!fs.existsSync(path.join(ROOT, "README.md"))) {
    error("Failed to generate docs/requirements/README.md");
  }

  if (fs.existsSync(path.join(process.cwd(), "docs", "req"))) {
    warn("Legacy docs/req/ still present — remove after migration is verified");
  }

  report(frIds.size, nfrDocs.length, referenceDocs.length);
}

function report(frCount = 0, nfrCount = 0, referenceCount = 0) {
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
    `✓ Requirements valid: ${frCount} functional specifications, ${nfrCount} NFR documents, ${referenceCount} reference docs`,
  );
}

main();

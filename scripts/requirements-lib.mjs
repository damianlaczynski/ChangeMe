import fs from "node:fs";
import path from "node:path";

export const ROOT = path.join(process.cwd(), "docs", "requirements");
export const FUNCTIONAL_DIR = path.join(ROOT, "functional");

export function parseFrontmatter(content) {
  if (!content.startsWith("---\n")) return null;
  const end = content.indexOf("\n---\n", 4);
  if (end === -1) return null;
  const block = content.slice(4, end);
  const meta = {};
  for (const line of block.split("\n")) {
    const m = line.match(/^([\w_]+):\s*(.+)$/);
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

export function listMarkdownFiles(dir) {
  if (!fs.existsSync(dir)) return [];
  return fs
    .readdirSync(dir)
    .filter((name) => name.endsWith(".md"))
    .sort((a, b) => a.localeCompare(b));
}

export function listDomainDirs() {
  if (!fs.existsSync(FUNCTIONAL_DIR)) return [];
  return fs
    .readdirSync(FUNCTIONAL_DIR, { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => entry.name)
    .sort((a, b) => a.localeCompare(b));
}

export function collectFunctionalFiles() {
  const files = [];
  for (const domain of listDomainDirs()) {
    const dir = path.join(FUNCTIONAL_DIR, domain);
    for (const name of listMarkdownFiles(dir)) {
      files.push({ domain, name, path: path.join(dir, name) });
    }
  }
  return files;
}

export function collectReferenceDocs() {
  const dir = path.join(ROOT, "_shared", "reference");
  return listMarkdownFiles(dir).map((file) => ({
    file,
    relPath: `_shared/reference/${file}`,
    path: path.join(dir, file),
  }));
}

function collectSharedDocs(subdir, expectedType) {
  const dir = path.join(ROOT, "_shared", subdir);
  return listMarkdownFiles(dir).map((file) => {
    const filePath = path.join(dir, file);
    const content = fs.readFileSync(filePath, "utf8");
    const meta = parseFrontmatter(content) ?? {};
    return {
      id: meta.id ?? "",
      title: meta.title ?? "",
      type: meta.type ?? "",
      status: meta.status ?? "active",
      file,
      relPath: `_shared/${subdir}/${file}`,
      path: filePath,
      expectedType,
    };
  });
}

export function collectNfrDocs() {
  return collectSharedDocs("non-functional", "non-functional");
}

export function collectSharedFunctionalDocs() {
  return collectSharedDocs("functional", "functional");
}

export function parseAcceptanceScenarios(body) {
  const section = body.match(
    /## Acceptance scenarios\s*\n+(\|[^\n]*\bID\b[^\n]*\bGiven\b[^\n]*\bWhen\b[^\n]*\bThen\b[^\n]*\|[\s\S]*?)(?=\n## |\n---\s*$|$)/,
  );
  if (!section) return [];
  const rows = section[1].split("\n").filter((l) => l.startsWith("| AC-"));
  return rows.map((row) => {
    const cols = row.split("|").map((c) => c.trim());
    return { id: cols[1], given: cols[2], when: cols[3], then: cols[4] };
  });
}

export function formatDomainLabel(domain) {
  return domain
    .split(/[-_]/)
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}

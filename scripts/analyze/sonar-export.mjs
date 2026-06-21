import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { ensureSonarToken, loadSonarConfig } from "./sonar-bootstrap.mjs";

const PROJECTS = [
  { key: "changeme-backend", name: "ChangeMe Backend" },
  { key: "changeme-frontend", name: "ChangeMe Frontend" },
];

const METRIC_KEYS = [
  "coverage",
  "bugs",
  "vulnerabilities",
  "code_smells",
  "security_hotspots",
  "duplicated_lines_density",
  "ncloc",
  "sqale_index",
  "reliability_rating",
  "security_rating",
  "sqale_rating",
].join(",");

async function sonarGet(baseUrl, token, path) {
  const response = await fetch(`${baseUrl}${path}`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!response.ok) {
    throw new Error(
      `SonarQube API ${path} failed: ${response.status} ${await response.text()}`,
    );
  }

  return response.json();
}

function measuresToMap(component) {
  const map = {};
  for (const measure of component.measures ?? []) {
    map[measure.metric] = measure.value;
  }
  return map;
}

async function fetchAllIssues(baseUrl, token, projectKey) {
  const issues = [];
  const pageSize = 500;
  let page = 1;

  while (true) {
    const data = await sonarGet(
      baseUrl,
      token,
      `/api/issues/search?componentKeys=${encodeURIComponent(projectKey)}&ps=${pageSize}&p=${page}`,
    );
    issues.push(...data.issues);
    if (page * pageSize >= data.total) {
      break;
    }
    page += 1;
  }

  return { total: issues.length, issues };
}

async function exportProject(baseUrl, token, project, outDir) {
  const [measures, qualityGate, issueReport] = await Promise.all([
    sonarGet(
      baseUrl,
      token,
      `/api/measures/component?component=${encodeURIComponent(project.key)}&metricKeys=${encodeURIComponent(METRIC_KEYS)}`,
    ),
    sonarGet(
      baseUrl,
      token,
      `/api/qualitygates/project_status?projectKey=${encodeURIComponent(project.key)}`,
    ),
    fetchAllIssues(baseUrl, token, project.key),
  ]);

  const metricMap = measuresToMap(measures.component);
  const report = {
    projectKey: project.key,
    projectName: project.name,
    exportedAt: new Date().toISOString(),
    qualityGate: qualityGate.projectStatus,
    measures: metricMap,
    issues: issueReport,
  };

  writeFileSync(
    join(outDir, `${project.key}-report.json`),
    `${JSON.stringify(report, null, 2)}\n`,
    "utf8",
  );

  return report;
}

function formatRating(value) {
  const ratings = {
    "1.0": "A",
    "2.0": "B",
    "3.0": "C",
    "4.0": "D",
    "5.0": "E",
  };
  return ratings[value] ?? value ?? "—";
}

function buildSummaryText(reports) {
  const lines = [`SonarQube export — ${new Date().toISOString()}`, ""];

  for (const report of reports) {
    const m = report.measures;
    lines.push(`${report.projectName} (${report.projectKey})`);
    lines.push(`  Quality gate: ${report.qualityGate?.status ?? "UNKNOWN"}`);
    lines.push(`  Coverage: ${m.coverage ?? "—"}%`);
    lines.push(
      `  Bugs: ${m.bugs ?? "—"}, Vulnerabilities: ${m.vulnerabilities ?? "—"}, Code smells: ${m.code_smells ?? "—"}, Hotspots: ${m.security_hotspots ?? "—"}`,
    );
    lines.push(
      `  Ratings — reliability: ${formatRating(m.reliability_rating)}, security: ${formatRating(m.security_rating)}, maintainability: ${formatRating(m.sqale_rating)}`,
    );
    lines.push(`  Issues exported: ${report.issues.total}`);
    lines.push(`  JSON: artifacts/sonar/${report.projectKey}-report.json`);
    lines.push("");
  }

  return `${lines.join("\n")}\n`;
}

export async function exportSonarReports(root = process.cwd()) {
  const config = loadSonarConfig(root);
  const outDir = join(root, "artifacts", "sonar");
  mkdirSync(outDir, { recursive: true });

  const tokenPath = join(outDir, "token");
  let token;

  if (existsSync(tokenPath)) {
    token = readFileSync(tokenPath, "utf8").trim();
  } else {
    token = await ensureSonarToken(root);
  }

  const reports = [];
  for (const project of PROJECTS) {
    reports.push(
      await exportProject(config.SONAR_HOST_URL, token, project, outDir),
    );
  }

  writeFileSync(
    join(outDir, "summary.json"),
    `${JSON.stringify({ exportedAt: new Date().toISOString(), projects: reports }, null, 2)}\n`,
    "utf8",
  );
  writeFileSync(join(outDir, "summary.txt"), buildSummaryText(reports), "utf8");

  console.log(`Wrote ${join(outDir, "summary.txt")}`);
  for (const project of PROJECTS) {
    console.log(`Wrote ${join(outDir, `${project.key}-report.json`)}`);
  }

  return reports;
}

if (
  import.meta.url.startsWith("file:") &&
  process.argv[1]?.includes("sonar-export.mjs")
) {
  await exportSonarReports();
}

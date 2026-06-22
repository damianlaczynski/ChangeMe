import { execSync } from "node:child_process";
import { ensureSonarToken } from "./sonar-bootstrap.mjs";
import { exportSonarReports } from "./sonar-export.mjs";

const targets = new Set(process.argv.slice(2));
const runFrontend = targets.size === 0 || targets.has("frontend");
const runBackend = targets.size === 0 || targets.has("backend");

if (
  targets.size > 0 &&
  ![...targets].every((target) => target === "frontend" || target === "backend")
) {
  console.error(
    "Usage: node scripts/analyze/sonar-run.mjs [frontend] [backend]",
  );
  process.exit(1);
}

const COMPOSE_ANALYZE =
  "docker compose -f docker-compose.yml -f docker-compose.analyze.yml";

const failed = [];

function runCompose(command, extraEnv = {}) {
  execSync(command, {
    stdio: "inherit",
    shell: true,
    cwd: process.cwd(),
    env: { ...process.env, ...extraEnv },
  });
}

function runStep(name, fn) {
  try {
    fn();
  } catch {
    failed.push(name);
    console.error(
      `[analyze:sonar] ${name} failed — continuing so remaining Sonar steps and export still run.\n`,
    );
  }
}

async function exportWithRetry(attempts = 3, delayMs = 5000) {
  for (let attempt = 1; attempt <= attempts; attempt += 1) {
    try {
      await exportSonarReports();
      return;
    } catch (error) {
      if (attempt === attempts) {
        throw error;
      }
      console.warn(
        `Sonar export attempt ${attempt}/${attempts} failed (${error.message}) — retrying in ${delayMs / 1000}s...`,
      );
      await new Promise((resolve) => setTimeout(resolve, delayMs));
    }
  }
}

console.log("Starting SonarQube (profile security)...");
runCompose(
  `${COMPOSE_ANALYZE} --profile security up -d sonarqube-db sonarqube`,
);

const token = await ensureSonarToken();

if (runFrontend) {
  runStep("frontend-coverage", () => {
    console.log("Running frontend tests with coverage...");
    execSync("npm run test:frontend:coverage", { stdio: "inherit" });
  });

  runStep("sonar-frontend", () => {
    console.log("Running SonarScanner for frontend...");
    runCompose(
      `${COMPOSE_ANALYZE} --profile security run --rm sonar-frontend`,
      {
        SONAR_TOKEN: token,
      },
    );
  });
}

if (runBackend) {
  runStep("sonar-backend", () => {
    console.log("Running dotnet-sonarscanner for backend...");
    runCompose(`${COMPOSE_ANALYZE} --profile security run --rm sonar-backend`, {
      SONAR_TOKEN: token,
    });
  });
}

console.log("Exporting SonarQube reports to artifacts/sonar/...");
try {
  await exportWithRetry();
} catch {
  failed.push("sonar-export");
  console.error(
    "[analyze:sonar] Export failed after retries — run npm run analyze:sonar:export when SonarQube is up.\n",
  );
}

console.log("SonarQube dashboard: http://localhost:9000");

if (failed.length > 0) {
  console.error(`Sonar steps with errors: ${failed.join(", ")}`);
  process.exit(1);
}

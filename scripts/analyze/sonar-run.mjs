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

function runCompose(command, extraEnv = {}) {
  execSync(command, {
    stdio: "inherit",
    shell: true,
    cwd: process.cwd(),
    env: { ...process.env, ...extraEnv },
  });
}

console.log("Starting SonarQube (profile security)...");
runCompose(
  `${COMPOSE_ANALYZE} --profile security up -d sonarqube-db sonarqube`,
);

const token = await ensureSonarToken();

if (runFrontend) {
  console.log("Running frontend tests with coverage...");
  execSync("npm run test:frontend:coverage", { stdio: "inherit" });
  console.log("Running SonarScanner for frontend...");
  runCompose(`${COMPOSE_ANALYZE} --profile security run --rm sonar-frontend`, {
    SONAR_TOKEN: token,
  });
}

if (runBackend) {
  console.log("Running dotnet-sonarscanner for backend...");
  runCompose(`${COMPOSE_ANALYZE} --profile security run --rm sonar-backend`, {
    SONAR_TOKEN: token,
  });
}

console.log("Exporting SonarQube reports to artifacts/sonar/...");
await exportSonarReports();

console.log("SonarQube dashboard: http://localhost:9000");

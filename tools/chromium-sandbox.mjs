import { existsSync, mkdirSync } from 'node:fs';
import path from 'node:path';
import {
  Browser,
  computeExecutablePath,
  detectBrowserPlatform,
} from '@puppeteer/browsers';

export function chromiumSandboxEnv(name) {
  if (!process.env.TRAE_SANDBOX_SBOX_ID) return process.env;
  const home = path.resolve('.qa-device', 'chromium-home', name);
  mkdirSync(home, { recursive: true });
  return { ...process.env, HOME: home };
}

export function chromiumSandboxArgs(name) {
  if (!process.env.TRAE_SANDBOX_SBOX_ID) return [];
  const crashDir = path.resolve('.qa-device', 'chromium-crashpad', name);
  mkdirSync(crashDir, { recursive: true });
  return [`--crash-dumps-dir=${crashDir}`];
}

export function chromiumSandboxExecutablePath(chromeExecutablePath) {
  if (!process.env.TRAE_SANDBOX_SBOX_ID) return undefined;
  const platform = detectBrowserPlatform();
  const marker = `${path.sep}chrome${path.sep}${platform}-`;
  const markerIndex = chromeExecutablePath.lastIndexOf(marker);
  if (markerIndex < 0) return undefined;

  const buildStart = markerIndex + marker.length;
  const buildEnd = chromeExecutablePath.indexOf(path.sep, buildStart);
  if (buildEnd < 0) return undefined;
  const executablePath = computeExecutablePath({
    browser: Browser.CHROMEHEADLESSSHELL,
    buildId: chromeExecutablePath.slice(buildStart, buildEnd),
    cacheDir: chromeExecutablePath.slice(0, markerIndex),
    platform,
  });
  return existsSync(executablePath) ? executablePath : undefined;
}

export function chromiumSandboxLaunchOptions(name, chromeExecutablePath) {
  const executablePath = chromiumSandboxExecutablePath(chromeExecutablePath);
  return {
    env: chromiumSandboxEnv(name),
    ...(executablePath ? { executablePath } : {}),
  };
}
